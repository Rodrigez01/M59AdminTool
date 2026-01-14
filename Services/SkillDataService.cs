using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace M59AdminTool.Services
{
    public sealed class SkillDataService
    {
        private readonly string? _blakstonPath;
        private readonly string? _serverRoot;

        public SkillDataService()
        {
            _blakstonPath = ResolveBlakstonPath();
            _serverRoot = ResolveServerRoot(_blakstonPath);
        }

        public async Task<Dictionary<int, SkillInfo>> LoadSkillsAsync()
        {
            var skills = new Dictionary<int, SkillInfo>();
            if (string.IsNullOrWhiteSpace(_serverRoot))
                return skills;

            var khdPath = _blakstonPath;
            if (string.IsNullOrWhiteSpace(khdPath) || !File.Exists(khdPath))
                return skills;

            var skillRoot = Path.Combine(_serverRoot, "kod", "object", "passive", "skill");
            if (!Directory.Exists(skillRoot))
                return skills;

            var skidMap = await LoadSkillIdMapAsync(khdPath);
            var schoolNames = LoadSchoolNames();

            foreach (var kodFile in Directory.EnumerateFiles(skillRoot, "*.kod", SearchOption.AllDirectories))
            {
                var content = await File.ReadAllTextAsync(kodFile, GetKodEncoding());
                var classMatch = Regex.Match(content, @"(?m)^\s*(\w+)\s+is\s+Skill\b");
                if (!classMatch.Success)
                    continue;

                var className = classMatch.Groups[1].Value;
                var lowerClassName = className.ToLowerInvariant();

                var skillNumMatch = Regex.Match(content, @"\bviSkill_num\s*=\s*(SKID_[A-Za-z0-9_]+|\d+)");
                if (!skillNumMatch.Success)
                    continue;

                var skillId = ResolveSkillId(skillNumMatch.Groups[1].Value, skidMap);
                if (skillId <= 0)
                    continue;

                var schoolMatch = Regex.Match(content, @"\bviSchool\s*=\s*([A-Za-z0-9_]+)");
                var schoolConst = schoolMatch.Success ? schoolMatch.Groups[1].Value : string.Empty;
                schoolNames.TryGetValue(schoolConst, out var schoolName);
                var schoolNameEn = schoolName?.English;
                var schoolNameDe = schoolName?.German;

                var level = 0;
                var levelMatch = Regex.Match(content, @"\bviSkill_Level\s*=\s*(\d+)");
                if (levelMatch.Success)
                    int.TryParse(levelMatch.Groups[1].Value, out level);

                var englishName = className;
                var nameRegex = new Regex($@"\b{Regex.Escape(lowerClassName)}_name_rsc\s*=\s*""([^""]+)""",
                    RegexOptions.IgnoreCase);
                var nameMatch = nameRegex.Match(content);
                if (nameMatch.Success)
                    englishName = nameMatch.Groups[1].Value;

                string? germanName = null;
                var lkodPath = Path.ChangeExtension(kodFile, ".lkod");
                if (File.Exists(lkodPath))
                {
                    var lkodContent = await File.ReadAllTextAsync(lkodPath, Encoding.Default);
                    var germanRegex = new Regex($@"\b{Regex.Escape(lowerClassName)}_name_rsc\s*=\s*de\s*""([^""]+)""",
                        RegexOptions.IgnoreCase);
                    var germanMatch = germanRegex.Match(lkodContent);
                    if (germanMatch.Success)
                        germanName = germanMatch.Groups[1].Value;
                }

                skills[skillId] = new SkillInfo
                {
                    SkillId = skillId,
                    ClassName = className,
                    EnglishName = englishName,
                    GermanName = germanName,
                    SchoolConst = schoolConst,
                    SchoolName = schoolNameEn,
                    SchoolNameDe = schoolNameDe,
                    Level = level
                };
            }

            return skills;
        }

        public async Task<Dictionary<int, SkillSchoolInfo>> LoadSkillSchoolsByIdAsync()
        {
            var map = new Dictionary<int, SkillSchoolInfo>();
            if (string.IsNullOrWhiteSpace(_blakstonPath) || !File.Exists(_blakstonPath))
                return map;

            var lines = await File.ReadAllLinesAsync(_blakstonPath, GetKodEncoding());
            var regex = new Regex(@"^\s*(SKS_[A-Za-z0-9_]+)\s*=\s*(\d+)", RegexOptions.Compiled);
            var names = LoadSchoolNames();

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                if (!int.TryParse(match.Groups[2].Value, out var id))
                    continue;

                var constName = match.Groups[1].Value;
                var suffix = constName.Substring(4);
                var key = suffix.ToLowerInvariant();
                names.TryGetValue(key, out var school);

                map[id] = new SkillSchoolInfo
                {
                    ConstName = constName,
                    EnglishName = school?.English,
                    GermanName = school?.German
                };
            }

            return map;
        }

        private static int ResolveSkillId(string token, Dictionary<string, int> skidMap)
        {
            if (token.StartsWith("SKID_", StringComparison.OrdinalIgnoreCase))
            {
                return skidMap.TryGetValue(token, out var id) ? id : 0;
            }

            return int.TryParse(token, out var numeric) ? numeric : 0;
        }

        private static async Task<Dictionary<string, int>> LoadSkillIdMapAsync(string khdPath)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var regex = new Regex(@"^\s*(SKID_[A-Za-z0-9_]+)\s*=\s*(\d+)", RegexOptions.Compiled);
            var lines = await File.ReadAllLinesAsync(khdPath, GetKodEncoding());

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                if (!int.TryParse(match.Groups[2].Value, out var id))
                    continue;

                var name = match.Groups[1].Value;
                if (!map.ContainsKey(name))
                    map[name] = id;
            }

            return map;
        }

        private Dictionary<string, SchoolName> LoadSchoolNames()
        {
            var map = new Dictionary<string, SchoolName>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(_serverRoot))
                return map;

            var skillKod = Path.Combine(_serverRoot, "kod", "object", "passive", "skill.kod");
            var skillLkod = Path.Combine(_serverRoot, "kod", "object", "passive", "skill.lkod");

            var english = LoadSchoolNamesFromFile(skillKod, "skill_school_");
            var german = LoadSchoolNamesFromFile(skillLkod, "skill_school_");

            foreach (var kvp in english)
            {
                map[kvp.Key] = new SchoolName { English = kvp.Value, German = german.TryGetValue(kvp.Key, out var de) ? de : null };
            }

            return map;
        }

        private static Dictionary<string, string> LoadSchoolNamesFromFile(string path, string prefix)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(path))
                return map;

            var lines = File.ReadAllLines(path, GetKodEncoding());
            var regex = new Regex(@"^\s*" + Regex.Escape(prefix) + @"([A-Za-z0-9_]+)\s*=\s*(?:de\s*)?""([^""]+)""",
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                var suffix = match.Groups[1].Value.Trim();
                var value = match.Groups[2].Value.Trim();
                if (!map.ContainsKey(suffix))
                    map[suffix] = value;
            }

            return map;
        }

        private static Encoding GetKodEncoding()
        {
            return Encoding.GetEncoding(1252);
        }

        private static string? ResolveBlakstonPath()
        {
            var candidates = new[]
            {
                AppDomain.CurrentDomain.BaseDirectory,
                Environment.CurrentDirectory
            };

            foreach (var start in candidates)
            {
                var dir = new DirectoryInfo(start);
                for (var i = 0; i < 6 && dir != null; i++)
                {
                    var candidate = Path.Combine(dir.FullName, "Server-104-main", "kod", "include", "blakston.khd");
                    if (File.Exists(candidate))
                        return candidate;

                    dir = dir.Parent;
                }
            }

            return null;
        }

        private static string? ResolveServerRoot(string? blakstonPath)
        {
            if (string.IsNullOrWhiteSpace(blakstonPath))
                return null;

            var includeDir = Path.GetDirectoryName(blakstonPath);
            var kodDir = includeDir != null ? Directory.GetParent(includeDir) : null;
            var serverDir = kodDir != null ? Directory.GetParent(kodDir.FullName) : null;

            return serverDir?.FullName;
        }

        private sealed class SchoolName
        {
            public string? English { get; set; }
            public string? German { get; set; }
        }
    }

    public sealed class SkillInfo
    {
        public int SkillId { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public string EnglishName { get; set; } = string.Empty;
        public string? GermanName { get; set; }
        public string SchoolConst { get; set; } = string.Empty;
        public string? SchoolName { get; set; }
        public string? SchoolNameDe { get; set; }
        public int Level { get; set; }
    }

    public sealed class SkillSchoolInfo
    {
        public string ConstName { get; set; } = string.Empty;
        public string? EnglishName { get; set; }
        public string? GermanName { get; set; }
    }
}

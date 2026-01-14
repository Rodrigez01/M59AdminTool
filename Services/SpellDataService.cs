using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;

namespace M59AdminTool.Services
{
    public class SpellDataService
    {
        private readonly string? _blakstonPath;
        private readonly string? _serverRoot;
        private readonly string? _spellDatabasePath;

        public SpellDataService()
        {
            _blakstonPath = ResolveBlakstonPath();
            _serverRoot = ResolveServerRoot(_blakstonPath);
            _spellDatabasePath = ResolveSpellDatabasePath();
        }

        public async Task<Dictionary<int, SpellInfo>> LoadSpellIdsAsync()
        {
            if (string.IsNullOrWhiteSpace(_blakstonPath) || !File.Exists(_blakstonPath))
                return new Dictionary<int, SpellInfo>();

            var sidMap = await LoadSidMapAsync();
            if (sidMap.Count == 0)
                return new Dictionary<int, SpellInfo>();

            var classMap = await LoadSpellClassMapAsync();
            var classResources = LoadSpellResourceMap();

            var map = new Dictionary<int, SpellInfo>();
            foreach (var kvp in sidMap)
            {
                var id = kvp.Key;
                var sidName = kvp.Value;
                var germanName = ResolveGermanName(sidName, classMap, classResources);
                map[id] = new SpellInfo(sidName, germanName);
            }

            return map;
        }

        public async Task<HashSet<int>> LoadItemSpellIdsAsync()
        {
            var set = new HashSet<int>();
            if (string.IsNullOrWhiteSpace(_serverRoot))
                return set;

            var spellcasterPath = Path.Combine(_serverRoot, "kod", "object", "passive", "itematt", "weapatt", "waspell.kod");
            if (!File.Exists(spellcasterPath))
                return set;

            var sidMap = await LoadSidMapAsync();
            if (sidMap.Count == 0)
                return set;

            var nameToId = sidMap.ToDictionary(kvp => kvp.Value, kvp => kvp.Key, StringComparer.OrdinalIgnoreCase);
            var text = await File.ReadAllTextAsync(spellcasterPath, GetKodEncoding());
            var regex = new Regex(@"\bSID_[A-Za-z0-9_]+\b", RegexOptions.Compiled);

            foreach (Match match in regex.Matches(text))
            {
                var sidName = match.Value.Trim();
                if (nameToId.TryGetValue(sidName, out var value))
                {
                    set.Add(value);
                }
            }

            return set;
        }

        public async Task<Dictionary<string, SpellSchoolInfo>> LoadSpellSchoolMapAsync()
        {
            var map = new Dictionary<string, SpellSchoolInfo>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(_spellDatabasePath) || !File.Exists(_spellDatabasePath))
                return map;

            string json;
            try
            {
                json = await File.ReadAllTextAsync(_spellDatabasePath, Encoding.UTF8);
            }
            catch
            {
                return map;
            }

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("SpellSchools", out var schools))
                    return map;

                foreach (var schoolProp in schools.EnumerateObject())
                {
                    var schoolObj = schoolProp.Value;
                    var schoolName = schoolObj.TryGetProperty("Name", out var nameEl) ? nameEl.GetString() : null;
                    if (string.IsNullOrWhiteSpace(schoolName))
                        continue;

                    if (!schoolObj.TryGetProperty("Spells", out var spells))
                        continue;

                    foreach (var spell in spells.EnumerateArray())
                    {
                        if (!spell.TryGetProperty("Id", out var idEl))
                            continue;
                        var sidName = idEl.GetString();
                        if (string.IsNullOrWhiteSpace(sidName))
                            continue;

                        var level = spell.TryGetProperty("Value", out var levelEl) ? levelEl.GetInt32() : 0;
                        map[sidName] = new SpellSchoolInfo(schoolName, level);
                    }
                }
            }
            catch
            {
                return map;
            }

            return map;
        }

        public async Task<Dictionary<int, string>> LoadSpellSchoolsByIdAsync()
        {
            var map = new Dictionary<int, string>();
            if (string.IsNullOrWhiteSpace(_spellDatabasePath) || !File.Exists(_spellDatabasePath))
                return map;

            string json;
            try
            {
                json = await File.ReadAllTextAsync(_spellDatabasePath, Encoding.UTF8);
            }
            catch
            {
                return map;
            }

            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("SpellSchools", out var schools))
                    return map;

                foreach (var schoolProp in schools.EnumerateObject())
                {
                    var schoolObj = schoolProp.Value;
                    if (!schoolObj.TryGetProperty("Id", out var idEl))
                        continue;
                    if (!schoolObj.TryGetProperty("Name", out var nameEl))
                        continue;
                    var name = nameEl.GetString();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;
                    var id = idEl.GetInt32();
                    if (!map.ContainsKey(id))
                        map[id] = name;
                }
            }
            catch
            {
                return map;
            }

            return map;
        }

        private async Task<Dictionary<int, string>> LoadSidMapAsync()
        {
            var lines = await ReadAllLinesAsync(_blakstonPath!);
            var map = new Dictionary<int, string>();
            var regex = new Regex(@"^\s*(SID_[A-Za-z0-9_]+)\s*=\s*(\d+)", RegexOptions.Compiled);

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                var sidName = match.Groups[1].Value.Trim();
                if (!int.TryParse(match.Groups[2].Value, out var id))
                    continue;

                if (!map.ContainsKey(id))
                {
                    map[id] = sidName;
                }
            }

            return map;
        }

        

        private async Task<Dictionary<string, string>> LoadSpellClassMapAsync()
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(_serverRoot))
                return map;

            var systemPath = Path.Combine(_serverRoot, "kod", "util", "system.kod");
            if (!File.Exists(systemPath))
                return map;

            var lines = await ReadAllLinesAsync(systemPath);
            var regex = new Regex(@"CreateOneSpellIfNew,#num=(SID_[A-Za-z0-9_]+),#class=&([A-Za-z0-9_]+)",
                RegexOptions.Compiled);

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                var sidName = match.Groups[1].Value.Trim();
                var className = match.Groups[2].Value.Trim();
                map[sidName] = className;
            }

            return map;
        }

        private Dictionary<string, SpellResourceInfo> LoadSpellResourceMap()
        {
            var map = new Dictionary<string, SpellResourceInfo>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(_serverRoot))
                return map;

            var spellRoot = Path.Combine(_serverRoot, "kod", "object", "passive", "spell");
            if (!Directory.Exists(spellRoot))
                return map;

            foreach (var file in Directory.EnumerateFiles(spellRoot, "*.kod", SearchOption.AllDirectories))
            {
                string text;
                try
                {
                    text = ReadAllText(file);
                }
                catch
                {
                    continue;
                }

                var classMatch = Regex.Match(text, @"^\s*(\w+)\s+is\s+", RegexOptions.Multiline);
                if (!classMatch.Success)
                    continue;

                var className = classMatch.Groups[1].Value.Trim();
                if (map.ContainsKey(className))
                    continue;

                var lkodMatch = Regex.Match(text, @"include\s+([^\s]+\.lkod)", RegexOptions.IgnoreCase);
                if (!lkodMatch.Success)
                    continue;

                var vrNameMatch = Regex.Match(text, @"\bvrName\s*=\s*(\w+)", RegexOptions.IgnoreCase);
                if (!vrNameMatch.Success)
                    continue;

                var lkodFile = lkodMatch.Groups[1].Value.Trim();
                var lkodPath = Path.Combine(Path.GetDirectoryName(file) ?? string.Empty, lkodFile);
                if (!File.Exists(lkodPath))
                    continue;

                map[className] = new SpellResourceInfo(lkodPath, vrNameMatch.Groups[1].Value.Trim());
            }

            return map;
        }

        private static string? ResolveGermanName(
            string sidName,
            Dictionary<string, string> sidToClass,
            Dictionary<string, SpellResourceInfo> classResources)
        {
            if (!sidToClass.TryGetValue(sidName, out var className))
                return null;

            if (!classResources.TryGetValue(className, out var resInfo))
                return null;

            return TryLoadGermanName(resInfo.LkodPath, resInfo.ResourceKey);
        }

        private static string? TryLoadGermanName(string lkodPath, string resourceKey)
        {
            try
            {
                var lines = ReadAllLines(lkodPath);
                var regex = new Regex(
                    @"^\s*" + Regex.Escape(resourceKey) + @"\s*=\s*de\s*\""(.*)\""",
                    RegexOptions.Compiled);

                foreach (var line in lines)
                {
                    var match = regex.Match(line);
                    if (!match.Success)
                        continue;

                    return match.Groups[1].Value.Trim();
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static Encoding GetKodEncoding()
        {
            return Encoding.GetEncoding(1252);
        }

        private static async Task<string[]> ReadAllLinesAsync(string path)
        {
            return await File.ReadAllLinesAsync(path, GetKodEncoding());
        }

        private static string[] ReadAllLines(string path)
        {
            return File.ReadAllLines(path, GetKodEncoding());
        }

        private static string ReadAllText(string path)
        {
            return File.ReadAllText(path, GetKodEncoding());
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

        private static string? ResolveSpellDatabasePath()
        {
            var candidates = new[]
            {
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SpellDatabase.json"),
                Path.Combine(Environment.CurrentDirectory, "Data", "SpellDatabase.json")
            };

            foreach (var candidate in candidates)
            {
                if (File.Exists(candidate))
                    return candidate;
            }

            foreach (var start in candidates.Select(Path.GetDirectoryName))
            {
                if (string.IsNullOrWhiteSpace(start))
                    continue;

                var dir = new DirectoryInfo(start);
                for (var i = 0; i < 6 && dir != null; i++)
                {
                    var candidate = Path.Combine(dir.FullName, "SpellDatabase.json");
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
    }

    public sealed class SpellInfo
    {
        public SpellInfo(string sidName, string? germanName)
        {
            SidName = sidName;
            GermanName = germanName;
        }

        public string SidName { get; }
        public string? GermanName { get; }
    }

    public sealed class SpellSchoolInfo
    {
        public SpellSchoolInfo(string schoolName, int level)
        {
            SchoolName = schoolName;
            Level = level;
        }

        public string SchoolName { get; }
        public int Level { get; }
    }

    public sealed class SpellResourceInfo
    {
        public SpellResourceInfo(string lkodPath, string resourceKey)
        {
            LkodPath = lkodPath;
            ResourceKey = resourceKey;
        }

        public string LkodPath { get; }
        public string ResourceKey { get; }
    }
}

using M59AdminTool.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Reflection;

namespace M59AdminTool.Services
{
    public class MonstersDataService
    {
        private readonly string _monstersFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public MonstersDataService()
        {
            // Path to monsters JSON (in application Data folder)
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            _monstersFilePath = Path.Combine(appFolder, "Data", "monsters.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ObservableCollection<Monster>> LoadMonstersAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Looking for monsters at: {_monstersFilePath}");

                if (!File.Exists(_monstersFilePath))
                {
                    System.Diagnostics.Debug.WriteLine("Monsters file not found!");
                    return new ObservableCollection<Monster>();
                }

                var json = await File.ReadAllTextAsync(_monstersFilePath, System.Text.Encoding.UTF8);
                var monstersDict = JsonSerializer.Deserialize<Dictionary<string, MonsterData>>(json, _jsonOptions);

                if (monstersDict == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to deserialize monsters!");
                    return new ObservableCollection<Monster>();
                }

                var monsters = new ObservableCollection<Monster>();
                foreach (var kvp in monstersDict)
                {
                    monsters.Add(new Monster
                    {
                        ClassName = kvp.Value.ClassName ?? kvp.Key,
                        EnglishName = kvp.Value.EnglishName ?? kvp.Key,
                        GermanName = kvp.Value.GermanName,
                        DmCommand = kvp.Value.DmCommand ?? $"dm monster {kvp.Key}",
                        KodFile = kvp.Value.KodFile
                    });
                }

                System.Diagnostics.Debug.WriteLine($"Loaded {monsters.Count} monsters");
                return monsters;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading monsters: {ex.Message}");
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_MonstersLoadError"), ex.Message),
                    loc.GetString("Title_LoadError"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

                return new ObservableCollection<Monster>();
            }
        }

        public async Task SaveMonstersAsync(ObservableCollection<Monster> monsters)
        {
            try
            {
                // Convert to dictionary format for JSON
                var monstersDict = new Dictionary<string, MonsterData>();
                foreach (var monster in monsters)
                {
                    monstersDict[monster.ClassName] = new MonsterData
                    {
                        ClassName = monster.ClassName,
                        EnglishName = monster.EnglishName,
                        GermanName = monster.GermanName,
                        DmCommand = monster.DmCommand,
                        KodFile = monster.KodFile
                    };
                }

                var json = JsonSerializer.Serialize(monstersDict, _jsonOptions);
                await File.WriteAllTextAsync(_monstersFilePath, json);
            }
            catch (Exception ex)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_MonstersSaveError"), ex.Message),
                    loc.GetString("Title_SaveError"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public async Task<(bool Success, string? Error)> RefreshMonstersAsync()
        {
            try
            {
                var appFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                var workspaceRoot = FindWorkspaceRoot(appFolder);
                if (workspaceRoot == null)
                    return (false, "Workspace root not found (expected Server-104-main next to M59AdminTool).");

                var kodPath = Path.Combine(workspaceRoot, "Server-104-main", "kod", "object", "active", "holder", "nomoveon", "battler", "monster");
                if (!Directory.Exists(kodPath))
                    return (false, $"Monster KOD path not found: {kodPath}");

                var monsters = ExtractMonsters(kodPath);
                var outputDir = Path.GetDirectoryName(_monstersFilePath);
                if (!string.IsNullOrEmpty(outputDir))
                    Directory.CreateDirectory(outputDir);

                var json = JsonSerializer.Serialize(monsters, _jsonOptions);
                await File.WriteAllTextAsync(_monstersFilePath, json, Encoding.UTF8);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private static string? FindWorkspaceRoot(string startDir)
        {
            var current = new DirectoryInfo(startDir);
            while (current != null)
            {
                var serverPath = Path.Combine(current.FullName, "Server-104-main");
                var toolPath = Path.Combine(current.FullName, "M59AdminTool");
                if (Directory.Exists(serverPath) && Directory.Exists(toolPath))
                    return current.FullName;

                current = current.Parent;
            }

            return null;
        }

        private static Dictionary<string, MonsterData> ExtractMonsters(string kodPath)
        {
            var monsters = new Dictionary<string, MonsterData>(StringComparer.OrdinalIgnoreCase);
            var classRegex = new Regex(@"(?m)^\s*(\w+)\s+is\s+(\w+)\s*$", RegexOptions.Compiled);

            var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Resource",
                "Factions",
                "Towns",
                "Temples",
                "GraphicTester",
                "Monster"
            };

            var classParents = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var classFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kodFile in Directory.EnumerateFiles(kodPath, "*.kod", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(kodFile);
                foreach (Match match in classRegex.Matches(content))
                {
                    var className = match.Groups[1].Value;
                    var parentName = match.Groups[2].Value;
                    if (!classParents.ContainsKey(className))
                        classParents[className] = parentName;
                    if (!classFiles.ContainsKey(className))
                        classFiles[className] = kodFile;
                }
            }

            var isMonsterCache = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var className in classParents.Keys)
            {
                if (skip.Contains(className))
                    continue;
                if (!IsMonsterClass(className, classParents, isMonsterCache))
                    continue;

                var kodFile = classFiles[className];
                var content = File.ReadAllText(kodFile);
                var lowerClassName = className.ToLowerInvariant();
                var nameRegex = new Regex($@"{Regex.Escape(lowerClassName)}_name_rsc\s*=\s*""([^""]+)""",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled);
                var nameMatch = nameRegex.Match(content);
                var englishName = nameMatch.Success ? nameMatch.Groups[1].Value : className;

                string? germanName = null;
                var lkodFile = Path.ChangeExtension(kodFile, ".lkod");
                if (File.Exists(lkodFile))
                {
                    var lkodContent = File.ReadAllText(lkodFile, Encoding.Default);
                    var germanRegex = new Regex($@"{Regex.Escape(lowerClassName)}_name_rsc\s*=\s*de\s*""([^""]+)""",
                        RegexOptions.IgnoreCase | RegexOptions.Compiled);
                    var germanMatch = germanRegex.Match(lkodContent);
                    if (germanMatch.Success)
                        germanName = germanMatch.Groups[1].Value;
                }

                monsters[className] = new MonsterData
                {
                    ClassName = className,
                    EnglishName = englishName,
                    GermanName = germanName,
                    DmCommand = $"dm monster {className.ToLowerInvariant()}",
                    KodFile = Path.GetFileName(kodFile)
                };
            }

            return monsters;
        }

        private static bool IsMonsterClass(
            string className,
            Dictionary<string, string> parents,
            Dictionary<string, bool> cache)
        {
            if (cache.TryGetValue(className, out var cached))
                return cached;

            if (!parents.TryGetValue(className, out var parent))
            {
                cache[className] = false;
                return false;
            }

            if (parent.Equals("Monster", StringComparison.OrdinalIgnoreCase))
            {
                cache[className] = true;
                return true;
            }

            var result = IsMonsterClass(parent, parents, cache);
            cache[className] = result;
            return result;
        }

        // Helper class for deserializing monster data from JSON
        private class MonsterData
        {
            public string? ClassName { get; set; }
            public string? EnglishName { get; set; }
            public string? GermanName { get; set; }
            public string? DmCommand { get; set; }
            public string? KodFile { get; set; }
        }
    }
}

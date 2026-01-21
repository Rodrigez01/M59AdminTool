using M59AdminTool.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Diagnostics;
using System.Reflection;

namespace M59AdminTool.Services
{
    public class WarpsDataService
    {
        private readonly string _dataFilePath;
        private readonly string _germanNamesFilePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private Dictionary<string, RoomTranslation>? _germanNames;

        public WarpsDataService()
        {
            // Store warps in user's AppData folder
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "M59AdminTool");

            Directory.CreateDirectory(appDataPath);
            _dataFilePath = Path.Combine(appDataPath, "warps.json");

            // Path to German names JSON (in application Data folder)
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            _germanNamesFilePath = Path.Combine(appFolder, "Data", "room_names_german.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

            LoadGermanNames();
        }

        private void LoadGermanNames()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Looking for German names at: {_germanNamesFilePath}");
                if (File.Exists(_germanNamesFilePath))
                {
                    var json = File.ReadAllText(_germanNamesFilePath, System.Text.Encoding.UTF8);
                    _germanNames = JsonSerializer.Deserialize<Dictionary<string, RoomTranslation>>(json, _jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"Loaded {_germanNames?.Count ?? 0} German room translations");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("German names file not found!");
                    _germanNames = null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading German names: {ex.Message}");
                _germanNames = null;
            }
        }

        private class RoomTranslation
        {
            public string? EnglishName { get; set; }
            public string? GermanName { get; set; }
            public string? KodFile { get; set; }
        }

        public async Task<ObservableCollection<WarpCategory>> LoadWarpsAsync()
        {
            try
            {
                if (!File.Exists(_dataFilePath))
                {
                    return GetDefaultWarps();
                }

                var json = await File.ReadAllTextAsync(_dataFilePath);
                var categories = JsonSerializer.Deserialize<List<WarpCategory>>(json, _jsonOptions);

                if (categories == null || categories.Count == 0)
                {
                    return GetDefaultWarps();
                }

                // Add German names to all warps
                EnrichWithGermanNames(categories);

                return new ObservableCollection<WarpCategory>(categories);
            }
            catch (Exception ex)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_WarpsLoadError"), ex.Message),
                    loc.GetString("Title_LoadError"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

                return GetDefaultWarps();
            }
        }

        public async Task<(bool Success, string? Error)> RefreshExtractedRoomsAsync()
        {
            try
            {
                var appFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
                var settings = SettingsService.Load();
                var serverRoot = settings.ServerRootPath;
                if (string.IsNullOrWhiteSpace(serverRoot))
                {
                    var workspaceRoot = FindWorkspaceRoot(appFolder);
                    if (workspaceRoot == null)
                        return (false, "Server root not configured. Use File -> Konfiguration.");
                    serverRoot = Path.Combine(workspaceRoot, "Server-104-main");
                }

                var kodRoot = string.IsNullOrWhiteSpace(settings.KodPath)
                    ? Path.Combine(serverRoot, "kod")
                    : settings.KodPath;

                var toolRoot = Path.GetFullPath(Path.Combine(appFolder, "..", "..", "..", ".."));
                var systemKodPath = Path.Combine(kodRoot, "util", "system.kod");
                var khdPath = Path.Combine(kodRoot, "include", "blakston.khd");
                var kodRoomPath = Path.Combine(kodRoot, "object", "active", "holder", "room");
                var outputPath = Path.Combine(toolRoot, "extracted_rooms.json");
                var germanNamesPath = _germanNamesFilePath;

                if (!File.Exists(systemKodPath))
                    return (false, $"system.kod not found: {systemKodPath}");
                if (!File.Exists(khdPath))
                    return (false, $"blakston.khd not found: {khdPath}");

                var translations = LoadRoomTranslations(kodRoomPath);
                WriteRoomTranslationsJson(germanNamesPath, translations);
                LoadGermanNames();

                var ridById = BuildRidMap(khdPath);
                var extracted = BuildExtractedRoomsFromSystemKod(systemKodPath, ridById, translations);
                var jsonOptions = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                var json = JsonSerializer.Serialize(extracted, jsonOptions);
                await File.WriteAllTextAsync(outputPath, json, Encoding.UTF8);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public ObservableCollection<WarpCategory> LoadExtractedWarps()
        {
            return GetDefaultWarps();
        }

        private static Dictionary<string, RoomTranslation> LoadRoomTranslations(string kodRoomPath)
        {
            var translations = new Dictionary<string, RoomTranslation>(StringComparer.OrdinalIgnoreCase);
            if (!Directory.Exists(kodRoomPath))
                return translations;

            var ridRegex = new Regex(@"piRoom_num\s*=\s*(RID_[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var nameRegex = new Regex(@"name_\w+\s*=\s*""([^""]+)""", RegexOptions.Compiled);
            var nameDeRegex = new Regex(@"name_\w+\s*=\s*de\s*""([^""]+)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            foreach (var kodFile in Directory.EnumerateFiles(kodRoomPath, "*.kod", SearchOption.AllDirectories))
            {
                var kodContent = File.ReadAllText(kodFile);
                var ridMatch = ridRegex.Match(kodContent);
                if (!ridMatch.Success)
                    continue;

                var rid = ridMatch.Groups[1].Value;
                var nameMatch = nameRegex.Match(kodContent);
                var englishName = nameMatch.Success ? nameMatch.Groups[1].Value : null;

                var lkodFile = Path.ChangeExtension(kodFile, ".lkod");
                string? germanName = null;
                if (File.Exists(lkodFile))
                {
                    var lkodContent = File.ReadAllText(lkodFile, Encoding.Default);
                    var germanMatch = nameDeRegex.Match(lkodContent);
                    if (germanMatch.Success)
                        germanName = germanMatch.Groups[1].Value;
                }

                translations[rid] = new RoomTranslation
                {
                    EnglishName = englishName,
                    GermanName = germanName,
                    KodFile = Path.GetFileName(kodFile)
                };
            }

            return translations;
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

        private static void WriteRoomTranslationsJson(string outputPath, Dictionary<string, RoomTranslation> translations)
        {
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir))
                Directory.CreateDirectory(outputDir);

            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(translations, jsonOptions);
            File.WriteAllText(outputPath, json, Encoding.UTF8);
        }

        private static ExtractedRoomsData BuildExtractedRoomsFromSystemKod(
            string systemKodPath,
            Dictionary<int, List<string>> ridById,
            Dictionary<string, RoomTranslation> translations)
        {
            var extracted = new ExtractedRoomsData { Categories = new List<ExtractedCategory>() };
            var categoryByName = new Dictionary<string, ExtractedCategory>(StringComparer.OrdinalIgnoreCase);
            var seenRids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var ridToId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var entry in ridById)
            {
                foreach (var rid in entry.Value)
                {
                    if (!ridToId.ContainsKey(rid))
                        ridToId[rid] = entry.Key;
                }
            }

            var ridRegex = new Regex(@"#num\s*=\s*(RID_[A-Za-z0-9_]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var inFunction = false;
            var inBody = false;
            var braceDepth = 0;
            ExtractedCategory? currentCategory = null;

            foreach (var rawLine in File.ReadLines(systemKodPath))
            {
                var line = rawLine;
                var trimmed = line.Trim();

                if (!inFunction)
                {
                    if (trimmed.StartsWith("CreateAllRoomsIfNew(", StringComparison.Ordinal))
                        inFunction = true;
                    continue;
                }

                if (!inBody)
                {
                    if (trimmed.Contains("{"))
                    {
                        braceDepth += CountChar(trimmed, '{');
                        braceDepth -= CountChar(trimmed, '}');
                        if (braceDepth > 0)
                            inBody = true;
                    }
                    continue;
                }

                if (trimmed.StartsWith("//"))
                {
                    var name = trimmed.TrimStart('/').Trim();
                    if (!string.IsNullOrWhiteSpace(name))
                        currentCategory = GetOrCreateCategory(name, extracted, categoryByName);
                }

                var match = ridRegex.Match(line);
                if (match.Success)
                {
                    var rid = match.Groups[1].Value;
                    if (seenRids.Add(rid))
                    {
                        if (currentCategory == null)
                            currentCategory = GetOrCreateCategory("Misc", extracted, categoryByName);

                        var englishName = rid;
                        string? germanName = null;
                        string? kodFile = null;
                        if (translations.TryGetValue(rid, out var translation))
                        {
                            englishName = translation.EnglishName ?? englishName;
                            germanName = translation.GermanName;
                            kodFile = translation.KodFile;
                        }

                        ridToId.TryGetValue(rid, out var idValue);
                        ridById.TryGetValue(idValue, out var ridList);

                        currentCategory.Rooms ??= new List<ExtractedRoom>();
                        currentCategory.Rooms.Add(new ExtractedRoom
                        {
                            Name = englishName,
                            NameDe = string.IsNullOrWhiteSpace(germanName) ? null : germanName,
                            Rid = rid,
                            Id = idValue,
                            File = kodFile,
                            Aliases = ridList != null && ridList.Count > 1
                                ? ridList.Where(r => !string.Equals(r, rid, StringComparison.OrdinalIgnoreCase)).ToList()
                                : new List<string>()
                        });
                    }
                }

                braceDepth += CountChar(trimmed, '{');
                braceDepth -= CountChar(trimmed, '}');
                if (braceDepth <= 0)
                    break;
            }

            return extracted;
        }

        private static ExtractedCategory GetOrCreateCategory(
            string name,
            ExtractedRoomsData extracted,
            Dictionary<string, ExtractedCategory> categoryByName)
        {
            if (categoryByName.TryGetValue(name, out var existing))
                return existing;

            var category = new ExtractedCategory
            {
                Name = name,
                Rooms = new List<ExtractedRoom>()
            };
            extracted.Categories.Add(category);
            categoryByName[name] = category;
            return category;
        }

        private static int CountChar(string value, char target)
        {
            var count = 0;
            foreach (var c in value)
            {
                if (c == target)
                    count++;
            }
            return count;
        }

        private static Dictionary<int, List<string>> BuildRidMap(string khdPath)
        {
            var ridById = new Dictionary<int, List<string>>();
            var ridRegex = new Regex(@"^\s*(RID_[A-Za-z0-9_]+)\s*=\s*(-?\d+)\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach (var line in File.ReadLines(khdPath))
            {
                var match = ridRegex.Match(line);
                if (!match.Success)
                    continue;

                var ridName = match.Groups[1].Value;
                if (ridName == "RID_DEFAULT")
                    continue;

                if (!int.TryParse(match.Groups[2].Value, out var ridId))
                    continue;
                if (ridId < 0)
                    continue;

                if (!ridById.TryGetValue(ridId, out var list))
                {
                    list = new List<string>();
                    ridById[ridId] = list;
                }
                if (!list.Contains(ridName))
                    list.Add(ridName);
            }

            return ridById;
        }

        private void EnrichWithGermanNames(List<WarpCategory> categories)
        {
            if (_germanNames == null)
            {
                System.Diagnostics.Debug.WriteLine("Cannot enrich warps - German names dictionary is null");
                return;
            }

            int enrichedCount = 0;
            foreach (var category in categories)
            {
                foreach (var warp in category.Locations)
                {
                    if (!string.IsNullOrEmpty(warp.RoomId) && _germanNames.TryGetValue(warp.RoomId, out var translation))
                    {
                        warp.NameDe = translation.GermanName;
                        enrichedCount++;
                    }
                }
            }
            System.Diagnostics.Debug.WriteLine($"Total warps enriched with German names: {enrichedCount}");
        }

        public async Task SaveWarpsAsync(ObservableCollection<WarpCategory> categories)
        {
            try
            {
                var json = JsonSerializer.Serialize(categories.ToList(), _jsonOptions);
                await File.WriteAllTextAsync(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_WarpsSaveError"), ex.Message),
                    loc.GetString("Title_SaveError"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public async Task ExportWarpsAsync(string filePath, ObservableCollection<WarpCategory> categories)
        {
            try
            {
                var json = JsonSerializer.Serialize(categories.ToList(), _jsonOptions);
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_WarpsExportError"), ex.Message),
                    loc.GetString("Title_ExportError"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public async Task<ObservableCollection<WarpCategory>> ImportWarpsAsync(string filePath)
        {
            try
            {
                var json = await File.ReadAllTextAsync(filePath);
                var categories = JsonSerializer.Deserialize<List<WarpCategory>>(json, _jsonOptions);

                if (categories == null || categories.Count == 0)
                {
                    var loc = LocalizationService.Instance;
                    System.Windows.MessageBox.Show(
                        loc.GetString("Message_WarpsImportInvalid"),
                        loc.GetString("Title_ImportError"),
                        System.Windows.MessageBoxButton.OK,
                        System.Windows.MessageBoxImage.Warning);

                    return new ObservableCollection<WarpCategory>();
                }

                return new ObservableCollection<WarpCategory>(categories);
            }
            catch (Exception ex)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_WarpsImportError"), ex.Message),
                    loc.GetString("Title_ImportError"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);

                return new ObservableCollection<WarpCategory>();
            }
        }

        private ObservableCollection<WarpCategory> GetDefaultWarps()
        {
            var categories = new ObservableCollection<WarpCategory>();
            var loc = LocalizationService.Instance;

            try
            {
                // Load extracted room data from JSON file
                var extractedPath = Path.Combine(
                    Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? "",
                    "..", "..", "..", "..", "extracted_rooms.json");

                if (File.Exists(extractedPath))
                {
                    var json = File.ReadAllText(extractedPath);
                    var extractedData = JsonSerializer.Deserialize<ExtractedRoomsData>(json, _jsonOptions);

                    if (extractedData?.Categories != null)
                    {
                        foreach (var cat in extractedData.Categories)
                        {
                            var warpCategory = new WarpCategory
                            {
                                Name = cat.Name,
                                IsExpanded = false
                            };

                            foreach (var room in cat.Rooms ?? new List<ExtractedRoom>())
                            {
                                warpCategory.Locations.Add(new WarpLocation
                                {
                                    Name = room.Name ?? loc.GetString("Default_Unknown"),
                                    NameDe = room.NameDe,
                                    Category = cat.Name,
                                    RoomId = room.Rid,
                                    Description = loc.GetString("Default_ExtractedDescription")
                                });
                            }

                            categories.Add(warpCategory);
                        }

                        // Enrich with German names
                        EnrichWithGermanNames(categories.ToList());

                        return categories;
                    }
                }
            }
            catch (Exception ex)
            {
                var locError = LocalizationService.Instance;
                System.Windows.MessageBox.Show(
                    string.Format(locError.GetString("Message_ExtractedRoomsLoadWarning"), ex.Message),
                    locError.GetString("Title_Warning"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }

            // Fallback to minimal default data if extraction file not found
            var specialRoomName = loc.GetString("Default_SpecialRoomCategory");
            var specialRoom = new WarpCategory { Name = specialRoomName, IsExpanded = true };
            specialRoom.Locations.Add(new WarpLocation
            {
                Name = loc.GetString("Default_SchoolRoomName"),
                Category = specialRoomName,
                RoomId = "RID_SCHOOL",
                X = 50,
                Y = 50,
                Description = loc.GetString("Default_SchoolRoomDescription")
            });
            specialRoom.Locations.Add(new WarpLocation
            {
                Name = loc.GetString("Default_GodsMeetingName"),
                Category = specialRoomName,
                RoomId = "RID_GODS",
                X = 100,
                Y = 100,
                Description = loc.GetString("Default_GodsMeetingDescription")
            });

            categories.Add(specialRoom);

            return categories;
        }

        // Helper classes for deserializing extracted room data
        private class ExtractedRoomsData
        {
            public List<ExtractedCategory>? Categories { get; set; }
        }

        private class ExtractedCategory
        {
            public string Name { get; set; } = string.Empty;
            public List<ExtractedRoom>? Rooms { get; set; }
        }

        private class ExtractedRoom
        {
            public string? Name { get; set; }
            public string? NameDe { get; set; }
            public string? Rid { get; set; }
            public int? Id { get; set; }
            public string? File { get; set; }
            public List<string>? Aliases { get; set; }
        }
    }
}

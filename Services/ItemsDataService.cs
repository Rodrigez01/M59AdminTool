using M59AdminTool.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Reflection;

namespace M59AdminTool.Services
{
    public class ItemsDataService
    {
        private readonly string _itemsFilePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public ItemsDataService()
        {
            // Path to items JSON (in application Data folder)
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            _itemsFilePath = Path.Combine(appFolder, "Data", "items.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ObservableCollection<Item>> LoadItemsAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Looking for items at: {_itemsFilePath}");

                if (!File.Exists(_itemsFilePath))
                {
                    System.Diagnostics.Debug.WriteLine("Items file not found!");
                    return new ObservableCollection<Item>();
                }

                var json = await File.ReadAllTextAsync(_itemsFilePath, System.Text.Encoding.UTF8);
                var itemsDict = JsonSerializer.Deserialize<Dictionary<string, ItemData>>(json, _jsonOptions);

                if (itemsDict == null)
                {
                    System.Diagnostics.Debug.WriteLine("Failed to deserialize items!");
                    return new ObservableCollection<Item>();
                }

                var items = new ObservableCollection<Item>();
                foreach (var kvp in itemsDict)
                {
                    items.Add(new Item
                    {
                        ClassName = kvp.Value.ClassName ?? kvp.Key,
                        EnglishName = kvp.Value.EnglishName ?? kvp.Key,
                        GermanName = kvp.Value.GermanName,
                        DmCommand = kvp.Value.DmCommand ?? $"dm item {kvp.Key}",
                        KodFile = kvp.Value.KodFile,
                        Category = kvp.Value.Category
                    });
                }

                System.Diagnostics.Debug.WriteLine($"Loaded {items.Count} items");
                return items;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading items: {ex.Message}");
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_ItemsLoadError"), ex.Message),
                    loc.GetString("Title_LoadError"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);

                return new ObservableCollection<Item>();
            }
        }

        public async Task SaveItemsAsync(ObservableCollection<Item> items)
        {
            try
            {
                // Convert to dictionary format for JSON
                var itemsDict = new Dictionary<string, ItemData>();
                foreach (var item in items)
                {
                    itemsDict[item.ClassName] = new ItemData
                    {
                        ClassName = item.ClassName,
                        EnglishName = item.EnglishName,
                        GermanName = item.GermanName,
                        DmCommand = item.DmCommand,
                        KodFile = item.KodFile,
                        Category = item.Category
                    };
                }

                var json = JsonSerializer.Serialize(itemsDict, _jsonOptions);
                await File.WriteAllTextAsync(_itemsFilePath, json);
            }
            catch (Exception ex)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_ItemsSaveError"), ex.Message),
                    loc.GetString("Title_SaveError"),
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }

        public async Task<(bool Success, string? Error)> RefreshItemsAsync()
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
                var kodPath = Path.Combine(kodRoot, "object", "item");
                if (!Directory.Exists(kodPath))
                    return (false, $"Item KOD path not found: {kodPath}");

                var existing = await LoadExistingItemsAsync();
                var extracted = ExtractItems(kodPath, existing);
                var outputDir = Path.GetDirectoryName(_itemsFilePath);
                if (!string.IsNullOrEmpty(outputDir))
                    Directory.CreateDirectory(outputDir);

                var json = JsonSerializer.Serialize(extracted, _jsonOptions);
                await File.WriteAllTextAsync(_itemsFilePath, json, Encoding.UTF8);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        private async Task<Dictionary<string, ItemData>> LoadExistingItemsAsync()
        {
            try
            {
                if (!File.Exists(_itemsFilePath))
                    return new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);

                var json = await File.ReadAllTextAsync(_itemsFilePath, Encoding.UTF8);
                var items = JsonSerializer.Deserialize<Dictionary<string, ItemData>>(json, _jsonOptions);
                return items ?? new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                return new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
            }
        }

        private static Dictionary<string, ItemData> ExtractItems(string kodPath, Dictionary<string, ItemData> existing)
        {
            var items = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
            var classRegex = new Regex(@"(?m)^\s*(\w+)\s+is\s+(\w+)", RegexOptions.Compiled);

            var skip = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Resource",
                "GraphicTester",
                "TestItem",
                "ActiveItem",
                "PassiveItem"
            };

            foreach (var kodFile in Directory.EnumerateFiles(kodPath, "*.kod", SearchOption.AllDirectories))
            {
                var content = File.ReadAllText(kodFile);
                var classMatch = classRegex.Match(content);
                if (!classMatch.Success)
                    continue;

                var className = classMatch.Groups[1].Value;
                if (skip.Contains(className))
                    continue;

                var lowerClassName = className.ToLowerInvariant();
                var englishName = className;
                var nameRegex = new Regex($@"{Regex.Escape(lowerClassName)}_name_rsc\s*=\s*""([^""]+)""",
                    RegexOptions.IgnoreCase | RegexOptions.Compiled);
                var nameMatch = nameRegex.Match(content);
                if (nameMatch.Success)
                    englishName = nameMatch.Groups[1].Value;

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

                existing.TryGetValue(className, out var existingItem);
                var category = existingItem?.Category;
                if (string.IsNullOrWhiteSpace(germanName))
                    germanName = existingItem?.GermanName;

                items[className] = new ItemData
                {
                    ClassName = className,
                    EnglishName = englishName,
                    GermanName = germanName,
                    DmCommand = $"dm item {englishName}",
                    KodFile = Path.GetFileName(kodFile),
                    Category = category
                };
            }

            return items;
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

        // Helper class for deserializing item data from JSON
        private class ItemData
        {
            public string? ClassName { get; set; }
            public string? EnglishName { get; set; }
            public string? GermanName { get; set; }
            public string? DmCommand { get; set; }
            public string? KodFile { get; set; }
            public string? Category { get; set; }
        }
    }
}

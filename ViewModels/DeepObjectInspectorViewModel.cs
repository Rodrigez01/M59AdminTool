using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M59AdminTool.Models;
using M59AdminTool.Services;
using M59AdminTool.Views;
using Application = System.Windows.Application;

namespace M59AdminTool.ViewModels
{
    public partial class DeepObjectInspectorViewModel : ObservableObject
    {
        private readonly ConnectionViewModel? _connectionViewModel;
        private M59ServerConnection? _lastConnection;
        private readonly Stack<string> _navigationHistory = new();
        private bool _isListInspection;
        private CancellationTokenSource? _resolveCts;
        private readonly ItemsDataService _itemsDataService = new();
        private Dictionary<string, Item> _itemsByClass = new(StringComparer.OrdinalIgnoreCase);
        private bool _itemsLoaded;
        private readonly SpellDataService _spellDataService = new();
        private Dictionary<int, SpellInfo> _spellsById = new();
        private bool _spellsLoaded;
        private readonly ItemAttributeDataService _itemAttributeDataService = new();
        private Dictionary<int, string> _itemAttributesById = new();
        private bool _itemAttributesLoaded;
        private readonly SkillDataService _skillDataService = new();
        private Dictionary<int, SkillInfo> _skillsById = new();
        private Dictionary<int, SkillSchoolInfo> _skillSchoolsById = new();
        private bool _skillsLoaded;
        private string? _currentListPropertyName;
        private HashSet<int> _itemSpellIds = new();
        private Dictionary<int, string> _attackSpellTypesByValue = new();
        private bool _itemSpellsLoaded;
        private bool _attackSpellTypesLoaded;
        private Dictionary<string, SpellSchoolInfo> _spellSchoolsBySid = new(StringComparer.OrdinalIgnoreCase);
        private Dictionary<int, string> _spellSchoolsById = new();
        private bool _spellSchoolsLoaded;
        private string? _currentListOwnerObjectId;
        private List<ObjectProperty> _currentListProperties = new();

        private const string ObjectTokenPrefix = "obj:";
        private const string ListTokenPrefix = "list:";

        [ObservableProperty]
        private string _objectId = string.Empty;

        [ObservableProperty]
        private ObjectInspectionResult? _currentInspection = new();

        [ObservableProperty]
        private string _statusMessage = "Ready";

        public bool CanNavigateBack => _navigationHistory.Count > 0;

        public ObservableCollection<string> NavigationHistoryDisplay => new(
            _navigationHistory.Reverse().Select(FormatHistoryEntry)
        );

        public DeepObjectInspectorViewModel(ConnectionViewModel? connectionViewModel = null)
        {
            _connectionViewModel = connectionViewModel;
        }

        [RelayCommand]
        private async Task LoadObject()
        {
            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                StatusMessage = "❌ Nicht verbunden. Bitte im Connection-Tab einloggen.";
                return;
            }

            if (string.IsNullOrWhiteSpace(ObjectId))
            {
                StatusMessage = "❌ Bitte Object ID eingeben.";
                return;
            }

            var input = ObjectId.Trim();
            if (TryParseListInput(input, out var listId))
            {
                _currentListPropertyName = null;
                await LoadListInternal(listId);
                return;
            }

            await LoadObjectInternal(input);
        }

        [RelayCommand]
        private async Task NavigateToObject(string? targetObjectId)
        {
            if (string.IsNullOrWhiteSpace(targetObjectId))
                return;

            // Push current object to history before navigating
            if (CurrentInspection != null && !string.IsNullOrWhiteSpace(CurrentInspection.ObjectId))
            {
                var token = BuildHistoryToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _navigationHistory.Push(token);
                }
            }

            ObjectId = targetObjectId;
            await LoadObjectInternal(targetObjectId);

            // Notify UI of history changes
            OnPropertyChanged(nameof(CanNavigateBack));
            OnPropertyChanged(nameof(NavigationHistoryDisplay));
        }

        [RelayCommand]
        private async Task NavigateToList(string? listId)
        {
            if (string.IsNullOrWhiteSpace(listId))
                return;

            _currentListPropertyName = GetListPropertyName(listId);
            _currentListOwnerObjectId = CurrentInspection?.ObjectId;

            if (CurrentInspection != null && !string.IsNullOrWhiteSpace(CurrentInspection.ObjectId))
            {
                var token = BuildHistoryToken();
                if (!string.IsNullOrWhiteSpace(token))
                {
                    _navigationHistory.Push(token);
                }
            }

            ObjectId = $"list {listId}";
            await LoadListInternal(listId);

            OnPropertyChanged(nameof(CanNavigateBack));
            OnPropertyChanged(nameof(NavigationHistoryDisplay));
        }

        [RelayCommand]
        private async Task NavigateToNamedList(string? propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return;

            string listId;
            _currentListPropertyName = propertyName;
            _currentListOwnerObjectId = CurrentInspection?.ObjectId;
            if (propertyName.Equals("inventory", StringComparison.OrdinalIgnoreCase))
            {
                listId = GetFirstAvailableListId(
                    "plInventory",
                    "plItems",
                    "plActive",
                    "plPassive"
                );
            }
            else
            {
                listId = GetListIdFromProperty(propertyName);
            }
            if (string.IsNullOrWhiteSpace(listId))
            {
                StatusMessage = $"❌ Liste '{propertyName}' nicht gefunden oder leer.";
                return;
            }

            ObjectId = $"list {listId}";
            await NavigateToList(listId);
        }

        [RelayCommand]
        private async Task NavigateToHistoryEntry(string? entry)
        {
            if (string.IsNullOrWhiteSpace(entry))
                return;

            var trimmed = entry.Trim();
            if (trimmed.StartsWith("List ", StringComparison.OrdinalIgnoreCase))
            {
                _currentListPropertyName = null;
                _currentListOwnerObjectId = null;
                var listId = trimmed.Substring(5).Trim();
                if (!string.IsNullOrWhiteSpace(listId))
                {
                    ObjectId = $"list {listId}";
                    await LoadListInternal(listId);
                }
                return;
            }

            if (trimmed.StartsWith("Object ", StringComparison.OrdinalIgnoreCase))
            {
                var objectId = trimmed.Substring(7).Trim();
                if (!string.IsNullOrWhiteSpace(objectId))
                {
                    ObjectId = objectId;
                    await LoadObjectInternal(objectId);
                }
                return;
            }

            if (TryParseHistoryToken(trimmed, out var isList, out var id))
            {
                ObjectId = isList ? $"list {id}" : id;
                if (isList)
                {
                    _currentListPropertyName = null;
                    _currentListOwnerObjectId = null;
                    await LoadListInternal(id);
                }
                else
                {
                    await LoadObjectInternal(id);
                }
                return;
            }

            if (TryParseListInput(trimmed, out var fallbackListId))
            {
                _currentListPropertyName = null;
                _currentListOwnerObjectId = null;
                ObjectId = $"list {fallbackListId}";
                await LoadListInternal(fallbackListId);
                return;
            }

            ObjectId = trimmed;
            await LoadObjectInternal(trimmed);
        }

        [RelayCommand(CanExecute = nameof(CanNavigateBack))]
        private async Task NavigateBack()
        {
            if (_navigationHistory.Count == 0)
                return;

            var previousToken = _navigationHistory.Pop();
            if (TryParseHistoryToken(previousToken, out var isList, out var id))
            {
                ObjectId = isList ? $"list {id}" : id;
                if (isList)
                {
                    await LoadListInternal(id);
                }
                else
                {
                    await LoadObjectInternal(id);
                }
            }

            OnPropertyChanged(nameof(CanNavigateBack));
            OnPropertyChanged(nameof(NavigationHistoryDisplay));
        }

        [RelayCommand]
        private async Task EditProperty(ObjectProperty? property)
        {
            if (property == null || CurrentInspection == null)
            {
                StatusMessage = "❌ Bitte Property auswählen.";
                return;
            }

            if (_isListInspection && IsItemAttributeListEntry(property))
            {
                await EditItemAttributeListEntryAsync(property);
                return;
            }

            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                StatusMessage = "❌ Nicht verbunden.";
                return;
            }

            var currentType = property.Type;
            var currentValue = property.Value;
            string? finalVal = null;

            if (IsSpellProperty(property))
            {
                await EnsureSpellsLoadedAsync();
                if (int.TryParse(currentValue, out var spellId) && _spellsById.ContainsKey(spellId))
                {
                    var spellItems = _spellsById
                        .Select(kvp => new SpellPickerItem(kvp.Key, kvp.Value.SidName, kvp.Value.GermanName))
                        .OrderBy(i => i.Id)
                        .ToList();

                    var selected = SpellSelectWindow.ShowDialog(spellItems, spellId);
                    if (selected.HasValue)
                    {
                        finalVal = $"INT {selected.Value}";
                    }
                }
            }
            else if (IsAttackSpellTypeProperty(property))
            {
                await EnsureAttackSpellTypesLoadedAsync();
                if (int.TryParse(currentValue, out var attackValue) &&
                    _attackSpellTypesByValue.ContainsKey(attackValue))
                {
                    var attackItems = _attackSpellTypesByValue
                        .OrderBy(kvp => kvp.Key)
                        .Select(kvp => new SpellPickerItem(kvp.Key, kvp.Value, null))
                        .ToList();

                    var selected = SpellSelectWindow.ShowDialog(attackItems, attackValue);
                    if (selected.HasValue)
                    {
                        finalVal = $"INT {selected.Value}";
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(finalVal))
            {
                var newVal = PromptForInput(
                    $"Property: {property.Name}\n" +
                    $"Current: {property.DisplayValue}\n\n" +
                    $"Enter new value (type optional, defaults to {currentType}):",
                    "Edit Property"
                );

                if (string.IsNullOrWhiteSpace(newVal))
                    return;

                // Type preservation: If user only enters value, prepend type
                finalVal = newVal.Trim();
                if (!finalVal.Contains(" ", StringComparison.Ordinal))
                {
                    finalVal = $"{currentType} {finalVal}";
                }
            }

            StatusMessage = $"Setting {property.Name} = {finalVal}...";

            try
            {
                await conn.SendAdminCommandAsync($"set o {CurrentInspection.ObjectId} {property.Name} {finalVal}");
                await Task.Delay(500);
                await RefreshCurrentObject();
                StatusMessage = $"✅ Property {property.Name} updated.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task RefreshCurrentObject()
        {
            if (CurrentInspection != null && !string.IsNullOrWhiteSpace(CurrentInspection.ObjectId))
            {
                if (_isListInspection)
                {
                    ObjectId = $"list {CurrentInspection.ObjectId}";
                    await LoadListInternal(CurrentInspection.ObjectId);
                }
                else
                {
                    ObjectId = CurrentInspection.ObjectId;
                    await LoadObjectInternal(CurrentInspection.ObjectId);
                }
            }
        }

        // Helper Methods
        private async Task LoadObjectInternal(string objectId)
        {
            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                StatusMessage = "❌ Nicht verbunden. Bitte im Connection-Tab einloggen.";
                return;
            }

            CancelResolve();
            EnsureSubscription(conn);
            StatusMessage = $"Lade Object {objectId}...";

            var responses = await SendAndCollectAsync($"show object {objectId}", 1500);

            if (responses.Count == 0)
            {
                StatusMessage = "❌ Keine Antwort vom Server erhalten.";
                return;
            }

            var result = ParseShowObjectResponse(responses);

            if (string.IsNullOrEmpty(result.ObjectId) || string.IsNullOrEmpty(result.ClassName))
            {
                StatusMessage = $"❌ Parse-Fehler für Object {objectId}.";
                return;
            }

            _isListInspection = false;
            RunOnUi(() =>
            {
                CurrentInspection = result;
                StatusMessage = $"✅ Object {result.ObjectId} geladen ({result.PropertyGroups.Sum(g => g.Properties.Count)} Properties).";
            });

            await ResolveSpellPropertiesAsync(result);
            await ResolveAttackTypePropertiesAsync(result);
        }

        private async Task LoadListInternal(string listId)
        {
            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                StatusMessage = "❌ Nicht verbunden. Bitte im Connection-Tab einloggen.";
                return;
            }

            CancelResolve();
            EnsureSubscription(conn);
            StatusMessage = $"Lade List {listId}...";

            var responses = await SendAndCollectAsync($"show list {listId}", 1500);

            if (responses.Count == 0)
            {
                StatusMessage = "❌ Keine Antwort vom Server erhalten.";
                return;
            }

            var isItemAttributesList = string.Equals(_currentListPropertyName, "plItem_attributes", StringComparison.OrdinalIgnoreCase);
            var result = ParseShowListResponse(responses, listId, isItemAttributesList);
            if (string.IsNullOrEmpty(result.ObjectId))
            {
                StatusMessage = $"❌ Parse-Fehler für List {listId}.";
                return;
            }

            _isListInspection = true;
            RunOnUi(() =>
            {
                CurrentInspection = result;
                StatusMessage = $"✅ List {result.ObjectId} geladen ({result.PropertyGroups.Sum(g => g.Properties.Count)} Einträge).";
            });

            var listProps = result.PropertyGroups.SelectMany(g => g.Properties).ToList();
            _currentListProperties = listProps;
            var cts = new CancellationTokenSource();
            _resolveCts = cts;
            _ = ResolveListEntriesAsync(listProps, cts.Token);
        }

        private async Task<List<string>> SendAndCollectAsync(string command, int waitMs = 1500)
        {
            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                StatusMessage = "❌ Not connected.";
                return new List<string>();
            }

            EnsureSubscription(conn);
            var collected = new List<string>();

            void Handler(object? s, string resp)
            {
                lock (collected)
                {
                    collected.Add(resp);
                }
            }

            conn.ResponseReceived += Handler;

            try
            {
                await conn.SendAdminCommandAsync(command);
                await Task.Delay(waitMs);
            }
            catch (Exception ex)
            {
                StatusMessage = $"❌ Error sending command: {ex.Message}";
            }
            finally
            {
                conn.ResponseReceived -= Handler;
            }

            return collected;
        }

        private ObjectInspectionResult ParseShowObjectResponse(List<string> responses)
        {
            var result = new ObjectInspectionResult();
            var properties = new List<ObjectProperty>();

            // Regex patterns
            var headerRegex = new Regex(
                @":< OBJECT (?<id>\d+) is CLASS (?<classname>.*)",
                RegexOptions.Compiled
            );
            var propertyRegex = new Regex(
                @":\s+(?<name>\w+)\s*=\s+(?<type>[\w$]+)\s+(?<value>.*)",
                RegexOptions.Compiled
            );

            foreach (var line in responses)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith(">", StringComparison.Ordinal)) continue; // Skip echo

                // Try header parsing
                var headerMatch = headerRegex.Match(line);
                if (headerMatch.Success)
                {
                    result.ObjectId = headerMatch.Groups["id"].Value;
                    result.ClassName = headerMatch.Groups["classname"].Value.Trim();
                    continue;
                }

                // Try property parsing with regex
                var propMatch = propertyRegex.Match(line);
                if (propMatch.Success)
                {
                    properties.Add(new ObjectProperty
                    {
                        Name = propMatch.Groups["name"].Value,
                        Type = propMatch.Groups["type"].Value,
                        Value = propMatch.Groups["value"].Value.Trim(),
                        RawLine = line,
                        IsEditable = true
                    });
                    continue;
                }

                // Fallback: Manual parsing (PlayersViewModel style)
                if (line.Contains("="))
                {
                    var idx = line.IndexOf('=');
                    var name = line.Substring(0, idx).Trim().TrimStart(':');
                    var valueStr = line.Substring(idx + 1).Trim();

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var tokens = valueStr.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        if (tokens.Length >= 2)
                        {
                            properties.Add(new ObjectProperty
                            {
                                Name = name,
                                Type = tokens[0],
                                Value = tokens[1],
                                RawLine = line,
                                IsEditable = true
                            });
                        }
                        else if (tokens.Length == 1)
                        {
                            // Type only, no value (rare case)
                            properties.Add(new ObjectProperty
                            {
                                Name = name,
                                Type = tokens[0],
                                Value = "",
                                RawLine = line,
                                IsEditable = true
                            });
                        }
                    }
                }
            }

            // Group properties
            GroupProperties(result, properties);

            return result;
        }

        private ObjectInspectionResult ParseShowListResponse(IEnumerable<string> responses, string listId, bool isItemAttributesList)
        {
            var result = new ObjectInspectionResult
            {
                ObjectId = listId,
                ClassName = "LIST"
            };
            var properties = new List<ObjectProperty>();
            var listItemRegex = new Regex(
                @"^:\s*(?<type>[\w$]+)\s*(?<value>.*)$",
                RegexOptions.Compiled
            );
            var listStartRegex = new Regex(@"^:\s*\[\s*$", RegexOptions.Compiled);
            var listEndRegex = new Regex(@"^:\s*\]\s*$", RegexOptions.Compiled);

            var depth = 0;
            var sublistId = 0;
            var sublistIdStack = new Stack<int>();
            var elementIndexByDepth = new List<int> { 0 };
            var canEditListEntries = isItemAttributesList && !string.IsNullOrWhiteSpace(_currentListOwnerObjectId);
            var sublistItemAttIds = new Dictionary<int, int>();

            var index = 0;
            foreach (var line in responses)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith(">", StringComparison.Ordinal)) continue;

                var trimmed = line.Trim();
                if (trimmed == ":<" || trimmed == ":>")
                    continue;

                if (listStartRegex.IsMatch(trimmed))
                {
                    depth++;
                    while (elementIndexByDepth.Count <= depth)
                    {
                        elementIndexByDepth.Add(0);
                    }

                    elementIndexByDepth[depth] = 0;
                    if (depth >= 2)
                    {
                        sublistId++;
                        sublistIdStack.Push(sublistId);
                    }
                    continue;
                }

                if (listEndRegex.IsMatch(trimmed))
                {
                    if (depth >= 2 && sublistIdStack.Count > 0)
                    {
                        sublistIdStack.Pop();
                    }
                    depth = Math.Max(0, depth - 1);
                    continue;
                }

                var match = listItemRegex.Match(trimmed);
                if (!match.Success)
                    continue;

                var type = match.Groups["type"].Value;
                var value = match.Groups["value"].Value.Trim();
                if (type == "[" || type == "]") continue;

                var sublistIndex = depth < elementIndexByDepth.Count ? elementIndexByDepth[depth] : 0;
                if (depth < elementIndexByDepth.Count)
                {
                    elementIndexByDepth[depth] = sublistIndex + 1;
                }

                var currentSublistId = sublistIdStack.Count > 0 ? sublistIdStack.Peek() : -1;
                if (canEditListEntries && depth >= 2 && sublistIndex == 0 && type.Equals("INT", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(value, out var intValue))
                    {
                        sublistItemAttIds[currentSublistId] = intValue / 100;
                    }
                }

                var isEditable = false;
                if (canEditListEntries && depth >= 2 && currentSublistId >= 0)
                {
                    if (sublistIndex == 0)
                    {
                        isEditable = true;
                    }
                    else if (sublistIndex == 1 && sublistItemAttIds.TryGetValue(currentSublistId, out var itemAttId)
                             && itemAttId == 68)
                    {
                        isEditable = true;
                    }
                    else if (sublistIndex == 2 && sublistItemAttIds.TryGetValue(currentSublistId, out var attackItemAttId)
                             && attackItemAttId == 55)
                    {
                        isEditable = true;
                    }
                }

                properties.Add(new ObjectProperty
                {
                    Name = $"[{index}]",
                    Type = type,
                    Value = value,
                    RawLine = line,
                    IsEditable = isEditable,
                    ListDepth = depth,
                    SublistIndex = sublistIndex,
                    SublistId = currentSublistId,
                    ListIndex = index,
                    CategoryOverride = isItemAttributesList && depth >= 2 ? "ItemAtt in Lists" : string.Empty
                });
                index++;
            }

            GroupProperties(result, properties);
            return result;
        }

        private void GroupProperties(ObjectInspectionResult result, List<ObjectProperty> properties)
        {
            var grouped = properties
                .GroupBy(p => p.CategoryGroup)
                .OrderBy(g => PropertyCategorizer.GetCategoryOrder(g.Key));

            result.PropertyGroups.Clear();
            foreach (var group in grouped)
            {
                var pg = new PropertyGroup { Name = group.Key };
                foreach (var prop in group.OrderBy(p => p.Name))
                {
                    pg.Properties.Add(prop);
                }
                result.PropertyGroups.Add(pg);
            }
        }

        private void EnsureSubscription(M59ServerConnection connection)
        {
            if (_lastConnection == connection) return;

            if (_lastConnection != null)
            {
                _lastConnection.ResponseReceived -= OnServerResponse;
            }

            _lastConnection = connection;
            _lastConnection.ResponseReceived += OnServerResponse;
        }

        private void OnServerResponse(object? sender, string response)
        {
            // Optional: Log responses for debugging
        }

        private void RunOnUi(Action action)
        {
            Application.Current?.Dispatcher.Invoke(action);
        }

        private async Task ResolveListEntriesAsync(List<ObjectProperty> listProps, CancellationToken token)
        {
            await EnsureItemsLoadedAsync();
            var isItemAttributesList = string.Equals(_currentListPropertyName, "plItem_attributes", StringComparison.OrdinalIgnoreCase);
            var isSpellList = string.Equals(_currentListPropertyName, "plSpells", StringComparison.OrdinalIgnoreCase);
            var isSkillList = string.Equals(_currentListPropertyName, "plSkills", StringComparison.OrdinalIgnoreCase);
            var isSchoolTotalsList = string.Equals(_currentListPropertyName, "plSchools", StringComparison.OrdinalIgnoreCase);

            if (isItemAttributesList)
            {
                await EnsureItemAttributesLoadedAsync();
                await EnsureSpellsLoadedAsync();
                await EnsureItemSpellsLoadedAsync();
                await EnsureAttackSpellTypesLoadedAsync();
            }

            if (isSpellList)
            {
                await EnsureSpellsLoadedAsync();
                await EnsureSpellSchoolsLoadedAsync();
            }

            if (isSkillList)
            {
                await EnsureSkillsLoadedAsync();
            }

            if (isSchoolTotalsList)
            {
                await EnsureSpellSchoolsLoadedAsync();
                await EnsureSkillSchoolsLoadedAsync();
            }

            foreach (var prop in listProps.OrderBy(p => p.ListIndex < 0 ? int.MaxValue : p.ListIndex))
            {
                if (token.IsCancellationRequested)
                    return;

                if (isItemAttributesList && prop.ListDepth >= 2)
                {
                    if (prop.Type.Equals("INT", StringComparison.OrdinalIgnoreCase) &&
                        int.TryParse(prop.Value, out var intValue))
                    {
                        if (prop.SublistIndex == 0)
                        {
                            var itemAttId = intValue / 100;
                            if (_itemAttributesById.TryGetValue(itemAttId, out var itemAttName))
                            {
                                var power = (intValue / 10) % 10;
                                var identified = (intValue % 2) == 1 ? "identified" : "unidentified";
                                RunOnUi(() => prop.ResolvedValue = $"INT {intValue} ({itemAttName}, power {power}, {identified})");
                            }
                        }
                        else if (prop.SublistIndex == 1 && TryGetItemAttId(listProps, prop.SublistId, out var itemAttId)
                                 && itemAttId == 68 && _itemSpellIds.Contains(intValue)
                                 && _spellsById.TryGetValue(intValue, out var spellInfo))
                        {
                            var label = $"INT {intValue} ({spellInfo.SidName}";
                            if (!string.IsNullOrWhiteSpace(spellInfo.GermanName))
                            {
                                label += $" - {spellInfo.GermanName}";
                            }
                            label += ")";
                            RunOnUi(() => prop.ResolvedValue = label);
                        }
                        else if (prop.SublistIndex == 2 && TryGetItemAttId(listProps, prop.SublistId, out var attackItemAttId)
                                 && attackItemAttId == 55
                                 && _attackSpellTypesByValue.TryGetValue(intValue, out var attackType))
                        {
                            RunOnUi(() => prop.ResolvedValue = $"INT {intValue} ({attackType})");
                        }
                    }
                }

                if ((isSpellList || isSkillList) && prop.Type.Equals("INT", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(prop.Value, out var compoundValue))
                {
                    var abs = Math.Abs(compoundValue);
                    var ability = abs % 100;
                    var used = compoundValue > 0;
                    var number = abs / 100;

                    if (isSpellList && number > 0 && _spellsById.TryGetValue(number, out var spellInfo))
                    {
                        var label = $"INT {compoundValue} ({spellInfo.SidName}";
                        if (!string.IsNullOrWhiteSpace(spellInfo.GermanName))
                            label += $" - {spellInfo.GermanName}";
                        if (_spellSchoolsBySid.TryGetValue(spellInfo.SidName, out var schoolInfo))
                            label += $", {schoolInfo.SchoolName} L{schoolInfo.Level}";
                        label += $", ability {ability}, {(used ? "used" : "unused")})";
                        RunOnUi(() => prop.ResolvedValue = label);
                    }
                    else if (isSkillList && number > 0 && _skillsById.TryGetValue(number, out var skillInfo))
                    {
                        var name = !string.IsNullOrWhiteSpace(skillInfo.GermanName) ? skillInfo.GermanName : skillInfo.EnglishName;
                        var school = !string.IsNullOrWhiteSpace(skillInfo.SchoolNameDe) ? skillInfo.SchoolNameDe : skillInfo.SchoolName;
                        var label = $"INT {compoundValue} ({skillInfo.ClassName}";
                        if (!string.IsNullOrWhiteSpace(name))
                            label += $" - {name}";
                        if (!string.IsNullOrWhiteSpace(school))
                            label += $", {school}";
                        if (skillInfo.Level > 0)
                            label += $" L{skillInfo.Level}";
                        label += $", ability {ability}, {(used ? "used" : "unused")})";
                        RunOnUi(() => prop.ResolvedValue = label);
                    }
                }

                if (isSchoolTotalsList && prop.Type.Equals("INT", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(prop.Value, out var schoolTotal) && prop.ListIndex >= 0)
                {
                    var schoolId = prop.ListIndex + 1;
                    var label = $"INT {schoolTotal} (School {schoolId}";
                    if (_spellSchoolsById.TryGetValue(schoolId, out var spellSchool))
                    {
                        label += $": {spellSchool}";
                    }
                    else if (_skillSchoolsById.TryGetValue(schoolId, out var skillSchool))
                    {
                        var schoolName = !string.IsNullOrWhiteSpace(skillSchool.GermanName)
                            ? skillSchool.GermanName
                            : skillSchool.EnglishName;
                        if (!string.IsNullOrWhiteSpace(schoolName))
                            label += $": {schoolName}";
                    }
                    label += ")";
                    RunOnUi(() => prop.ResolvedValue = label);
                }

                if (prop.Type.Equals("OBJECT", StringComparison.OrdinalIgnoreCase))
                {
                    var objectId = prop.Value.Trim();
                    if (string.IsNullOrWhiteSpace(objectId) || objectId == "0")
                        continue;

                    var className = await TryFetchObjectClassAsync(objectId, token);
                    var label = $"OBJECT {objectId}";
                    if (!string.IsNullOrWhiteSpace(className))
                    {
                        label += $" (Class {className})";
                        if (_itemsByClass.TryGetValue(className, out var item))
                        {
                            var itemName = string.IsNullOrWhiteSpace(item.GermanName) ? item.EnglishName : item.GermanName;
                            if (!string.IsNullOrWhiteSpace(itemName))
                            {
                                label += $" - {itemName}";
                            }
                        }
                    }

                    RunOnUi(() => prop.ResolvedValue = label);
                    await Task.Delay(60, token);
                }
                else if (prop.Type.Equals("CLASS", StringComparison.OrdinalIgnoreCase))
                {
                    var className = prop.Value.Trim();
                    if (!string.IsNullOrWhiteSpace(className) && _itemsByClass.TryGetValue(className, out var item))
                    {
                        var itemName = string.IsNullOrWhiteSpace(item.GermanName) ? item.EnglishName : item.GermanName;
                        if (!string.IsNullOrWhiteSpace(itemName))
                        {
                            RunOnUi(() => prop.ResolvedValue = $"CLASS {className} - {itemName}");
                        }
                    }
                }
            }
        }

        private async Task<string> TryFetchObjectClassAsync(string objectId, CancellationToken token)
        {
            var responses = await SendAndCollectAsync($"show object {objectId}", 600);
            if (token.IsCancellationRequested)
                return string.Empty;

            var headerRegex = new Regex(
                @":< OBJECT (?<id>\d+) is CLASS (?<classname>.*)",
                RegexOptions.Compiled
            );

            foreach (var line in responses)
            {
                var match = headerRegex.Match(line);
                if (match.Success)
                    return match.Groups["classname"].Value.Trim();
            }

            return string.Empty;
        }

        private async Task EnsureItemsLoadedAsync()
        {
            if (_itemsLoaded)
                return;

            var items = await _itemsDataService.LoadItemsAsync();
            _itemsByClass = items.ToDictionary(i => i.ClassName, StringComparer.OrdinalIgnoreCase);
            _itemsLoaded = true;
        }


        private async Task EnsureSpellsLoadedAsync()
        {
            if (_spellsLoaded)
                return;

            _spellsById = await _spellDataService.LoadSpellIdsAsync();
            _spellsLoaded = true;
        }

        private async Task EnsureSpellSchoolsLoadedAsync()
        {
            if (_spellSchoolsLoaded)
                return;

            _spellSchoolsBySid = await _spellDataService.LoadSpellSchoolMapAsync();
            _spellSchoolsById = await _spellDataService.LoadSpellSchoolsByIdAsync();
            _spellSchoolsLoaded = true;
        }

        private async Task EnsureItemAttributesLoadedAsync()
        {
            if (_itemAttributesLoaded)
                return;

            _itemAttributesById = await _itemAttributeDataService.LoadItemAttributesAsync();
            _itemAttributesLoaded = true;
        }

        private async Task EnsureSkillsLoadedAsync()
        {
            if (_skillsLoaded)
                return;

            _skillsById = await _skillDataService.LoadSkillsAsync();
            _skillsLoaded = true;
        }

        private async Task EnsureSkillSchoolsLoadedAsync()
        {
            if (_skillsLoaded && _skillSchoolsById.Count > 0)
                return;

            _skillSchoolsById = await _skillDataService.LoadSkillSchoolsByIdAsync();
        }

        private async Task EnsureItemSpellsLoadedAsync()
        {
            if (_itemSpellsLoaded)
                return;

            _itemSpellIds = await _spellDataService.LoadItemSpellIdsAsync();
            _itemSpellsLoaded = true;
        }

        private async Task EnsureAttackSpellTypesLoadedAsync()
        {
            if (_attackSpellTypesLoaded)
                return;

            _attackSpellTypesByValue = await _itemAttributeDataService.LoadAttackSpellTypesAsync();
            _attackSpellTypesLoaded = true;
        }

        private static bool TryGetItemAttId(IEnumerable<ObjectProperty> props, int sublistId, out int itemAttId)
        {
            itemAttId = 0;
            if (sublistId < 0)
                return false;

            var first = props.FirstOrDefault(p => p.SublistId == sublistId && p.SublistIndex == 0);
            if (first == null)
                return false;

            if (!first.Type.Equals("INT", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!int.TryParse(first.Value, out var intValue))
                return false;

            itemAttId = intValue / 100;
            return itemAttId > 0;
        }

        private async Task ResolveSpellPropertiesAsync(ObjectInspectionResult result)
        {
            await EnsureSpellsLoadedAsync();
            if (_spellsById.Count == 0)
                return;

            var props = result.PropertyGroups.SelectMany(g => g.Properties);
            foreach (var prop in props)
            {
                if (!prop.Name.Equals("poSpell", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!prop.Type.Equals("INT", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!int.TryParse(prop.Value, out var spellId))
                    continue;

                if (spellId <= 0)
                    continue;

                if (_spellsById.TryGetValue(spellId, out var spellInfo))
                {
                    var label = $"INT {spellId} ({spellInfo.SidName}";
                    if (!string.IsNullOrWhiteSpace(spellInfo.GermanName))
                    {
                        label += $" - {spellInfo.GermanName}";
                    }
                    label += ")";
                    RunOnUi(() => prop.ResolvedValue = label);
                }
            }
        }

        private async Task ResolveAttackTypePropertiesAsync(ObjectInspectionResult result)
        {
            await EnsureAttackSpellTypesLoadedAsync();
            if (_attackSpellTypesByValue.Count == 0)
                return;

            var props = result.PropertyGroups.SelectMany(g => g.Properties);
            foreach (var prop in props)
            {
                if (!IsAttackSpellTypeProperty(prop))
                    continue;

                if (!int.TryParse(prop.Value, out var attackValue))
                    continue;

                if (_attackSpellTypesByValue.TryGetValue(attackValue, out var attackName))
                {
                    RunOnUi(() => prop.ResolvedValue = $"INT {attackValue} ({attackName})");
                }
            }
        }

        private bool IsSpellProperty(ObjectProperty property)
        {
            if (!property.Type.Equals("INT", StringComparison.OrdinalIgnoreCase))
                return false;

            if (property.Name.IndexOf("spell", StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return int.TryParse(property.Value, out _);
        }

        private bool IsAttackSpellTypeProperty(ObjectProperty property)
        {
            if (!property.Type.Equals("INT", StringComparison.OrdinalIgnoreCase))
                return false;

            var name = property.Name;
            if (name.IndexOf("attack", StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            return name.IndexOf("spell", StringComparison.OrdinalIgnoreCase) >= 0
                   || name.IndexOf("type", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool IsItemAttributeListEntry(ObjectProperty property)
        {
            if (!_isListInspection)
                return false;

            if (!string.Equals(_currentListPropertyName, "plItem_attributes", StringComparison.OrdinalIgnoreCase))
                return false;

            return property.ListDepth >= 2 && property.SublistIndex == 0;
        }

        private async Task EditItemAttributeListEntryAsync(ObjectProperty property)
        {
            if (string.IsNullOrWhiteSpace(_currentListOwnerObjectId))
            {
                StatusMessage = "❌ Kein Objekt-Kontext fuer List-Edit vorhanden.";
                return;
            }

            if (!int.TryParse(property.Value, out var compound))
            {
                StatusMessage = "❌ ItemAtt-Wert ist kein INT.";
                return;
            }

            await EnsureItemAttributesLoadedAsync();
            if (_itemAttributesById.Count == 0)
            {
                StatusMessage = "❌ ItemAtt-Liste nicht geladen.";
                return;
            }

            if (property.SublistIndex == 0)
            {
                var currentItemAttId = compound / 100;
                var power = (compound / 10) % 10;
                var identified = compound % 2;

                var items = _itemAttributesById
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => new SpellPickerItem(kvp.Key, kvp.Value, null))
                    .ToList();

                var selectedItemAtt = SpellSelectWindow.ShowDialog(items, currentItemAttId);
                if (!selectedItemAtt.HasValue)
                    return;

                var powerItems = Enumerable.Range(0, 10)
                    .Select(i => new SpellPickerItem(i, $"Power {i}", null))
                    .ToList();
                var selectedPower = SpellSelectWindow.ShowDialog(powerItems, power);
                if (!selectedPower.HasValue)
                    return;

                var idItems = new List<SpellPickerItem>
                {
                    new SpellPickerItem(0, "Unidentified / Unidentifiziert", null),
                    new SpellPickerItem(1, "Identified / Identifiziert", null)
                };
                var selectedIdent = SpellSelectWindow.ShowDialog(idItems, identified);
                if (!selectedIdent.HasValue)
                    return;

                var newCompound = (selectedItemAtt.Value * 100) + (selectedPower.Value * 10) + selectedIdent.Value;
                await RebuildItemAttributeListAsync(property.SublistId, property.SublistIndex, "INT", newCompound.ToString());
                return;
            }

            if (!TryGetItemAttId(_currentListProperties, property.SublistId, out var itemAttId))
                return;

            if (property.SublistIndex == 1 && itemAttId == 68)
            {
                await EnsureSpellsLoadedAsync();
                await EnsureItemSpellsLoadedAsync();

                var spellItems = _spellsById
                    .Where(kvp => _itemSpellIds.Contains(kvp.Key))
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => new SpellPickerItem(kvp.Key, kvp.Value.SidName, kvp.Value.GermanName))
                    .ToList();

                if (spellItems.Count == 0)
                    return;

                var selectedSpell = SpellSelectWindow.ShowDialog(spellItems, compound);
                if (!selectedSpell.HasValue)
                    return;

                await RebuildItemAttributeListAsync(property.SublistId, property.SublistIndex, "INT", selectedSpell.Value.ToString());
                return;
            }

            if (property.SublistIndex == 2 && itemAttId == 55)
            {
                await EnsureAttackSpellTypesLoadedAsync();
                var attackItems = _attackSpellTypesByValue
                    .OrderBy(kvp => kvp.Key)
                    .Select(kvp => new SpellPickerItem(kvp.Key, kvp.Value, null))
                    .ToList();

                if (attackItems.Count == 0)
                    return;

                var selectedAttack = SpellSelectWindow.ShowDialog(attackItems, compound);
                if (!selectedAttack.HasValue)
                    return;

                await RebuildItemAttributeListAsync(property.SublistId, property.SublistIndex, "INT", selectedAttack.Value.ToString());
            }
        }

        private async Task RebuildItemAttributeListAsync(int targetSublistId, int targetSublistIndex, string newType, string newValue)
        {
            if (string.IsNullOrWhiteSpace(_currentListOwnerObjectId))
                return;

            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                StatusMessage = "❌ Nicht verbunden.";
                return;
            }

            var entries = _currentListProperties
                .Where(p => p.SublistId >= 0 && p.ListDepth >= 2)
                .GroupBy(p => p.SublistId)
                .OrderBy(g => g.Min(p => p.ListIndex))
                .ToList();

            if (entries.Count == 0)
            {
                StatusMessage = "❌ Keine Listeneintraege gefunden.";
                return;
            }

            StatusMessage = "⏳ Erstelle neue ItemAtt-Liste...";

            var sublistIds = new List<int>();
            foreach (var group in entries)
            {
                var ordered = group.OrderBy(p => p.SublistIndex).ToList();
                var listHead = 0;

                for (var i = ordered.Count - 1; i >= 0; i--)
                {
                    var entry = ordered[i];
                    var type = entry.Type;
                    var value = entry.Value;

                    if (group.Key == targetSublistId && entry.SublistIndex == targetSublistIndex)
                    {
                        type = newType;
                        value = newValue;
                    }

                    var firstValue = FormatListNodeValue(type, value);
                    var restValue = listHead == 0 ? "$ 0" : $"list {listHead}";
                    listHead = await CreateListNodeAsync(firstValue, restValue);
                }

                if (listHead == 0)
                {
                    StatusMessage = "❌ Fehler beim Erstellen der Unterliste.";
                    return;
                }

                sublistIds.Add(listHead);
            }

            var mainListHead = 0;
            for (var i = sublistIds.Count - 1; i >= 0; i--)
            {
                var firstValue = $"list {sublistIds[i]}";
                var restValue = mainListHead == 0 ? "$ 0" : $"list {mainListHead}";
                mainListHead = await CreateListNodeAsync(firstValue, restValue);
            }

            if (mainListHead == 0)
            {
                StatusMessage = "❌ Fehler beim Erstellen der Hauptliste.";
                return;
            }

            await conn.SendAdminCommandAsync($"set o {_currentListOwnerObjectId} plItem_attributes LIST {mainListHead}");
            await Task.Delay(500);

            ObjectId = $"list {mainListHead}";
            await LoadListInternal(mainListHead.ToString());
            StatusMessage = "✅ ItemAtt-Liste aktualisiert.";
        }

        private async Task<int> CreateListNodeAsync(string firstValue, string restValue)
        {
            var responses = await SendAndCollectAsync($"create listnode {firstValue} {restValue}", 800);
            var idRegex = new Regex(@"Created list node\s+(?<id>\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var ids = new List<int>();

            foreach (var line in responses)
            {
                var match = idRegex.Match(line);
                if (match.Success && int.TryParse(match.Groups["id"].Value, out var parsed) && parsed > 0)
                {
                    ids.Add(parsed);
                }
            }

            if (ids.Count == 0)
                return 0;

            var normalizedFirst = NormalizeListValue(firstValue);
            var normalizedRest = NormalizeListValue(restValue);

            for (var i = ids.Count - 1; i >= 0; i--)
            {
                var id = ids[i];
                var (first, tail) = await FetchListNodeValuesAsync(id);
                if (string.Equals(first, normalizedFirst, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(tail, normalizedRest, StringComparison.OrdinalIgnoreCase))
                {
                    return id;
                }
            }

            return 0;
        }

        private static string FormatListNodeValue(string type, string value)
        {
            if (string.IsNullOrWhiteSpace(type))
                return value;

            var normalized = type.Trim();
            if (normalized == "$")
                return "$ 0";

            if (normalized.Equals("INT", StringComparison.OrdinalIgnoreCase))
                return $"int {value}";
            if (normalized.Equals("TIMER", StringComparison.OrdinalIgnoreCase))
                return $"timer {value}";
            if (normalized.Equals("OBJECT", StringComparison.OrdinalIgnoreCase))
                return $"object {value}";
            if (normalized.Equals("CLASS", StringComparison.OrdinalIgnoreCase))
                return $"class {value}";
            if (normalized.Equals("LIST", StringComparison.OrdinalIgnoreCase))
                return $"list {value}";
            if (normalized.Equals("RESOURCE", StringComparison.OrdinalIgnoreCase))
                return $"resource {value}";
            if (normalized.Equals("STRING", StringComparison.OrdinalIgnoreCase))
                return $"string {value}";

            return $"{normalized.ToLowerInvariant()} {value}".Trim();
        }

        private static string NormalizeListValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            var trimmed = value.Trim();
            if (trimmed.StartsWith("$", StringComparison.Ordinal))
            {
                return "$ 0";
            }

            var parts = trimmed.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 1)
                return parts[0].ToUpperInvariant();

            return $"{parts[0].ToUpperInvariant()} {string.Join(' ', parts.Skip(1))}";
        }

        private async Task<(string First, string TailValue)> FetchListNodeValuesAsync(int nodeId)
        {
            var responses = await SendAndCollectAsync($"show listnode {nodeId}", 600);
            var firstRegex = new Regex(@"\bfirst\s*=\s*(?<value>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var restRegex = new Regex(@"\brest\s*=\s*(?<value>.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var first = string.Empty;
            var rest = string.Empty;

            foreach (var line in responses)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var firstMatch = firstRegex.Match(line);
                if (firstMatch.Success)
                {
                    first = NormalizeListValue(firstMatch.Groups["value"].Value.Trim());
                }

                var restMatch = restRegex.Match(line);
                if (restMatch.Success)
                {
                    rest = NormalizeListValue(restMatch.Groups["value"].Value.Trim());
                }
            }

            return (first, rest);
        }

        private void CancelResolve()
        {
            if (_resolveCts == null)
                return;

            _resolveCts.Cancel();
            _resolveCts.Dispose();
            _resolveCts = null;
        }

        private string GetListIdFromProperty(string propertyName)
        {
            if (CurrentInspection == null)
                return string.Empty;

            foreach (var group in CurrentInspection.PropertyGroups)
            {
                foreach (var prop in group.Properties)
                {
                    if (prop.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase) && prop.IsList)
                    {
                        return prop.ListId;
                    }
                }
            }

            return string.Empty;
        }

        private string? GetListPropertyName(string listId)
        {
            if (CurrentInspection == null || string.IsNullOrWhiteSpace(listId))
                return null;

            foreach (var group in CurrentInspection.PropertyGroups)
            {
                foreach (var prop in group.Properties)
                {
                    if (prop.IsList && prop.ListId == listId)
                    {
                        return prop.Name;
                    }
                }
            }

            return null;
        }

        private string GetFirstAvailableListId(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                var listId = GetListIdFromProperty(name);
                if (!string.IsNullOrWhiteSpace(listId))
                    return listId;
            }

            return string.Empty;
        }

        private static bool TryParseListInput(string input, out string listId)
        {
            listId = string.Empty;
            if (input.StartsWith("list ", StringComparison.OrdinalIgnoreCase))
            {
                listId = input.Substring(5).Trim();
            }
            else if (input.StartsWith("list:", StringComparison.OrdinalIgnoreCase))
            {
                listId = input.Substring(5).Trim();
            }

            return !string.IsNullOrWhiteSpace(listId);
        }

        private string BuildHistoryToken()
        {
            if (CurrentInspection == null || string.IsNullOrWhiteSpace(CurrentInspection.ObjectId))
                return string.Empty;

            var prefix = _isListInspection ? ListTokenPrefix : ObjectTokenPrefix;
            return $"{prefix}{CurrentInspection.ObjectId}";
        }

        private static bool TryParseHistoryToken(string token, out bool isList, out string id)
        {
            isList = false;
            id = string.Empty;

            if (token.StartsWith(ListTokenPrefix, StringComparison.OrdinalIgnoreCase))
            {
                isList = true;
                id = token.Substring(ListTokenPrefix.Length);
                return !string.IsNullOrWhiteSpace(id);
            }

            if (token.StartsWith(ObjectTokenPrefix, StringComparison.OrdinalIgnoreCase))
            {
                id = token.Substring(ObjectTokenPrefix.Length);
                return !string.IsNullOrWhiteSpace(id);
            }

            id = token;
            return !string.IsNullOrWhiteSpace(id);
        }

        private static string FormatHistoryEntry(string token)
        {
            if (TryParseHistoryToken(token, out var isList, out var id))
            {
                return isList ? $"List {id}" : $"Object {id}";
            }

            return token;
        }

        private string? PromptForInput(string message, string title)
        {
            // Simple input dialog using InputBox pattern (same as PlayersViewModel)
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 200,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var grid = new System.Windows.Controls.Grid();
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });

            var textBlock = new System.Windows.Controls.TextBlock
            {
                Text = message,
                Margin = new Thickness(10),
                TextWrapping = TextWrapping.Wrap
            };
            System.Windows.Controls.Grid.SetRow(textBlock, 0);

            var textBox = new System.Windows.Controls.TextBox
            {
                Margin = new Thickness(10),
                FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                FontSize = 14
            };
            System.Windows.Controls.Grid.SetRow(textBox, 1);

            var buttonPanel = new System.Windows.Controls.StackPanel
            {
                Orientation = System.Windows.Controls.Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                Margin = new Thickness(10)
            };

            var okButton = new System.Windows.Controls.Button
            {
                Content = "OK",
                Width = 75,
                Margin = new Thickness(5),
                IsDefault = true
            };
            okButton.Click += (s, e) => dialog.DialogResult = true;

            var cancelButton = new System.Windows.Controls.Button
            {
                Content = "Cancel",
                Width = 75,
                Margin = new Thickness(5),
                IsCancel = true
            };
            cancelButton.Click += (s, e) => dialog.DialogResult = false;

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            var mainPanel = new System.Windows.Controls.StackPanel();
            mainPanel.Children.Add(textBlock);
            mainPanel.Children.Add(textBox);
            mainPanel.Children.Add(buttonPanel);

            dialog.Content = mainPanel;
            textBox.Focus();

            var result = dialog.ShowDialog();
            return result == true ? textBox.Text : null;
        }
    }
}

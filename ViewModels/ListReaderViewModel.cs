using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M59AdminTool.Models;
using M59AdminTool.Services;
using Application = System.Windows.Application;

namespace M59AdminTool.ViewModels
{
    public partial class ListReaderViewModel : ObservableObject
    {
        private readonly ConnectionViewModel? _connectionViewModel;
        private M59ServerConnection? _lastConnection;
        private static readonly Regex ListIdRegex = new(@"\bLIST\b[^0-9]*(\d+)", RegexOptions.IgnoreCase);
        private static readonly Regex ListIdAltRegex = new(@"\blist[_\s]?id\b[^0-9]*(\d+)", RegexOptions.IgnoreCase);
        private static readonly Regex PropertyLineRegex = new(
            @"^\s*:?\s*(?<name>[\w]+)\s*=\s*(?<type>[\w$]+)\s+(?<value>.+)$",
            RegexOptions.Compiled
        );

        // Command Type Options
        public List<CommandTypeOption> CommandTypes { get; }

        // Collections
        public ObservableCollection<ObjectListEntry> Results { get; } = new();
        public ObservableCollection<ObjectListEntry> FilteredResults { get; } = new();
        public ObservableCollection<string> DetailLines { get; } = new();
        public ObservableCollection<string> SystemClasses { get; } = new();
        public ObservableCollection<ImportantListOption> ImportantLists { get; } = new();

        // Observable Properties
        [ObservableProperty]
        private CommandTypeOption? _selectedCommandType;

        [ObservableProperty]
        private string _className = string.Empty;

        [ObservableProperty]
        private string? _selectedSystemClass;

        [ObservableProperty]
        private ImportantListOption? _selectedImportantList;

        [ObservableProperty]
        private ObjectListEntry? _selectedResult;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _lastResponse = string.Empty;

        [ObservableProperty]
        private string _statusMessage = "Ready";

        // Constructor
        public ListReaderViewModel(ConnectionViewModel? connectionViewModel = null)
        {
            _connectionViewModel = connectionViewModel;

            var loc = LocalizationService.Instance;
            CommandTypes = new List<CommandTypeOption>
            {
                new(loc.GetString("ListReader_CommandType_Class"), "class"),
                new(loc.GetString("ListReader_CommandType_Instances"), "instances"),
                new(loc.GetString("ListReader_CommandType_All"), "all"),
                new(loc.GetString("ListReader_CommandType_List"), "list")
            };

            SelectedCommandType = CommandTypes.First();

            _ = LoadSystemClassesAsync();
            _ = LoadImportantListsAsync();

            // Subscribe to property changes for filtering
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SearchText))
                {
                    UpdateFilteredResults();
                }
            };
        }

        partial void OnSelectedSystemClassChanged(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (!string.Equals(ClassName, value, StringComparison.OrdinalIgnoreCase))
            {
                ClassName = value;
            }

            if (ExecuteQueryCommand.CanExecute(null))
            {
                ExecuteQueryCommand.Execute(null);
            }
        }

        partial void OnSelectedImportantListChanged(ImportantListOption? value)
        {
            if (value == null)
                return;

            if (string.IsNullOrWhiteSpace(value.ListId))
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_ListIdMissing");
                return;
            }

            var listCommand = CommandTypes.FirstOrDefault(opt => string.Equals(opt.CommandName, "list", StringComparison.OrdinalIgnoreCase));
            if (listCommand != null)
            {
                SelectedCommandType = listCommand;
            }

            ClassName = value.ListId;
            if (ExecuteQueryCommand.CanExecute(null))
            {
                ExecuteQueryCommand.Execute(null);
            }
        }

        public bool TryExtractListId(string line, out string listId)
        {
            listId = string.Empty;
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var match = ListIdRegex.Match(line);
            if (!match.Success)
                match = ListIdAltRegex.Match(line);

            if (!match.Success)
                return false;

            listId = match.Groups[1].Value;
            return !string.IsNullOrWhiteSpace(listId);
        }

        public async Task<IReadOnlyList<string>> FetchListEntriesAsync(string listId)
        {
            if (string.IsNullOrWhiteSpace(listId))
                return Array.Empty<string>();

            var responses = await SendAndCollectAsync($"show list {listId}", 1500);
            if (responses.Count == 0)
                return Array.Empty<string>();

            return responses
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith(">", StringComparison.Ordinal))
                .Select(line => line.Trim())
                .ToList();
        }

        public bool TryParseIntPropertyLine(string line, out string name, out string currentValue)
        {
            name = string.Empty;
            currentValue = string.Empty;

            if (string.IsNullOrWhiteSpace(line))
                return false;

            var match = PropertyLineRegex.Match(line);
            if (!match.Success)
                return false;

            var type = match.Groups["type"].Value.Trim();
            if (!type.Equals("INT", StringComparison.OrdinalIgnoreCase))
                return false;

            name = match.Groups["name"].Value.Trim();
            currentValue = match.Groups["value"].Value.Trim();

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(currentValue))
                return false;

            var firstToken = currentValue.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(firstToken))
                currentValue = firstToken;

            return true;
        }

        public async Task EditIntFromDetailLineAsync(string? line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return;

            if (!TryParseIntPropertyLine(line, out var name, out var currentValue))
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_EditIntNotInt");
                return;
            }

            var objectId = SelectedResult?.ObjectId;
            if (string.IsNullOrWhiteSpace(objectId))
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_EditIntNoObjectId");
                return;
            }

            var loc = LocalizationService.Instance;
            var input = PromptForInput(
                string.Format(loc.GetString("ListReader_Prompt_EditInt_Message"), name, currentValue),
                loc.GetString("ListReader_Prompt_EditInt_Title")
            );
            if (string.IsNullOrWhiteSpace(input))
                return;

            if (!int.TryParse(input.Trim(), out _))
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_InvalidIntValue");
                return;
            }

            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_NotConnected");
                return;
            }

            try
            {
                await conn.SendAdminCommandAsync($"set o {objectId} {name} INT {input.Trim()}");
                await Task.Delay(500);
                await ShowObjectDetails();
                StatusMessage = string.Format(loc.GetString("ListReader_Message_EditIntUpdated"), name);
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(loc.GetString("ListReader_Message_SendFailed"), ex.Message);
            }
        }

        // Commands
        [RelayCommand]
        private async Task ExecuteQuery()
        {
            // Validate connection
            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_NotConnected");
                return;
            }

            // Validate class name
            if (string.IsNullOrWhiteSpace(ClassName))
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_ClassNameRequired");
                return;
            }

            // Validate command type
            if (SelectedCommandType == null)
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_CommandTypeRequired");
                return;
            }

            // Clear previous results
            Results.Clear();
            FilteredResults.Clear();
            DetailLines.Clear();
            StatusMessage = string.Format(LocalizationService.Instance.GetString("ListReader_Message_Executing"), SelectedCommandType.DisplayName, ClassName);

            // Build command string based on selected type
            string command = $"show {SelectedCommandType.CommandName} {ClassName}";

            // Send command and collect responses
            var responses = await SendAndCollectAsync(command, 1500);

            if (responses.Count == 0)
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_NoServerResponse");
                return;
            }

            // Parse responses based on command type
            List<ObjectListEntry> entries;
            switch (SelectedCommandType.CommandName.ToLower())
            {
                case "class":
                    entries = ParseClassResponse(responses);
                    break;
                case "instances":
                    entries = ParseInstancesResponse(responses);
                    break;
                case "all":
                    entries = ParseAllResponse(responses);
                    break;
                case "list":
                    entries = ParseListResponse(responses);
                    break;
                default:
                    StatusMessage = string.Format(LocalizationService.Instance.GetString("ListReader_Message_UnknownCommandType"), SelectedCommandType.CommandName);
                    return;
            }

            // Populate Results collection
            RunOnUi(() =>
            {
                foreach (var entry in entries)
                {
                    Results.Add(entry);
                }
                UpdateFilteredResults();
                StatusMessage = string.Format(LocalizationService.Instance.GetString("ListReader_Message_ResultsFound"), entries.Count);
            });
        }

        [RelayCommand]
        private void ClearResults()
        {
            Results.Clear();
            FilteredResults.Clear();
            DetailLines.Clear();
            StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_ResultsCleared");
        }

        [RelayCommand]
        private void ExportResults()
        {
            if (Results.Count == 0)
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_NoResultsToExport");
                return;
            }

            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = $"ListReader_{ClassName}_{DateTime.Now:yyyyMMdd_HHmmss}",
                    DefaultExt = ".txt",
                    Filter = "Text files (*.txt)|*.txt|JSON files (*.json)|*.json|All files (*.*)|*.*"
                };

                if (dialog.ShowDialog() == true)
                {
                    var lines = new List<string>();
                    lines.Add($"List Reader Export - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                    lines.Add($"Command Type: {SelectedCommandType?.DisplayName}");
                    lines.Add($"Class Name: {ClassName}");
                    lines.Add($"Results: {Results.Count}");
                    lines.Add("");
                    lines.Add("----------------------------------------");
                    lines.Add("");

                    foreach (var entry in Results)
                    {
                        lines.Add($"DisplayText: {entry.DisplayText}");
                        if (!string.IsNullOrEmpty(entry.ObjectId))
                            lines.Add($"ObjectId: {entry.ObjectId}");
                        lines.Add($"ClassName: {entry.ClassName}");

                        if (entry.Properties.Count > 0)
                        {
                            lines.Add("Properties:");
                            foreach (var prop in entry.Properties)
                            {
                                lines.Add($"  {prop.Key} = {prop.Value}");
                            }
                        }

                        lines.Add("");
                        lines.Add("--- Raw Response ---");
                        lines.Add(entry.Raw);
                        lines.Add("");
                        lines.Add("----------------------------------------");
                        lines.Add("");
                    }

                    System.IO.File.WriteAllLines(dialog.FileName, lines);
                    StatusMessage = string.Format(LocalizationService.Instance.GetString("ListReader_Message_ExportSuccess"), dialog.FileName);
                }
                else
                {
                    StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_ExportCanceled");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = string.Format(LocalizationService.Instance.GetString("ListReader_Message_ExportFailed"), ex.Message);
            }
        }

        [RelayCommand]
        private async Task ShowObjectDetails()
        {
            if (SelectedResult == null)
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_SelectResultFirst");
                return;
            }

            DetailLines.Clear();

            // If object ID is available, query detailed properties
            if (!string.IsNullOrEmpty(SelectedResult.ObjectId))
            {
                StatusMessage = string.Format(LocalizationService.Instance.GetString("ListReader_Message_LoadingDetails"), SelectedResult.ObjectId);

                var responses = await SendAndCollectAsync($"show object {SelectedResult.ObjectId}", 1500);

                if (responses.Count > 0)
                {
                    RunOnUi(() =>
                    {
                        foreach (var line in responses)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            if (line.StartsWith(">", StringComparison.Ordinal)) continue;
                            DetailLines.Add(line);
                        }
                        StatusMessage = string.Format(LocalizationService.Instance.GetString("ListReader_Message_DetailsLoaded"), SelectedResult.ObjectId);
                    });
                }
                else
                {
                    StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_NoDetailsReceived");
                }
            }
            // Otherwise show properties from ParseClassResponse
            else if (SelectedResult.Properties.Count > 0)
            {
                RunOnUi(() =>
                {
                    DetailLines.Add($"Class: {SelectedResult.ClassName}");
                    DetailLines.Add("");
                    foreach (var prop in SelectedResult.Properties)
                    {
                        DetailLines.Add($"{prop.Key} = {prop.Value}");
                    }
                    StatusMessage = string.Format(LocalizationService.Instance.GetString("ListReader_Message_PropertiesShown"), SelectedResult.Properties.Count);
                });
            }
            // Fallback: show raw response
            else if (!string.IsNullOrEmpty(SelectedResult.Raw))
            {
                RunOnUi(() =>
                {
                    DetailLines.Add(SelectedResult.Raw);
                    StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_RawResponseShown");
                });
            }
            else
            {
                StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_NoDetailsAvailable");
            }
        }

        private string? PromptForInput(string message, string title)
        {
            var dialog = new System.Windows.Window
            {
                Title = title,
                Width = 400,
                Height = 220,
                SizeToContent = System.Windows.SizeToContent.Height,
                MinWidth = 350,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                ResizeMode = System.Windows.ResizeMode.NoResize
            };

            var grid = new System.Windows.Controls.Grid { Margin = new System.Windows.Thickness(10) };
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });

            var label = new System.Windows.Controls.TextBlock { Text = message, Margin = new System.Windows.Thickness(0, 0, 0, 10) };
            System.Windows.Controls.Grid.SetRow(label, 0);
            var textBox = new System.Windows.Controls.TextBox { Margin = new System.Windows.Thickness(0, 0, 0, 10), Padding = new System.Windows.Thickness(5) };
            System.Windows.Controls.Grid.SetRow(textBox, 1);

            var buttonPanel = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            System.Windows.Controls.Grid.SetRow(buttonPanel, 2);

            var okButton = new System.Windows.Controls.Button { Content = "OK", Width = 75, Margin = new System.Windows.Thickness(0, 0, 5, 0) };
            var cancelButton = new System.Windows.Controls.Button { Content = "Cancel", Width = 75 };
            okButton.Click += (_, _) => { dialog.DialogResult = true; dialog.Close(); };
            cancelButton.Click += (_, _) => { dialog.DialogResult = false; dialog.Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(label);
            grid.Children.Add(textBox);
            grid.Children.Add(buttonPanel);

            dialog.Content = grid;
            if (dialog.ShowDialog() == true)
            {
                return textBox.Text;
            }
            return null;
        }

        // Helper Methods
        private async Task<List<string>> SendAndCollectAsync(string command, int waitMs = 1500)
        {
            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                Log(LocalizationService.Instance.GetString("ListReader_Message_NotConnected"));
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
            Log($"> {command}");

            try
            {
                await conn.SendAdminCommandAsync(command);
                await Task.Delay(waitMs);
            }
            catch (Exception ex)
            {
                Log(string.Format(LocalizationService.Instance.GetString("ListReader_Message_SendFailed"), ex.Message));
            }
            finally
            {
                conn.ResponseReceived -= Handler;
            }

            return collected;
        }

        private List<ObjectListEntry> ParseClassResponse(IEnumerable<string> responses)
        {
            var result = new ObjectListEntry
            {
                ClassName = ClassName
            };

            foreach (var line in responses)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Skip command echo lines
                if (line.StartsWith(">", StringComparison.Ordinal)) continue;

                // Parse "Class: ClassName" format
                if (line.StartsWith("Class:", StringComparison.OrdinalIgnoreCase))
                {
                    result.ClassName = line.Substring(6).Trim();
                    result.DisplayText = $"Class: {result.ClassName}";
                }
                // Parse property lines (format: property = value)
                else if (line.Contains("="))
                {
                    var idx = line.IndexOf('=');
                    var propName = line.Substring(0, idx).Trim().TrimStart(':');
                    var propValue = line.Substring(idx + 1).Trim();
                    result.Properties[propName] = propValue;
                }

                result.Raw += line + Environment.NewLine;
            }

            if (string.IsNullOrEmpty(result.DisplayText))
            {
                result.DisplayText = $"Class: {ClassName}";
            }

            return new List<ObjectListEntry> { result };
        }

        private List<ObjectListEntry> ParseInstancesResponse(IEnumerable<string> responses)
        {
            var results = new List<ObjectListEntry>();
            var regex = new Regex(@"OBJECT\s+(\d+)", RegexOptions.IgnoreCase);

            foreach (var resp in responses)
            {
                if (string.IsNullOrWhiteSpace(resp)) continue;

                // Skip command echo lines
                if (resp.StartsWith(">", StringComparison.Ordinal)) continue;

                foreach (Match match in regex.Matches(resp))
                {
                    var objectId = match.Groups[1].Value;
                    var entry = new ObjectListEntry
                    {
                        ObjectId = objectId,
                        ClassName = ClassName,
                        DisplayText = $"Object {objectId}",
                        Raw = resp
                    };
                    results.Add(entry);
                }
            }

            return results;
        }

        private List<ObjectListEntry> ParseAllResponse(IEnumerable<string> responses)
        {
            // "show all" typically returns instances first, then properties
            // Try to parse as instances response
            var entries = ParseInstancesResponse(responses);

            // If no instances found, try parsing as class response
            if (entries.Count == 0)
            {
                entries = ParseClassResponse(responses);
            }

            return entries;
        }

        private List<ObjectListEntry> ParseListResponse(IEnumerable<string> responses)
        {
            var entries = new List<ObjectListEntry>();
            foreach (var line in responses)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (line.StartsWith(">", StringComparison.Ordinal)) continue;

                entries.Add(new ObjectListEntry
                {
                    DisplayText = line.Trim(),
                    ClassName = ClassName,
                    Raw = line
                });
            }

            return entries;
        }

        private void UpdateFilteredResults()
        {
            FilteredResults.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                foreach (var item in Results)
                {
                    FilteredResults.Add(item);
                }
            }
            else
            {
                var searchLower = SearchText.ToLower();
                foreach (var item in Results)
                {
                    if (item.DisplayText.ToLower().Contains(searchLower) ||
                        (item.ObjectId?.ToLower().Contains(searchLower) ?? false) ||
                        item.ClassName.ToLower().Contains(searchLower))
                    {
                        FilteredResults.Add(item);
                    }
                }
            }

            StatusMessage = string.Format(LocalizationService.Instance.GetString("ListReader_Message_ResultsDisplayed"), FilteredResults.Count);
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
            RunOnUi(() =>
            {
                LastResponse = response;
            });
        }

        private void Log(string message)
        {
            RunOnUi(() =>
            {
                StatusMessage = message;
            });
        }

        private void RunOnUi(Action action)
        {
            Application.Current?.Dispatcher.Invoke(action);
        }

        private async Task LoadSystemClassesAsync()
        {
            var service = new SystemKodService();
            var classes = await Task.Run(service.LoadClassNames);

            RunOnUi(() =>
            {
                SystemClasses.Clear();
                foreach (var name in classes)
                {
                    SystemClasses.Add(name);
                }
            });
        }

        private async Task LoadImportantListsAsync()
        {
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "important_lists.json");
            if (!System.IO.File.Exists(path))
                return;

            try
            {
                var json = await System.IO.File.ReadAllTextAsync(path);
                var lists = JsonSerializer.Deserialize<List<ImportantListOption>>(json) ?? new List<ImportantListOption>();
                RunOnUi(() =>
                {
                    ImportantLists.Clear();
                    foreach (var entry in lists)
                    {
                        ImportantLists.Add(entry);
                    }
                });
            }
            catch
            {
                // Ignore malformed list config.
            }
        }
    }

    public record CommandTypeOption(string DisplayName, string CommandName);
    public record ImportantListOption(string Name, string ListId)
    {
        public string DisplayName => string.IsNullOrWhiteSpace(ListId) ? Name : $"{Name} ({ListId})";
    }
}

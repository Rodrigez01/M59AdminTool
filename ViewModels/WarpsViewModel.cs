using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M59AdminTool.Models;
using M59AdminTool.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using System.Linq;
using System.Collections.Generic;

namespace M59AdminTool.ViewModels
{
    public partial class WarpsViewModel : ObservableObject
    {
        private readonly WarpsDataService _dataService;
        private readonly M59ClientService _clientService;

        [ObservableProperty]
        private ObservableCollection<WarpCategory> _warpCategories = new();

        [ObservableProperty]
        private WarpLocation? _selectedWarp;

        [ObservableProperty]
        private WarpCategory? _selectedCategory;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<WarpCategoryView> _filteredWarpCategories = new();

        [ObservableProperty]
        private int _filteredWarpsCount;

        [ObservableProperty]
        private int _totalWarpsCount;

        // For editing
        [ObservableProperty]
        private string _editName = string.Empty;

        [ObservableProperty]
        private string _editRoomId = string.Empty;

        [ObservableProperty]
        private string _editX = string.Empty;

        [ObservableProperty]
        private string _editY = string.Empty;

        [ObservableProperty]
        private string _editDescription = string.Empty;

        [ObservableProperty]
        private string _editCategory = string.Empty;

        public WarpsViewModel()
        {
            _dataService = new WarpsDataService();
            _clientService = new M59ClientService();
            _ = LoadWarpsAsync();
        }

        private async Task LoadWarpsAsync()
        {
            WarpCategories = await _dataService.LoadWarpsAsync();
            FilterWarps();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterWarps();
        }

        partial void OnSelectedWarpChanged(WarpLocation? value)
        {
            if (value != null)
            {
                EditName = value.Name;
                EditRoomId = value.RoomId ?? string.Empty;
                EditX = value.X?.ToString() ?? string.Empty;
                EditY = value.Y?.ToString() ?? string.Empty;
                EditDescription = value.Description ?? string.Empty;
                EditCategory = value.Category;
            }
        }

        [RelayCommand]
        private void AddWarp()
        {
            if (SelectedCategory == null)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(loc.GetString("Message_CategoryRequired"), loc.GetString("Title_CategoryRequired"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var localization = LocalizationService.Instance;
            var newWarp = new WarpLocation
            {
                Name = localization.GetString("Default_WarpName"),
                Category = SelectedCategory.Name,
                RoomId = "RID_",
                X = 0,
                Y = 0,
                Description = localization.GetString("Default_WarpDescription")
            };

            SelectedCategory.Locations.Add(newWarp);
            SelectedWarp = newWarp;
            FilterWarps();
        }

        [RelayCommand]
        private void RemoveWarp()
        {
            if (SelectedWarp == null) return;

            var category = WarpCategories.FirstOrDefault(c => c.Name == SelectedWarp.Category);
            if (category != null)
            {
                var loc = LocalizationService.Instance;
                var result = System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_DeleteWarp"), SelectedWarp.Name),
                    loc.GetString("Title_DeleteWarp"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    category.Locations.Remove(SelectedWarp);
                    SelectedWarp = null;
                    FilterWarps();
                }
            }
        }

        [RelayCommand]
        private void SaveWarp()
        {
            if (SelectedWarp == null) return;

            SelectedWarp.Name = EditName;
            SelectedWarp.RoomId = EditRoomId;
            SelectedWarp.X = int.TryParse(EditX, out var x) ? x : null;
            SelectedWarp.Y = int.TryParse(EditY, out var y) ? y : null;
            SelectedWarp.Description = EditDescription;

            // Trigger UI update
            OnPropertyChanged(nameof(SelectedWarp));
            FilterWarps();

            var loc = LocalizationService.Instance;
            System.Windows.MessageBox.Show(loc.GetString("Message_WarpSaved"), loc.GetString("Title_Success"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private void AddCategory()
        {
            var loc = LocalizationService.Instance;
            var newCategory = new WarpCategory
            {
                Name = string.Format(loc.GetString("Default_CategoryName"), WarpCategories.Count + 1),
                IsExpanded = true
            };

            WarpCategories.Add(newCategory);
            SelectedCategory = newCategory;
            FilterWarps();
        }

        [RelayCommand]
        private void RemoveCategory()
        {
            if (SelectedCategory == null) return;

            if (SelectedCategory.Locations.Count > 0)
            {
                var loc = LocalizationService.Instance;
                var result = System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_CategoryDeleteConfirm"), SelectedCategory.Name, SelectedCategory.Locations.Count),
                    loc.GetString("Title_DeleteCategory"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            WarpCategories.Remove(SelectedCategory);
            SelectedCategory = null;
            FilterWarps();
        }

        [RelayCommand]
        private async Task ExecuteWarp()
        {
            if (SelectedWarp == null) return;

            // Check if client is running
            if (!_clientService.IsClientRunning())
            {
                var loc = LocalizationService.Instance;
                var result = System.Windows.MessageBox.Show(
                    loc.GetString("Message_ClientNotRunning"),
                    loc.GetString("Title_ClientNotFound"),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    var command = $"go {SelectedWarp.RoomId}";
                    System.Windows.Clipboard.SetText(command);
                    System.Windows.MessageBox.Show(
                        string.Format(loc.GetString("Message_CommandCopied"), command),
                        loc.GetString("Title_CommandCopied"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                return;
            }

            // M59 Warp Command: go RID_LOCATION
            // aus blakston.khd (z.B. RID_TOS, RID_MARION, etc.)
            var warpCommand = $"go {SelectedWarp.RoomId}";

            // Send command to client
            await _clientService.SendCommandAsync(warpCommand);
        }

        [RelayCommand]
        private async Task SaveAllWarps()
        {
            await _dataService.SaveWarpsAsync(WarpCategories);
            var loc = LocalizationService.Instance;
            System.Windows.MessageBox.Show(loc.GetString("Message_WarpsSaved"), loc.GetString("Title_Saved"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task ReloadWarps()
        {
            var loc = LocalizationService.Instance;
            var result = System.Windows.MessageBox.Show(
                loc.GetString("Message_ReloadWarpsConfirm"),
                loc.GetString("Title_ReloadWarps"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await LoadWarpsAsync();
                System.Windows.MessageBox.Show(loc.GetString("Message_WarpsReloaded"), loc.GetString("Title_Success"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private async Task RefreshRooms()
        {
            var loc = LocalizationService.Instance;
            var result = await _dataService.RefreshExtractedRoomsAsync();
            if (!result.Success)
            {
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_RefreshRoomsFailed"), result.Error ?? "Unknown error"),
                    loc.GetString("Title_RefreshRooms"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            await LoadWarpsAsync();
            System.Windows.MessageBox.Show(loc.GetString("Message_RefreshRoomsOk"), loc.GetString("Title_RefreshRooms"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Refreshes display names for all warps (for language changes)
        /// </summary>
        public void RefreshDisplayNames()
        {
            System.Diagnostics.Debug.WriteLine($"RefreshDisplayNames called. Categories: {WarpCategories.Count}");
            int warpCount = 0;
            foreach (var category in WarpCategories)
            {
                foreach (var warp in category.Locations)
                {
                    warpCount++;
                    warp.RefreshDisplayName();
                }
            }
            System.Diagnostics.Debug.WriteLine($"Refreshed {warpCount} warp display names");
            FilterWarps();
        }

        [RelayCommand]
        private async Task ExportWarps()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON Dateien (*.json)|*.json",
                DefaultExt = ".json",
                FileName = "warps_export.json"
            };

            if (dialog.ShowDialog() == true)
            {
                await _dataService.ExportWarpsAsync(dialog.FileName, WarpCategories);
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(string.Format(loc.GetString("Message_WarpsExported"), dialog.FileName), loc.GetString("Title_ExportSuccess"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private async Task ImportWarps()
        {
            var loc = LocalizationService.Instance;
            var result = System.Windows.MessageBox.Show(
                loc.GetString("Message_WarpsImportConfirm"),
                loc.GetString("Title_ImportWarps"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "JSON Dateien (*.json)|*.json",
                DefaultExt = ".json"
            };

            if (dialog.ShowDialog() == true)
            {
                var imported = await _dataService.ImportWarpsAsync(dialog.FileName);
                if (imported.Count > 0)
                {
                    WarpCategories = imported;
                    FilterWarps();
                    System.Windows.MessageBox.Show(string.Format(loc.GetString("Message_WarpsImported"), imported.Sum(c => c.Locations.Count)), loc.GetString("Title_ImportSuccess"),
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void FilterWarps()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredWarpCategories = new ObservableCollection<WarpCategoryView>(
                    WarpCategories.Select(category => new WarpCategoryView(category, category.Locations)));
                UpdateCounts();
                return;
            }

            var searchLower = SearchText.ToLower();
            var filtered = new List<WarpCategoryView>();

            foreach (var category in WarpCategories)
            {
                var categoryMatches = category.Name.ToLower().Contains(searchLower);
                var matchingLocations = category.Locations.Where(warp =>
                    (warp.DisplayName?.ToLower().Contains(searchLower) ?? false) ||
                    (warp.Name?.ToLower().Contains(searchLower) ?? false) ||
                    (warp.NameDe?.ToLower().Contains(searchLower) ?? false) ||
                    (warp.RoomId?.ToLower().Contains(searchLower) ?? false) ||
                    (warp.Description?.ToLower().Contains(searchLower) ?? false)
                ).ToList();

                if (categoryMatches || matchingLocations.Count > 0)
                {
                    var locationsToShow = categoryMatches && matchingLocations.Count == 0
                        ? category.Locations
                        : new ObservableCollection<WarpLocation>(matchingLocations);

                    filtered.Add(new WarpCategoryView(category, locationsToShow));
                }
            }

            FilteredWarpCategories = new ObservableCollection<WarpCategoryView>(filtered);
            UpdateCounts();
        }

        private void UpdateCounts()
        {
            TotalWarpsCount = WarpCategories.Sum(category => category.Locations.Count);
            FilteredWarpsCount = FilteredWarpCategories.Sum(category => category.Locations.Count);
        }
    }
}

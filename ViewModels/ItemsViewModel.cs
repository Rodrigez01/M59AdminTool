using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M59AdminTool.Models;
using M59AdminTool.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace M59AdminTool.ViewModels
{
    public partial class ItemsViewModel : ObservableObject
    {
        private readonly ItemsDataService _dataService;
        private readonly M59ClientService _clientService;

        [ObservableProperty]
        private ObservableCollection<Item> _items = new();

        [ObservableProperty]
        private Item? _selectedItem;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Item> _filteredItems = new();

        [ObservableProperty]
        private int _filteredItemsCount;

        [ObservableProperty]
        private int _totalItemsCount;

        [ObservableProperty]
        private string _editClassName = string.Empty;

        [ObservableProperty]
        private string _editEnglishName = string.Empty;

        [ObservableProperty]
        private string? _editGermanName;

        [ObservableProperty]
        private string _editDmCommand = string.Empty;

        [ObservableProperty]
        private string? _editCategory;

        private int _spawnQuantity = 1;

        public int SpawnQuantity
        {
            get => _spawnQuantity;
            set => SetProperty(ref _spawnQuantity, Math.Max(1, value));
        }

        public ItemsViewModel()
        {
            _dataService = new ItemsDataService();
            _clientService = new M59ClientService();
            _ = LoadItemsAsync();
        }

        private async Task LoadItemsAsync()
        {
            Items = await _dataService.LoadItemsAsync();
            Items = new ObservableCollection<Item>(Items.OrderBy(i => i.Category).ThenBy(i => i.DisplayName));
            FilterItems();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterItems();
        }

        partial void OnSelectedItemChanged(Item? value)
        {
            if (value != null)
            {
                EditClassName = value.ClassName;
                EditEnglishName = value.EnglishName;
                EditGermanName = value.GermanName;
                EditDmCommand = value.DmCommand;
                EditCategory = value.Category;
            }
        }

        private void FilterItems()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredItems = new ObservableCollection<Item>(Items);
                UpdateCounts();
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = Items.Where(i =>
                    i.DisplayName.ToLower().Contains(searchLower) ||
                    i.ClassName.ToLower().Contains(searchLower) ||
                    (i.EnglishName?.ToLower().Contains(searchLower) ?? false) ||
                    (i.GermanName?.ToLower().Contains(searchLower) ?? false) ||
                    (i.Category?.ToLower().Contains(searchLower) ?? false)
                ).ToList();

                FilteredItems = new ObservableCollection<Item>(filtered.OrderBy(i => i.Category).ThenBy(i => i.DisplayName));
                UpdateCounts();
            }
        }

        private void UpdateCounts()
        {
            TotalItemsCount = Items.Count;
            FilteredItemsCount = FilteredItems.Count;
        }

        [RelayCommand]
        private async Task SpawnItem()
        {
            if (SelectedItem == null)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(loc.GetString("Message_SelectItemFirst"), loc.GetString("Title_NoItemSelected"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int quantity = Math.Max(1, SpawnQuantity);

            for (int i = 0; i < quantity; i++)
            {
                await _clientService.SendCommandAsync(SelectedItem.DmCommand);
            }
        }

        [RelayCommand]
        private void AddItem()
        {
            var loc = LocalizationService.Instance;
            var newItem = new Item
            {
                ClassName = loc.GetString("Default_ItemClassName"),
                EnglishName = loc.GetString("Default_ItemEnglishName"),
                GermanName = null,
                DmCommand = loc.GetString("Default_ItemDmCommand")
            };

            Items.Add(newItem);
            FilterItems();
            SelectedItem = newItem;
        }

        [RelayCommand]
        private void RemoveItem()
        {
            if (SelectedItem == null)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(loc.GetString("Message_SelectItemFirst"), loc.GetString("Title_NoItemSelected"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var locDelete = LocalizationService.Instance;
            var result = System.Windows.MessageBox.Show(
                string.Format(locDelete.GetString("Message_DeleteItem"), SelectedItem.DisplayName),
                locDelete.GetString("Title_DeleteItem"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Items.Remove(SelectedItem);
                FilterItems();
                SelectedItem = null;
            }
        }

        [RelayCommand]
        private void SaveItem()
        {
            if (SelectedItem == null)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(loc.GetString("Message_SelectItemFirst"), loc.GetString("Title_NoItemSelected"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update the selected item with edited values
            SelectedItem.ClassName = EditClassName;
            SelectedItem.EnglishName = EditEnglishName;
            SelectedItem.GermanName = EditGermanName;
            SelectedItem.DmCommand = EditDmCommand;
            SelectedItem.Category = EditCategory;

            // Refresh the filter to update the display
            FilterItems();

            var locSaved = LocalizationService.Instance;
            System.Windows.MessageBox.Show(locSaved.GetString("Message_ItemSaved"), locSaved.GetString("Title_Saved"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task SaveAllItems()
        {
            await _dataService.SaveItemsAsync(Items);
            var loc = LocalizationService.Instance;
            System.Windows.MessageBox.Show(loc.GetString("Message_ItemsSaved"), loc.GetString("Title_Saved"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task ReloadItems()
        {
            var loc = LocalizationService.Instance;
            var result = System.Windows.MessageBox.Show(
                loc.GetString("Message_ReloadItemsConfirm"),
                loc.GetString("Title_ReloadItems"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await LoadItemsAsync();
                System.Windows.MessageBox.Show(loc.GetString("Message_ItemsReloaded"), loc.GetString("Title_Success"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private async Task RefreshItems()
        {
            var loc = LocalizationService.Instance;
            var result = await _dataService.RefreshItemsAsync();
            if (!result.Success)
            {
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_RefreshItemsFailed"), result.Error ?? "Unknown error"),
                    loc.GetString("Title_RefreshItems"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            await LoadItemsAsync();
            System.Windows.MessageBox.Show(loc.GetString("Message_RefreshItemsOk"), loc.GetString("Title_RefreshItems"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Refreshes display names for all items (for language changes)
        /// </summary>
        public void RefreshDisplayNames()
        {
            System.Diagnostics.Debug.WriteLine($"RefreshDisplayNames called for items. Count: {Items.Count}");
            foreach (var item in Items)
            {
                item.RefreshDisplayName();
            }
            // Re-filter to update the filtered list
            FilterItems();
            System.Diagnostics.Debug.WriteLine($"Refreshed {Items.Count} item display names");
        }
    }
}

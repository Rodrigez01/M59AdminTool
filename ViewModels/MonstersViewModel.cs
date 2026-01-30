using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M59AdminTool.Models;
using M59AdminTool.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace M59AdminTool.ViewModels
{
    public partial class MonstersViewModel : ObservableObject
    {
        private readonly MonstersDataService _dataService;
        private readonly M59ClientService _clientService;

        [ObservableProperty]
        private ObservableCollection<Monster> _monsters = new();

        [ObservableProperty]
        private Monster? _selectedMonster;

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private ObservableCollection<Monster> _filteredMonsters = new();

        [ObservableProperty]
        private int _filteredMonstersCount;

        [ObservableProperty]
        private int _totalMonstersCount;

        [ObservableProperty]
        private string _editClassName = string.Empty;

        [ObservableProperty]
        private string _editEnglishName = string.Empty;

        [ObservableProperty]
        private string? _editGermanName;

        [ObservableProperty]
        private string _editDmCommand = string.Empty;

        private int _spawnQuantity = 1;

        public int SpawnQuantity
        {
            get => _spawnQuantity;
            set => SetProperty(ref _spawnQuantity, Math.Max(1, value));
        }

        public MonstersViewModel()
        {
            _dataService = new MonstersDataService();
            _clientService = new M59ClientService();
            _ = LoadMonstersAsync();
        }

        private async Task LoadMonstersAsync()
        {
            Monsters = await _dataService.LoadMonstersAsync();
            FilterMonsters();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterMonsters();
        }

        partial void OnSelectedMonsterChanged(Monster? value)
        {
            if (value != null)
            {
                EditClassName = value.ClassName;
                EditEnglishName = value.EnglishName;
                EditGermanName = value.GermanName;
                EditDmCommand = value.DmCommand;
            }
        }

        private void FilterMonsters()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredMonsters = new ObservableCollection<Monster>(Monsters);
                UpdateCounts();
            }
            else
            {
                var searchLower = SearchText.ToLower();
                var filtered = Monsters.Where(m =>
                    m.DisplayName.ToLower().Contains(searchLower) ||
                    m.ClassName.ToLower().Contains(searchLower) ||
                    (m.EnglishName?.ToLower().Contains(searchLower) ?? false) ||
                    (m.GermanName?.ToLower().Contains(searchLower) ?? false)
                ).ToList();

                FilteredMonsters = new ObservableCollection<Monster>(filtered);
                UpdateCounts();
            }
        }

        private void UpdateCounts()
        {
            TotalMonstersCount = Monsters.Count;
            FilteredMonstersCount = FilteredMonsters.Count;
        }

        [RelayCommand]
        private async Task SpawnMonster()
        {
            if (SelectedMonster == null)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(loc.GetString("Message_SelectMonsterFirst"), loc.GetString("Title_NoMonsterSelected"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int quantity = Math.Max(1, SpawnQuantity);

            // Send DM command to M59 client for each quantity
            for (int i = 0; i < quantity; i++)
            {
                await _clientService.SendCommandAsync(SelectedMonster.DmCommand);
            }
        }

        [RelayCommand]
        private void AddMonster()
        {
            var loc = LocalizationService.Instance;
            var newMonster = new Monster
            {
                ClassName = loc.GetString("Default_MonsterClassName"),
                EnglishName = loc.GetString("Default_MonsterEnglishName"),
                GermanName = null,
                DmCommand = loc.GetString("Default_MonsterDmCommand")
            };

            Monsters.Add(newMonster);
            FilterMonsters();
            SelectedMonster = newMonster;
        }

        [RelayCommand]
        private void RemoveMonster()
        {
            if (SelectedMonster == null)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(loc.GetString("Message_SelectMonsterFirst"), loc.GetString("Title_NoMonsterSelected"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var locDelete = LocalizationService.Instance;
            var result = System.Windows.MessageBox.Show(
                string.Format(locDelete.GetString("Message_DeleteMonster"), SelectedMonster.DisplayName),
                locDelete.GetString("Title_DeleteMonster"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Monsters.Remove(SelectedMonster);
                FilterMonsters();
                SelectedMonster = null;
            }
        }

        [RelayCommand]
        private void SaveMonster()
        {
            if (SelectedMonster == null)
            {
                var loc = LocalizationService.Instance;
                System.Windows.MessageBox.Show(loc.GetString("Message_SelectMonsterFirst"), loc.GetString("Title_NoMonsterSelected"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Update the selected monster with edited values
            SelectedMonster.ClassName = EditClassName;
            SelectedMonster.EnglishName = EditEnglishName;
            SelectedMonster.GermanName = EditGermanName;
            SelectedMonster.DmCommand = EditDmCommand;

            // Refresh the filter to update the display
            FilterMonsters();

            var locSaved = LocalizationService.Instance;
            System.Windows.MessageBox.Show(locSaved.GetString("Message_MonsterSaved"), locSaved.GetString("Title_Saved"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task SaveAllMonsters()
        {
            await _dataService.SaveMonstersAsync(Monsters);
            var loc = LocalizationService.Instance;
            System.Windows.MessageBox.Show(loc.GetString("Message_MonstersSaved"), loc.GetString("Title_Saved"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        [RelayCommand]
        private async Task ReloadMonsters()
        {
            var loc = LocalizationService.Instance;
            var result = System.Windows.MessageBox.Show(
                loc.GetString("Message_ReloadMonstersConfirm"),
                loc.GetString("Title_ReloadMonsters"),
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await LoadMonstersAsync();
                System.Windows.MessageBox.Show(loc.GetString("Message_MonstersReloaded"), loc.GetString("Title_Success"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        [RelayCommand]
        private async Task RefreshMonsters()
        {
            var loc = LocalizationService.Instance;
            var result = await _dataService.RefreshMonstersAsync();
            if (!result.Success)
            {
                System.Windows.MessageBox.Show(
                    string.Format(loc.GetString("Message_RefreshMonstersFailed"), result.Error ?? "Unknown error"),
                    loc.GetString("Title_RefreshMonsters"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            await LoadMonstersAsync();
            System.Windows.MessageBox.Show(loc.GetString("Message_RefreshMonstersOk"), loc.GetString("Title_RefreshMonsters"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Refreshes display names for all monsters (for language changes)
        /// </summary>
        public void RefreshDisplayNames()
        {
            System.Diagnostics.Debug.WriteLine($"RefreshDisplayNames called for monsters. Count: {Monsters.Count}");
            foreach (var monster in Monsters)
            {
                monster.RefreshDisplayName();
            }
            // Re-filter to update the filtered list
            FilterMonsters();
            System.Diagnostics.Debug.WriteLine($"Refreshed {Monsters.Count} monster display names");
        }
    }
}

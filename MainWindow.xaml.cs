using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using M59AdminTool.ViewModels;
using M59AdminTool.Models;
using M59AdminTool.Services;
using Meridian59EventManager;

namespace M59AdminTool;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly WarpsViewModel _warpsViewModel;
    private readonly MonstersViewModel _monstersViewModel;
    private readonly ItemsViewModel _itemsViewModel;
    private readonly DmCommandsViewModel _dmCommandsViewModel;
    private readonly AdminCommandsViewModel _adminCommandsViewModel;
    private readonly ConnectionViewModel _connectionViewModel;
    private readonly PlayersViewModel _playersViewModel;
    private readonly ListReaderViewModel _listReaderViewModel;
    private readonly DeepObjectInspectorViewModel _deepObjectInspectorViewModel;
    private readonly BGFConverterViewModel _bgfConverterViewModel;
    private readonly M59ServerConnectionTransport _sharedEventManagerTransport;
    private readonly LocalizationService _localization;
    private WindowsFormsHost? _eventManagerHost;
    private MainForm? _eventManagerForm;

    public MainWindow()
    {
        InitializeComponent();

        // Initialize Services
        _localization = LocalizationService.Instance;
        _localization.LanguageChanged += OnLanguageChanged;

        // Initialize ViewModels
        _connectionViewModel = new ConnectionViewModel();
        _warpsViewModel = new WarpsViewModel();
        _monstersViewModel = new MonstersViewModel();
        _itemsViewModel = new ItemsViewModel();
        _dmCommandsViewModel = new DmCommandsViewModel();
        _adminCommandsViewModel = new AdminCommandsViewModel(_connectionViewModel);
        _playersViewModel = new PlayersViewModel(_connectionViewModel, _adminCommandsViewModel);
        _listReaderViewModel = new ListReaderViewModel(_connectionViewModel);
        _deepObjectInspectorViewModel = new DeepObjectInspectorViewModel(_connectionViewModel);
        _bgfConverterViewModel = new BGFConverterViewModel();
        _sharedEventManagerTransport = new M59ServerConnectionTransport();

        _connectionViewModel.PropertyChanged += ConnectionViewModel_PropertyChanged;

        // Set DataContext for tabs
        SetTabDataContext();

        InitializeEventManagerHost();
        _ = RefreshSharedConnectionStateAsync();

    }

    private void SetTabDataContext()
    {
        // Find Connection tab and set DataContext
        if (FindName("ConnectionTab") is TabItem connectionTab)
        {
            connectionTab.DataContext = new { ConnectionVM = _connectionViewModel };
        }

        // Find Warps tab and set DataContext
        if (FindName("WarpsTab") is TabItem warpsTab)
        {
            warpsTab.DataContext = _warpsViewModel;
        }

        // Find Monsters tab and set DataContext
        if (FindName("MonstersTab") is TabItem monstersTab)
        {
            monstersTab.DataContext = _monstersViewModel;
        }

        // Find Items tab and set DataContext
        if (FindName("ItemsTab") is TabItem itemsTab)
        {
            itemsTab.DataContext = _itemsViewModel;
        }

        // Find DM tab and set DataContext
        if (FindName("DmTab") is TabItem dmTab)
        {
            dmTab.DataContext = _dmCommandsViewModel;
        }

        // Find Admin tab and set DataContext
        if (FindName("AdminTab") is TabItem adminTab)
        {
            adminTab.DataContext = _adminCommandsViewModel;
        }

        // Find Admin Console tab and set DataContext
        if (FindName("AdminConsoleTab") is TabItem adminConsoleTab)
        {
            adminConsoleTab.DataContext = _adminCommandsViewModel;
        }

        // Event Manager
        // Players tab
        if (FindName("PlayersTab") is TabItem playersTab)
        {
            playersTab.DataContext = _playersViewModel;
        }

        // List Reader tab
        if (FindName("ListReaderTab") is TabItem listReaderTab)
        {
            listReaderTab.DataContext = _listReaderViewModel;
        }

        // Deep Object Inspector tab
        if (FindName("DeepObjectInspectorTab") is TabItem deepInspectorTab)
        {
            deepInspectorTab.DataContext = _deepObjectInspectorViewModel;
        }

        // BGF Converter tab
        if (FindName("BGFConverterTab") is TabItem bgfConverterTab)
        {
            bgfConverterTab.DataContext = _bgfConverterViewModel;
        }
    }

    private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is TreeViewItem item)
        {
            if (item.DataContext is WarpLocation warp)
            {
                _warpsViewModel.SelectedWarp = warp;
            }
            else if (item.DataContext is WarpCategory category)
            {
                _warpsViewModel.SelectedCategory = category;
            }
            else if (item.DataContext is WarpCategoryView categoryView)
            {
                _warpsViewModel.SelectedCategory = categoryView.Source;
            }
        }
    }

    private void TreeView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"TreeView_MouseDoubleClick fired! OriginalSource: {e.OriginalSource?.GetType().Name}");

        // Get the clicked element
        var treeView = sender as System.Windows.Controls.TreeView;
        if (treeView == null)
        {
            System.Diagnostics.Debug.WriteLine("Sender is not TreeView");
            return;
        }

        // Check what was actually clicked
        var item = e.OriginalSource as FrameworkElement;
        if (item == null)
        {
            System.Diagnostics.Debug.WriteLine("OriginalSource is not FrameworkElement");
            return;
        }

        // Find the data context (should be WarpLocation)
        var dataContext = item.DataContext;
        System.Diagnostics.Debug.WriteLine($"DataContext type: {dataContext?.GetType().Name}");

        // Only execute if it's a WarpLocation (not a Category)
        if (dataContext is WarpLocation warp)
        {
            System.Diagnostics.Debug.WriteLine($"Executing warp: {warp.Name}");

            // Make sure it's selected
            _warpsViewModel.SelectedWarp = warp;

            // Execute the warp
            _warpsViewModel.ExecuteWarpCommand.Execute(null);

            // Mark as handled so it doesn't bubble up
            e.Handled = true;

            System.Diagnostics.Debug.WriteLine("Warp executed and event marked as handled");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"DataContext is not WarpLocation, it's: {dataContext?.GetType().Name ?? "null"}");
        }
    }

    private void DeepInspectorHistory_DoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not System.Windows.Controls.ListBox listBox)
            return;

        if (listBox.SelectedItem is not string entry)
            return;

        if (_deepObjectInspectorViewModel.NavigateToHistoryEntryCommand.CanExecute(entry))
        {
            _deepObjectInspectorViewModel.NavigateToHistoryEntryCommand.Execute(entry);
        }
    }

    private void BtnLanguage_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Language button clicked. Current: {_localization.CurrentLanguage}");

        // Toggle language
        _localization.CurrentLanguage = _localization.CurrentLanguage == Services.Language.German
            ? Services.Language.English
            : Services.Language.German;

        System.Diagnostics.Debug.WriteLine($"Language changed to: {_localization.CurrentLanguage}");
    }

    private void OnLanguageChanged(object? sender, Services.Language newLanguage)
    {
        System.Diagnostics.Debug.WriteLine($"OnLanguageChanged event fired. New language: {newLanguage}");
        // Update displayed names for all warps, monsters and items
        _warpsViewModel.RefreshDisplayNames();
        _monstersViewModel.RefreshDisplayNames();
        _itemsViewModel.RefreshDisplayNames();
        System.Diagnostics.Debug.WriteLine("RefreshDisplayNames completed");
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.PasswordBox passwordBox)
        {
            _connectionViewModel.Password = passwordBox.Password;
        }
    }

    private void MonsterListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"MonsterListBox_MouseDoubleClick fired! OriginalSource: {e.OriginalSource?.GetType().Name}");

        // Get the clicked element
        var listBox = sender as System.Windows.Controls.ListBox;
        if (listBox == null)
        {
            System.Diagnostics.Debug.WriteLine("Sender is not ListBox");
            return;
        }

        // Check what was actually clicked
        var item = e.OriginalSource as FrameworkElement;
        if (item == null)
        {
            System.Diagnostics.Debug.WriteLine("OriginalSource is not FrameworkElement");
            return;
        }

        // Find the data context (should be Monster)
        var dataContext = item.DataContext;
        System.Diagnostics.Debug.WriteLine($"DataContext type: {dataContext?.GetType().Name}");

        // Only execute if it's a Monster
        if (dataContext is Monster monster)
        {
            System.Diagnostics.Debug.WriteLine($"Executing monster spawn: {monster.ClassName}");

            // Make sure it's selected
            _monstersViewModel.SelectedMonster = monster;

            // Execute the spawn command
            _monstersViewModel.SpawnMonsterCommand.Execute(null);

            // Mark as handled so it doesn't bubble up
            e.Handled = true;

            System.Diagnostics.Debug.WriteLine("Monster spawn executed and event marked as handled");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"DataContext is not Monster, it's: {dataContext?.GetType().Name ?? "null"}");
        }
    }

    private void ItemListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"ItemListBox_MouseDoubleClick fired! OriginalSource: {e.OriginalSource?.GetType().Name}");

        // Get the clicked element
        var listBox = sender as System.Windows.Controls.ListBox;
        if (listBox == null)
        {
            System.Diagnostics.Debug.WriteLine("Sender is not ListBox");
            return;
        }

        // Check what was actually clicked
        var item = e.OriginalSource as FrameworkElement;
        if (item == null)
        {
            System.Diagnostics.Debug.WriteLine("OriginalSource is not FrameworkElement");
            return;
        }

        // Find the data context (should be Item)
        var dataContext = item.DataContext;
        System.Diagnostics.Debug.WriteLine($"DataContext type: {dataContext?.GetType().Name}");

        // Only execute if it's an Item
        if (dataContext is Item itemData)
        {
            System.Diagnostics.Debug.WriteLine($"Executing item spawn: {itemData.ClassName}");

            // Make sure it's selected
            _itemsViewModel.SelectedItem = itemData;

            // Execute the spawn command
            _itemsViewModel.SpawnItemCommand.Execute(null);

            // Mark as handled so it doesn't bubble up
            e.Handled = true;

            System.Diagnostics.Debug.WriteLine("Item spawn executed and event marked as handled");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"DataContext is not Item, it's: {dataContext?.GetType().Name ?? "null"}");
        }
    }

        private async void PlayersList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ListBox list && list.SelectedItem is Models.PlayerEntry entry)
            {
                _playersViewModel.SelectedPlayer = entry;
                _playersViewModel.ShowSelectedPlayerCommand.Execute(entry);
            }
        }

    private async void PlayerDetails_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.ListBox list && list.SelectedItem is string line)
        {
            await _playersViewModel.EditDetailLineAsync(line);
        }
    }

    private void ListReaderDetails_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.C &&
            (System.Windows.Input.Keyboard.Modifiers & System.Windows.Input.ModifierKeys.Control) == System.Windows.Input.ModifierKeys.Control)
        {
            if (sender is System.Windows.Controls.ListBox listBox)
            {
                CopyListBoxItems(listBox, selectedOnly: true);
                e.Handled = true;
            }
        }
    }

    private void InitializeEventManagerHost()
    {
        if (FindName("EventManagerHost") is WindowsFormsHost host)
        {
            _eventManagerHost = host;
            var form = new MainForm(_sharedEventManagerTransport)
            {
                TopLevel = false,
                FormBorderStyle = System.Windows.Forms.FormBorderStyle.None,
                Dock = DockStyle.Fill
            };
            host.Child = form;
            _eventManagerForm = form;
            form.Show();
        }
    }

    private async Task RefreshSharedConnectionStateAsync()
    {
        if (_connectionViewModel.IsConnected)
        {
            _sharedEventManagerTransport.AttachConnection(_connectionViewModel.ServerConnection);
            if (_eventManagerForm != null)
            {
                await _eventManagerForm.TryConnectWithSharedTransportAsync();
            }
        }
        else
        {
            _sharedEventManagerTransport.AttachConnection(null);
            _eventManagerForm?.HandleSharedConnectionLost();
        }
    }

    private void ConnectionViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ConnectionViewModel.IsConnected))
        {
            _ = Dispatcher.InvokeAsync(async () => await RefreshSharedConnectionStateAsync());
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _eventManagerForm?.Dispose();
        base.OnClosed(e);
    }

    private void ListReaderDetails_CopySelected_Click(object sender, RoutedEventArgs e)
    {
        if (TryGetListBoxFromMenuSender(sender, out var listBox))
        {
            CopyListBoxItems(listBox, selectedOnly: true);
        }
    }

    private void ListReaderDetails_CopyAll_Click(object sender, RoutedEventArgs e)
    {
        if (TryGetListBoxFromMenuSender(sender, out var listBox))
        {
            CopyListBoxItems(listBox, selectedOnly: false);
        }
    }

    private async void ListReaderDetails_EditInt_Click(object sender, RoutedEventArgs e)
    {
        if (TryGetListBoxFromMenuSender(sender, out var listBox))
        {
            await _listReaderViewModel.EditIntFromDetailLineAsync(listBox.SelectedItem as string);
        }
    }

    private void ListReaderResults_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (_listReaderViewModel.ShowObjectDetailsCommand.CanExecute(null))
        {
            _listReaderViewModel.ShowObjectDetailsCommand.Execute(null);
        }
    }

    private async void ListReaderDetails_OpenList_Click(object sender, RoutedEventArgs e)
    {
        if (TryGetListBoxFromMenuSender(sender, out var listBox))
        {
            await OpenListFromDetailLineAsync(listBox.SelectedItem as string);
        }
    }

    private async void ListReaderDetails_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is System.Windows.Controls.ListBox listBox && listBox.SelectedItem is string line)
        {
            if (_listReaderViewModel.TryExtractListId(line, out _))
            {
                await OpenListFromDetailLineAsync(line);
            }
            else
            {
                await _listReaderViewModel.EditIntFromDetailLineAsync(line);
            }
        }
    }

    private async Task OpenListFromDetailLineAsync(string? line)
    {
        if (string.IsNullOrWhiteSpace(line))
            return;

        if (!_listReaderViewModel.TryExtractListId(line, out var listId))
        {
            _listReaderViewModel.StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_ListIdNotFound");
            return;
        }

        var entries = await _listReaderViewModel.FetchListEntriesAsync(listId);
        if (entries.Count == 0)
        {
            _listReaderViewModel.StatusMessage = LocalizationService.Instance.GetString("ListReader_Message_NoDetailsReceived");
            return;
        }

        var window = new Views.ListPreviewWindow(listId, entries)
        {
            Owner = this
        };
        window.Show();
        window.Activate();
    }

    private void PlayersList_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not System.Windows.Controls.ListBox listBox)
            return;

        var clicked = e.OriginalSource as DependencyObject;
        if (clicked == null)
            return;

        var item = FindAncestor<System.Windows.Controls.ListBoxItem>(clicked);
        if (item?.DataContext is Models.PlayerEntry entry)
        {
            listBox.SelectedItem = entry;
        }
    }

    private void PlayersOpenDeepInspector_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.MenuItem menuItem)
            return;

        if (menuItem.Parent is not System.Windows.Controls.ContextMenu menu ||
            menu.PlacementTarget is not System.Windows.Controls.ListBox listBox)
            return;

        if (listBox.SelectedItem is not Models.PlayerEntry entry)
            return;

        if (string.IsNullOrWhiteSpace(entry.ObjectId))
            return;

        if (FindName("DeepObjectInspectorTab") is TabItem deepInspectorTab)
        {
            deepInspectorTab.IsSelected = true;
        }

        _deepObjectInspectorViewModel.ObjectId = entry.ObjectId;
        if (_deepObjectInspectorViewModel.LoadObjectCommand.CanExecute(null))
        {
            _deepObjectInspectorViewModel.LoadObjectCommand.Execute(null);
        }
    }

    private void PlayerDetails_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        HandleListBoxMouseWheel(sender as System.Windows.Controls.ListBox, e);
    }

    private void ListBox_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        HandleListBoxMouseWheel(sender as System.Windows.Controls.ListBox, e);
    }

    private void HandleListBoxMouseWheel(System.Windows.Controls.ListBox? listBox, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (listBox == null)
            return;

        var scrollViewer = FindVisualChild<ScrollViewer>(listBox);
        if (scrollViewer == null)
            return;

        double offsetChange = e.Delta > 0 ? -1 : 1;
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + offsetChange * 3);
        e.Handled = true;
    }

    private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
    {
        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);
            if (child is T typed)
                return typed;

            T? result = FindVisualChild<T>(child);
            if (result is not null)
                return result;
        }

        return null;
    }

    private static T? FindAncestor<T>(DependencyObject child) where T : DependencyObject
    {
        var current = child;
        while (current != null)
        {
            if (current is T typed)
                return typed;

            current = VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static bool TryGetListBoxFromMenuSender(object sender, out System.Windows.Controls.ListBox listBox)
    {
        listBox = null!;
        if (sender is not System.Windows.Controls.MenuItem menuItem)
            return false;

        if (menuItem.Parent is not System.Windows.Controls.ContextMenu menu ||
            menu.PlacementTarget is not System.Windows.Controls.ListBox target)
            return false;

        listBox = target;
        return true;
    }

    private static void CopyListBoxItems(System.Windows.Controls.ListBox listBox, bool selectedOnly)
    {
        var lines = new List<string>();
        if (selectedOnly)
        {
            foreach (var item in listBox.SelectedItems)
            {
                if (item is string line)
                    lines.Add(line);
            }
        }
        else
        {
            foreach (var item in listBox.Items)
            {
                if (item is string line)
                    lines.Add(line);
            }
        }

        if (lines.Count > 0)
        {
            System.Windows.Clipboard.SetText(string.Join(Environment.NewLine, lines));
        }
    }

    private void AdminScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer sv)
        {
            double offsetChange = e.Delta > 0 ? -1 : 1;
            sv.ScrollToVerticalOffset(sv.VerticalOffset + offsetChange * 3);
            e.Handled = true;
        }
    }

    private void Menu_OpenHelp_Click(object sender, RoutedEventArgs e)
    {
        var window = new Views.HelpWindow
        {
            Owner = this
        };
        window.Show();
        window.Activate();
    }
}

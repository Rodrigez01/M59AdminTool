using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Controls.Primitives;
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
    private System.Windows.Point? _warpDragStart;
    private WarpLocation? _dragWarp;

    // Mini/Full Mode
    private bool _isAdvancedMode = false;
    private const double MINI_WIDTH = 400;
    private const double MINI_HEIGHT = 600;
    private const double FULL_WIDTH = 1000;
    private const double FULL_HEIGHT = 700;
    private readonly string[] _miniModeTabs = { "DM", "Monsters", "Items", "Warps", "DJ", "Arena" };
    private readonly Random _random = new Random();

    public MainWindow()
    {
        InitializeComponent();

        AddHandler(UIElement.PreviewMouseWheelEvent, new System.Windows.Input.MouseWheelEventHandler(Window_PreviewMouseWheel), true);
        if (MainTabControl != null)
        {
            MainTabControl.AddHandler(UIElement.PreviewMouseWheelEvent, new System.Windows.Input.MouseWheelEventHandler(MainTabControl_PreviewMouseWheel), true);
        }

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

        // Start in Mini Mode
        ApplyViewMode(animated: false);

        // Start in Warps Tab
        if (WarpsTab != null)
        {
            MainTabControl.SelectedItem = WarpsTab;
        }
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

    private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (sender is ScrollViewer sv)
        {
            sv.ScrollToVerticalOffset(sv.VerticalOffset - e.Delta / 3.0);
            e.Handled = true;
        }
    }

    private void Window_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (TryScrollActiveTab(e))
        {
            e.Handled = true;
        }
    }

    private void MainTabControl_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        if (TryScrollActiveTab(e))
        {
            e.Handled = true;
        }
    }

    private bool TryScrollActiveTab(System.Windows.Input.MouseWheelEventArgs e)
    {
        var targetScrollViewer = GetActiveTabScrollViewer();
        if (targetScrollViewer == null || targetScrollViewer.ScrollableHeight <= 0)
            return false;

        double offsetChange = e.Delta > 0 ? -1 : 1;
        targetScrollViewer.ScrollToVerticalOffset(targetScrollViewer.VerticalOffset + offsetChange * 20);
        return true;
    }

    private ScrollViewer? GetActiveTabScrollViewer()
    {
        if (MainTabControl?.SelectedItem is not TabItem tabItem)
            return null;

        if (tabItem.Content is DependencyObject contentRoot)
            return FindScrollableScrollViewer(contentRoot);

        return null;
    }

    private ScrollViewer? FindScrollableScrollViewer(DependencyObject root)
    {
        ScrollViewer? fallback = null;
        var queue = new Queue<DependencyObject>();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current is ScrollViewer sv)
            {
                if (!IsItemsControlScrollViewer(sv) && !IsTextBoxScrollViewer(sv))
                {
                    fallback ??= sv;
                    if (sv.ScrollableHeight > 0)
                        return sv;
                }
            }

            int count = VisualTreeHelper.GetChildrenCount(current);
            for (int i = 0; i < count; i++)
            {
                queue.Enqueue(VisualTreeHelper.GetChild(current, i));
            }
        }

        return fallback;
    }

    private static bool IsItemsControlScrollViewer(ScrollViewer scrollViewer)
    {
        if (scrollViewer.TemplatedParent is ItemsControl)
            return true;

        return false;
    }

    private static bool IsTextBoxScrollViewer(ScrollViewer scrollViewer)
    {
        if (scrollViewer.TemplatedParent is System.Windows.Controls.Primitives.TextBoxBase)
            return true;

        return false;
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

    private void Menu_Configuration_Click(object sender, RoutedEventArgs e)
    {
        var window = new Views.PathConfigWindow
        {
            Owner = this
        };
        window.ShowDialog();
    }

    private void WarpsTreeView_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not System.Windows.Controls.TreeView treeView)
            return;

        var item = FindAncestor<System.Windows.Controls.TreeViewItem>(e.OriginalSource as DependencyObject);
        if (item?.DataContext is WarpLocation warp)
        {
            _warpDragStart = e.GetPosition(treeView);
            _dragWarp = warp;
        }
        else
        {
            _warpDragStart = null;
            _dragWarp = null;
        }
    }

    private void WarpsTreeView_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (_dragWarp == null || _warpDragStart == null)
            return;
        if (e.LeftButton != System.Windows.Input.MouseButtonState.Pressed)
            return;

        if (sender is not System.Windows.Controls.TreeView treeView)
            return;

        var currentPos = e.GetPosition(treeView);
        var dx = Math.Abs(currentPos.X - _warpDragStart.Value.X);
        var dy = Math.Abs(currentPos.Y - _warpDragStart.Value.Y);
        if (dx < SystemParameters.MinimumHorizontalDragDistance &&
            dy < SystemParameters.MinimumVerticalDragDistance)
        {
            return;
        }

        DragDrop.DoDragDrop(treeView, _dragWarp, System.Windows.DragDropEffects.Move);
        _warpDragStart = null;
        _dragWarp = null;
    }

    private void WarpsTreeView_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(WarpLocation)))
        {
            e.Effects = System.Windows.DragDropEffects.Move;
        }
        else
        {
            e.Effects = System.Windows.DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void WarpsTreeView_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (sender is not System.Windows.Controls.TreeView treeView)
            return;

        if (e.Data.GetData(typeof(WarpLocation)) is not WarpLocation warp)
            return;

        var item = FindAncestor<System.Windows.Controls.TreeViewItem>(e.OriginalSource as DependencyObject);
        WarpCategory? targetCategory = null;
        if (item?.DataContext is WarpCategoryView categoryView)
        {
            targetCategory = categoryView.Source;
        }
        else if (item?.DataContext is WarpLocation targetWarp &&
                 treeView.DataContext is WarpsViewModel viewModel)
        {
            targetCategory = viewModel.WarpCategories.FirstOrDefault(c => c.Locations.Contains(targetWarp));
        }

        if (targetCategory == null)
            return;

        if (treeView.DataContext is WarpsViewModel warpsViewModel)
        {
            if (warpsViewModel.MoveWarpToCategory(warp, targetCategory))
                e.Handled = true;
        }
    }

    #region Mini/Full Mode Toggle

    private void BtnAdvanced_Click(object sender, RoutedEventArgs e)
    {
        _isAdvancedMode = !_isAdvancedMode;
        ApplyViewMode(animated: true);
    }

    private void ApplyViewMode(bool animated)
    {
        double targetWidth = _isAdvancedMode ? FULL_WIDTH : MINI_WIDTH;
        double targetHeight = _isAdvancedMode ? FULL_HEIGHT : MINI_HEIGHT;

        // Update button text
        if (MenuAdvanced != null)
        {
            MenuAdvanced.Header = _isAdvancedMode ? "⚡ Simple" : "⚡ Advanced";
        }

        // Filter tabs
        FilterTabsForMode();

        // Change tab orientation based on mode
        if (MainTabControl != null)
        {
            MainTabControl.TabStripPlacement = _isAdvancedMode ? Dock.Top : Dock.Left;
        }

        // Adjust header for mode
        if (HeaderTitle != null)
        {
            HeaderTitle.FontSize = _isAdvancedMode ? 20 : 14;
        }
        if (HeaderSubtitle != null)
        {
            HeaderSubtitle.Visibility = _isAdvancedMode ? Visibility.Visible : Visibility.Collapsed;
        }
        if (HeaderBorder != null)
        {
            HeaderBorder.Padding = _isAdvancedMode ? new Thickness(15) : new Thickness(8);
        }

        // Switch DM Tab content between Mini and Full layouts
        if (DmContentMini != null)
        {
            DmContentMini.Visibility = _isAdvancedMode ? Visibility.Collapsed : Visibility.Visible;
        }
        if (DmContentFull != null)
        {
            DmContentFull.Visibility = _isAdvancedMode ? Visibility.Visible : Visibility.Collapsed;
        }
        if (DmHeader != null)
        {
            DmHeader.FontSize = _isAdvancedMode ? 20 : 14;
        }

        if (animated)
        {
            // Spawn magic stars (more stars!)
            SpawnMagicStars(70);

            // Animate window size
            AnimateWindowSize(targetWidth, targetHeight);
        }
        else
        {
            Width = targetWidth;
            Height = targetHeight;
        }
    }

    private void FilterTabsForMode()
    {
        if (MainTabControl == null) return;

        foreach (TabItem tab in MainTabControl.Items)
        {
            if (tab.Tag is string tagName)
            {
                bool shouldShow = _isAdvancedMode || Array.Exists(_miniModeTabs, t => t.Equals(tagName, StringComparison.OrdinalIgnoreCase));
                tab.Visibility = shouldShow ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // Select first visible tab if current is hidden
        if (MainTabControl.SelectedItem is TabItem selectedTab && selectedTab.Visibility == Visibility.Collapsed)
        {
            foreach (TabItem tab in MainTabControl.Items)
            {
                if (tab.Visibility == Visibility.Visible)
                {
                    MainTabControl.SelectedItem = tab;
                    break;
                }
            }
        }
    }

    private void AnimateWindowSize(double targetWidth, double targetHeight)
    {
        var duration = TimeSpan.FromMilliseconds(400);
        var easing = new QuadraticEase { EasingMode = EasingMode.EaseInOut };

        // Width animation
        var widthAnim = new DoubleAnimation
        {
            From = Width,
            To = targetWidth,
            Duration = duration,
            EasingFunction = easing
        };

        // Height animation
        var heightAnim = new DoubleAnimation
        {
            From = Height,
            To = targetHeight,
            Duration = duration,
            EasingFunction = easing
        };

        // Center the window during resize
        double currentCenterX = Left + Width / 2;
        double currentCenterY = Top + Height / 2;
        double newLeft = currentCenterX - targetWidth / 2;
        double newTop = currentCenterY - targetHeight / 2;

        var leftAnim = new DoubleAnimation
        {
            From = Left,
            To = Math.Max(0, newLeft),
            Duration = duration,
            EasingFunction = easing
        };

        var topAnim = new DoubleAnimation
        {
            From = Top,
            To = Math.Max(0, newTop),
            Duration = duration,
            EasingFunction = easing
        };

        BeginAnimation(WidthProperty, widthAnim);
        BeginAnimation(HeightProperty, heightAnim);
        BeginAnimation(LeftProperty, leftAnim);
        BeginAnimation(TopProperty, topAnim);
    }

    private void SpawnMagicStars(int count)
    {
        if (StarCanvas == null) return;

        for (int i = 0; i < count; i++)
        {
            var star = CreateStar();
            StarCanvas.Children.Add(star);

            // Random start position
            double startX = _random.NextDouble() * ActualWidth;
            double startY = _random.NextDouble() * ActualHeight;

            Canvas.SetLeft(star, startX);
            Canvas.SetTop(star, startY);

            // Animate the star
            AnimateStar(star, startX, startY);
        }
    }

    private Polygon CreateStar()
    {
        var star = new Polygon
        {
            Points = CreateStarPoints(8, 4),
            Fill = new SolidColorBrush(GetRandomStarColor()),
            Opacity = 0.9,
            RenderTransform = new RotateTransform(0, 8, 8),
            Width = 16,
            Height = 16
        };
        return star;
    }

    private PointCollection CreateStarPoints(double outerRadius, double innerRadius)
    {
        var points = new PointCollection();
        double centerX = outerRadius;
        double centerY = outerRadius;

        for (int i = 0; i < 10; i++)
        {
            double angle = Math.PI / 2 + i * Math.PI / 5;
            double radius = (i % 2 == 0) ? outerRadius : innerRadius;
            double x = centerX + radius * Math.Cos(angle);
            double y = centerY - radius * Math.Sin(angle);
            points.Add(new System.Windows.Point(x, y));
        }

        return points;
    }

    private System.Windows.Media.Color GetRandomStarColor()
    {
        var colors = new[]
        {
            System.Windows.Media.Color.FromRgb(255, 215, 0),   // Gold
            System.Windows.Media.Color.FromRgb(255, 255, 100), // Yellow
            System.Windows.Media.Color.FromRgb(200, 150, 255), // Purple
            System.Windows.Media.Color.FromRgb(100, 200, 255), // Cyan
            System.Windows.Media.Color.FromRgb(255, 180, 100), // Orange
            System.Windows.Media.Color.FromRgb(255, 255, 255), // White
        };
        return colors[_random.Next(colors.Length)];
    }

    private void AnimateStar(Polygon star, double startX, double startY)
    {
        var duration = TimeSpan.FromMilliseconds(800 + _random.Next(400));

        // Random end position (flying outward)
        double angle = _random.NextDouble() * Math.PI * 2;
        double distance = 100 + _random.NextDouble() * 150;
        double endX = startX + Math.Cos(angle) * distance;
        double endY = startY + Math.Sin(angle) * distance;

        // Position animations
        var xAnim = new DoubleAnimation
        {
            From = startX,
            To = endX,
            Duration = duration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var yAnim = new DoubleAnimation
        {
            From = startY,
            To = endY,
            Duration = duration,
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        // Fade out
        var fadeAnim = new DoubleAnimation
        {
            From = 0.9,
            To = 0,
            Duration = duration
        };

        // Rotation
        var rotateAnim = new DoubleAnimation
        {
            From = 0,
            To = 360 + _random.Next(360),
            Duration = duration
        };

        // Scale
        var scaleAnim = new DoubleAnimation
        {
            From = 1.0,
            To = 0.2,
            Duration = duration
        };

        // Remove star when done
        fadeAnim.Completed += (s, e) =>
        {
            StarCanvas?.Children.Remove(star);
        };

        star.BeginAnimation(Canvas.LeftProperty, xAnim);
        star.BeginAnimation(Canvas.TopProperty, yAnim);
        star.BeginAnimation(OpacityProperty, fadeAnim);

        if (star.RenderTransform is RotateTransform rotateTransform)
        {
            rotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnim);
        }
    }

    #endregion
}

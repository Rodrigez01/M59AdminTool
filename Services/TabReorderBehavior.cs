using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace M59AdminTool.Services
{
    public static class TabReorderBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(TabReorderBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        private static readonly DependencyProperty DragStateProperty =
            DependencyProperty.RegisterAttached(
                "DragState",
                typeof(TabDragState),
                typeof(TabReorderBehavior),
                new PropertyMetadata(null));

        public static bool GetIsEnabled(DependencyObject obj)
            => (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value)
            => obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not System.Windows.Controls.TabControl tabControl)
                return;

            if (e.NewValue is bool enabled && enabled)
            {
                tabControl.AllowDrop = true;
                tabControl.PreviewMouseLeftButtonDown += TabControl_PreviewMouseLeftButtonDown;
                tabControl.MouseMove += TabControl_MouseMove;
                tabControl.DragOver += TabControl_DragOver;
                tabControl.Drop += TabControl_Drop;
                tabControl.Loaded += TabControl_Loaded;
                tabControl.Unloaded += TabControl_Unloaded;
                if (tabControl.GetValue(DragStateProperty) is not TabDragState)
                {
                    tabControl.SetValue(DragStateProperty, new TabDragState());
                }
            }
            else
            {
                tabControl.AllowDrop = false;
                tabControl.PreviewMouseLeftButtonDown -= TabControl_PreviewMouseLeftButtonDown;
                tabControl.MouseMove -= TabControl_MouseMove;
                tabControl.DragOver -= TabControl_DragOver;
                tabControl.Drop -= TabControl_Drop;
                tabControl.Loaded -= TabControl_Loaded;
                tabControl.Unloaded -= TabControl_Unloaded;
                tabControl.ClearValue(DragStateProperty);
            }
        }

        private static void TabControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TabControl tabControl)
            {
                TabOrderService.ApplyOrder(tabControl);
            }
        }

        private static void TabControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.TabControl tabControl)
            {
                TabOrderService.SaveOrder(tabControl);
            }
        }

        private static void TabControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not System.Windows.Controls.TabControl tabControl)
                return;

            var tabItem = GetTabItemFromElement(e.OriginalSource as DependencyObject);
            if (tabItem == null)
                return;

            var state = GetDragState(tabControl);
            if (state == null)
                return;

            state.DragItem = tabItem;
            state.DragStart = e.GetPosition(tabControl);
        }

        private static void TabControl_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is not System.Windows.Controls.TabControl tabControl)
                return;

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var state = GetDragState(tabControl);
            if (state?.DragStart == null || state.DragItem == null)
                return;

            var pos = e.GetPosition(tabControl);
            var dx = Math.Abs(pos.X - state.DragStart.Value.X);
            var dy = Math.Abs(pos.Y - state.DragStart.Value.Y);
            if (dx < SystemParameters.MinimumHorizontalDragDistance &&
                dy < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }

            var dragItem = state.DragItem;
            state.DragStart = null;
            DragDrop.DoDragDrop(tabControl, dragItem, System.Windows.DragDropEffects.Move);
        }

        private static void TabControl_DragOver(object sender, System.Windows.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(System.Windows.Controls.TabItem)))
            {
                e.Effects = System.Windows.DragDropEffects.Move;
            }
            else
            {
                e.Effects = System.Windows.DragDropEffects.None;
            }
            e.Handled = true;
        }

        private static void TabControl_Drop(object sender, System.Windows.DragEventArgs e)
        {
            if (sender is not System.Windows.Controls.TabControl tabControl)
                return;

            if (!e.Data.GetDataPresent(typeof(System.Windows.Controls.TabItem)))
                return;

            var sourceItem = e.Data.GetData(typeof(System.Windows.Controls.TabItem)) as System.Windows.Controls.TabItem;
            if (sourceItem == null)
                return;

            var targetItem = GetTabItemFromPoint(tabControl, e.GetPosition(tabControl));
            if (targetItem == null)
            {
                MoveTabItem(tabControl, sourceItem, tabControl.Items.Count - 1);
                TabOrderService.SaveOrder(tabControl);
                return;
            }

            if (ReferenceEquals(sourceItem, targetItem))
                return;

            var targetIndex = tabControl.Items.IndexOf(targetItem);
            MoveTabItem(tabControl, sourceItem, targetIndex);
            TabOrderService.SaveOrder(tabControl);
        }

        private static void MoveTabItem(System.Windows.Controls.TabControl tabControl, System.Windows.Controls.TabItem item, int targetIndex)
        {
            var oldIndex = tabControl.Items.IndexOf(item);
            if (oldIndex < 0)
                return;

            if (targetIndex < 0)
                targetIndex = 0;

            if (oldIndex == targetIndex)
                return;

            tabControl.Items.RemoveAt(oldIndex);
            if (oldIndex < targetIndex)
            {
                targetIndex = Math.Max(0, targetIndex - 1);
            }

            targetIndex = Math.Min(targetIndex, tabControl.Items.Count);
            tabControl.Items.Insert(targetIndex, item);
            tabControl.SelectedItem = item;
        }

        private static TabDragState? GetDragState(System.Windows.Controls.TabControl tabControl)
            => tabControl.GetValue(DragStateProperty) as TabDragState;

        private static System.Windows.Controls.TabItem? GetTabItemFromPoint(System.Windows.Controls.TabControl tabControl, System.Windows.Point point)
        {
            var result = VisualTreeHelper.HitTest(tabControl, point);
            return GetTabItemFromElement(result?.VisualHit);
        }

        private static System.Windows.Controls.TabItem? GetTabItemFromElement(DependencyObject? element)
        {
            var current = element;
            while (current != null && current is not System.Windows.Controls.TabItem)
            {
                current = VisualTreeHelper.GetParent(current);
            }

            return current as System.Windows.Controls.TabItem;
        }

        private sealed class TabDragState
        {
            public System.Windows.Point? DragStart { get; set; }
            public System.Windows.Controls.TabItem? DragItem { get; set; }
        }
    }
}

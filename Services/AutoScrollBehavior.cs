using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace M59AdminTool.Services
{
    public static class AutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollToEndProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollToEnd",
                typeof(bool),
                typeof(AutoScrollBehavior),
                new PropertyMetadata(false, OnAutoScrollToEndChanged));

        private static readonly DependencyProperty AutoScrollHandlerProperty =
            DependencyProperty.RegisterAttached(
                "AutoScrollHandler",
                typeof(NotifyCollectionChangedEventHandler),
                typeof(AutoScrollBehavior),
                new PropertyMetadata(null));

        public static bool GetAutoScrollToEnd(DependencyObject obj)
            => (bool)obj.GetValue(AutoScrollToEndProperty);

        public static void SetAutoScrollToEnd(DependencyObject obj, bool value)
            => obj.SetValue(AutoScrollToEndProperty, value);

        private static void OnAutoScrollToEndChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not System.Windows.Controls.ItemsControl itemsControl)
                return;

            if (e.NewValue is bool enabled && enabled)
            {
                itemsControl.Loaded += ItemsControl_Loaded;
                itemsControl.Unloaded += ItemsControl_Unloaded;
                HookCollection(itemsControl);
                ScrollToEnd(itemsControl);
            }
            else
            {
                itemsControl.Loaded -= ItemsControl_Loaded;
                itemsControl.Unloaded -= ItemsControl_Unloaded;
                UnhookCollection(itemsControl);
            }
        }

        private static void ItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.ItemsControl itemsControl)
            {
                HookCollection(itemsControl);
                ScrollToEnd(itemsControl);
            }
        }

        private static void ItemsControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.ItemsControl itemsControl)
            {
                UnhookCollection(itemsControl);
            }
        }

        private static void HookCollection(System.Windows.Controls.ItemsControl itemsControl)
        {
            if (itemsControl.Items is not INotifyCollectionChanged incc)
                return;

            var existing = (NotifyCollectionChangedEventHandler?)itemsControl.GetValue(AutoScrollHandlerProperty);
            if (existing != null)
                return;

            NotifyCollectionChangedEventHandler handler = (_, __) => ScrollToEnd(itemsControl);
            itemsControl.SetValue(AutoScrollHandlerProperty, handler);
            incc.CollectionChanged += handler;
        }

        private static void UnhookCollection(System.Windows.Controls.ItemsControl itemsControl)
        {
            if (itemsControl.Items is not INotifyCollectionChanged incc)
                return;

            var handler = (NotifyCollectionChangedEventHandler?)itemsControl.GetValue(AutoScrollHandlerProperty);
            if (handler == null)
                return;

            incc.CollectionChanged -= handler;
            itemsControl.ClearValue(AutoScrollHandlerProperty);
        }

        private static void ScrollToEnd(System.Windows.Controls.ItemsControl itemsControl)
        {
            if (itemsControl.Items.Count == 0)
                return;

            var lastItem = itemsControl.Items[itemsControl.Items.Count - 1];
            itemsControl.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (itemsControl is System.Windows.Controls.ListBox listBox)
                {
                    listBox.ScrollIntoView(lastItem);
                }
                else if (itemsControl is System.Windows.Controls.ListView listView)
                {
                    listView.ScrollIntoView(lastItem);
                }

                var scrollViewer = FindVisualChild<System.Windows.Controls.ScrollViewer>(itemsControl);
                scrollViewer?.ScrollToEnd();
            }));
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typed)
                    return typed;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}

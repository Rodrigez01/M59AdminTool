using System;
using System.Collections.Specialized;
using System.Windows;
using M59AdminTool.Services;

namespace M59AdminTool.Views
{
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Subscribe to collection changes for auto-scroll
            DebugLogger.Messages.CollectionChanged += Messages_CollectionChanged;

            UpdateMessageCount();
        }

        public System.Collections.ObjectModel.ObservableCollection<string> Messages => DebugLogger.Messages;

        private void Messages_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            UpdateMessageCount();

            // Auto-scroll to bottom if enabled
            if (AutoScrollCheckBox.IsChecked == true && LogListBox.Items.Count > 0)
            {
                LogListBox.ScrollIntoView(LogListBox.Items[LogListBox.Items.Count - 1]);
            }
        }

        private void UpdateMessageCount()
        {
            Dispatcher.Invoke(() =>
            {
                var loc = LocalizationService.Instance;
                MessageCountText.Text = string.Format(loc.GetString("Label_MessageCountFormat"), DebugLogger.Messages.Count);
            });
        }

        private void CopyAllButton_Click(object sender, RoutedEventArgs e)
        {
            string allMessages = DebugLogger.GetAllMessages();
            if (!string.IsNullOrEmpty(allMessages))
            {
                System.Windows.Clipboard.SetText(allMessages);
                var loc = LocalizationService.Instance;
                StatusText.Text = string.Format(loc.GetString("Message_LogCopied"), DebugLogger.Messages.Count);
            }
            else
            {
                StatusText.Text = LocalizationService.Instance.GetString("Message_NoMessagesToCopy");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            DebugLogger.Clear();
            StatusText.Text = LocalizationService.Instance.GetString("Message_LogCleared");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AutoScrollCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            bool isChecked = AutoScrollCheckBox.IsChecked == true;
            var loc = LocalizationService.Instance;
            StatusText.Text = string.Format(loc.GetString("Message_AutoScrollStatus"), isChecked ? loc.GetString("Label_On") : loc.GetString("Label_Off"));
        }

        protected override void OnClosed(EventArgs e)
        {
            DebugLogger.Messages.CollectionChanged -= Messages_CollectionChanged;
            base.OnClosed(e);
        }
    }
}

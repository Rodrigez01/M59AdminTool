using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using M59AdminTool.Services;

namespace M59AdminTool.Views
{
    public partial class ListPreviewWindow : Window
    {
        public ObservableCollection<string> Entries { get; }
        public string ListId { get; }

        public ListPreviewWindow(string listId, IReadOnlyList<string> entries)
        {
            InitializeComponent();

            ListId = listId ?? string.Empty;
            Entries = new ObservableCollection<string>(entries ?? Array.Empty<string>());
            DataContext = this;

            var loc = LocalizationService.Instance;
            Title = string.Format(loc.GetString("Window_ListPreview_TitleFormat"), ListId);
            HeaderText.Text = string.Format(loc.GetString("Header_ListPreview_Format"), ListId, Entries.Count);
        }
    }
}

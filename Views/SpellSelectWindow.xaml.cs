using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Application = System.Windows.Application;
using M59AdminTool.Models;

namespace M59AdminTool.Views
{
    public partial class SpellSelectWindow : Window
    {
        public SpellSelectWindow()
        {
            InitializeComponent();
        }

        public List<SpellPickerItem> Spells
        {
            get => SpellComboBox.ItemsSource as List<SpellPickerItem> ?? new List<SpellPickerItem>();
            set => SpellComboBox.ItemsSource = value;
        }

        public SpellPickerItem? SelectedSpell => SpellComboBox.SelectedItem as SpellPickerItem;

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public static int? ShowDialog(List<SpellPickerItem> spells, int? selectedId = null)
        {
            var window = new SpellSelectWindow
            {
                Owner = Application.Current?.MainWindow
            };

            window.Spells = spells;
            if (selectedId.HasValue)
            {
                var selected = spells.FirstOrDefault(s => s.Id == selectedId.Value);
                if (selected != null)
                {
                    window.SpellComboBox.SelectedItem = selected;
                }
            }

            if (window.ShowDialog() == true && window.SelectedSpell != null)
            {
                return window.SelectedSpell.Id;
            }

            return null;
        }
    }
}

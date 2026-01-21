using System.IO;
using System.Windows;
using System.Windows.Forms;
using M59AdminTool.Services;

namespace M59AdminTool.Views
{
    public partial class PathConfigWindow : Window
    {
        public PathConfigWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            var settings = SettingsService.Load();
            ServerRootPathBox.Text = settings.ServerRootPath;
            KodPathBox.Text = settings.KodPath;
        }

        private void BrowseServerRoot_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "Server Root Ordner auswaehlen",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ServerRootPathBox.Text = dialog.SelectedPath;
                if (string.IsNullOrWhiteSpace(KodPathBox.Text))
                {
                    var autoKod = Path.Combine(dialog.SelectedPath, "kod");
                    KodPathBox.Text = autoKod;
                }
            }
        }

        private void BrowseKodPath_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "KOD Ordner auswaehlen",
                UseDescriptionForTitle = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                KodPathBox.Text = dialog.SelectedPath;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var serverRoot = ServerRootPathBox.Text.Trim();
            var kodPath = KodPathBox.Text.Trim();

            if (!string.IsNullOrWhiteSpace(serverRoot) && !Directory.Exists(serverRoot))
            {
                System.Windows.MessageBox.Show("Server Root existiert nicht.", "Konfiguration",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!string.IsNullOrWhiteSpace(kodPath) && !Directory.Exists(kodPath))
            {
                System.Windows.MessageBox.Show("KOD Pfad existiert nicht.", "Konfiguration",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(kodPath) && !string.IsNullOrWhiteSpace(serverRoot))
            {
                kodPath = Path.Combine(serverRoot, "kod");
            }

            SettingsService.Save(new AppSettings
            {
                ServerRootPath = serverRoot,
                KodPath = kodPath
            });

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

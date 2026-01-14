using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using M59AdminTool.Services;
using M59AdminTool.Models;
using System.Windows.Controls;

namespace M59AdminTool.ViewModels
{
    /// <summary>
    /// Online-Player/Rooms Anzeige (basiert auf Admin-Befehl "who")
    /// </summary>
    public partial class PlayersViewModel : ObservableObject
    {
        private readonly ConnectionViewModel? _connectionViewModel;
        private readonly AdminCommandsViewModel? _adminViewModel;
        private EventHandler<string>? _handler;
        private bool _capturing;
        private EventHandler<string>? _detailHandler;
        private bool _capturingDetails;
        private bool _receivedDetail;

        public ObservableCollection<PlayerEntry> Players { get; } = new();
        public ObservableCollection<string> Rooms { get; } = new();
        public ObservableCollection<string> PlayerDetails { get; } = new();

        [ObservableProperty]
        private string _lastPlayerDetail = string.Empty;

        [ObservableProperty]
        private PlayerEntry? _selectedPlayer;

        public PlayersViewModel(ConnectionViewModel? connectionViewModel, AdminCommandsViewModel? adminViewModel = null)
        {
            _connectionViewModel = connectionViewModel;
            _adminViewModel = adminViewModel;
        }

        [RelayCommand]
        private async Task Refresh()
        {
            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                var loc = LocalizationService.Instance;
                MessageBox.Show(loc.GetString("Message_NotConnectedConnectionTab"), loc.GetString("Title_NotConnected"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _adminViewModel?.SubscribeConnection(conn);

            Players.Clear();
            Rooms.Clear();
            _capturing = true;
            _receivedDetail = false;

            if (_handler == null)
            {
                _handler = (s, resp) =>
                {
                    if (!_capturing) return;
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        foreach (var line in resp.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (line.StartsWith(">", StringComparison.Ordinal)) continue; // echo
                            var entry = ParsePlayer(line);
                            Players.Add(entry);
                        }
                    });
                };
            }

            conn.ResponseReceived -= _handler;
            conn.ResponseReceived += _handler;

            await conn.SendAdminCommandAsync("who");

            // kurze Sammelzeit, dann stoppen
            await Task.Delay(2000);
            _capturing = false;
            conn.ResponseReceived -= _handler;
        }

        [RelayCommand]
        private async Task ShowSelectedPlayer(object? parameter)
        {
            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected) return;
            if (parameter is not PlayerEntry entry) return;
            if (string.IsNullOrWhiteSpace(entry.ObjectId)) return;

            SelectedPlayer = entry;
            _adminViewModel?.SubscribeConnection(conn);

            // sicherstellen, dass wir who-capture nicht stören
            _capturing = false;
            if (_handler != null)
            {
                conn.ResponseReceived -= _handler;
            }

            PlayerDetails.Clear();
            LastPlayerDetail = string.Empty;
            _capturingDetails = true;
            _receivedDetail = false;

            if (_detailHandler == null)
            {
                _detailHandler = (s, resp) =>
                {
                    if (!_capturingDetails) return;
                    Application.Current?.Dispatcher.Invoke(() =>
                    {
                        foreach (var line in resp.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            PlayerDetails.Add(line);
                            LastPlayerDetail = line;
                            _receivedDetail = true;
                            if (PlayerDetails.Count > 200)
                                PlayerDetails.RemoveAt(0);
                        }
                    });
                };
            }

            conn.ResponseReceived -= _detailHandler;
            conn.ResponseReceived += _detailHandler;

            await conn.SendAdminCommandAsync($"show object {entry.ObjectId}");

            await Task.Delay(1500);
            _capturingDetails = false;
            conn.ResponseReceived -= _detailHandler;

            if (!_receivedDetail)
            {
                PlayerDetails.Add(LocalizationService.Instance.GetString("Message_NoResponseReceived"));
            }
        }

        private PlayerEntry ParsePlayer(string line)
        {
            // Beispiel: "rodadmin (4259) <status>"
            string objId = string.Empty;
            string name = line;

            int open = line.IndexOf('(');
            int close = line.IndexOf(')');
            if (open >= 0 && close > open)
            {
                objId = line.Substring(open + 1, close - open - 1).Trim();
                name = line.Substring(0, open).Trim();
            }

            return new PlayerEntry
            {
                Name = name,
                ObjectId = objId,
                Raw = line
            };
        }


        [RelayCommand]
        private void CopyPlayerDetails()
        {
            var textAll = string.Join(Environment.NewLine, PlayerDetails);
            if (!string.IsNullOrEmpty(textAll))
            {
                System.Windows.Clipboard.SetText(textAll);
            }
        }

        public async Task EditDetailLineAsync(string line)
        {
            if (SelectedPlayer == null)
            {
                var loc = LocalizationService.Instance;
                MessageBox.Show(loc.GetString("Message_SelectPlayerFirst"), loc.GetString("Title_NoSelection"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var idx = line.IndexOf('=');
            if (idx <= 0) return;

            var name = line.Substring(0, idx).Trim().TrimStart(':');
            var value = line.Substring(idx + 1).Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            // Try to capture current type token (first word of value) to reuse as default type
            var currentTokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentType = currentTokens.Length > 0 ? currentTokens[0] : string.Empty;

            var locPrompt = LocalizationService.Instance;
            var newVal = PromptForInput(string.Format(locPrompt.GetString("Prompt_EditDetailMessage"), value, name),
                locPrompt.GetString("Title_SetObjectProperty"));
            if (string.IsNullOrWhiteSpace(newVal)) return;

            var finalVal = newVal.Trim();
            if (!finalVal.Contains(" ", StringComparison.Ordinal))
            {
                // User provided only the value part; reuse current type if available, otherwise default to INT
                var typeToken = string.IsNullOrWhiteSpace(currentType) ? "INT" : currentType;
                finalVal = $"{typeToken} {finalVal}";
            }

            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected) return;

            await conn.SendAdminCommandAsync($"set o {SelectedPlayer.ObjectId} {name} {finalVal}");

            // Refresh show object after setting the value
            await ShowSelectedPlayer(SelectedPlayer);
        }

        private string? PromptForInput(string message, string title)
        {
            var loc = LocalizationService.Instance;
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 220,
                SizeToContent = SizeToContent.Height,
                MinWidth = 350,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var label = new TextBlock { Text = message, Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(label, 0);
            var textBox = new System.Windows.Controls.TextBox { Margin = new Thickness(0, 0, 0, 10), Padding = new Thickness(5) };
            System.Windows.Controls.Grid.SetRow(textBox, 1);

            var buttonPanel = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            System.Windows.Controls.Grid.SetRow(buttonPanel, 2);

            var okButton = new System.Windows.Controls.Button { Content = loc.GetString("Button_Ok"), Width = 75, Margin = new Thickness(0, 0, 5, 0) };
            var cancelButton = new System.Windows.Controls.Button { Content = loc.GetString("Button_Cancel"), Width = 75 };
            okButton.Click += (_, _) => { dialog.DialogResult = true; dialog.Close(); };
            cancelButton.Click += (_, _) => { dialog.DialogResult = false; dialog.Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(label);
            grid.Children.Add(textBox);
            grid.Children.Add(buttonPanel);

            dialog.Content = grid;
            if (dialog.ShowDialog() == true)
            {
                return textBox.Text;
            }
            return null;
        }

    }
}

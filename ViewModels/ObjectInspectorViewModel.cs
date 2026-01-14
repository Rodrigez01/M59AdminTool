using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M59AdminTool.Services;
using System;
using System.Threading.Tasks;
using Application = System.Windows.Application;
using System.Collections.ObjectModel;

namespace M59AdminTool.ViewModels
{
    public partial class ObjectInspectorViewModel : ObservableObject
    {
        private readonly ConnectionViewModel? _connectionViewModel;
        private M59ServerConnection? _lastConnection;
        private bool _capturing;
        private bool _receivedAny;

        [ObservableProperty]
        private string _objectId = "";

        [ObservableProperty]
        private string _responseText = "";

        [ObservableProperty]
        private string _lastCommand = "";

        [ObservableProperty]
        private string? _selectedLine;

        public ObservableCollection<string> ResponseLines { get; } = new();

        public ObjectInspectorViewModel(ConnectionViewModel? connectionViewModel = null)
        {
            _connectionViewModel = connectionViewModel;
        }

        [RelayCommand]
        private async Task ShowObject()
        {
            if (string.IsNullOrWhiteSpace(ObjectId))
            {
                ResponseText = "❌ Bitte Object ID eingeben!";
                return;
            }

            var command = $"show object {ObjectId}";
            await SendAndCaptureAsync(command);
        }

        [RelayCommand]
        private async Task ShowUsers()
        {
            var command = "show users";
            await SendAndCaptureAsync(command);
        }

        [RelayCommand]
        private async Task ShowAccounts()
        {
            var command = "show accounts";
            await SendAndCaptureAsync(command);
        }

        [RelayCommand]
        private async Task SendCustomCommand()
        {
            if (string.IsNullOrWhiteSpace(LastCommand))
            {
                ResponseText = "❌ Bitte zuerst einen Befehl eingeben!";
                return;
            }

            await SendAndCaptureAsync(LastCommand);
        }

        private async Task SendAndCaptureAsync(string command)
        {
            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected)
            {
                ResponseText = "❌ Bitte zuerst im Tab \"Connection\" einloggen. Admin-Befehle werden direkt an den Server gesendet.";
                return;
            }

            EnsureSubscription(conn);

            LastCommand = command;
            _capturing = true;
            _receivedAny = false;
            ResponseText = $"⏳ Sende Befehl: {command}";
            ResponseLines.Clear();
            ResponseLines.Add(ResponseText);

            try
            {
                await conn.SendAdminCommandAsync(command);
            }
            catch (Exception ex)
            {
                ResponseText = $"❌ Fehler beim Senden: {ex.Message}";
                _capturing = false;
                return;
            }

            // wait briefly for responses
            await Task.Delay(1500);
            _capturing = false;

            if (!_receivedAny)
            {
                ResponseText += Environment.NewLine + "(Keine Antwort erhalten)";
            }
        }

        private void EnsureSubscription(M59ServerConnection connection)
        {
            if (_lastConnection == connection) return;

            if (_lastConnection != null)
            {
                _lastConnection.ResponseReceived -= OnServerResponse;
            }

            _lastConnection = connection;
            _lastConnection.ResponseReceived += OnServerResponse;
        }

        private void OnServerResponse(object? sender, string response)
        {
            if (!_capturing) return;

            _receivedAny = true;

            Application.Current?.Dispatcher.Invoke(() =>
            {
                if (string.IsNullOrEmpty(ResponseText))
                    ResponseText = response;
                else
                    ResponseText += Environment.NewLine + response;

                foreach (var line in response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    ResponseLines.Add(line);
                    SelectedLine = line;
                }
            });
        }

        [RelayCommand]
        private void ClearResponse()
        {
            ResponseText = "";
            ResponseLines.Clear();
        }

        [RelayCommand]
        private async Task EditSelectedLine()
        {
            if (string.IsNullOrWhiteSpace(SelectedLine) || string.IsNullOrWhiteSpace(ObjectId))
                return;

            var line = SelectedLine;
            var idx = line.IndexOf('=');
            if (idx <= 0) return;

            var name = line.Substring(0, idx).Trim().TrimStart(':');
            var value = line.Substring(idx + 1).Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            var tokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var currentType = tokens.Length > 0 ? tokens[0] : "INT";

            var newVal = PromptForInput($"Current: {value}\n\nNew value for {name} (leave type blank to reuse {currentType}):", "Set Object Property");
            if (string.IsNullOrWhiteSpace(newVal)) return;

            var finalVal = newVal.Trim();
            if (!finalVal.Contains(" ", StringComparison.Ordinal))
            {
                finalVal = $"{currentType} {finalVal}";
            }

            var conn = _connectionViewModel?.ServerConnection;
            if (conn == null || !conn.IsConnected) return;

            await conn.SendAdminCommandAsync($"set o {ObjectId} {name} {finalVal}");

            await SendAndCaptureAsync($"show object {ObjectId}");
        }

        private string? PromptForInput(string message, string title)
        {
            var dialog = new System.Windows.Window
            {
                Title = title,
                Width = 400,
                Height = 220,
                SizeToContent = System.Windows.SizeToContent.Height,
                MinWidth = 350,
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen,
                ResizeMode = System.Windows.ResizeMode.NoResize
            };

            var grid = new System.Windows.Controls.Grid { Margin = new System.Windows.Thickness(10) };
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });
            grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = System.Windows.GridLength.Auto });

            var label = new System.Windows.Controls.TextBlock { Text = message, Margin = new System.Windows.Thickness(0, 0, 0, 10) };
            System.Windows.Controls.Grid.SetRow(label, 0);
            var textBox = new System.Windows.Controls.TextBox { Margin = new System.Windows.Thickness(0, 0, 0, 10), Padding = new System.Windows.Thickness(5) };
            System.Windows.Controls.Grid.SetRow(textBox, 1);

            var buttonPanel = new System.Windows.Controls.StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal, HorizontalAlignment = System.Windows.HorizontalAlignment.Right };
            System.Windows.Controls.Grid.SetRow(buttonPanel, 2);

            var okButton = new System.Windows.Controls.Button { Content = "OK", Width = 75, Margin = new System.Windows.Thickness(0, 0, 5, 0) };
            var cancelButton = new System.Windows.Controls.Button { Content = "Cancel", Width = 75 };
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

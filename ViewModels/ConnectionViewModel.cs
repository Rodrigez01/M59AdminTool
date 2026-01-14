using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M59AdminTool.Services;
using System.Threading.Tasks;
using System.Diagnostics;
using static M59AdminTool.Services.DebugLogger;

namespace M59AdminTool.ViewModels
{
    public partial class ConnectionViewModel : ObservableObject
    {
        private M59ServerConnection? _serverConnection;

        [ObservableProperty]
        private string _serverIp = "127.0.0.1";

        [ObservableProperty]
        private int _serverPort = 5959;

        [ObservableProperty]
        private string _username = "";

        [ObservableProperty]
        private string _password = "";

        [ObservableProperty]
        private string _secretKey = "347";

        [ObservableProperty]
        private bool _isConnected = false;

        [ObservableProperty]
        private string _connectionStatus = "Nicht verbunden";

        [ObservableProperty]
        private string _lastResponse = "";

        public ConnectionViewModel()
        {
        }

        [RelayCommand]
        private async Task Connect()
        {
            try
            {
                if (IsConnected)
                {
                    ConnectionStatus = "⚠️ Bereits verbunden!";
                    return;
                }

                if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
                {
                    ConnectionStatus = "❌ Bitte Username und Password eingeben!";
                    return;
                }

                ConnectionStatus = $"⏳ Verbinde zu {ServerIp}:{ServerPort} als {Username}...";
                Log($"[ConnectionViewModel] Starting connection attempt...");

                _serverConnection = new M59ServerConnection(ServerIp, ServerPort);

                // Subscribe to response events
                _serverConnection.ResponseReceived += OnResponseReceived;

                Log($"[ConnectionViewModel] Calling ConnectAndLoginAsync...");
                string effectiveSecret = string.IsNullOrWhiteSpace(SecretKey) ? "347" : SecretKey.Trim();
                bool success = await _serverConnection.ConnectAndLoginAsync(Username, Password, effectiveSecret);

                if (success)
                {
                    IsConnected = true;
                    ConnectionStatus = $"✅ Eingeloggt als {Username} auf {ServerIp}:{ServerPort}";
                    Log("[ConnectionViewModel] Connected and logged in successfully!");
                }
                else
                {
                    IsConnected = false;
                    ConnectionStatus = $"❌ Login fehlgeschlagen!";
                    Log("[ConnectionViewModel] Login failed!");
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                ConnectionStatus = $"❌ FEHLER: {ex.Message}";
                Log($"[ConnectionViewModel] EXCEPTION: {ex.Message}");
                Log($"[ConnectionViewModel] StackTrace: {ex.StackTrace}");

                // Show error to user with copyable text
                string errorText = $"FEHLER: {ex.Message}\n\n" +
                                   $"STACK TRACE:\n{ex.StackTrace}\n\n" +
                                   $"INNER EXCEPTION:\n{ex.InnerException?.Message ?? "Keine"}\n\n" +
                                   $"SERVER: {ServerIp}:{ServerPort}\n" +
                                   $"USERNAME: {Username}";

                var errorWindow = new M59AdminTool.Views.ErrorWindow(errorText);
                errorWindow.ShowDialog();
            }
        }

        [RelayCommand]
        private void Disconnect()
        {
            if (!IsConnected)
            {
                ConnectionStatus = "⚠️ Nicht verbunden!";
                return;
            }

            _serverConnection?.Disconnect();
            _serverConnection = null;
            IsConnected = false;
            ConnectionStatus = "⚪ Getrennt";
            Log("[ConnectionViewModel] Disconnected");
        }

        [RelayCommand]
        private async Task TestCommand()
        {
            if (!IsConnected || _serverConnection == null)
            {
                LastResponse = "❌ Nicht verbunden! Bitte erst verbinden.";
                return;
            }

            LastResponse = "⏳ Sende Test-Befehl 'who'...";

            bool success = await _serverConnection.SendAdminCommandAsync("who");

            if (success)
            {
                LastResponse = "✅ Befehl gesendet! Warte auf Antwort...";
            }
            else
            {
                LastResponse = "❌ Senden fehlgeschlagen!";
            }
        }

        private void OnResponseReceived(object? sender, string response)
        {
            LastResponse = $"📥 Server Antwort:\n{response}";
            Log($"[ConnectionViewModel] Response received: {response}");
        }

        [RelayCommand]
        private void OpenDebugLog()
        {
            var debugWindow = new M59AdminTool.Views.DebugWindow();
            debugWindow.Show();
        }

        public M59ServerConnection? ServerConnection => _serverConnection;
    }
}

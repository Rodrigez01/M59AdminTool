using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M59AdminTool.Services;
using System.Collections.ObjectModel;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Button = System.Windows.Controls.Button;
using TextBox = System.Windows.Controls.TextBox;
using TextBlock = System.Windows.Controls.TextBlock;
using Grid = System.Windows.Controls.Grid;
using StackPanel = System.Windows.Controls.StackPanel;
using Orientation = System.Windows.Controls.Orientation;
using RowDefinition = System.Windows.Controls.RowDefinition;

namespace M59AdminTool.ViewModels
{
    public partial class AdminCommandsViewModel : ObservableObject
    {
        private readonly M59ClientService _clientService;
        private readonly ConnectionViewModel? _connectionViewModel;
        private M59ServerConnection? _lastConnection;
        private static LocalizationService Loc => LocalizationService.Instance;

        [ObservableProperty]
        private string _customCommand = string.Empty;

        public ObservableCollection<string> AdminResponses { get; } = new();

        [ObservableProperty]
        private string _lastAdminResponse = string.Empty;

        [RelayCommand]
        private void CopyAdminResponses()
        {
            var text = string.Join(Environment.NewLine, AdminResponses);
            if (!string.IsNullOrEmpty(text))
            {
                System.Windows.Clipboard.SetText(text);
            }
        }

        public AdminCommandsViewModel(ConnectionViewModel? connectionViewModel = null)
        {
            _clientService = new M59ClientService();
            _connectionViewModel = connectionViewModel;
            SubscribeConnection();
        }

        // Account Management
        [RelayCommand]
        private async Task CreateAdminAccount()
        {
            var username = PromptForInput(Loc.GetString("Prompt_Username"), Loc.GetString("Title_CreateAdminAccount"));
            if (string.IsNullOrEmpty(username)) return;

            var password = PromptForInput(Loc.GetString("Prompt_Password"), Loc.GetString("Title_CreateAdminAccount"));
            if (string.IsNullOrEmpty(password)) return;

            var email = PromptForInput(Loc.GetString("Prompt_Email"), Loc.GetString("Title_CreateAdminAccount"));
            if (string.IsNullOrEmpty(email)) return;

            await SendAdminCommand($"create account admin {username} {password} {email}");

            var acct = PromptForInput(Loc.GetString("Prompt_AccountNumber"), Loc.GetString("Title_CreateAdminAccountFinalize"));
            if (!string.IsNullOrEmpty(acct))
            {
                await SendAdminCommand($"create admin {acct}");
            }
        }

        [RelayCommand]
        private async Task CreateDmAccount()
        {
            var username = PromptForInput(Loc.GetString("Prompt_Username"), Loc.GetString("Title_CreateDmAccount"));
            if (string.IsNullOrEmpty(username)) return;

            var password = PromptForInput(Loc.GetString("Prompt_Password"), Loc.GetString("Title_CreateDmAccount"));
            if (string.IsNullOrEmpty(password)) return;

            var email = PromptForInput(Loc.GetString("Prompt_Email"), Loc.GetString("Title_CreateDmAccount"));
            if (string.IsNullOrEmpty(email)) return;

            await SendAdminCommand($"create account dm {username} {password} {email}");
        }

        [RelayCommand]
        private async Task CreateUserAccount()
        {
            var username = PromptForInput(Loc.GetString("Prompt_Username"), Loc.GetString("Title_CreateUserAccount"));
            if (string.IsNullOrEmpty(username)) return;

            var password = PromptForInput(Loc.GetString("Prompt_Password"), Loc.GetString("Title_CreateUserAccount"));
            if (string.IsNullOrEmpty(password)) return;

            var email = PromptForInput(Loc.GetString("Prompt_Email"), Loc.GetString("Title_CreateUserAccount"));
            if (string.IsNullOrEmpty(email)) return;

            await SendAdminCommand($"create account user {username} {password} {email}");

            var acct = PromptForInput(Loc.GetString("Prompt_AccountNumber"), Loc.GetString("Title_CreateUserAccountFinalize"));
            if (!string.IsNullOrEmpty(acct))
            {
                // vierfach ausführen
                for (int i = 0; i < 4; i++)
                {
                    await SendAdminCommand($"create user {acct}");
                }
            }
        }

        // System Commands
        [RelayCommand]
        private async Task SaveGame()
        {
            await SendAdminCommand("save game");
        }

        [RelayCommand]
        private async Task ReloadSystem()
        {
            await SendAdminCommand("reload system");
        }

        [RelayCommand]
        private async Task RecreateGame()
        {
            await SendAdminCommand("send o 0 recreateall");
        }

        // Object Management
        [RelayCommand]
        private async Task CreateObject()
        {
            var className = PromptForInput(Loc.GetString("Prompt_ClassName"), Loc.GetString("Title_CreateObject"));
            if (!string.IsNullOrEmpty(className))
            {
                await SendAdminCommand($"create object {className}");
            }
        }

        [RelayCommand]
        private async Task ShowObject()
        {
            var objectId = PromptForInput(Loc.GetString("Prompt_ObjectId"), Loc.GetString("Title_ShowObject"));
            if (!string.IsNullOrEmpty(objectId))
            {
                await SendAdminCommand($"show object {objectId}");
            }
        }

        [RelayCommand]
        private async Task DeleteObject()
        {
            var objectId = PromptForInput(Loc.GetString("Prompt_ObjectId"), Loc.GetString("Title_DeleteObject"));
            if (string.IsNullOrEmpty(objectId)) return;

            await SendAdminCommand($"send o {objectId} delete");
        }

        [RelayCommand]
        private async Task TeleportObject()
        {
            var objectId = PromptForInput(Loc.GetString("Prompt_ObjectId"), Loc.GetString("Title_TeleportObject"));
            if (string.IsNullOrEmpty(objectId)) return;

            var roomId = PromptForInput(Loc.GetString("Prompt_RoomId"), Loc.GetString("Title_TeleportObject"));
            if (string.IsNullOrEmpty(roomId)) return;

            await SendAdminCommand($"send object {objectId} teleportto rid int {roomId}");
        }

        // Player Management
        [RelayCommand]
        private async Task TeleportPlayerToSafety()
        {
            var objectId = PromptForInput(Loc.GetString("Prompt_PlayerObjectId"), Loc.GetString("Title_TeleportToSafety"));
            if (!string.IsNullOrEmpty(objectId))
            {
                await SendAdminCommand($"send o {objectId} admingotosafety");
            }
        }

        [RelayCommand]
        private async Task TeleportAllToSafety()
        {
            await SendAdminCommand("send class user admingotosafety");
        }

        [RelayCommand]
        private async Task GivePlayerSpell()
        {
            var objectId = PromptForInput(Loc.GetString("Prompt_PlayerObjectId"), Loc.GetString("Title_GiveSpell"));
            if (string.IsNullOrEmpty(objectId)) return;

            var spellId = PromptForInput(Loc.GetString("Prompt_SpellId"), Loc.GetString("Title_GiveSpell"));
            if (string.IsNullOrEmpty(spellId)) return;

            var ability = PromptForInput(Loc.GetString("Prompt_AbilityPercent"), Loc.GetString("Title_GiveSpell"));
            if (string.IsNullOrEmpty(ability)) return;

            await SendAdminCommand($"send o {objectId} adminsetspell num int {spellId} ability int {ability}");
        }

        [RelayCommand]
        private async Task GivePlayerSkill()
        {
            var objectId = PromptForInput(Loc.GetString("Prompt_PlayerObjectId"), Loc.GetString("Title_GiveSkill"));
            if (string.IsNullOrEmpty(objectId)) return;

            var skillId = PromptForInput(Loc.GetString("Prompt_SkillId"), Loc.GetString("Title_GiveSkill"));
            if (string.IsNullOrEmpty(skillId)) return;

            var ability = PromptForInput(Loc.GetString("Prompt_AbilityPercent"), Loc.GetString("Title_GiveSkill"));
            if (string.IsNullOrEmpty(ability)) return;

            await SendAdminCommand($"send o {objectId} adminsetskill num int {skillId} ability int {ability}");
        }

        [RelayCommand]
        private async Task GiveAllPlayersSpells()
        {
            await SendAdminCommand("send class user giveplayerallspells");
        }

        [RelayCommand]
        private async Task GiveAllPlayersSkills()
        {
            await SendAdminCommand("send class user giveplayerallskills");
        }

        // Server Settings
        [RelayCommand]
        private async Task SetServerHour()
        {
            var hour = PromptForInput(Loc.GetString("Prompt_ServerHour"), Loc.GetString("Title_SetServerHour"));
            if (!string.IsNullOrEmpty(hour))
            {
                await SendAdminCommand($"send o 0 sethour num int {hour}");
            }
        }

        [RelayCommand]
        private async Task AddGameDay()
        {
            await SendAdminCommand("send o 0 newgameday");
        }

        [RelayCommand]
        private async Task AddGameYear()
        {
            await SendAdminCommand("send c system newyear");
        }

        [RelayCommand]
        private async Task EnableSacredHaven()
        {
            await SendAdminCommand("set o 0 piServer_type INT 1");
        }

        [RelayCommand]
        private async Task DisableSacredHaven()
        {
            await SendAdminCommand("set o 0 piServer_type INT 0");
        }

        // Global Actions
        [RelayCommand]
        private async Task GlobalGiveItem()
        {
            var itemClass = PromptForInput(Loc.GetString("Prompt_ItemClassName"), Loc.GetString("Title_GlobalGiveItem"));
            if (string.IsNullOrEmpty(itemClass)) return;

            var count = PromptForInput(Loc.GetString("Prompt_Count"), Loc.GetString("Title_GlobalGiveItem"));
            if (string.IsNullOrEmpty(count)) return;

            await SendAdminCommand($"send o 0 globalgive number int {count} classtype c {itemClass}");
        }

        [RelayCommand]
        private async Task UpdateHallOfHeroes()
        {
            await SendAdminCommand("send c system updatehallofheroes");
        }

        [RelayCommand]
        private async Task StartFrenzy()
        {
            await SendAdminCommand("send class system StartChaosNight");
        }

        // Resource Management
        [RelayCommand]
        private async Task CreateResource()
        {
            var resourceName = PromptForInput(Loc.GetString("Prompt_ResourceName"), Loc.GetString("Title_CreateResource"));
            if (!string.IsNullOrEmpty(resourceName))
            {
                await SendAdminCommand($"create resource {resourceName}");
            }
        }

        [RelayCommand]
        private async Task ShowInstance()
        {
            var className = PromptForInput(Loc.GetString("Prompt_ClassName"), Loc.GetString("Title_ShowInstance"));
            if (!string.IsNullOrEmpty(className))
            {
                await SendAdminCommand($"show instance {className}");
            }
        }

        [RelayCommand]
        private async Task ShowMessage()
        {
            var className = PromptForInput(Loc.GetString("Prompt_ClassName"), Loc.GetString("Title_ShowMessage"));
            if (string.IsNullOrEmpty(className)) return;

            var messageName = PromptForInput(Loc.GetString("Prompt_MessageName"), Loc.GetString("Title_ShowMessage"));
            if (string.IsNullOrEmpty(messageName)) return;

            await SendAdminCommand($"show message {className} {messageName}");
        }

        // Advanced Commands
        [RelayCommand]
        private async Task SendObjectCommand()
        {
            var objectId = PromptForInput(Loc.GetString("Prompt_ObjectId"), Loc.GetString("Title_SendObjectCommand"));
            if (string.IsNullOrEmpty(objectId)) return;

            var message = PromptForInput(Loc.GetString("Prompt_MessageCommand"), Loc.GetString("Title_SendObjectCommand"));
            if (string.IsNullOrEmpty(message)) return;

            await SendAdminCommand($"send o {objectId} {message}");
        }

        [RelayCommand]
        private async Task SendClassCommand()
        {
            var className = PromptForInput(Loc.GetString("Prompt_ClassName"), Loc.GetString("Title_SendClassCommand"));
            if (string.IsNullOrEmpty(className)) return;

            var message = PromptForInput(Loc.GetString("Prompt_MessageCommand"), Loc.GetString("Title_SendClassCommand"));
            if (string.IsNullOrEmpty(message)) return;

            await SendAdminCommand($"send c {className} {message}");
        }

        [RelayCommand]
        private async Task SetObjectProperty()
        {
            var objectId = PromptForInput(Loc.GetString("Prompt_ObjectId"), Loc.GetString("Title_SetObjectProperty"));
            if (string.IsNullOrEmpty(objectId)) return;

            var property = PromptForInput(Loc.GetString("Prompt_PropertyName"), Loc.GetString("Title_SetObjectProperty"));
            if (string.IsNullOrEmpty(property)) return;

            var value = PromptForInput(Loc.GetString("Prompt_PropertyValue"), Loc.GetString("Title_SetObjectProperty"));
            if (string.IsNullOrEmpty(value)) return;

            await SendAdminCommand($"set o {objectId} {property} {value}");
        }

        // Custom Command
        [RelayCommand]
        private async Task ExecuteCustomCommand()
        {
            if (!string.IsNullOrWhiteSpace(CustomCommand))
            {
                await SendAdminCommand(CustomCommand);
                CustomCommand = string.Empty;
            }
        }

        private async Task SendAdminCommand(string command)
        {
            var serverConnection = _connectionViewModel?.ServerConnection;
            if (serverConnection == null || !serverConnection.IsConnected)
            {
                MessageBox.Show(Loc.GetString("Message_AdminNotConnected"), Loc.GetString("Title_NotConnected"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            EnsureResponseSubscription(serverConnection);

            try
            {
                bool ok = await serverConnection.SendAdminCommandAsync(command);
                if (!ok)
                {
                    MessageBox.Show(Loc.GetString("Message_AdminSendFailed"), Loc.GetString("Title_SendFailed"),
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format(Loc.GetString("Message_AdminCommandAborted"), ex.Message), Loc.GetString("Title_Error"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EnsureResponseSubscription(M59ServerConnection connection)
        {
            SubscribeConnection(connection);
        }

        public void SubscribeConnection(M59ServerConnection? connection = null)
        {
            var conn = connection ?? _connectionViewModel?.ServerConnection;
            if (conn == null) return;

            if (_lastConnection == conn) return;

            if (_lastConnection != null)
            {
                _lastConnection.ResponseReceived -= OnServerResponse;
            }

            _lastConnection = conn;
            _lastConnection.ResponseReceived += OnServerResponse;
        }

        private void OnServerResponse(object? sender, string response)
        {
            Application.Current?.Dispatcher.Invoke(() =>
            {
                AdminResponses.Add(response);
                LastAdminResponse = response;
                if (AdminResponses.Count > 200)
                    AdminResponses.RemoveAt(0);
            });

            // Mirror to global logger with simple categorization
            var lower = response.ToLowerInvariant();
            if (lower.Contains("error") || lower.Contains("cannot") || lower.Contains("can't") || lower.Contains("invalid") || lower.Contains("failed") || lower.Contains("missing parameter"))
            {
                M59AdminTool.Services.DebugLogger.LogError(response);
            }
            else
            {
                M59AdminTool.Services.DebugLogger.Log(response);
            }
        }

        // Helper method to prompt for input
        private string? PromptForInput(string message, string title)
        {
            var dialog = new Window
            {
                Title = title,
                Width = 400,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var grid = new Grid
            {
                Margin = new Thickness(10, 10, 10, 10)
            };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var label = new TextBlock
            {
                Text = message,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(label, 0);

            var textBox = new TextBox
            {
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(5, 5, 5, 5)
            };
            Grid.SetRow(textBox, 1);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetRow(buttonPanel, 2);

            var okButton = new Button
            {
                Content = Loc.GetString("Button_Ok"),
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(10, 5, 10, 5)
            };
            okButton.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };

            var cancelButton = new Button
            {
                Content = Loc.GetString("Button_Cancel"),
                Width = 80,
                Padding = new Thickness(10, 5, 10, 5)
            };
            cancelButton.Click += (s, e) => { dialog.DialogResult = false; dialog.Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(label);
            grid.Children.Add(textBox);
            grid.Children.Add(buttonPanel);

            dialog.Content = grid;

            textBox.Focus();
            textBox.KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Enter)
                {
                    dialog.DialogResult = true;
                    dialog.Close();
                }
            };

            return dialog.ShowDialog() == true ? textBox.Text : null;
        }
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using M59AdminTool.Services;
using System.Windows;
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
    public partial class DmCommandsViewModel : ObservableObject
    {
        private readonly M59ClientService _clientService;

        [ObservableProperty]
        private string _customCommand = string.Empty;

        public DmCommandsViewModel()
        {
            _clientService = new M59ClientService();
        }

        // Player Movement Commands
        [RelayCommand]
        private async Task GoRoom()
        {
            var roomString = PromptForInput("Room String/ID eingeben (z.B. 50 oder rid):", "Go Room");
            if (!string.IsNullOrEmpty(roomString))
            {
                await _clientService.SendCommandAsync($"goroom {roomString}");
            }
        }

        [RelayCommand]
        private async Task GotoPlayer()
        {
            var playerName = PromptForInput("Spielername eingeben:", "Goto Player");
            if (!string.IsNullOrEmpty(playerName))
            {
                await _clientService.SendCommandAsync($"goplayer {playerName}");
            }
        }

        [RelayCommand]
        private async Task GetPlayer()
        {
            var playerName = PromptForInput("Spielername eingeben:", "Get Player");
            if (!string.IsNullOrEmpty(playerName))
            {
                await _clientService.SendCommandAsync($"getplayer {playerName}");
            }
        }

        // Visibility Commands
        [RelayCommand]
        private async Task GoInvisible()
        {
            await _clientService.SendCommandAsync("dm stealth on");
        }

        [RelayCommand]
        private async Task GoVisible()
        {
            await _clientService.SendCommandAsync("dm stealth off");
        }

        [RelayCommand]
        private async Task GoAnonymous()
        {
            await _clientService.SendCommandAsync("dm anonymous");
        }

        [RelayCommand]
        private async Task GoBlank()
        {
            await _clientService.SendCommandAsync("dm blank");
        }

        [RelayCommand]
        private async Task GoHidden()
        {
            await _clientService.SendCommandAsync("dm hidden");
        }

        [RelayCommand]
        private async Task GoShadow()
        {
            await _clientService.SendCommandAsync("dm shadow");
        }

        // Mortal/Immortal Commands
        [RelayCommand]
        private async Task GodModeOn()
        {
            await _clientService.SendCommandAsync("dm immortal");
        }

        [RelayCommand]
        private async Task GodModeOff()
        {
            await _clientService.SendCommandAsync("dm mortal");
        }

        // PK Commands
        [RelayCommand]
        private async Task PkEnable()
        {
            await _clientService.SendCommandAsync("dm pk enable");
        }

        [RelayCommand]
        private async Task PkDisable()
        {
            await _clientService.SendCommandAsync("dm pk disable");
        }

        [RelayCommand]
        private async Task PkLock()
        {
            await _clientService.SendCommandAsync("dm pk lock");
        }

        [RelayCommand]
        private async Task PkUnlock()
        {
            await _clientService.SendCommandAsync("dm pk unlock");
        }

        // Heal/Boost Commands
        [RelayCommand]
        private async Task BoostStats()
        {
            await _clientService.SendCommandAsync("dm boost stats");
        }

        // Karma Commands
        [RelayCommand]
        private async Task SetKarmaGood()
        {
            await _clientService.SendCommandAsync("dm good");
        }

        [RelayCommand]
        private async Task SetKarmaNeutral()
        {
            await _clientService.SendCommandAsync("dm neutral");
        }

        [RelayCommand]
        private async Task SetKarmaEvil()
        {
            await _clientService.SendCommandAsync("dm evil");
        }

        // Get Item Commands
        [RelayCommand]
        private async Task GetSpells()
        {
            await _clientService.SendCommandAsync("dm get spells");
        }

        [RelayCommand]
        private async Task GetSkills()
        {
            await _clientService.SendCommandAsync("dm get skills");
        }

        [RelayCommand]
        private async Task GetMoney()
        {
            await _clientService.SendCommandAsync("dm get money");
        }

        [RelayCommand]
        private async Task GetAmmo()
        {
            await _clientService.SendCommandAsync("dm get ammo");
        }

        [RelayCommand]
        private async Task GetRings()
        {
            await _clientService.SendCommandAsync("dm get rings");
        }

        [RelayCommand]
        private async Task GetWands()
        {
            await _clientService.SendCommandAsync("dm get wands");
        }

        [RelayCommand]
        private async Task GetNecklaces()
        {
            await _clientService.SendCommandAsync("dm get necklaces");
        }

        [RelayCommand]
        private async Task GetGems()
        {
            await _clientService.SendCommandAsync("dm get gems");
        }

        [RelayCommand]
        private async Task GetSundries()
        {
            await _clientService.SendCommandAsync("dm get sundries");
        }

        [RelayCommand]
        private async Task GetMisc()
        {
            await _clientService.SendCommandAsync("dm get misc");
        }

        [RelayCommand]
        private async Task GetItem()
        {
            var itemName = PromptForInput("Item Name eingeben:", "Get Item");
            if (!string.IsNullOrEmpty(itemName))
            {
                await _clientService.SendCommandAsync($"dm get item {itemName}");
            }
        }

        [RelayCommand]
        private async Task CreateItemAttribute()
        {
            var attr = PromptForInput("Attributname eingeben:", "Create Item Attribute");
            if (!string.IsNullOrEmpty(attr))
            {
                await _clientService.SendCommandAsync($"dm create item attribute {attr}");
            }
        }

        // Time Commands
        [RelayCommand]
        private async Task SetMorning()
        {
            await _clientService.SendCommandAsync("dm morning");
        }

        [RelayCommand]
        private async Task SetAfternoon()
        {
            await _clientService.SendCommandAsync("dm afternoon");
        }

        [RelayCommand]
        private async Task SetEvening()
        {
            await _clientService.SendCommandAsync("dm evening");
        }

        [RelayCommand]
        private async Task SetNight()
        {
            await _clientService.SendCommandAsync("dm night");
        }

        [RelayCommand]
        private async Task RestoreTime()
        {
            await _clientService.SendCommandAsync("dm restore time");
        }

        // Map Commands
        [RelayCommand]
        private async Task ShowMap()
        {
            await _clientService.SendCommandAsync("showmap");
        }

        [RelayCommand]
        private async Task HideMap()
        {
            await _clientService.SendCommandAsync("hidemap");
        }

        [RelayCommand]
        private async Task OpenGChannel()
        {
            await _clientService.SendCommandAsync("gchannel");
        }

        [RelayCommand]
        private async Task ResetData()
        {
            await _clientService.SendCommandAsync("reset");
        }

        [RelayCommand]
        private async Task SendDmMessage()
        {
            var text = PromptForInput("DM-Nachricht eingeben:", "dm");
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _clientService.SendCommandAsync($"dm {text}");
            }
        }

        [RelayCommand]
        private async Task EchoMessage()
        {
            var text = PromptForInput("Echo Nachricht eingeben:", "echo");
            if (!string.IsNullOrWhiteSpace(text))
            {
                await _clientService.SendCommandAsync($"echo {text}");
            }
        }

        // Tour Commands
        [RelayCommand]
        private async Task StartTour()
        {
            await _clientService.SendCommandAsync("dm start tour");
        }

        [RelayCommand]
        private async Task EndTour()
        {
            await _clientService.SendCommandAsync("dm end tour");
        }

        // Test Commands
        [RelayCommand]
        private async Task TestMonsterGenPoints()
        {
            await _clientService.SendCommandAsync("dm testmonstergenpoints");
        }

        [RelayCommand]
        private async Task TestExitPoints()
        {
            await _clientService.SendCommandAsync("dm testexitpoints");
        }

        [RelayCommand]
        private async Task TestItemPoints()
        {
            await _clientService.SendCommandAsync("dm testitempoints");
        }

        // Monster/Place Commands
        [RelayCommand]
        private async Task SummonMonster()
        {
            var monsterName = PromptForInput("Monstername eingeben:", "dm monster");
            if (!string.IsNullOrEmpty(monsterName))
            {
                await _clientService.SendCommandAsync($"dm monster {monsterName}");
            }
        }

        [RelayCommand]
        private async Task PlaceCandle()
        {
            await _clientService.SendCommandAsync("dm place candle");
        }

        [RelayCommand]
        private async Task PlaceCandelabra()
        {
            await _clientService.SendCommandAsync("dm place candelabra");
        }

        [RelayCommand]
        private async Task PlaceBrazier()
        {
            await _clientService.SendCommandAsync("dm place brazier");
        }

        [RelayCommand]
        private async Task PlaceLamp()
        {
            await _clientService.SendCommandAsync("dm place lamp");
        }

        [RelayCommand]
        private async Task PlaceFirepit()
        {
            await _clientService.SendCommandAsync("dm place firepit");
        }

        [RelayCommand]
        private async Task PlaceDynamicLight()
        {
            await _clientService.SendCommandAsync("dm place dynamic light");
        }

        // Logoff Ghost Commands
        [RelayCommand]
        private async Task LogoffGhostOn()
        {
            await _clientService.SendCommandAsync("dm logoffghost on");
        }

        [RelayCommand]
        private async Task LogoffGhostOff()
        {
            await _clientService.SendCommandAsync("dm logoffghost off");
        }

        [RelayCommand]
        private async Task LogoffGhostTempOff()
        {
            await _clientService.SendCommandAsync("dm logoffghost temp off");
        }

        // Misc Commands
        [RelayCommand]
        private async Task AppealOn()
        {
            await _clientService.SendCommandAsync("dm appeal on");
        }

        [RelayCommand]
        private async Task AppealOff()
        {
            await _clientService.SendCommandAsync("dm appeal off");
        }

        [RelayCommand]
        private async Task ClearAbilities()
        {
            await _clientService.SendCommandAsync("dm clear abilities");
        }

        [RelayCommand]
        private async Task ClearInventory()
        {
            await _clientService.SendCommandAsync("dm clear inventory");
        }

        [RelayCommand]
        private async Task Rumble()
        {
            await _clientService.SendCommandAsync("dm rumble");
        }

        [RelayCommand]
        private async Task Portal()
        {
            await _clientService.SendCommandAsync("dm portal");
        }

        [RelayCommand]
        private async Task GetTotem()
        {
            await _clientService.SendCommandAsync("dm totem");
        }

        [RelayCommand]
        private async Task GetRelic()
        {
            var relicNum = PromptForInput("Relic Nummer (1-5) eingeben:", "Get Relic");
            if (!string.IsNullOrEmpty(relicNum))
            {
                await _clientService.SendCommandAsync($"dm relic {relicNum}");
            }
        }

        [RelayCommand]
        private async Task NpcChat()
        {
            await _clientService.SendCommandAsync("dm npc chat");
        }

        [RelayCommand]
        private async Task PlainForm()
        {
            await _clientService.SendCommandAsync("dm plain");
        }

        [RelayCommand]
        private async Task HumanForm()
        {
            await _clientService.SendCommandAsync("dm human");
        }

        // Disguise Commands
        [RelayCommand]
        private async Task Disguise()
        {
            var disguiseName = PromptForInput("Disguise Name eingeben (z.B. ant, troll):", "Disguise");
            if (!string.IsNullOrEmpty(disguiseName))
            {
                await _clientService.SendCommandAsync($"dm disguise {disguiseName}");
            }
        }

        // Custom Command
        [RelayCommand]
        private async Task ExecuteCustomCommand()
        {
            if (!string.IsNullOrWhiteSpace(CustomCommand))
            {
                await _clientService.SendCommandAsync(CustomCommand);
                CustomCommand = string.Empty;
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
                Content = "OK",
                Width = 80,
                Margin = new Thickness(0, 0, 10, 0),
                Padding = new Thickness(10, 5, 10, 5)
            };
            okButton.Click += (s, e) => { dialog.DialogResult = true; dialog.Close(); };

            var cancelButton = new Button
            {
                Content = "Abbrechen",
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

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using M59AdminTool.Services;

namespace M59AdminTool.ViewModels
{
    public partial class HelpViewModel : ObservableObject
    {
        public ObservableCollection<HelpSection> Sections { get; }

        [ObservableProperty]
        private HelpSection? _selectedSection;

        public HelpViewModel()
        {
            var loc = LocalizationService.Instance;
            Sections = new ObservableCollection<HelpSection>
            {
                new HelpSection(
                    loc.GetString("Help_Section_Welcome"),
                    loc.GetString("Help_Content_Welcome")),
                new HelpSection(
                    loc.GetString("Help_Section_QuickStart"),
                    loc.GetString("Help_Content_QuickStart")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_Connection"),
                    loc.GetString("Help_Content_Tab_Connection")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_Warps"),
                    loc.GetString("Help_Content_Tab_Warps")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_Monsters"),
                    loc.GetString("Help_Content_Tab_Monsters")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_Items"),
                    loc.GetString("Help_Content_Tab_Items")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_DM"),
                    loc.GetString("Help_Content_Tab_DM")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_Admin"),
                    loc.GetString("Help_Content_Tab_Admin")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_DJ"),
                    loc.GetString("Help_Content_Tab_DJ")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_Arena"),
                    loc.GetString("Help_Content_Tab_Arena")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_Players"),
                    loc.GetString("Help_Content_Tab_Players")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_QuestEditor"),
                    loc.GetString("Help_Content_Tab_QuestEditor")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_DeepInspector"),
                    loc.GetString("Help_Content_Tab_DeepInspector")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_ListReader"),
                    loc.GetString("Help_Content_Tab_ListReader")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_AdminConsole"),
                    loc.GetString("Help_Content_Tab_AdminConsole")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_ObjectInspector"),
                    loc.GetString("Help_Content_Tab_ObjectInspector")),
                new HelpSection(
                    loc.GetString("Help_Section_Tab_EventManager"),
                    loc.GetString("Help_Content_Tab_EventManager")),
                new HelpSection(
                    loc.GetString("Help_Section_Tabs"),
                    loc.GetString("Help_Content_Tabs")),
                new HelpSection(
                    loc.GetString("Help_Section_Workflows"),
                    loc.GetString("Help_Content_Workflows")),
                new HelpSection(
                    loc.GetString("Help_Section_Workarounds"),
                    loc.GetString("Help_Content_Workarounds")),
                new HelpSection(
                    loc.GetString("Help_Section_Output"),
                    loc.GetString("Help_Content_Output")),
                new HelpSection(
                    loc.GetString("Help_Section_About"),
                    loc.GetString("Help_Content_About"))
            };

            SelectedSection = Sections.Count > 0 ? Sections[0] : null;
        }
    }

    public class HelpSection
    {
        public HelpSection(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public string Title { get; }
        public string Content { get; }
    }
}

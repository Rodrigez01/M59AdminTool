using System.Windows;
using M59AdminTool.Services;

namespace M59AdminTool.Views
{
    public partial class ErrorWindow : Window
    {
        public string ErrorText { get; set; }

        public ErrorWindow(string errorText)
        {
            InitializeComponent();
            ErrorText = errorText;
            ErrorTextBox.Text = errorText;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(ErrorText);
            var loc = LocalizationService.Instance;
            System.Windows.MessageBox.Show(loc.GetString("Message_ErrorCopied"), loc.GetString("Title_Copied"),
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

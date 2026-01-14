using System.Configuration;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using M59AdminTool.Services;

namespace M59AdminTool;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        base.OnStartup(e);

        DispatcherUnhandledException += (s, args) =>
        {
            ShowUnhandledException(args.Exception);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                ShowUnhandledException(ex);
            }
        };
    }

    private void ShowUnhandledException(Exception ex)
    {
        try
        {
            var logPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "M59AdminTool.crash.log");
            System.IO.File.AppendAllText(logPath, $"[{DateTime.Now:O}] {ex}\n\n");
        }
        catch
        {
            // Ignore logging failures.
        }

        var window = new Window
        {
            Title = LocalizationService.Instance.GetString("Window_UnexpectedError_Title"),
            Width = 900,
            Height = 600,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        var grid = new System.Windows.Controls.Grid { Margin = new Thickness(10) };
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new System.Windows.Controls.RowDefinition { Height = GridLength.Auto });

        var header = new System.Windows.Controls.TextBlock
        {
            Text = LocalizationService.Instance.GetString("Header_UnexpectedError"),
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        };
        System.Windows.Controls.Grid.SetRow(header, 0);

        var textBox = new System.Windows.Controls.TextBox
        {
            Text = ex.ToString(),
            IsReadOnly = true,
            TextWrapping = TextWrapping.Wrap,
            VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
            FontFamily = new System.Windows.Media.FontFamily("Consolas")
        };
        System.Windows.Controls.Grid.SetRow(textBox, 1);

        var buttons = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            Margin = new Thickness(0, 8, 0, 0)
        };
        System.Windows.Controls.Grid.SetRow(buttons, 2);

        var copyButton = new System.Windows.Controls.Button
        {
            Content = LocalizationService.Instance.GetString("Button_Copy"),
            Width = 90,
            Margin = new Thickness(0, 0, 10, 0)
        };
        copyButton.Click += async (_, _) =>
        {
            if (!await TrySetClipboardAsync(textBox.Text))
            {
                System.Windows.MessageBox.Show(
                    LocalizationService.Instance.GetString("Message_ClipboardBusy"),
                    LocalizationService.Instance.GetString("Title_CopyFailed"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        };

        var closeButton = new System.Windows.Controls.Button
        {
            Content = LocalizationService.Instance.GetString("Button_Close"),
            Width = 90
        };
        closeButton.Click += (_, _) => window.Close();

        buttons.Children.Add(copyButton);
        buttons.Children.Add(closeButton);

        grid.Children.Add(header);
        grid.Children.Add(textBox);
        grid.Children.Add(buttons);

        window.Content = grid;
        window.ShowDialog();
    }

    private static async Task<bool> TrySetClipboardAsync(string text)
    {
        const int maxAttempts = 5;
        const int delayMs = 100;

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            try
            {
                System.Windows.Clipboard.SetText(text);
                return true;
            }
            catch (COMException)
            {
                await Task.Delay(delayMs);
            }
        }

        return false;
    }
}


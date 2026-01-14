using System;
using System.Windows;
using System.Windows.Threading;

namespace M59AdminTool.Views
{
    public partial class NotificationWindow : Window
    {
        private readonly DispatcherTimer _timer;

        public NotificationWindow(string message, int durationSeconds = 3)
        {
            InitializeComponent();

            MessageText.Text = message;

            // Position in bottom-right corner
            Left = SystemParameters.PrimaryScreenWidth - Width - 20;
            Top = SystemParameters.PrimaryScreenHeight - Height - 60;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(durationSeconds)
            };
            _timer.Tick += (s, e) =>
            {
                _timer.Stop();
                Close();
            };
            _timer.Start();
        }

        public static void Show(string message, int durationSeconds = 3)
        {
            var notification = new NotificationWindow(message, durationSeconds);
            notification.Show();
        }
    }
}

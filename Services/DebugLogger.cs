using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace M59AdminTool.Services
{
    /// <summary>
    /// Sammelt alle Debug-Nachrichten für Anzeige im UI
    /// </summary>
    public static class DebugLogger
    {
        private static ObservableCollection<string> _messages = new ObservableCollection<string>();
        private static ObservableCollection<string> _errors = new ObservableCollection<string>();
        private static ObservableCollection<string> _debug = new ObservableCollection<string>();
        private static object _lock = new object();

        public static ObservableCollection<string> Messages => _messages;
        public static ObservableCollection<string> Errors => _errors;
        public static ObservableCollection<string> DebugMessages => _debug;

        public static void Log(string message)
        {
            string timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";

            // Write to Debug output
            Debug.WriteLine(timestampedMessage);

            // Add to collection on UI thread
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                lock (_lock)
                {
                    _messages.Add(timestampedMessage);
                    _debug.Add(timestampedMessage);

                    // Keep only last 1000 messages
                    if (_messages.Count > 1000)
                        _messages.RemoveAt(0);
                    if (_debug.Count > 1000)
                        _debug.RemoveAt(0);
                }
            });
        }

        public static void LogError(string message)
        {
            string timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] ERROR: {message}";

            Debug.WriteLine(timestampedMessage);

            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                lock (_lock)
                {
                    _messages.Add(timestampedMessage);
                    _errors.Add(timestampedMessage);
                    if (_messages.Count > 1000)
                        _messages.RemoveAt(0);
                    if (_errors.Count > 1000)
                        _errors.RemoveAt(0);
                }
            });
        }

        public static void Clear()
        {
            System.Windows.Application.Current?.Dispatcher.Invoke(() =>
            {
                lock (_lock)
                {
                    _messages.Clear();
                    _errors.Clear();
                    _debug.Clear();
                }
            });
        }

        public static string GetAllMessages()
        {
            lock (_lock)
            {
                return string.Join(Environment.NewLine, _messages);
            }
        }
    }
}

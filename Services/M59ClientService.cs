using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using WinForms = System.Windows.Forms;

namespace M59AdminTool.Services
{
    /// <summary>
    /// Service for communicating with the Meridian 59 client
    /// Uses Named Pipe (preferred) with automatic fallback to SendKeys
    /// </summary>
    public class M59ClientService
    {
        // Cache for pipe availability check
        private static bool? _pipeAvailable = null;
        #region Win32 API Imports

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        private const int SW_RESTORE = 9;

        #endregion

        /// <summary>
        /// Sends a command to the M59 client chat window (for DM commands)
        /// Tries Named Pipe first, falls back to SendKeys if pipe not available
        /// </summary>
        public async Task<bool> SendCommandAsync(string command)
        {
            try
            {
                Debug.WriteLine($"[M59ClientService] Attempting to send command: {command}");

                // Normalize command (strip leading slash, single line)
                var normalized = NormalizeCommand(command);

                // Try Named Pipe first (if not already failed)
                if (_pipeAvailable != false)
                {
                    bool pipeSuccess = await TrySendViaPipeAsync(normalized);
                    if (pipeSuccess)
                    {
                        _pipeAvailable = true;
                        Debug.WriteLine("[M59ClientService] ✓ Command sent via Named Pipe!");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("[M59ClientService] ⚠ Named Pipe failed, falling back to SendKeys...");
                        _pipeAvailable = false;
                    }
                }

                // Fallback to SendKeys method
                return await SendViaSendKeysAsync(normalized);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[M59ClientService] ERROR: {ex.Message}");
                Debug.WriteLine($"[M59ClientService] StackTrace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Sends an admin command using the same method as DM commands
        /// Tries Named Pipe first, falls back to SendKeys if pipe not available
        /// </summary>
        public async Task<bool> SendAdminCommandAsync(string command)
        {
            try
            {
                Debug.WriteLine($"[M59ClientService] Attempting to send ADMIN command: {command}");

                // Normalize command (strip leading slash, single line)
                var normalized = NormalizeCommand(command);

                // Try Named Pipe first (if not already failed)
                if (_pipeAvailable != false)
                {
                    bool pipeSuccess = await TrySendViaPipeAsync(normalized);
                    if (pipeSuccess)
                    {
                        _pipeAvailable = true;
                        Debug.WriteLine("[M59ClientService] ✓ Admin command sent via Named Pipe!");
                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("[M59ClientService] ⚠ Named Pipe failed, falling back to SendKeys...");
                        _pipeAvailable = false;
                    }
                }

                // Fallback to SendKeys method
                return await SendViaSendKeysAsync(normalized);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[M59ClientService] ✗ ERROR: {ex.Message}");
                Debug.WriteLine($"[M59ClientService] StackTrace: {ex.StackTrace}");
                return false;
            }
        }


        /// <summary>
        /// Checks if the M59 client is running
        /// </summary>
        public bool IsClientRunning()
        {
            try
            {
                return TryFindClientWindow() != IntPtr.Zero;
            }
            catch
            {
                return false;
            }
        }

        private static async Task<IntPtr> FindClientWindowAsync()
        {
            for (var attempt = 0; attempt < 6; attempt++)
            {
                var handle = TryFindClientWindow();
                if (handle != IntPtr.Zero)
                    return handle;

                await Task.Delay(300);
            }

            return IntPtr.Zero;
        }

        private static IntPtr TryFindClientWindow()
        {
            Debug.WriteLine("[M59ClientService] Searching for Meridian client window...");
            var currentProcessId = Process.GetCurrentProcess().Id;
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
                    if (proc.Id == currentProcessId)
                        continue;

                    if (!string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    {
                        if (IsMeridianTitle(proc.MainWindowTitle))
                        {
                            Debug.WriteLine($"[M59ClientService] Found window: {proc.ProcessName}, Title: '{proc.MainWindowTitle}', Handle: {proc.MainWindowHandle}");
                            return proc.MainWindowHandle;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[M59ClientService] Error checking process: {ex.Message}");
                }
            }

            Debug.WriteLine("[M59ClientService] Trying FindWindow fallbacks...");
            string[] titles = { "meridian.exe", "Meridian 59", "Meridian", "meridian" };
            foreach (var title in titles)
            {
                var hWnd = FindWindow(null, title);
                if (hWnd != IntPtr.Zero)
                {
                    Debug.WriteLine($"[M59ClientService] ✓ Found client via FindWindow with title '{title}' (Handle: {hWnd})");
                    return hWnd;
                }
            }

            return IntPtr.Zero;
        }

        private static bool IsMeridianTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return false;

            if (title.IndexOf("admin tool", StringComparison.OrdinalIgnoreCase) >= 0)
                return false;

            return title.IndexOf("meridian", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static async Task<bool> TrySetClipboardTextAsync(string text)
        {
            Exception? lastError = null;
            for (var attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    WinForms.Clipboard.SetText(text);
                    return true;
                }
                catch (Exception ex)
                {
                    lastError = ex;
                    Debug.WriteLine($"[M59ClientService] Clipboard busy (attempt {attempt + 1}/5): {ex.Message}");
                    await Task.Delay(40);
                }
            }

            if (lastError != null)
                Debug.WriteLine($"[M59ClientService] Clipboard set failed: {lastError.Message}");
            return false;
        }

        /// <summary>
        /// Tries to send command via Named Pipe
        /// </summary>
        private static async Task<bool> TrySendViaPipeAsync(string command)
        {
            try
            {
                using (var injector = new M59ClientCommandInjector())
                {
                    if (injector.Connect())
                    {
                        return injector.SendCommand(command);
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[M59ClientService] Named Pipe error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Sends command via SendKeys method (legacy fallback)
        /// </summary>
        private static async Task<bool> SendViaSendKeysAsync(string command)
        {
            try
            {
                Debug.WriteLine("[M59ClientService] Using SendKeys method...");

                // Find the M59 client window
                var hWnd = await FindClientWindowAsync();

                if (hWnd == IntPtr.Zero)
                {
                    Debug.WriteLine("[M59ClientService] ✗ ERROR: Client window not found!");
                    return false;
                }

                // Restore if minimized
                if (IsIconic(hWnd))
                {
                    Debug.WriteLine("[M59ClientService] Window is minimized, restoring...");
                    ShowWindow(hWnd, SW_RESTORE);
                }

                // Activate window
                Debug.WriteLine("[M59ClientService] Setting foreground window...");
                SetForegroundWindow(hWnd);

                // Wait for window to be ready
                await Task.Delay(150);

                // Copy command to clipboard
                Debug.WriteLine("[M59ClientService] Copying command to clipboard...");
                if (!await TrySetClipboardTextAsync(command))
                    return false;

                // Press TAB twice to jump to chat input field
                Debug.WriteLine("[M59ClientService] Sending keys: TAB TAB CTRL+V ENTER");
                WinForms.SendKeys.SendWait("{TAB}");
                WinForms.SendKeys.SendWait("{TAB}");

                // Paste command with Ctrl+V
                WinForms.SendKeys.SendWait("^v");

                // Send Enter to execute
                WinForms.SendKeys.SendWait("{ENTER}");

                Debug.WriteLine("[M59ClientService] ✓ Command sent via SendKeys!");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[M59ClientService] SendKeys error: {ex.Message}");
                return false;
            }
        }

        private static string NormalizeCommand(string command)
        {
            var text = command ?? string.Empty;
            text = text.Trim();
            if (text.StartsWith("/"))
                text = text.Substring(1);
            return text.Replace(Environment.NewLine, " ");
        }

    }
}

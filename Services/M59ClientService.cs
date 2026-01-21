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

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
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

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // Constants
        private const int SW_RESTORE = 9;
        private const uint GW_CHILD = 5;
        private const int IDC_TEXTINPUT = 1046;
        private const uint WM_SETTEXT = 0x000C;
        private const uint WM_KEYDOWN = 0x0100;
        private const int VK_RETURN = 0x0D;

        #endregion

        /// <summary>
        /// Sends a command to the M59 client chat window (for DM commands)
        /// Uses GUIDEHELPER method: WM_SETTEXT + WM_KEYDOWN
        /// </summary>
        public async Task<bool> SendCommandAsync(string command)
        {
            try
            {
                Debug.WriteLine($"[M59ClientService] Attempting to send command: {command}");

                // Normalize command (strip leading slash, single line)
                var normalized = NormalizeCommand(command);

                // Use GUIDEHELPER method (WM_SETTEXT + WM_KEYDOWN)
                return await SendViaWindowMessageAsync(normalized);
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
        /// Uses GUIDEHELPER method: WM_SETTEXT + WM_KEYDOWN
        /// </summary>
        public async Task<bool> SendAdminCommandAsync(string command)
        {
            try
            {
                Debug.WriteLine($"[M59ClientService] Attempting to send ADMIN command: {command}");

                // Normalize command (strip leading slash, single line)
                var normalized = NormalizeCommand(command);

                // Use GUIDEHELPER method (WM_SETTEXT + WM_KEYDOWN)
                return await SendViaWindowMessageAsync(normalized);
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
        /// Sends command via Window Messages (GUIDEHELPER method)
        /// Uses WM_SETTEXT + WM_KEYDOWN - works in background without stealing focus
        /// </summary>
        private static async Task<bool> SendViaWindowMessageAsync(string command)
        {
            try
            {
                Debug.WriteLine("[M59ClientService] Using GUIDEHELPER method (WM_SETTEXT)...");

                // Step 1: Find M59 window by CLASS NAME (not title!)
                var hWnd = FindWindow("Meridian 59", null);
                if (hWnd == IntPtr.Zero)
                {
                    Debug.WriteLine("[M59ClientService] ✗ ERROR: Meridian 59 window not found (class lookup)!");
                    return false;
                }
                Debug.WriteLine($"[M59ClientService] ✓ Found M59 window: 0x{hWnd:X8}");

                // Step 2: Get ComboBox (ID 1046 = IDC_TEXTINPUT)
                var hCombo = GetDlgItem(hWnd, IDC_TEXTINPUT);
                if (hCombo == IntPtr.Zero)
                {
                    Debug.WriteLine("[M59ClientService] ✗ ERROR: ComboBox (ID 1046) not found!");
                    return false;
                }
                Debug.WriteLine($"[M59ClientService] ✓ Found ComboBox: 0x{hCombo:X8}");

                // Step 3: Get Edit control (child of ComboBox)
                var hEdit = GetWindow(hCombo, GW_CHILD);
                if (hEdit == IntPtr.Zero)
                {
                    Debug.WriteLine("[M59ClientService] ✗ ERROR: Edit control not found in ComboBox!");
                    return false;
                }
                Debug.WriteLine($"[M59ClientService] ✓ Found Edit: 0x{hEdit:X8}");

                // Step 4: Set text via WM_SETTEXT
                SendMessage(hEdit, WM_SETTEXT, IntPtr.Zero, command);
                Debug.WriteLine($"[M59ClientService] ✓ Text set: {command}");

                // Small delay to ensure text is set
                await Task.Delay(10);

                // Step 5: Send Enter key via WM_KEYDOWN
                PostMessage(hEdit, WM_KEYDOWN, (IntPtr)VK_RETURN, IntPtr.Zero);
                Debug.WriteLine("[M59ClientService] ✓ Enter key sent!");

                Debug.WriteLine("[M59ClientService] ✓ Command sent via GUIDEHELPER method!");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[M59ClientService] GUIDEHELPER error: {ex.Message}");
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

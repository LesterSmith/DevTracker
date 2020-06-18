using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppWrapper;
using System.Diagnostics;

namespace DevTracker.Classes
{
    internal static class ProcessData
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        public static Tuple<string, string, string, IntPtr> GetCurrentProcessData()
        {
            const string AccessDenied = "AccessDenied";
            var accessDenied = false;
            var moduleName = string.Empty;
            Process p = null;
            IntPtr hwnd;
            try
            {
                p = GetActiveProcess(out hwnd);
            }
            catch (Exception ex)
            {
                return null;
            }

            try
            {
                moduleName = p.MainModule.ModuleName;
            }
            catch (Exception ex)
            {
                // process access denied b/c it is running as admin 
                moduleName = "Process-Access Denied";
                accessDenied = true;
            }

            string currentApp = string.Empty;
            try
            {
                currentApp = !accessDenied ? p.ProcessName : AccessDenied;
            }
            catch (Exception ex)
            {
                Util.LogError($"WindowChangeEvent, can't determine AppName: {ex.Message}");
                currentApp = "Unknown";
            }

            var gawtTitle = GetActiveWindowTitle();
            var mwTitle = p.MainWindowTitle;
            var title = !string.IsNullOrWhiteSpace(gawtTitle)
                ? gawtTitle : !string.IsNullOrWhiteSpace(mwTitle)
                ? mwTitle : $"Unknown title from {currentApp}";

            return Tuple.Create(currentApp, moduleName, title, hwnd);

        }

        public static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }

        //public static Process GetAppProcess(IntPtr hwnd) //, out string modName)
        //{
        //    Process p = null;
        //    try
        //    {
        //        Int32 pid = win32.GetWindowProcessID(hwnd);
        //        p = Process.GetProcessById(pid);
        //        return p;
        //    }
        //    catch (Exception ex)
        //    {
        //    }
        //    return p;// appName;
        //}
        private static Process GetActiveProcess(out IntPtr hanWnd)
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);
            hanWnd = hwnd;
            return p;
        }
    }
}

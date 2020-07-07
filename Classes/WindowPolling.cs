using System;
using System.Timers;
using System.Diagnostics;
using AppWrapper;
using DevTrackerLogging;
namespace DevTracker.Classes
{
    public static class WindowPolling 
    {
        // private static string LastTitle = "DevTracker";
        private static string LastApp = "devenv";
        public static Timer Timer { get; set; } 

        /// <summary>
        /// Called when computer is locked or Laptop Lid is closed
        /// </summary>
        public static void SuspendWindowPolling()
        {
            Timer.Enabled = false;
        }

        public static void ResumeWindowPolling()
        {
            Timer.Enabled = true;
        }
        public static void StartPolling()
        {
            var o = Globals.ConfigOptions.Find(x => x.Name == AppWrapper.AppWrapper.PollingTimeInterval);
            var timerInterval = o != null ? int.Parse(o.Value) : 100;

            Timer = new Timer { Interval = timerInterval, Enabled = false};
            Timer.Elapsed += new ElapsedEventHandler(Timer_Tick);
            Timer.Enabled = true;
        }

        /// <summary>
        /// Check for window title change by getting the current window title and comparing
        /// it to LastTitle, if different call 
        /// </summary>
        public static void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            try
            {
                Timer.Enabled = false;

                //string title = Globals.WindowChangeEventHandler.GetActiveWindowTitle(out IntPtr hwnd);
                Tuple<string, string, string, IntPtr> tuple = ProcessData.GetCurrentProcessData();
                if (tuple == null)
                {
                    Timer.Enabled = true;
                    return;
                }

                var currentApp = tuple.Item1;
                IntPtr hwnd = tuple.Item4;
                //if (title == null || LastTitle == title)
                if (currentApp == null || currentApp == "explorer" || currentApp == "AccessDenied" || LastApp == currentApp)
                {
                    Timer.Enabled = true;
                    return;
                }

                //Debug.WriteLine($"LastApp: {LastApp}  CurrentApp: {currentApp} Time: {DateTime.Now.ToString("MM/ddy/yyy HH:mm:ss")}");
                // remember the new title 
                //LastTitle = !string.IsNullOrWhiteSpace(title) ? title : "Title empty";
                LastApp = currentApp;

                IntPtr intPtr = new IntPtr();
                uint uInt = new uint();
                Timer.Enabled = false;

                // call the WindowChangeEventHandler.WinEventProc to simulate what SetWinEventHook would
                // do.  Only the window handle is needed
                Globals.WindowChangeEventHandler.WinEventProc(intPtr, uInt, hwnd, 0, 0, uInt, uInt);
                Timer.Enabled = true;
                return;
            }
            catch (Exception ex)
            {
                _ = new LogError(ex, false, "WindowPolling.Timer_Tick");
            }
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading;
using System.Timers;
using System.Diagnostics;
using System.Threading.Tasks;

namespace DevTracker.Classes
{
    public static class WindowPolling 
    {
        private static string LastTitle = "DevTracker";
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

                string title = Globals.WindowChangeEventHandler.GetActiveWindowTitle(out IntPtr hwnd);

                if (title == null || LastTitle == title)
                {
                    Timer.Enabled = true;
                    return;
                }

                // remember the new title 
                LastTitle = !string.IsNullOrWhiteSpace(title) ? title : "Title empty";

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
                Debug.WriteLine(ex.Message);
            }
        }

    }
}

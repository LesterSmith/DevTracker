using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using BusinessObjects;
using System.DirectoryServices.AccountManagement;
using AppWrapper;
using DevTrackerLogging;
using System.ComponentModel;

namespace DevTracker.Classes
{
    public class WindowChangeEvents
    {
        #region private members and Windows DLL descriptors
        WinEventDelegate dele = null;
        IntPtr m_hhook = IntPtr.Zero;
        DateTime _startTime = DateTime.Now;
        delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        List<ProjectName> projects = new List<ProjectName>();
        #endregion

        #region ..ctor
        public WindowChangeEvents()
        {
            dele = new WinEventDelegate(WinEventProc);
            // if debugging the filewather bypass starting windowwatcher
            m_hhook = SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);

            // create dummy window else file watcher throws exception if a file 
            // event is raised before a window clicked
            Globals.LastWindowEvent = new WindowEvent
            {
                AppName = "DevTracker",
                DevProjectName = "DevTracker",
                StartTime = DateTime.Now,
                ID = Guid.NewGuid().ToString(),
                MachineName = Environment.MachineName,
                ModuleName = "DevTracker",
                WindowTitle = "DevTracker",
                UserName = Environment.UserName,
                UserDisplayName = UserPrincipal.Current.DisplayName,
                ITProjectID = string.Empty
            };
            //Globals.CurrentApp = "DevTracker";
        }

        public WindowChangeEvents(bool runFromTimer)
        {
            // the object of this constructor is to do the below and return this object
            // GetAppProcess and GetWindowTitle will be called by the timer object  
            // to poll for window changes, hopefully that will not be slowed down like
            // WinEventProc is when connected to a high speed internet, for some reason
            // it slows tremendously when connected

            // create dummy window else file watcher throws exception if a file 
            // event is raised before a window clicked
            Globals.LastWindowEvent = new WindowEvent
            {
                AppName = "DevTracker",
                DevProjectName = "DevTracker",
                StartTime = DateTime.Now,
                ID = Guid.NewGuid().ToString(),
                MachineName = Environment.MachineName,
                ModuleName = "DevTracker",
                WindowTitle = "DevTracker",
                UserName = Environment.UserName,
                UserDisplayName = UserPrincipal.Current.DisplayName,
                ITProjectID = string.Empty
            };

        }
        #endregion

        #region public methods

        //private bool _busy = false;
        /// <summary>
        /// This method fires every time a window gets focus.  It is called from Windows b/c
        /// we have registered to be be called by calling SetWinEventHook
        /// </summary>
        /// <param name="hWinEventHook"></param>
        /// <param name="eventType"></param>
        /// <param name="hwnd"></param>
        /// <param name="idObject"></param>
        /// <param name="idChild"></param>
        /// <param name="dwEventThread"></param>
        /// <param name="dwmsEventTime"></param>
        //public void WinEventProc_old(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        //{
        //    try
        //    {
        //        if (_busy)
        //        {
        //            // dont throw away events, wait
        //            while (_busy)
        //                Thread.Sleep(10);
        //        }
        //        _busy = true;

        //        Process p = GetAppProcess(hwnd);

        //        if (p == null)
        //        {
        //            _busy = false;
        //            return;
        //        }

        //        // create a new WindowEvents object to process the window change
        //        var we = new WindowEvents(p, null); // Log);

        //        #region moved to WindowEvents class
        //        //var moduleName = p.MainModule.ModuleName;
        //        //var s = string.Format("Start: {3}  Title: {0}     AppName: {1}    ModuleName: {2}", GetActiveWindowTitle(), p.ProcessName, moduleName, DateTime.Now.ToString());
        //        //var now = DateTime.Now;
        //        //var title = GetActiveWindowTitle();
        //        //if (title == null) return;
        //        //if (title == "Window Change Log" || title == "Project Description" || p.ProcessName == "DevTracker" || p.ProcessName == "explorer")
        //        //    return;
        //        //ListViewItem lvi = new ListViewItem(now.ToString("MM/dd/yyyy HH:mm:ss"));
        //        //lvi.SubItems.Add(title);
        //        //lvi.SubItems.Add(p.ProcessName);   // appName                    //GetAppName(hwnd, out moduleName));
        //        //lvi.SubItems.Add(moduleName);
        //        //TimeSpan elapsedTime = now - _startTime;
        //        //_startTime = now;
        //        //lvi.SubItems.Add("");// elapsedTime.ToString());
        //        //if (p.ProcessName == "devenv")
        //        //{
        //        //    var patt = "^(?<PrjName>.*?) - Microsoft Visual Studio";
        //        //    var m = Regex.Match(title, patt);
        //        //    var prjName = string.Empty;
        //        //    if (m.Success && m.Groups["PrjName"] != null)
        //        //    {
        //        //        prjName = m.Groups["PrjName"].Value.Replace("(Running)", string.Empty).Replace("(Debugging)", string.Empty).Trim();
        //        //    }
        //        //    lvi.SubItems.Add(prjName);
        //        //}
        //        //else
        //        //    lvi.SubItems.Add(p.ProcessName);
        //        //lvi.SubItems.Add(""); //ID
        //        //lvi.SubItems.Add("");  // desc
        //        //lvi.SubItems.Add(p.StartInfo.WorkingDirectory);
        //        //var known = GetProjectIdIfKnown(lvi);
        //        //if (known)
        //        //    UpdateAllItems();
        //        //if (Log.Items.Count >0)
        //        //    Log.Items[Log.Items.Count-1].SubItems[4].Text = elapsedTime.ToString();
        //        //Log.Items.Add(lvi);
        //        ////if (!Log.IsScrollbarUserControlled)
        //        ////{
        //        //    Log.Items[Log.Items.Count - 1].EnsureVisible();
        //        ////}
        //        ///
        //        #endregion

        //        _busy = false;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message, AppWrapper.AppWrapper.ProgramError, MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        _busy = false;
        //    }
        //}

        //NOTE WinEventProcess Change


        /// <summary>
        /// This is the new method designed to queue process in Globals.WindowQueue
        /// It will then create a new windowsevent object to process the queue
        /// 
        /// NOTE: You can put breakpoints in here, but if you do, then writing becomes iffy in spite of
        /// all that I have done to queue and thread, there are still issues in debugging
        /// b/c stopping in here changes the window event
        /// This method fires every time a new window gets focus.  It is called from Windows b/c
        /// we have registered to be be called by calling SetWinEventHook
        /// 
        /// NOTE: we may be calling this from a new timer class passing nulls to all params
        /// except hwnd bc none of the others is used in event handling
        /// Calling SetWinEventHook is causing an bad slowdown when connected to high speed
        /// internet (possibly caused by processing going on behind the scenes in windows, I
        /// really have not been able to figure it out, just figured that is what causes the
        /// slowdown and poll is hopefully going to work better on the developers machine.)
        /// </summary>
        /// <param name="hWinEventHook"></param>
        /// <param name="eventType"></param>
        /// <param name="hwnd"></param>
        /// <param name="idObject"></param>
        /// <param name="idChild"></param>
        /// <param name="dwEventThread"></param>
        /// <param name="dwmsEventTime"></param>
        public void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
        {
            try
            {
                /***** we must lock this whole method else we get conflicting data *****/
                // queue the  LastWindow Object and create thread to process it
                // if one is not already running: we will only allow one thread to run
                // it will loop thru any and all queued events and process
                lock (Globals.SyncLockObject)
                {
                    Process p = GetAppProcess(hwnd);

                    if (p == null)
                    {
                        return;
                    }

                    // create a copy of the LastWindowEvent lest we pass a pointer and 
                    // step on ourselves in processing
                    var now = DateTime.Now;

                    var we = new WindowEvent
                    {
                        AppName = Globals.LastWindowEvent.AppName,
                        DevProjectName = Globals.LastWindowEvent.DevProjectName,
                        StartTime = Globals.LastWindowEvent.StartTime,
                        ID = Globals.LastWindowEvent.ID,
                        MachineName = Globals.LastWindowEvent.MachineName,
                        ModuleName = Globals.LastWindowEvent.ModuleName,
                        WindowTitle = string.IsNullOrWhiteSpace(Globals.LastWindowEvent.WindowTitle) ? $"Unknow Titile from {Globals.LastWindowEvent.AppName}" : Globals.LastWindowEvent.WindowTitle, 
                        UserName = Globals.LastWindowEvent.UserName,
                        UserDisplayName = Globals.LastWindowEvent.UserDisplayName,
                        ITProjectID = Globals.LastWindowEvent.ITProjectID,
                        EndTime = now
                    };

                    if (string.IsNullOrWhiteSpace(we.WindowTitle) || (we.AppName.ToLower() == "ssms" && we.WindowTitle.Contains("Microsoft Visual Studio")))
                        _ = new LogError($"WindowChangeEvents, SSMS Bad Project for Title: {we.WindowTitle}", false, "WindowChangeEvents.WinEventProc");

                    Globals.WinEventQueue.Enqueue(
                        new WinEventProcesss
                        {
                            Starttime = now,
                            //WEProcess = p,
                            MyWindowEvent = we
                        });

                    // now while we have the LastWindowEvent locked in Globals
                    // and having queued it for processing
                    // create a new LastWindowEvent in Globals


                    // get process data to create new LastWindow event
                    const string AccessDenied = "AccessDenied";
                    var accessDenied = false;
                    var moduleName = string.Empty;
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
                        _ = new LogError($"WindowChangeEvent, can't determine AppName: {ex.Message}", false, "WindowChangeEvents.WinEventProc");
                        currentApp = "Unknown";
                    }

                    var gawtTitle = GetActiveWindowTitle();
                    var mwTitle = p.MainWindowTitle;
                    var title = !string.IsNullOrWhiteSpace(gawtTitle)
                        ? gawtTitle : !string.IsNullOrWhiteSpace(mwTitle)
                        ? mwTitle : $"Unknown title from {currentApp}";

                    if (Globals.LastWindowEvent.WindowTitle.StartsWith("Unknown"))
                    {
                        _ = new LogError($"WindowChangeEvent, Bad Title from Title: {title}, GetActiveWindowTitle= '{title}'", false, "WindowChangeEvents.WinEventProc");
                    }

                    if (currentApp == "ssms" && title.Contains("Microsoft Visual Studio"))
                        _ = new LogError($"WindowChangeEvent, SSMS bad project from {title}", false, "WindowChangeEvent.WinEventProc");

                    string displayName = Globals.DisplayName;

                    // create new LastWindowEvent for the current active process
                    Globals.LastWindowEvent = new WindowEvent
                    {
                        AppName = currentApp,
                        DevProjectName = string.Empty, /* we do not know the project, the WindowEvent will try to determine that */
                        StartTime = now,
                        ID = Guid.NewGuid().ToString(),
                        MachineName = Environment.MachineName,
                        ModuleName = moduleName,
                        WindowTitle = title,
                        UserName = Environment.UserName,
                        UserDisplayName = displayName,
                        ITProjectID = string.Empty
                    };

                    if (!Globals.WindowEventThreadRunning)
                    {
                        Globals.WindowEventThreadRunning = true;
                        Thread t = new Thread(new ThreadStart(ProcessQueueEntry));
                        t.Start();
                    }
                } // end of lock(Globals.SyncLockObject)

                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, AppWrapper.AppWrapper.ProgramError, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //_busy = false;
            }
        }

        private void ProcessQueueEntry()
        {
            // create a new WindowEvents object to process the window change(S)
            _ = new WindowEvents(/*process.WEProcess, null*/);
        }

        public string GetActiveWindowTitle()
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

        /// <summary>
        /// Called by WindowPolling class to a window title change
        /// </summary>
        /// <param name="hwnd"></param>
        /// <returns></returns>
        public string GetActiveWindowTitle(out IntPtr hwnd)
        {
            const int nChars = 256;
            IntPtr handle = IntPtr.Zero;
            StringBuilder Buff = new StringBuilder(nChars);
            handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                hwnd = handle;
                return Buff.ToString();
            }
            hwnd = handle;
            return null;
        }

        public Process GetAppProcess(IntPtr hwnd) //, out string modName)
        {
            Process p = null;
            try
            {
                Int32 pid = win32.GetWindowProcessID(hwnd);
                p = Process.GetProcessById(pid);
                return p;
            }
            catch (Exception ex)
            {
            }
            return p;
        }

        public void Dispose()
        {
            UnhookWinEvent(m_hhook);
        }
        #endregion
    }
}

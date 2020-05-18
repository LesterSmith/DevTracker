using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Windows.Forms;
using DataHelpers;
using BusinessObjects;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;

namespace WindowChangeTracker.Classes
{
    public class WindowEvents
    {
        #region private members
        private ListView Log { get; set; }
        DateTime _startTime = DateTime.Now;
        WinEventDelegate dele = null;
        IntPtr m_hhook = IntPtr.Zero;
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
        private Process process { get; set; }
        #endregion

        #region ..ctor
        const string AccessDenied = "AccessDenied";
        /// <summary>
        /// If log is null do not create a listvieew item, just log to database
        /// </summary>
        /// <param name="p"></param>
        /// <param name="log"></param>
        public WindowEvents(Process p, ListView log)
        {
            try
            {
                var now = DateTime.Now; // time window changed
                Log = log;
                var title = GetActiveWindowTitle();
#if DEBUG
                Debug.WriteLine("**** " + title);
#endif                
                if (title == null) return;

                var accessDenied = false;
                var moduleName = string.Empty;
                try
                {
                    moduleName = p.MainModule.ModuleName;
                }
                catch (Win32Exception ex)
                {
                    // process access denied b/c it is running as admin 
                    moduleName = "Process-Access Denied";
                    accessDenied = true;
                }
                string displayName;
                try
                {
                    displayName = UserPrincipal.Current.DisplayName;
                }
                catch (Exception ex)
                {
                    displayName = Environment.UserName;
                }

                if (Globals.LastWindowEvent == null)
                    Globals.StartTime = now;

                TimeSpan elapsedTime = now - Globals.StartTime;
                Globals.StartTime = now;  // current window change time

                var devPrjName = string.Empty;

                // We set some globals so the file watcher knows who is running
                try
                {
                    Globals.CurrentApp = !accessDenied ? p.ProcessName : AccessDenied;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    Globals.CurrentApp = "Unknown";
                }

                if (!accessDenied && Globals.CurrentApp == AppWrapper.AppWrapper.devenv)
                {
                    var patt = "(?<PrjName>.*?)(?<spacer> - )*Microsoft Visual Studio|Microsoft Visual Studio";
                    var m = Regex.Match(title, patt);
                    devPrjName = string.Empty;
                    if (m.Success && m.Groups["PrjName"] != null && !string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value))
                    {
                        devPrjName = m.Groups["PrjName"].Value.Replace("(Running)", string.Empty).Replace("(Debugging)", string.Empty).Trim();
                        if (Globals.VSUnknowns)
                        {
                            Globals.VSUnknowns = false;
                            UpdateUnknownProjectNameinVSCode(devPrjName, AppWrapper.AppWrapper.devenv, AppWrapper.AppWrapper.VSUnknown);
                        }
                    }
                    else /* regex failed to get prjname */
                    {
                        /* this is one place where project name can be set to devenv 
                         * and should not be                                       */
                        //devPrjName = p.ProcessName;
                        devPrjName = AppWrapper.AppWrapper.VSUnknown; // "devenvUnKnown"; 
                    }
                }
                else if (!accessDenied && Globals.CurrentApp == AppWrapper.AppWrapper.VSCode)
                {
                    // we are in VSCode 
                    var pattVSCode = "^(?<FileName>.*?) - (?<PrjName>.*?) - Visual Studio Code|Welcome - Visual Studio Code|Open Folder";
                    var m = Regex.Match(title, pattVSCode);
                    devPrjName = string.Empty;
                    if (m.Success && m.Groups["PrjName"] != null && !string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value))
                    {
                        devPrjName = m.Groups["PrjName"].Value;
                        if (Globals.VSCodeUnknowns)
                        {
                            Globals.VSCodeUnknowns = false;
                            UpdateUnknownProjectNameinVSCode(devPrjName, Globals.CurrentApp, AppWrapper.AppWrapper.VSCodeUnknown);
                        }
                    }
                    else if (m.Success && string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value))
                    {
                        // in vscode the 
                        // developer has not opened a folder yet so mark
                        // the project name unknown until VSCode opens a folder and then
                        // we can update the database records
                        devPrjName = "UnKnown";
                        Globals.VSCodeUnknowns = true;
                    }
                    else
                    {
                        devPrjName = AppWrapper.AppWrapper.VSCodeUnknown;
                        Globals.VSCodeUnknowns = true;
                    }
                }
                else 
                {
                    // current app is not VS or VSCode we will only be able to determine
                    // the project name if this app saves a file to a known project path
                    devPrjName = !accessDenied ? p.ProcessName : AccessDenied;
                    if (devPrjName == "devenv")
                    {
                        devPrjName = AppWrapper.AppWrapper.VSUnknown;
                        Globals.VSUnknowns = true;
                    } 
                }


                //appears to be so FileWatcher can pick it out to use in creating FileAnalyzer
                // could be set in Globals.LastWindowEvent object not in two places
                // below is wrong b/c it is updating the last window that we are no longer in
                //Globals.LastWindowEvent.DevProjectName = devPrjName;

                // in practice, if you do not record all events, you could later want 
                // to see some that would not be recorded
                //if (title == "Window Change Log" || title == "Project Description" || p.ProcessName == "WindowChangeTracker" || p.ProcessName == "explorer")
                //    return;

                var currApp = !accessDenied ? p.ProcessName : AccessDenied;

                // see if we are interested in recording this window
                var weCare = Globals.NotableApplications.Find(o => o.AppName.ToLower() == currApp.ToLower());
                var writeDB = (weCare != null);
                if (currApp.ToLower() == "explorer") // && !title.StartsWith("File Explorer"))
                    writeDB = false; //  forget 

                WindowEvent item;

                /* there are other threads, FileWatcher, changing this object 
                   and we want no conflict  */
                if (Globals.LastWindowEvent != null)
                {
                    lock (Globals.LastWindowEvent)
                    {
                        item = Globals.LastWindowEvent;
                        item.EndTime = now;
                        Globals.StartTime = now;
                        if (writeDB)
                        {
                            var hlpr = new DHWindowEvents(AppWrapper.AppWrapper.DevTrkrConnectionString);
                            if (item.DevProjectName.Equals("devenv"))
                                Debug.WriteLine(item.DevProjectName);
                            var rows = hlpr.InsertWindowEvent(item); 
                        }
                    }
                }

                var appNName = !accessDenied ? p.ProcessName : AccessDenied;
                item = new WindowEvent
                {
                    ID = Guid.NewGuid().ToString(),
                    StartTime = now,
                    WindowTitle = title,
                    AppName = appNName,
                    ModuleName = moduleName,
                    EndTime = DateTime.MinValue,
                    DevProjectName = devPrjName,
                    ITProjectID = string.Empty,
                    UserName = Environment.UserName,
                    MachineName = Environment.MachineName,
                    UserDisplayName = displayName
                };
                Globals.LastWindowEvent = item;

                const string comma = ",";
#if DEBUG
                Debug.Write("*******" + title + comma +
                            _startTime.ToString("HH:mm:ss") + comma +
                            item.EndTime.ToString("HH:mm:ss") + comma +
                            item.AppName + comma +
                            item.ModuleName + comma +
                            devPrjName + Environment.NewLine);

#endif
                if ( Log != null && writeDB)
                {
                    ListViewItem lvi = new ListViewItem(Globals.StartTime.ToString("MM/dd/yyyy HH:mm:ss")); // starttime
                    lvi.SubItems.Add(title); // WindowTitle
                    lvi.SubItems.Add(item.AppName);   // AppName
                    lvi.SubItems.Add(moduleName); // ModuleName
                    lvi.SubItems.Add(TimeSpan.FromTicks(elapsedTime.Ticks).ToString());
                    lvi.SubItems.Add(devPrjName);
                    lvi.SubItems.Add(""); //ID
                    lvi.SubItems.Add("");  // desc
                    lvi.SubItems.Add(p.StartInfo.WorkingDirectory);
                    lvi.SubItems.Add(displayName);
                    log.Items.Add(lvi);
                }

            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);

            }
        }

        private void UpdateUnknownProjectNameinVSCode(string devProjectName, string appName, string unknownKey)
        {
            var hlpr = new DHWindowEvents(AppWrapper.AppWrapper.DevTrkrConnectionString);
            hlpr.UpdateUnknownProjectNameinVSCode(devProjectName, appName, unknownKey);
        }
        #endregion

        #region private methods
        private string GetActiveWindowTitle()
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

        private bool GetProjectIdIfKnown(ListViewItem lvi)
        {
            var id = string.Empty;
            // search window titles, if it matches we are good for sure...
            foreach (var p in projects)
            {
                if (p.WindowTitle == lvi.SubItems[1].Text)
                {
                    if (!string.IsNullOrWhiteSpace(p.ProjName))
                    {
                        lvi.SubItems[5].Text = p.ProjName;
                        lvi.SubItems[6].Text = p.ProjDescription;
                        return true;
                    }
                }

                // test appname
                if (lvi.SubItems[2].Text.IndexOf(p.AppName) > -1)
                {
                    lvi.SubItems[5].Text = p.ProjName;
                    lvi.SubItems[6].Text = p.ProjDescription;
                    return true;
                }

                // search for keywords in title
                foreach (var keyWord in p.Keywords)
                {
                    if (lvi.SubItems[1].Text.IndexOf(keyWord) > -1)
                    {
                        lvi.SubItems[5].Text = p.ProjName;
                        lvi.SubItems[6].Text = p.ProjDescription;
                        return true;
                    }

                }
            }
            return false;
        }

        private void UpdateAllItems()
        {
            foreach (ListViewItem lvi in Log.Items)
            {
                if (string.IsNullOrWhiteSpace(lvi.SubItems[5].Text))
                    GetProjectIdIfKnown(lvi);
            }
        }

        #endregion
    }
}

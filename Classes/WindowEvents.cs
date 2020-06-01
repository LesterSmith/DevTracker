//#define DONTSYNC
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using DataHelpers;
using BusinessObjects;
using DevProjects;

namespace DevTracker.Classes
{
    public class WindowEvents
    {
        #region private members
        private string _currentApp;

        private delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        //[DllImport("user32.dll")]
        //static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        private const uint WINEVENT_OUTOFCONTEXT = 0;
        private const uint EVENT_SYSTEM_FOREGROUND = 3;

        //[DllImport("user32.dll")]
        //static extern IntPtr GetForegroundWindow();

        //[DllImport("user32.dll")]
        //static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        //[DllImport("user32.dll")]
        //static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        List<ProjectName> projects = new List<ProjectName>();
        private Process process { get; set; }
        #endregion

        #region ..ctor
        const string AccessDenied = "AccessDenied";

        /// <summary>
        /// NOTE: You can put breakpoints in here, but if you do, then writing becomes iffy in spite of
        /// all that I have done to queue and thread, there are still issues in debugging
        /// b/c stopping in here changes the window event
        /// </summary>
        public WindowEvents()
        {
            WinEventProcesss wep;
            TopOfCode:
            try
            {
                // get a queue item if it exists
                while (true)
                {
                    lock (Globals.SyncLockObject)
                    {
                        if (Globals.WinEventQueue.Count.Equals(0))
                        {
                            Globals.WindowEventThreadRunning = false;
                            return;
                        }

                        wep = Globals.WinEventQueue.Peek();
                        Globals.WinEventQueue.Dequeue();
                        break;
                    }
                }

                var now = wep.Starttime; // time window changed
                var title = wep.MyWindowEvent.WindowTitle;

                if (string.IsNullOrWhiteSpace(title)) return;

                var accessDenied = false;
                var moduleName = wep.MyWindowEvent.ModuleName;
                string displayName = wep.MyWindowEvent.UserDisplayName;

                var devPrjName = string.Empty;

                // We set some properties so the file watcher knows who is running
                _currentApp = wep.MyWindowEvent.AppName;

                bool writeDB = false;

                /* make project extraction generic with regex */
                // new code ***** NOTE: THE ideMATCHES TABLE SHOULD ONLY HAVE IDES AND SSMS (DBMGRs) ****
                // **** WE ARE DOING THIS FOR SSMS THO NOT AN IDE B/C SO MUCH TIME IS SPENT THERE
                // **** AND EVEN GROUPING BY A SERVER.DBNAME MAY GIVE A CLUE TO THE DEV PROJECT
                IDEMatch ideMatchObject = null;

                // ssms failing to update project name here and the reason is that ide.DBUnknown is
                //  likely not set b/c ide switches when window changes, .^. some switch must reside in globals
                // that will persist between window changes.. and cache updates
                // i think that is what the index in the match table was
                // about and then I forgot its purpose and removed it

                //NOTE: cfp.GetProjectName also sets writeDB base on multiple checks 
                // including the config option RECORDAPPS so the decision whether
                // to record this window is made there
                var cfp = new CheckForProjectName();
                Tuple<string, IDEMatch, bool> cfpObject = cfp.GetProjectName(title, accessDenied, _currentApp, writeDB);
                devPrjName = cfpObject.Item1;
                writeDB = cfpObject.Item3;
                ideMatchObject = cfpObject.Item2;

                // if we are writing this window, and devProjectName not set yet
                // see if a known project name is being worked on by a non IDE
                if (writeDB)
                {
                    // check to see if the window title contains a known project name
                    if (string.IsNullOrWhiteSpace(devPrjName))
                    {
                        var s = cfp.IsProjectInNonIDETitle(title);
                        if (!string.IsNullOrWhiteSpace(s))
                            devPrjName = s;
                    }
                }
                else
                {
                    // one or more 
                    goto TopOfCode;
                }

                // try to get syncId from DevProjects
                var hlpr = new DHWindowEvents();
                if (ideMatchObject != null && !ideMatchObject.IsDBEngine && !string.IsNullOrWhiteSpace(devPrjName))
                {
                    MaintainProject mp = new MaintainProject();
                    // bypass next line until we get fw debugged
#if !DONTSYNC
                    wep.MyWindowEvent.SyncID = mp.GetProjectSyncIDForProjectName(devPrjName, _currentApp);
#endif
                    // here we should update any WindowEvents that have been created by this appname
                    // and for this project w/o a syncid
                    if (!string.IsNullOrWhiteSpace(wep.MyWindowEvent.SyncID))
                        hlpr.UpdateWindowEventsWithSyncID(devPrjName, _currentApp, wep.MyWindowEvent.SyncID);
                }

                WindowEvent item;

                /* Here, we are going to record the window in db
                 * there are other threads, FileWatcher, changing this object 
                 * and we want no conflict  */
                item = wep.MyWindowEvent;
                if (item.AppName == "ApplicationFrameHost" && item.WindowTitle.Contains("Solitaire"))
                    item.AppName = item.WindowTitle;

#if DEBUG
                if (!string.IsNullOrWhiteSpace(item.DevProjectName) && item.DevProjectName.Equals("devenv"))
                    Debug.WriteLine(item.DevProjectName);

                if (ideMatchObject != null && devPrjName == "DevTracker" && ideMatchObject.AppName == "ssms")
                    Debug.WriteLine("Conflict Devtracker should not be the project name for ssms");

                //NOTE: 4 / 27 / 2020 discontinued this doing anything...except writing bad data notes
                if (ideMatchObject != null && !ideMatchObject.IsIde && !devPrjName.ToLower().Contains(".sql"))
                {
                    // if next line true
                    if ((!string.IsNullOrWhiteSpace(ideMatchObject.AlternateProjName) && devPrjName == ideMatchObject.AlternateProjName) || devPrjName == ".")
                        Debug.WriteLine($"Bad Project Name: {devPrjName}");
                    else
                    {
                        // this is a check for convoluted name going to DevProjectName
                        if ("Connect.to_Repor._DevTrack.".Contains(devPrjName) || devPrjName.EndsWith("."))
                            Debug.WriteLine($"bad project name = {devPrjName}");

                        //NOTE: we should not set an unknown value in the devproject table
                        // the way the sproc is written, this call will insert a new project with unknown path
                        // if the project does not exist, and if it does exist, this call will not update
                        // the path b/c this call is passing xxUnknown as the the path, 
                        // if this is the way a project gets created in DevProjects, it will only get the correct path
                        // from the save of a project file, which only get done when a new project is created
                        else if (string.IsNullOrWhiteSpace(devPrjName) || string.IsNullOrWhiteSpace(ideMatchObject.UnknownValue))
                            Debug.WriteLine($"Missing Data, Project: {devPrjName}  Path: {ideMatchObject.UnknownValue}");
                        else
                            // Not sure we want to do this here anymore since
                            // determining a project name is not guaranteed to be
                            // correct as it is in the fileanalyzer
                            // when development of DevTracker was begun, windowevents
                            // was the only way we had of possibly getting the project
                            // name, but it did not, nor does it now have a way to get
                            // the path.  FileAnalyzer on the other hand has an exact way of
                            // deriving the path to the projectFile .xxproj so Les has made
                            // the decision of stopping what is at best doing half the job
                            Debug.WriteLine($"**** Would have checked for writing {devPrjName} to DevProjects");
                        // CheckForInsertingNewProjectPath(devPrjName, ideMatchObject.UnknownValue, Environment.UserName, Environment.MachineName, ideMatchObject.AppName, ideMatchObject.IsDBEngine);

                    }
                }
                if (item.AppName == "ssms" && item.DevProjectName == "Microsoft")
                    Debug.WriteLine("Bad Project name");
#endif
                item.DevProjectName = devPrjName;
                hlpr = new DHWindowEvents();
                int rows = hlpr.InsertWindowEvent(item);

                goto TopOfCode; // check for more queue entries
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            goto TopOfCode;
        }

        #endregion

        #region private methods
        //private string IsProjectInNonIDETitle(string title)
        //{
        //    var hlpr = new DHMisc();
        //    var projects = hlpr.GetDevProjects();
        //    var prjObject = projects.Find(x => title.Contains(x.DevProjectName));
        //    return prjObject != null ? prjObject.DevProjectName : string.Empty;
        //}


        #endregion
    }
}

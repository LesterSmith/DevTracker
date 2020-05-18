using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;
using DataHelpers;
using BusinessObjects;
using DevProjects;

namespace DevTracker.Classes
{
    public class WindowEvents
    {
        #region private members
        private string _currentApp;
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
        //        public WindowEvents(Process p, ListView log)
        //        {
        //            try
        //            {
        //                var now = DateTime.Now; // time window changed
        //                Log = log;
        //                var title = GetActiveWindowTitle();
        //#if DEBUG
        //                Debug.WriteLine("**** " + title);
        //#endif                
        //                if (title == null) return;

        //                var accessDenied = false;
        //                var moduleName = string.Empty;
        //                try
        //                {
        //                    moduleName = p.MainModule.ModuleName;
        //                }
        //                catch (Win32Exception ex)
        //                {
        //                    // process access denied b/c it is running as admin 
        //                    moduleName = "Process-Access Denied";
        //                    accessDenied = true;
        //                }
        //                string displayName;
        //                try
        //                {
        //                    displayName = UserPrincipal.Current.DisplayName;
        //                }
        //                catch (Exception ex)
        //                {
        //                    displayName = Environment.UserName;
        //                }

        //                if (Globals.LastWindowEvent == null)
        //                    Globals.StartTime = now;

        //                TimeSpan elapsedTime = now - Globals.StartTime;
        //                Globals.StartTime = now;  // current window change time

        //                var devPrjName = string.Empty;

        //                // We set some globals so the file watcher knows who is running
        //                try
        //                {
        //                     _currentApp = Globals.CurrentApp =!accessDenied ? p.ProcessName : AccessDenied;
        //                }
        //                catch (Exception ex)
        //                {
        //                    Debug.WriteLine(ex.Message);
        //                    Globals.CurrentApp = "Unknown";
        //                }

        //                bool writeDB = false;

        //                /* make project extraction generic with regex */
        //                // new code ***** NOTE: THE ideMATCHES TABLE SHOULD ONLY HAVE IDES AND SSMS ****
        //                // **** WE ARE DOING THIS FOR SSMS THO NOT AN IDE B/C SO MUCH TIME IS SPENT THERE
        //                // **** AND EVEN GROUPING BY A SERVER.DBNAME MAY GIVE A CLUE TO THE DEV PROJECT
        //                IDEMatch ideMatchObject = null;

        //                //new evidence for mulltiple IDEMatch objects for the same IDE i.e., devenv
        //                // title = "Add New Item - BusinessObjects"
        //                // title = "Add Existing Item - BusinessObjects"
        //                // title = "Reference Manager - DevTrkrReports"

        //                // ssms failing to update project name here and the reason is that ide.DBUnknown is
        //                //  likely not set b/c ide switches when window changes, .^. some switch must reside in globals
        //                // that will persist between window changes.. and cache updates
        //                // i think that is what the index in the match table was
        //                // about and then I forgot its purpose and removed it

        //                //NOTE: simplifying problem by updating below for whichever app when we find a new match
        //                //bool foundIDE = false;
        //                foreach (var ide in Globals.IDEMatches)
        //                {
        //                    if (!accessDenied && _currentApp.ToLower() == ide.AppName)
        //                    {
        //                        var pat = ide.Regex;
        //                        var m = Regex.Match(title, pat, RegexOptions.IgnoreCase);
        //                        devPrjName = string.Empty;
        //                        if (m.Success && m.Groups[ide.RegexGroupName] != null && 
        //                            !string.IsNullOrWhiteSpace(m.Groups[ide.RegexGroupName].Value))
        //                        {
        //#if DEBUG
        //                            if (ide.AppName == "ssms")
        //                                Debug.WriteLine(title); 
        //#endif
        //                            // we found the ide match we are looking for
        //                            ideMatchObject = ide;

        //                            // if we are concatinating two fields in ssms to get server.dbname
        //                            if (!string.IsNullOrWhiteSpace(ide.ProjNameConcat))
        //                            {
        //                                string[] concats = ide.ProjNameConcat.Split('|');
        //                                for (var i=0; i < concats.Length; i++)
        //                                {
        //                                    devPrjName += (i > 0 ? ide.ConcatChar : string.Empty) + m.Groups[concats[i]].Value; 
        //                                }
        //                            }
        //                            else
        //                                devPrjName = m.Groups[ide.RegexGroupName].Value;


        //                            if (!string.IsNullOrWhiteSpace(ide.ProjNameReplaces))
        //                            {
        //                                string[] replaces = ide.ProjNameReplaces.Split('|');
        //                                foreach (string s in replaces)
        //                                {
        //                                    devPrjName = devPrjName.Replace(s, string.Empty).Trim();
        //                                }
        //                            }

        //                            // NOTE: new logic for IDEMatch objects that have AlternateProjName
        //                            // if it is not null, replace devPrjName with it b/c altho we found a project name
        //                            // it is not one we want, so make it what we want (probably the same as the unknown value)
        //                            // so that it will be updated when we find the projname we want...
        //                            // e.g., ssms has master set so DBname = Server.master in table but new logic will be correctable
        //                            // e.g. ssms "not connected" can now have its own match object and will get set to unknow until
        //                            //       user connects to a database which they will have to do in order to do anything in ssms
        //                            if (ideMatchObject.AlternateProjName != null)
        //                                devPrjName = ideMatchObject.AlternateProjName;

        //                            // update project name in windowevents that were written with projname unknown
        //                            UpdateUnknownProjectNameForIDEMatch(devPrjName, ide.AppName, ide.UnknownValue, Environment.MachineName, Environment.UserName);

        //                            // since we have found and processed the ide that we wanted, get out
        //                            writeDB = true;
        //                            goto EndOfGenericCode;
        //                        }
        //                        else /// if (m.Success && string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value))
        //                        {
        //                            // ide has no project open yet set as unknown
        //                            devPrjName = ide.UnknownValue; ;
        //                            //ide.DBUnknown = true;
        //                            writeDB = true;
        //                            continue;  // loop to see if another IDEMatch row will get the projectname
        //                        }
        //                        // *** removing so we loop to see if another IDEMatch will get the PrjName ...goto EndOfGenericCode;
        //                    }
        //                }
        //                // if at this point we did not find an idematch just an unknown window
        //                EndOfGenericCode:
        //                // end new code

        //                #region old code
        //                /* strt of old cod
        //                if (!accessDenied && Globals.CurrentApp == AppWrapper.AppWrapper.devenv)
        //                {
        //                    var patt = "(?<PrjName>.*?)(?<spacer> - )*Microsoft Visual Studio|Microsoft Visual Studio";
        //                    var m = Regex.Match(title, patt);
        //                    devPrjName = string.Empty;
        //                    if (m.Success && m.Groups["PrjName"] != null && !string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value))
        //                    {
        //                        devPrjName = m.Groups["PrjName"].Value.Replace("(Running)", string.Empty).Replace("(Debugging)", string.Empty).Trim();
        //                        if (Globals.VSUnknowns)
        //                        {
        //                            Globals.VSUnknowns = false;
        //                            UpdateUnknownProjectNameinVSCode(devPrjName, AppWrapper.AppWrapper.devenv, AppWrapper.AppWrapper.VSUnknown);
        //                        }
        //                    }
        //                    else // regex failed to get prjname 
        //                    {
        //                        /* this is one place where project name can be set to devenv 
        //                         * and should not be                                       
        //                        //devPrjName = p.ProcessName;
        //                        devPrjName = AppWrapper.AppWrapper.VSUnknown; // "devenvUnKnown"; 
        //                        Globals.VSUnknowns = true;
        //                    }
        //                }
        //                else if (!accessDenied && Globals.CurrentApp == AppWrapper.AppWrapper.VSCode)
        //                {
        //                    // we are in VSCode 
        //                    var pattVSCode = "^(?<FileName>.*?) - (?<PrjName>.*?) - Visual Studio Code|Welcome - Visual Studio Code|Open Folder";
        //                    var m = Regex.Match(title, pattVSCode);
        //                    devPrjName = string.Empty;
        //                    if (m.Success && m.Groups["PrjName"] != null && !string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value))
        //                    {
        //                        devPrjName = m.Groups["PrjName"].Value;
        //                        if (Globals.VSCodeUnknowns)
        //                        {
        //                            Globals.VSCodeUnknowns = false;
        //                            UpdateUnknownProjectNameinVSCode(devPrjName, Globals.CurrentApp, AppWrapper.AppWrapper.VSCodeUnknown);
        //                        }
        //                    }
        //                    else if (m.Success && string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value))
        //                    {
        //                        // in vscode the 
        //                        // developer has not opened a folder yet so mark
        //                        // the project name unknown until VSCode opens a folder and then
        //                        // we can update the database records
        //                        devPrjName = AppWrapper.AppWrapper.VSCodeUnknown;
        //                        Globals.VSCodeUnknowns = true;
        //                    }
        //                    else
        //                    {
        //                        devPrjName = AppWrapper.AppWrapper.VSCodeUnknown;
        //                        Globals.VSCodeUnknowns = true;
        //                    }
        //                }
        //                else 
        //                {
        //                    // current app is not VS or VSCode we will only be able to determine
        //                    // the project name if this app saves a file to a known project path
        //                    devPrjName = !accessDenied ? p.ProcessName : AccessDenied;
        //                    if (devPrjName == "devenv")
        //                    {
        //                        devPrjName = AppWrapper.AppWrapper.VSUnknown;
        //                        Globals.VSUnknowns = true;
        //                    } 
        //                }
        //                 end of generic changes */

        //                //appears to be so FileWatcher can pick it out to use in creating FileAnalyzer
        //                // could be set in Globals.LastWindowEvent object not in two places
        //                // below is wrong b/c it is updating the last window that we are no longer in
        //                //Globals.LastWindowEvent.DevProjectName = devPrjName;

        //                // in practice, if you do not record all events, you could later want 
        //                // to see some that would not be recorded
        //                //if (title == "Window Change Log" || title == "Project Description" || p.ProcessName == "DevTracker" || p.ProcessName == "explorer")
        //                //    return;
        //                #endregion
        //                //var currApp = !accessDenied ? p.ProcessName : AccessDenied;

        //                // see if we are interested in recording this window
        //                // if writeDB set, then we already know to write this window b/c of ideMatch found
        //                if (!writeDB)
        //                {
        //                    var appConfig = Globals.ConfigOptions.Find(x => x.Name == "RECORDAPPS");
        //                    if (appConfig != null)
        //                    {
        //                        switch (appConfig.Value)
        //                        {
        //                            case "A":
        //                                writeDB = true;
        //                                break;
        //                            case "S":
        //                                var interestingApp = Globals.NotableApplications.Find(o => o.AppName.ToLower() == _currentApp.ToLower());
        //                                writeDB = (interestingApp != null);
        //                                break;
        //                        }
        //                    } 
        //                }

        //                if (_currentApp.ToLower() == "explorer") 
        //                    writeDB = false; //  forget 

        //                // if we are writing this window, and devProjectName not set yet
        //                // see if a known project name is being worked on by a non IDE
        //                //TODO in a large shop this may be time consuming
        //                if (writeDB && string.IsNullOrWhiteSpace(devPrjName))
        //                {
        //                    devPrjName = IsProjectInNonIDETitle(title);
        //                }


        //                WindowEvent item;

        //                /* there are other threads, FileWatcher, changing this object 
        //                   and we want no conflict  */
        //                if (Globals.LastWindowEvent != null)
        //                {
        //                    lock (Globals.LastWindowEvent)
        //                    {
        //                        item = Globals.LastWindowEvent;
        //                        item.EndTime = now;
        //                        Globals.StartTime = now;
        //                        if (writeDB)
        //                        {
        //                            var hlpr = new DHWindowEvents(AppWrapper.AppWrapper.DevTrkrConnectionString);
        //                            if (item.DevProjectName.Equals("devenv"))
        //                                Debug.WriteLine(item.DevProjectName);

        //                            if (ideMatchObject != null && !ideMatchObject.IsIde && !devPrjName.ToLower().Contains(".sql"))
        //                            {
        //                                // if next line true
        //                                if ((!string.IsNullOrWhiteSpace(ideMatchObject.AlternateProjName) && devPrjName == ideMatchObject.AlternateProjName) || devPrjName == ".")
        //                                    Debug.WriteLine(devPrjName);
        //                                else
        //                                    CheckForInsertingNewProjectPath(devPrjName, ideMatchObject.UnknownValue, Environment.UserName, Environment.MachineName, ideMatchObject.AppName);
        //                            }
        //                            int rows;
        //                            if (_currentApp.ToLower() == "explorer")
        //                            {
        //                                Debug.WriteLine("Dont write explorer");
        //                            }
        //                            rows = hlpr.InsertWindowEvent(item);
        //                        }
        //                    }
        //                }

        //                var appNName = !accessDenied ? p.ProcessName : AccessDenied;
        //                item = new WindowEvent
        //                {
        //                    ID = Guid.NewGuid().ToString(),
        //                    StartTime = now,
        //                    WindowTitle = title,
        //                    AppName = appNName,
        //                    ModuleName = moduleName,
        //                    EndTime = DateTime.MinValue,
        //                    DevProjectName = devPrjName,
        //                    ITProjectID = string.Empty,
        //                    UserName = Environment.UserName,
        //                    MachineName = Environment.MachineName,
        //                    UserDisplayName = displayName
        //                };
        //                Globals.LastWindowEvent = item;

        //                const string comma = ",";
        //#if DEBUG
        //                Debug.Write("*******" + title + comma +
        //                            _startTime.ToString("HH:mm:ss") + comma +
        //                            item.EndTime.ToString("HH:mm:ss") + comma +
        //                            item.AppName + comma +
        //                            item.ModuleName + comma +
        //                            devPrjName + Environment.NewLine);

        //#endif

        //            }
        //            catch (Exception ex)
        //            {
        //                Debug.Print(ex.Message);
        //            }
        //        }



        /// <summary>
        /// NOTE: new constructor
        /// NOTE: You can put breakpoints in here, but if you do, then writing becomes iffy in spite of
        /// all that I have done to queue and thread, there are still issues in debugging
        /// b/c stopping in here changes the window event
        /// </summary>
        public WindowEvents(/* blank constructor to process the queue */)
        {
            try
            {
                WinEventProcesss wep;

                TopOfCode:
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

                // We set some globals so the file watcher knows who is running
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

                //NOTE: simplifying problem by updating below for whichever app when we find a new match
                //bool foundIDE = false;

                var cfp = new CheckForProjectName();
                devPrjName = cfp.GetProjectName(title, ref accessDenied, _currentApp, out ideMatchObject, ref writeDB);

                #region code moved to CheckForProjectName classs b/c other objects need the code
                //                foreach (var ide in Globals.IDEMatches)
                //                {
                //                    if (!accessDenied && _currentApp.ToLower() == ide.AppName)
                //                    {
                //                        var pat = ide.Regex;
                //                        var m = Regex.Match(title, pat, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                //                        devPrjName = string.Empty;
                //                        if (m.Success && m.Groups[ide.RegexGroupName] != null &&
                //                            !string.IsNullOrWhiteSpace(m.Groups[ide.RegexGroupName].Value))
                //                        {
                //#if DEBUG
                //                            if (ide.AppName == "ssms")
                //                                Debug.WriteLine(title);
                //#endif
                //                            // we found the ide match we are looking for
                //                            ideMatchObject = ide;

                //                            // if we are concatinating two fields in ssms to get server.dbname
                //                            if (!string.IsNullOrWhiteSpace(ide.ProjNameConcat))
                //                            {
                //                                string[] concats = ide.ProjNameConcat.Split('|');
                //                                for (var i = 0; i < concats.Length; i++)
                //                                {
                //                                    if (!string.IsNullOrWhiteSpace(m.Groups[concats[i]].Captures[0].ToString()))
                //                                        devPrjName += (i > 0 ? ide.ConcatChar : string.Empty) + m.Groups[concats[i]].Captures[0];
                //                                }
                //                            }
                //                            else
                //                                devPrjName = m.Groups[ide.RegexGroupName].Value;


                //                            if (!string.IsNullOrWhiteSpace(ide.ProjNameReplaces))
                //                            {
                //                                string[] replaces = ide.ProjNameReplaces.Split('|');
                //                                foreach (string s in replaces)
                //                                {
                //                                    devPrjName = devPrjName.Replace(s, string.Empty).Trim();
                //                                }
                //                            }

                //                            // NOTE: new logic for IDEMatch objects that have AlternateProjName
                //                            // if it is not null, replace devPrjName with it b/c altho we found a project name
                //                            // it is not one we want, so make it what we want (probably the same as the unknown value)
                //                            // so that it will be updated when we find the projname we want...
                //                            // e.g., ssms has master set so DBname = Server.master in table but new logic will be correctable
                //                            // e.g. ssms "not connected" can now have its own match object and will get set to unknow until
                //                            //       user connects to a database which they will have to do in order to do anything in ssms
                //                            // but there is no need to run an update if the devPrjName == AlternateProjName b/c it is already
                //                            // that in database; we have to wait til we get a valid project name
                //                            if (!string.IsNullOrWhiteSpace(ideMatchObject.AlternateProjName))
                //                            {
                //                                devPrjName = ideMatchObject.AlternateProjName;
                //                            }
                //                            else
                //                            {
                //                                // update project name in windowevents that were written with projname unknown
                //                                if (!string.IsNullOrWhiteSpace(devPrjName))
                //                                    UpdateUnknownProjectNameForIDEMatch(devPrjName, ide.AppName, ide.UnknownValue, Environment.MachineName, Environment.UserName);
                //                            }

                //                            // since we have found and processed the ide that we wanted, get out
                //                            writeDB = true;
                //                            goto EndOfGenericCode;
                //                        }
                //                        else /// if (m.Success && string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value))
                //                        {
                //                            // ide has no project open yet set as unknown
                //                            devPrjName = ide.UnknownValue;
                //                            //ide.DBUnknown = true;
                //                            writeDB = true;
                //                            continue;  // loop to see if another IDEMatch row will get the projectname
                //                        }
                //                        // *** removing so we loop to see if another IDEMatch will get the PrjName ...goto EndOfGenericCode;
                //                    }
                //                }
                //                // if at this point we did not find an idematch just an unknown window
                //                EndOfGenericCode:


                //// see if we are interested in recording this window
                //// NOTE: we may always want to write the window to DB b/c the company may want to know every app being used
                //// especially if we want to run the Developer(user) Detail Report
                //// if writeDB set, then we already know to write this window b/c of ideMatch found
                //if (!writeDB)
                //{
                //    var appConfig = Globals.ConfigOptions.Find(x => x.Name == "RECORDAPPS");
                //    if (appConfig != null)
                //    {
                //        switch (appConfig.Value)
                //        {
                //            case "A":
                //                writeDB = true;
                //                break;
                //            case "S":
                //                var interestingApp = Globals.NotableApplications.Find(o => o.AppName.ToLower() == _currentApp.ToLower());
                //                writeDB = (interestingApp != null);
                //                break;
                //        }
                //    }
                //}

                //if (_currentApp.ToLower() == "explorer")
                //    writeDB = false; //  forget 
                // end new code
                #endregion

                // if we are writing this window, and devProjectName not set yet
                // see if a known project name is being worked on by a non IDE
                if (writeDB)
                {
                    // check to see if the window title contains a known project name
                    if (string.IsNullOrWhiteSpace(devPrjName))
                    {
                        var s = IsProjectInNonIDETitle(title);
                        if (!string.IsNullOrWhiteSpace(s))
                            devPrjName = s;
                    }
                }
                else
                {
                    goto TopOfCode;
                }

                // try to get syncId from DevProjects
                if (ideMatchObject != null && !ideMatchObject.IsDBEngine && !string.IsNullOrWhiteSpace(devPrjName))
                {
                    MaintainProject mp = new MaintainProject();
                    wep.MyWindowEvent.SyncID = mp.GetProjectSyncIDForProjectName(devPrjName, _currentApp);
                }

                WindowEvent item;

                /* Here, we are going to record the window in db
                 * there are other threads, FileWatcher, changing this object 
                 * and we want no conflict  */
                item = wep.MyWindowEvent;
                if (item.AppName == "ApplicationFrameHost" && item.WindowTitle.Contains("Solitaire"))
                    item.AppName = item.WindowTitle;

                if (!string.IsNullOrWhiteSpace(item.DevProjectName) && item.DevProjectName.Equals("devenv"))
                    Debug.WriteLine(item.DevProjectName);

                if (ideMatchObject != null && devPrjName == "DevTracker" && ideMatchObject.AppName == "ssms")
                    Debug.WriteLine("Conflict Devtracker should not be the project name for ssms");

                //NOTE: 4 / 27 / 2020 discontinued this doing anything...
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

                item.DevProjectName = devPrjName;
#if DEBUG
                if (item.AppName == "ssms" && item.DevProjectName == "Microsoft")
                    Debug.WriteLine("Bad Project name");
#endif
                var hlpr = new DHWindowEvents(AppWrapper.AppWrapper.DevTrkrConnectionString);
                int rows = hlpr.InsertWindowEvent(item);


                const string comma = ",";
#if DEBUG
                Debug.Write("******* " + title + comma +
                            _startTime.ToString("HH:mm:ss") + comma +
                            wep.MyWindowEvent.EndTime.ToString("HH:mm:ss") + comma +
                            wep.MyWindowEvent.AppName + comma +
                            wep.MyWindowEvent.ModuleName + comma +
                            devPrjName + Environment.NewLine);

#endif

                goto TopOfCode; // check for more queue entries
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
        }

        #endregion

        #region private methods
        private string IsProjectInNonIDETitle(string title)
        {
            var prjObject = Globals.ProjectList.Find(x => title.Contains(x.DevProjectName));
            return prjObject != null ? prjObject.DevProjectName : string.Empty;
        }


        #endregion
    }
}

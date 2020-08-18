using System;
using System.Collections.Generic;
using System.Threading;
using DataHelpers;
using BusinessObjects;
using System.DirectoryServices.AccountManagement;
//using OutlookCalendar; // add project and reference when uncommenting
namespace DevTracker.Classes
{
    public static class Startup
    {
        public static System.Windows.Forms.Timer CacheTimer { get; set; }
        public static void Init()
        {
            CacheTimer = new System.Windows.Forms.Timer
            {
                Enabled = false,
                Interval = 30
            };

            Globals.WinEventQueue = new Queue<BusinessObjects.WinEventProcesss>();
            Globals.FileChangeQueue = new Queue<FileChange>();
            Globals.SyncLockObject = new DummyLockObject();
            Globals.FilesLockObject = new DummyLockObject();

            // Load cached data which is expected to be in memory for fast access
            SetupCachedDatabaseData();

            // start up window change event tracking
            var o = Globals.ConfigOptions.Find(x => x.Name == AppWrapper.AppWrapper.WindowTypeEvents);
            Globals.WinEventType = int.Parse(o.Value) == 0 ? AppWrapper.AppWrapper.WindowEventType.EventHook : AppWrapper.AppWrapper.WindowEventType.Polling;

            if (Globals.WinEventType == AppWrapper.AppWrapper.WindowEventType.EventHook)
                Globals.WindowChangeEventHandler = new WindowChangeEvents();
            else
            {
                Globals.WindowChangeEventHandler = new WindowChangeEvents(true);
                WindowPolling.StartPolling();
            }

            // dont start filewatcher until window watcher is running
            Globals.FileWatchr = new FileWatcher();
        }

        public static void SetupCachedDatabaseData()
        {
            lock (Globals.SyncLockObject)
            {
                CacheTimer.Enabled = false;
                var hlpr = new DHMisc(AppWrapper.AppWrapper.DevTrkrConnectionString);
                Globals.NotableFiles = hlpr.GetNotableFileExtensions();
                Globals.IDEMatches = hlpr.GetProjectNameMatches();
                Globals.NotableApplications = hlpr.GetNotableApplications();
                Globals.ConfigOptions = hlpr.GetConfigOptions();

                AppWrapper.AppWrapper.UserPermissionLevel = hlpr.GetCurrentUserPermissionLevel(Environment.UserName);

                //NOTE: ProjectList will grow so large not feasible to maintain in cache
                // Also, only used on occassion, so removing 6/2/2020
                //Globals.ProjectList = hlpr.GetDevProjects(Environment.UserName, Environment.MachineName);

                // following code is obsolete since we save only develpment files and 
                // we no longer need this FilesToSave property
                //var fso = Globals.ConfigOptions.Find(o => o.Name == "RECORDFILES");
                //if (fso != null)
                //    Globals.FilesToSave =
                //        fso.Value.Equals("A") ? FileSaveOption.All
                //        : fso.Value.Equals("N") ? FileSaveOption.None
                //        : fso.Value.Equals("S") ? FileSaveOption.Selected
                //        : FileSaveOption.None;

                var ce = Globals.ConfigOptions.Find(o => o.Name == "CACHEEXPIRATIONTIME");
                Globals.CacheTimeout = ce != null ? int.Parse(ce.Value) : 15;

                // set up current user displayname
                try
                {
                    Globals.DisplayName = UserPrincipal.Current.DisplayName;
                }
                catch (Exception ex)
                {
                    Globals.DisplayName = Environment.UserName;
                }

                //NOTE: discontinued this 3/11/20 at 11:35am b/c windows has a Windows Default Lock Screen which
                // appears when the screen is locked, thus uncomplicating the locking situation
                // as it becomes the active screen until unlocked and then the screen that was extant at lock time 
                // starts a new time...(._.) the appname is LockApp.exe
                //Globals.workLock = new CheckForWorkstationLocking();
                //Globals.workLock.Run();

                //NOTE: Calendar querying is checked here and only done once a day to get yesterdays mtgs
                var queryDate = Globals.ConfigOptions.Find(o => o.Name == AppWrapper.AppWrapper.CalendarQueriedTime);
                if (queryDate != null && DateTime.Parse(queryDate.Value) < DateTime.Today)
                {
                    //NOTE ***** we started getting contextdeadlocks when this was implemented
                    //TODO query for today must be implemented here, next line is a dummy
                    // in the future we must read the calendar and write the data to the Meetings table
                    // for now we simply write one calendar entry per day.

                   //TODO: uncomment to run simulated calendar query _ = new CalendarQuery(DateTime.Today, true);

                    // now update the date in configoptions so we won't do this again today
                    queryDate.Value = DateTime.Today.ToString("MM/dd/yyyy HH:mm:ss");
                    _ = hlpr.InsertUpdateConfigOptions(queryDate);
                }

                CacheTimer.Tick += new EventHandler(CacheTimerProcessser);
                CacheTimer.Interval = Globals.CacheTimeout * 1000 * 60;
                CacheTimer.Enabled = true;

            }
        }

        public static void ShutDown()
        {
            CacheTimer.Stop();
            CacheTimer.Enabled = false;
            CacheTimer.Dispose();
            Globals.WindowChangeEventHandler.Dispose();
            //Globals.workLock.Dispose();
            Globals.FileWatchr.Dispose();
            WindowPolling.SuspendWindowPolling();
            WindowPolling.Timer.Dispose();
        }

        /// <summary>
        /// Start the cache update on a separate thread to try to
        /// mitigate the ContextDeadlockExceptions
        /// </summary>
        private static void CacheTimerProcessser(object sender, EventArgs e)
        {
            Thread t = new Thread(new ThreadStart(UpdateCachedData));
            t.Start();
        }

        private static void UpdateCachedData()
        {
            SetupCachedDatabaseData();
        }
    }
}

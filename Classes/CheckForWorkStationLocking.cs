using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Diagnostics;
using BusinessObjects;
using DataHelpers;
using System.DirectoryServices.AccountManagement;
namespace DevTracker.Classes
{
    public enum WinEvntState
    {
        Unknown,
        StillCurrent,
        InDatabase
    }

    /// <summary>
    /// NOT USED Class
    /// </summary>
    public class CheckForWorkstationLocking : IDisposable
    {
        private SessionSwitchEventHandler sseh;
        private bool _locked = false;
        private DateTime LastLockStartTime { get; set; }
        private DateTime LockStartTime { get; set; }
        private DateTime LockEndTime { get; set; }
        private WindowEvent _lastWindowEvent { get; set; }
        private const string locked = "ComputerLocked";
        private WinEvntState _lastWinState = WinEvntState.Unknown;
        public void Run()
        {
            //LastLockStartTime = DateTime.Now;
            sseh = new SessionSwitchEventHandler(ComputerLockCheck);
            SystemEvents.SessionSwitch += sseh;
        }

        private void ComputerLockCheck(object sender, SessionSwitchEventArgs e)
        {
            /* when computer is locked, we must simullate a window change
             * which will record the current Globals.LastWindowEvent so that time
             * is not charged to the current window, even though it is still the
             * current window.  Then when the computer is unlocked we start the
             * clock running on the current window again.  Tho this will cause two
             * entries in the db table, the sum of the two rows will be correct.
             * The mechanics of doing this will be accomplished in the switch construct
             * below.
             * 1) upon lock, write 
             */
            var status = _locked ? "Locked" : "Unlocked";
            Debug.WriteLine($"Lock Status: {status} Reason: {e.Reason}");
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock: 
                    Debug.WriteLine($"Lock Encountered at {DateTime.Now}  Status: {status}");
                    if (!_locked)
                    {
                        // first time locked
                        if (LastLockStartTime == null)
                            LastLockStartTime = DateTime.Now;
                        else
                        {
                            // this lock and unlock event is firing twice, this is a hack to try to ignore
                            int secondsDiff = ((TimeSpan)(DateTime.Now - LastLockStartTime)).Seconds;
                            if (secondsDiff < 10)
                                return;
                        }
                        Locked();
                    }
                    break;
                case SessionSwitchReason.SessionUnlock: 
                    Debug.WriteLine($"UnLock Encountered at {DateTime.Now} Status: {status}");
                    if (_locked)
                    {
                        //if (secondsDiff < )
                        Unlocked();
                    }
                    break;
            }
        }
        private void Locked()
        {
            var accessDenied = false;
            var _currentApp = Globals.LastWindowEvent.AppName;
            IDEMatch ideMatchObject = null;
            bool writeDB = false;

            _locked = true;

            // turn off polling while locked, so we will not see any window change while locked
            // therefore LastWindowEvent should be the one created below when we detect unlock
            WindowPolling.SuspendWindowPolling();

            // Try to get the project name for the Globals.LastWindowEvent
            var cfp = new Classes.CheckForProjectName();
            var devProjectName = cfp.GetProjectName(Globals.LastWindowEvent.WindowTitle, ref accessDenied, Globals.LastWindowEvent.AppName, out ideMatchObject, ref writeDB);
            if (!string.IsNullOrWhiteSpace(Globals.LastWindowEvent.DevProjectName))
                Globals.LastWindowEvent.DevProjectName = devProjectName;

            var hlpr = new DHWindowEvents(AppWrapper.AppWrapper.DevTrkrConnectionString);

            lock (Globals.SyncLockObject)
            {
                // now, make it look like the current window when the lock occurs is being moved away from
                // by writing it to database
#if DEBUG
                Console.WriteLine($"     ** Locked Writing Time: {LockStartTime} AppName: {Globals.LastWindowEvent.AppName} Title: {Globals.LastWindowEvent.WindowTitle} Project: {Globals.LastWindowEvent.DevProjectName}");
#endif
                Globals.LastWindowEvent.EndTime = LockStartTime;
                hlpr.InsertWindowEvent(Globals.LastWindowEvent);

                // next, start a new LastWindowEvent called ComputerLocked
                // and put it in Globals.LastWindowEvent
                string displayName;
                try
                {
                    displayName = UserPrincipal.Current.DisplayName;
                }
                catch (Exception ex)
                {
                    displayName = Environment.UserName;
                }

                var item = new WindowEvent
                {
                    ID = Guid.NewGuid().ToString(),
                    StartTime = LockStartTime,
                    WindowTitle = locked,
                    AppName = locked,
                    ModuleName = locked,
                    EndTime = LockEndTime,
                    DevProjectName = locked,
                    ITProjectID = string.Empty,
                    UserName = Environment.UserName,
                    MachineName = Environment.MachineName,
                    UserDisplayName = displayName
                };
                Globals.LastWindowEvent = item;
            }
        }

        private void Unlocked()
        {
            LockEndTime = DateTime.Now;
            _locked = false;
            _lastWinState = WinEvntState.Unknown;
            TimeSpan lockedInterval = LockEndTime - LockStartTime;
#if DEBUG
            Console.WriteLine($"     ** UnLocked Writing Time: {LockEndTime} AppName: {Globals.LastWindowEvent.AppName} Title: {Globals.LastWindowEvent.WindowTitle} Project: {Globals.LastWindowEvent.DevProjectName}");
#endif

            var hlpr = new DHWindowEvents(AppWrapper.AppWrapper.DevTrkrConnectionString);
            lock (Globals.SyncLockObject) 
            {
                Globals.LastWindowEvent.EndTime = LockEndTime;
                var row = hlpr.InsertWindowEvent(Globals.LastWindowEvent);

                //next, create a new LastWindowEvent
                // 2) create a locked window event in database
                string displayName;
                try
                {
                    displayName = UserPrincipal.Current.DisplayName;
                }
                catch (Exception ex)
                {
                    displayName = Environment.UserName;
                }
                var item = new WindowEvent
                {
                    ID = Guid.NewGuid().ToString(),
                    StartTime = LockStartTime,
                    WindowTitle = AppWrapper.AppWrapper.AppName,
                    AppName = AppWrapper.AppWrapper.AppName,
                    ModuleName = AppWrapper.AppWrapper.AppName,
                    //EndTime = LockEndTime,
                    DevProjectName = AppWrapper.AppWrapper.AppName,
                    ITProjectID = string.Empty,
                    UserName = Environment.UserName,
                    MachineName = Environment.MachineName,
                    UserDisplayName = displayName
                };
                Globals.LastWindowEvent = item;
                //var rows = hlpr.InsertWindowEvent(item);
            }

            WindowPolling.ResumeWindowPolling();

        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_locked)
            {
                Unlocked();
            }
            SystemEvents.SessionSwitch -= sseh;
        }

        #endregion
    }
}

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Windows.Forms;
using System.Security.Permissions;
using BusinessObjects;
using DataHelpers;
using System.DirectoryServices.AccountManagement;
using DevTracker.Classes;

namespace DevTracker.Forms
{
    /// <summary>
    /// This form may be considered a Hack by some, and may well be, but
    /// it was finally placed here to replace the use of a WindowsContext object
    /// which housed the notifyIcon, but could not provide a handle to be used to
    /// set a hook for WndProc.  This is needed to detect Laptop Lid closing.
    /// After struggling for days to solve the issue, I chose this hack.  The form
    /// will not be visible nor appear in the task bar nor will the app appear in the 
    /// TaskManager, which is the desired effect.  Remove at your own risk.
    /// </summary>
    public partial class MiscContainer : Form
    {
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private ToolStripMenuItem RunReports;
        private ToolStripMenuItem AboutForm;
        private ToolStripMenuItem OptionsForm;

        public MiscContainer()
        {
            InitializeComponent();
            InitializeComponent2();
            this.Left = -5000;
            this.Top = -5000;
            // run startup init processes
            // get configuration variables & start caching timer
            Classes.Startup.Init();
        }

        private void InitializeComponent2()
        {
            TrayIcon = new NotifyIcon();

            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.BalloonTipText = "Instead of double-clicking the Icon, please right-click the Icon and select a context menu option.";
            TrayIcon.BalloonTipTitle = "Use the Context Menu";
            TrayIcon.Text = "DevTrkr Context Menu";

            //The icon is added to the project resources. Here I assume that the name of the file is 'TrayIcon.ico'
            TrayIcon.Icon = Properties.Resources.Role;

            //Optional - handle doubleclicks on the icon:
            TrayIcon.DoubleClick += TrayIcon_DoubleClick;

            //Optional - Add a context menu to the TrayIcon:
            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem();
            RunReports = new ToolStripMenuItem();
            AboutForm = new ToolStripMenuItem();
            OptionsForm = new ToolStripMenuItem();

            TrayIconContextMenu.SuspendLayout();

            // 
            // TrayIconContextMenu
            // 
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[]
            {
                this.CloseMenuItem
            });
            this.TrayIconContextMenu.Name = "TrayIconContextMenu";
            this.TrayIconContextMenu.Size = new Size(153, 70);

            //
            // Form1
            //
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[]
            {
                this.RunReports
            });

            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[]
            {
                this.OptionsForm
            });
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[]
            {
                this.AboutForm
            });

            // 
            // CloseMenuItem
            // 
            this.CloseMenuItem.Name = "CloseMenuItem";
            this.CloseMenuItem.Size = new Size(152, 22);
            this.CloseMenuItem.Text = "Close DevTrkr Application";
            this.CloseMenuItem.Click += new EventHandler(this.CloseMenuItem_Click);

            this.RunReports.Name = "RunReports";
            this.RunReports.Size = new Size(152, 22);
            this.RunReports.Text = "Run Reports";
            this.RunReports.Click += new EventHandler(this.RunForm1_Click);

            this.OptionsForm.Name = "Options";
            this.OptionsForm.Size = new Size(152, 222);
            this.OptionsForm.Text = "Options";
            this.OptionsForm.Click += new EventHandler(this.OptionsForm_Click);

            this.AboutForm.Name = "AboutForm";
            this.AboutForm.Size = new Size(152, 22);
            this.AboutForm.Text = "About DevTracker";
            this.AboutForm.Click += new EventHandler(this.AboutForm_Click);

            TrayIconContextMenu.ResumeLayout(false);

            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
            TrayIcon.Visible = true;
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            //Here you can do stuff if the tray icon is doubleclicked
            TrayIcon.ShowBalloonTip(10000);
        }

        private void RunForm1_Click(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Application.DoEvents();
            var r = new DevTrkrReports.DevTrkrReports();
            r.RunForm();
            TrayIcon.Visible = true;
            Application.DoEvents();
        }

        private void OptionsForm_Click(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Application.DoEvents();
            var o = new Forms.Options();
            o.ShowDialog();
            TrayIcon.Visible = true;
            Application.DoEvents();
        }
        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Application.DoEvents();
            if (MessageBox.Show("Do you really want to close DevTrkr?  Your development activity will no longer be tracked, which may not be a good thing for you.",
                                "Close DevTrkr?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Classes.Startup.ShutDown();
                Application.Exit();
            }
            TrayIcon.Visible = true;
            Application.DoEvents();
        }

        private void AboutForm_Click(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Application.DoEvents();
            var f = new Forms.About();
            f.ShowDialog();
            TrayIcon.Visible = true;
            Application.DoEvents();
        }

        /// <summary>
        /// keep the form out of the taskbar
        /// </summary>
        /// <param name="value"></param>
        protected override void SetVisibleCore(bool value)
        {
            if (!IsHandleCreated)
            {
                this.CreateHandle();
                value = false;
            }
            base.SetVisibleCore(value);
        }


        #region Laptop Lid Action Monitoring
        /* all of the following is for ** Start of Lid Action ** */
        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification",
            CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid, Int32 Flags);

        static Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
        private const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        private const int WM_POWERBROADCAST = 0x0218;
        const int PBT_POWERSETTINGCHANGE = 0x8013;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        private bool? _previousLidState = null;


        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_POWERBROADCAST:
                    Console.WriteLine($"WndProc Message: {m}");
                    OnPowerBroadcast(m.WParam, m.LParam);
                    break;
                case SessionChangeMessage:
                    if (m.WParam.ToInt32() == SessionLockParam)
                        OnSessionLock(); // Do something when locked
                    else if (m.WParam.ToInt32() == SessionUnlockParam)
                        OnSessionUnlock(); // Do something when unlocked
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }


        private void RegisterForPowerNotifications()
        {
            IntPtr handle = this.Handle;
            //Util.LogError("Handle: " + handle.ToString()); //If this line is omitted, then lastError = 1008 which is ERROR_NO_TOKEN, otherwise, lastError = 0
            IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(handle,
                 ref GUID_LIDSWITCH_STATE_CHANGE,
                 DEVICE_NOTIFY_WINDOW_HANDLE);
            //Util.LogError("Registered: " + hLIDSWITCHSTATECHANGE.ToString());
            //Util.LogError("LastError:" + Marshal.GetLastWin32Error().ToString());
        }

        private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
        {
            Console.WriteLine($"OnPowerBroadcast wParam={wParam}, lParam={lParam}");
            if ((int)wParam == PBT_POWERSETTINGCHANGE)
            {
                POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
                //IntPtr pData = (IntPtr)((int)lParam + Marshal.SizeOf(ps));
                //Int32 iData = (Int32)Marshal.PtrToStructure(pData, typeof(Int32));
                if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
                {
                    Console.WriteLine($"ps.PowerSetting: {ps.PowerSetting}");
                    bool isLidOpen = ps.Data != 0;

                    if (!isLidOpen == _previousLidState)
                    {
                        LidStatusChanged(isLidOpen);
                    }

                    _previousLidState = isLidOpen;
                }
            }
        }

        private void LidStatusChanged(bool isLidOpen)
        {
            if (isLidOpen)
            {
                //Do some action on lid open event
                Console.WriteLine("Lid is now open");
            }
            else
            {
                //Do some action on lid close event
                Console.WriteLine("Lid is now closed");
            }
        }


        /// <summary>
        /// This registers for Lid Action and Computer Locking and Unlocking
        /// </summary>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterForPowerNotifications();
            WTSRegisterSessionNotification(this.Handle, NotifyForThisSession);
        }

        // ** End of Lid Action **


        #endregion


        #region Computer Lock Monitoring
        // ** start of lock action **
        [DllImport("wtsapi32.dll")]
        private static extern bool WTSRegisterSessionNotification(IntPtr hWnd, int dwFlags);

        [DllImport("wtsapi32.dll")]
        private static extern bool WTSUnRegisterSessionNotification(IntPtr hWnd);

        private const int NotifyForThisSession = 0; // This session only

        private const int SessionChangeMessage = 0x02B1;
        private const int SessionLockParam = 0x7;
        private const int SessionUnlockParam = 0x8;


        void OnSessionLock()
        {
            //Util.LogError($"** ComputerLocked...{DateTime.Now}");
            Locked();
        }

        void OnSessionUnlock()
        {
            //Util.LogError($"** Computer Unlocked...{DateTime.Now}");
            Unlocked();
        }

        private const string locked = "ComputerLocked";
        DateTime LockStartTime { get; set; }
        DateTime LockEndTime { get; set; }
        private void Locked()
        {
            LockStartTime = DateTime.Now;
            var accessDenied = false;
            var _currentApp = Globals.LastWindowEvent.AppName;
            IDEMatch ideMatchObject = null;
            bool writeDB = false;

            //_locked = true;

            // turn off polling while locked, so we will not see any window change while locked
            // therefore LastWindowEvent should be the one created below when we detect unlock
            WindowPolling.SuspendWindowPolling();

            // Try to get the project name for the Globals.LastWindowEvent
            var cfp = new Classes.CheckForProjectName();
#if DEBUG
            // the following call is not finding the projectname
            Console.WriteLine($"Call cfp.GetProjectNam, title={Globals.LastWindowEvent.WindowTitle}, appname={Globals.LastWindowEvent.AppName}");
#endif           
            Tuple<string, IDEMatch, bool> cfpObject = cfp.GetProjectName(Globals.LastWindowEvent.WindowTitle, accessDenied, Globals.LastWindowEvent.AppName, writeDB);
            string devProjectName = cfpObject.Item1;
            ideMatchObject = cfpObject.Item2;
            writeDB = cfpObject.Item3;
#if DEBUG
            var id = ideMatchObject != null ? ideMatchObject.ID.ToString() : string.Empty;
            Console.WriteLine($"devProjectName={devProjectName}, writeDB={writeDB}, MatchObject = {id}, LastWinEv.DevPrjName={Globals.LastWindowEvent.DevProjectName}");
#endif
            if (string.IsNullOrWhiteSpace(Globals.LastWindowEvent.DevProjectName))
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
            //_locked = false;
            //_lastWinState = WinEvntState.Unknown;
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


        #endregion

        private void MiscContainer_FormClosing(object sender, FormClosingEventArgs e)
        {
            WTSUnRegisterSessionNotification(this.Handle);

        }
    }
}

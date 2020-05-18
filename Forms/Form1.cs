using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
//using System.DirectoryServices.AccountManagement;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Security.Permissions;

namespace DevTracker
{
    public partial class Form1 : System.Windows.Forms.Form
    {
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private ToolStripMenuItem RunForm1;
        private ToolStripMenuItem AboutForm;
        private ToolStripMenuItem OptionsForm;


        //[DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification",
        //    CallingConvention = CallingConvention.StdCall)]
        //private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid,
        //    Int32 Flags);
        //WinEventDelegate dele = null;
        ////delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);
        //IntPtr m_hhook = IntPtr.Zero;

        //[DllImport("user32.dll")]
        //static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        //Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
        //const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        //const int WM_POWERBROADCAST = 0x0218;
        //private const uint WINEVENT_OUTOFCONTEXT = 0;
        //const int PBT_POWERSETTINGCHANGE = 0x8013;
        //private bool? _previousLidState = null;
        //delegate IntPtr WinEventDelegate(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled);


        //internal struct POWERBROADCAST_SETTING
        //{
        //    public Guid PowerSetting;
        //    public uint DataLength;
        //    public byte Data;
        //}
        public Form1()
        {
            InitializeComponent();
            InitializeComponent2();
            this.Left = -5000;
            this.Top = -5000;
            //WinEventDelegate dele = new WinEventDelegate(WinEventProc);
            // if debugging the filewather bypass starting windowwatcher
            //RegisterForPowerNotifications(this.Handle);
            //m_hhook = SetWinEventHook(WM_POWERBROADCAST, WM_POWERBROADCAST, IntPtr.Zero, dele, 0, 0, WINEVENT_OUTOFCONTEXT);
            //Console.WriteLine($"setHook: m_hhook {m_hhook}");
            //_ = new CheckForLaptopCloseOpenLid(this.Handle);
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
            RunForm1 = new ToolStripMenuItem();
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
                this.RunForm1
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

            this.RunForm1.Name = "RunReports";
            this.RunForm1.Size = new Size(152, 22);
            this.RunForm1.Text = "Run Reports";
            this.RunForm1.Click += new EventHandler(this.RunForm1_Click);

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
            //var r = new DevTrkrReports.DevTrkrReports();
            //r.RunForm();
            TrayIcon.Visible = true;
            Application.DoEvents();
        }

        private void OptionsForm_Click(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Application.DoEvents();
            //var o = new Forms.Options();
            //o.ShowDialog();
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
                //Startup.ShutDown();
                Application.Exit();
            }
            TrayIcon.Visible = true;
            Application.DoEvents();
        }

        private void AboutForm_Click(object sender, EventArgs e)
        {
            TrayIcon.Visible = false;
            Application.DoEvents();
            //var f = new Forms.About();
            //f.ShowDialog();
            TrayIcon.Visible = true;
            Application.DoEvents();
        }


        //private void RegisterForPowerNotifications(IntPtr handle)
        //{
        //    Console.WriteLine($"RegisterForPowerNotifications {handle}");
        //    //IntPtr handle = new WindowInteropHelper(Application.Current.Windows[0]).Handle;
        //    //IntPtr handle = this.//new WindowInteropHelper(this).Handle;
        //    IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(handle,
        //         ref GUID_LIDSWITCH_STATE_CHANGE,
        //         DEVICE_NOTIFY_WINDOW_HANDLE);
        //}

        //[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
        //IntPtr WinEventProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    Console.WriteLine($"WParam {wParam}, LParam {lParam}");
        //    switch (msg)
        //    {
        //        case WM_POWERBROADCAST:
        //            OnPowerBroadcast(wParam, lParam);
        //            break;
        //        default:
        //            break;
        //    }
        //    return IntPtr.Zero;
        //}
        //protected override void WndProc(ref Message m)
        //{
        //    Listen for operating system messages.
        //    switch (m.Msg)
        //        {
        //            case WM_POWERBROADCAST:
        //                OnPowerBroadcast(wParam, lParam);
        //                break;
        //            default:
        //                break;
        //        }
        //    return IntPtr.Zero;
        //    base.WndProc(ref m);
        //}
        //private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
        //{
        //    if ((int)wParam == PBT_POWERSETTINGCHANGE)
        //    {
        //        POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
        //        IntPtr pData = (IntPtr)((int)lParam + Marshal.SizeOf(ps));
        //        Int32 iData = (Int32)Marshal.PtrToStructure(pData, typeof(Int32));
        //        if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
        //        {
        //            bool isLidOpen = ps.Data != 0;

        //            if (!isLidOpen == _previousLidState)
        //            {
        //                LidStatusChanged(isLidOpen);
        //            }

        //            _previousLidState = isLidOpen;
        //        }
        //    }
        //}

        //private void LidStatusChanged(bool isLidOpen)
        //{
        //    if (isLidOpen)
        //    {
        //        //Do some action on lid open event
        //        Debug.WriteLine("{0}: Lid opened!", DateTime.Now);
        //    }
        //    else
        //    {
        //        //Do some action on lid close event
        //        Debug.WriteLine("{0}: Lid closed!", DateTime.Now);
        //    }
        //}


        /* all of the following code to the end of this class is for ** Start of Lid Action ** */
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


        //public Fo(IntPtr handle)
        //{
        //    RegisterForPowerNotifications();

        //}

        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_POWERBROADCAST:
                    Console.WriteLine($"WndProc Message: {m}");
                    OnPowerBroadcast(m.WParam, m.LParam);
                    break;
                default:
                    break;
            }
            base.WndProc(ref m);
        }


        private void RegisterForPowerNotifications()
        {
            IntPtr handle = this.Handle;
            Debug.WriteLine("Handle: " + handle.ToString()); //If this line is omitted, then lastError = 1008 which is ERROR_NO_TOKEN, otherwise, lastError = 0
            IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(handle,
                 ref GUID_LIDSWITCH_STATE_CHANGE,
                 DEVICE_NOTIFY_WINDOW_HANDLE);
            Debug.WriteLine("Registered: " + hLIDSWITCHSTATECHANGE.ToString());
            Debug.WriteLine("LastError:" + Marshal.GetLastWin32Error().ToString());
        }

        private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
        {
            Console.WriteLine($"OnPowerBroadcast wParam={wParam}, lParam={lParam}");
            if ((int)wParam == PBT_POWERSETTINGCHANGE)
            {
                POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
                IntPtr pData = (IntPtr)((int)lParam + Marshal.SizeOf(ps));
                Int32 iData = (Int32)Marshal.PtrToStructure(pData, typeof(Int32));
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
        /// attempt to keep the form out of the taskbar
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
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterForPowerNotifications();
        }

        // ** End of Lid Action **

    }
}

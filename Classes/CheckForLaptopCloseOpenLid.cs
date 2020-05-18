using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
//using System.Windows.Interop;

namespace DevTracker.Classes
{
    public partial class CheckForLaptopCloseOpenLid //: Window
    {
        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification",
            CallingConvention = CallingConvention.StdCall)]

        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid,
            Int32 Flags);

        internal struct POWERBROADCAST_SETTING
        {
            public Guid PowerSetting;
            public uint DataLength;
            public byte Data;
        }

        Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
        const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
        const int WM_POWERBROADCAST = 0x0218;
        const int PBT_POWERSETTINGCHANGE = 0x8013;

        private bool? _previousLidState = null;

        public CheckForLaptopCloseOpenLid()
        {
            RegisterForPowerNotifications();
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            HwndSource.FromHwnd(hwnd).AddHook(new HwndSourceHook(WndProc));
        }

        //IntPtr handle = new WindowInteropHelper(Application.Current.Windows[0]).Handle; to this: IntPtr handle = new WindowInteropHelper(this).Handle;
        private void RegisterForPowerNotifications()
        {
            //IntPtr handle = new WindowInteropHelper(Application.Current.Windows[0]).Handle;
            IntPtr handle = this.//new WindowInteropHelper(this).Handle;
            IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(handle,
                 ref GUID_LIDSWITCH_STATE_CHANGE,
                 DEVICE_NOTIFY_WINDOW_HANDLE);
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_POWERBROADCAST:
                    OnPowerBroadcast(wParam, lParam);
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }

        private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
        {
            if ((int)wParam == PBT_POWERSETTINGCHANGE)
            {
                POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
                IntPtr pData = (IntPtr)((int)lParam + Marshal.SizeOf(ps));
                Int32 iData = (Int32)Marshal.PtrToStructure(pData, typeof(Int32));
                if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
                {
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
                Debug.WriteLine("{0}: Lid opened!", DateTime.Now);
            }
            else
            {
                //Do some action on lid close event
                Debug.WriteLine("{0}: Lid closed!", DateTime.Now);
            }
        }

    }
}

//using System;
//using System.Diagnostics;
//using System.Runtime.InteropServices;
//using System.Windows;
//using System.Windows.Interop;

//namespace WpfApplication1
//{
//    /// <summary>
//    /// Interaction logic for MainWindow.xaml
//    /// </summary>
//    public partial class MainWindow : Window
//    {
//        [DllImport(@"User32", SetLastError = true, EntryPoint = "RegisterPowerSettingNotification",
//            CallingConvention = CallingConvention.StdCall)]

//        private static extern IntPtr RegisterPowerSettingNotification(IntPtr hRecipient, ref Guid PowerSettingGuid,
//            Int32 Flags);

//        internal struct POWERBROADCAST_SETTING
//        {
//            public Guid PowerSetting;
//            public uint DataLength;
//            public byte Data;
//        }

//        Guid GUID_LIDSWITCH_STATE_CHANGE = new Guid(0xBA3E0F4D, 0xB817, 0x4094, 0xA2, 0xD1, 0xD5, 0x63, 0x79, 0xE6, 0xA0, 0xF3);
//        const int DEVICE_NOTIFY_WINDOW_HANDLE = 0x00000000;
//        const int WM_POWERBROADCAST = 0x0218;
//        const int PBT_POWERSETTINGCHANGE = 0x8013;

//        private bool? _previousLidState = null;

//        public MainWindow()
//        {
//            InitializeComponent();
//            this.SourceInitialized += MainWindow_SourceInitialized;
//        }

//        void MainWindow_SourceInitialized(object sender, EventArgs e)
//        {
//            RegisterForPowerNotifications();
//            IntPtr hwnd = new WindowInteropHelper(this).Handle;
//            HwndSource.FromHwnd(hwnd).AddHook(new HwndSourceHook(WndProc));
//        }

//        private void RegisterForPowerNotifications()
//        {
//            IntPtr handle = new WindowInteropHelper(Application.Current.Windows[0]).Handle;
//            IntPtr hLIDSWITCHSTATECHANGE = RegisterPowerSettingNotification(handle,
//                 ref GUID_LIDSWITCH_STATE_CHANGE,
//                 DEVICE_NOTIFY_WINDOW_HANDLE);
//        }

//        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
//        {
//            switch (msg)
//            {
//                case WM_POWERBROADCAST:
//                    OnPowerBroadcast(wParam, lParam);
//                    break;
//                default:
//                    break;
//            }
//            return IntPtr.Zero;
//        }

//        private void OnPowerBroadcast(IntPtr wParam, IntPtr lParam)
//        {
//            if ((int)wParam == PBT_POWERSETTINGCHANGE)
//            {
//                POWERBROADCAST_SETTING ps = (POWERBROADCAST_SETTING)Marshal.PtrToStructure(lParam, typeof(POWERBROADCAST_SETTING));
//                IntPtr pData = (IntPtr)((int)lParam + Marshal.SizeOf(ps));
//                Int32 iData = (Int32)Marshal.PtrToStructure(pData, typeof(Int32));
//                if (ps.PowerSetting == GUID_LIDSWITCH_STATE_CHANGE)
//                {
//                    bool isLidOpen = ps.Data != 0;

//                    if (!isLidOpen == _previousLidState)
//                    {
//                        LidStatusChanged(isLidOpen);
//                    }

//                    _previousLidState = isLidOpen;
//                }
//            }
//        }

//        private void LidStatusChanged(bool isLidOpen)
//        {
//            if (isLidOpen)
//            {
//                //Do some action on lid open event
//                Debug.WriteLine("{0}: Lid opened!", DateTime.Now);
//            }
//            else
//            {
//                //Do some action on lid close event
//                Debug.WriteLine("{0}: Lid closed!", DateTime.Now);
//            }
//        }
//    }
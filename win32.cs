using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
namespace DevTracker
{
    class win32
    {
        [DllImport("user32")]
        private static extern UInt32 GetWindowThreadProcessId(
          Int32 hWnd,
          out Int32 lpdwProcessId
        );
        [DllImport("user32")]
        private static extern UInt32 GetWindowThreadProcessId(
          IntPtr hWnd,
          out Int32 lpdwProcessId
        );

        public static Int32 GetWindowProcessID(Int32 hwnd)
        {
            Int32 pid = 1;
            GetWindowThreadProcessId(hwnd, out pid);
            return pid;
        }

        public static Int32 GetWindowProcessID(IntPtr hwnd)
        {
            Int32 pid = 1;
            GetWindowThreadProcessId(hwnd, out pid);
            return pid;
        }

        public win32()
        {

        }
    }
}

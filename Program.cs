using System;
using System.Windows.Forms;
using AppWrapper;
using DevTrackerLogging;
namespace DevTracker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                //Application.Run(new DevTracker.Classes.WCTApplicationContext());
                Application.Run(new Forms.MiscContainer());
            }
            catch (Exception ex)
            {
                _ = new LogError(ex, false, "Program.Main");
            }

        }
    }
}

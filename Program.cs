﻿using System;
using System.Windows.Forms;

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
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new DevTracker.Classes.WCTApplicationContext());
            Application.Run(new Forms.MiscContainer());

        }
    }
}

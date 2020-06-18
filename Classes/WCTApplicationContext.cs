using System;

using System.Windows.Forms;
using System.Drawing;
namespace DevTracker.Classes
{
    /// <summary>
    /// This class allows us to place a context menu in the try icon area w/o a form
    /// </summary>
    public class WCTApplicationContext : ApplicationContext
    {
        private NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private ToolStripMenuItem RunForm1;
        private ToolStripMenuItem AboutForm;
        private ToolStripMenuItem OptionsForm;
        private DevTracker.Classes.WindowChangeEvents WCT;
        public WCTApplicationContext()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);

            InitializeComponent();

            TrayIcon.Visible = true;

            // run startup init processes
            // get configuration variables & start caching timer
            Startup.Init(); 
        }

        private void InitializeComponent()
        {
            TrayIcon = new NotifyIcon();

            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.BalloonTipText = "Instead of double-clicking the Icon, please right-click the Icon and select a context menu option.";
            TrayIcon.BalloonTipTitle = "Use the Context Menu";
            TrayIcon.Text = "DevTracker Context Menu";

            //The icon is added to the project resources. Here I assume that the name of the file is 'TrayIcon.ico'
            TrayIcon.Icon = Properties.Resources.TrayIcon;

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
            this.CloseMenuItem.Text = "Close DevTracker Application";
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
            this.AboutForm.Text  = "About DevTracker";
            this.AboutForm.Click += new EventHandler(this.AboutForm_Click);

            TrayIconContextMenu.ResumeLayout(false);

            TrayIcon.ContextMenuStrip = TrayIconContextMenu;

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
            if (MessageBox.Show("Do you really want to close DevTracker?  Your development activity will no longer be tracked, which may not be a good thing for you.",
                                "Close DevTracker?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                                MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Startup.ShutDown();
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
    }
}

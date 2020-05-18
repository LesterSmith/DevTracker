using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using DataHelpers;
using BusinessObjects;

namespace DevTracker.Forms
{
    public partial class FileWatcher : Form
    {
        private const string _pipe = "|";
        private List<NotableFileExtension> _notableFiles = new List<NotableFileExtension>(); //"cs|vb|csproj|vbproj|txt|frm|config|img|gif|sln|ico|jpg|xml|doc|docx|xls|xlsx|xlst|bmp";
        public FileWatcher()
        {
            InitializeComponent();
        }





        private void FileWatcher_Load(object sender, EventArgs e)
        {
            var hlpr = new DHMisc(AppWrapper.AppWrapper.DevTrkrConnectionString);
            _notableFiles = hlpr.GetNotableFileExtensions();

            fileSystemWatcher1.Filter = "*.*";
            fileSystemWatcher1.Path = @"c:\"; // txtFile.Text + "\\";
                                            //}
                                            //else
                                            //{
                                            //fileWatcher1.Filter = txtFile.Text.Substring(txtFile.Text.LastIndexOf('\\') + 1);
                                            //fileWatcher1.Path = txtFile.Text.Substring(0, txtFile.Text.Length - m_Watcher.Filter.Length);
                                            //}

            //if (chkSubFolder.Checked)
            //{
            fileSystemWatcher1.IncludeSubdirectories = true;
            //}

            fileSystemWatcher1.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            //fileWatcher1.Changed += new FileSystemEventHandler();
            //fileWatcher1.Created += new FileSystemEventHandler(OnChanged);
            //fileWatcher1.Deleted += new FileSystemEventHandler(OnChanged);
            //fileWatcher1.Renamed += new RenamedEventHandler(OnRenamed);
            fileSystemWatcher1.EnableRaisingEvents = true;
            }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (!File.Exists(e.FullPath) || !IsFileInteresting(e.FullPath)) return;

                var finfo = new FileInfo(e.FullPath);

               // string modifiedBy;
                //modifiedBy = finfo.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();

                /* end test code
                //var dir = new DirectoryInfo(Path.GetDirectoryName(e.FullPath));
                //var lastModified = dir.GetFiles()
                    //.OrderByDescending(fi => fi.LastWriteTime)
                    //.First();
                //modifiedBy = lastModified.GetAccessControl()
                    //.GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();*/

                //string[] whoDidIt = modifiedBy.Split('\\');
                //var machine = whoDidIt[0];
                var user = Environment.UserName; // whoDidIt[1];
                var myName = Environment.MachineName;
                if (IsFileInteresting(e.FullPath))
                    txtFileWatcher.AppendText(string.Format("File: {0} {1} at {2} by User: {4} on Computer: {3}." + Environment.NewLine, Path.Combine(e.FullPath, e.Name), "changed", DateTime.Now, myName, user));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
        {
            try
            {
                if (!File.Exists(e.FullPath) || !IsFileInteresting(e.FullPath)) return;

                var finfo = new FileInfo(e.FullPath);
                if (!finfo.Length.Equals(0))
                    return;
                //string modifiedBy;
                //modifiedBy = finfo.GetAccessControl().GetOwner(typeof(System.Security.Principal.NTAccount)).ToString();
                //var len = finfo.Length;

                //string[] whoDidIt = modifiedBy.Split('\\');
                var machine = Environment.MachineName; // whoDidIt[0];
                var user = Environment.UserName; // whoDidIt[1];
                var myName = Environment.MachineName;
                if (IsFileInteresting(e.FullPath))
                    txtFileWatcher.AppendText(string.Format("File: {0} {1} at {2} by User: {4} on Computer: {3}." + Environment.NewLine, Path.Combine(e.FullPath, e.Name), "changed", DateTime.Now, myName, user));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void fileSystemWatcher1_Deleted(object sender, FileSystemEventArgs e)
        {
            if (IsFileInteresting(e.FullPath))
                txtFileWatcher.AppendText(string.Format("File: {0} {1} at {2}." + Environment.NewLine, Path.Combine(e.FullPath, e.Name), "deleted", DateTime.Now));
        }

        private void fileSystemWatcher1_Renamed(object sender, RenamedEventArgs e)
        {
            if (IsFileInteresting(e.FullPath))
                txtFileWatcher.AppendText(string.Format("File: {0} {1} at {2}." + Environment.NewLine, Path.Combine(e.FullPath, e.Name), "renamed", DateTime.Now));
        }

        #region helper methods

        private void RecordFileEvent(string fullPath)
        {
            try
            {
                if (!IsFileInteresting(fullPath)) return;

                var finfo = new FileInfo(fullPath);
                var len = finfo.Length;
                if (len <= 0) return;


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Program Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool IsFileInteresting(string fileName)
        {
            try
            {
                // why would I try to write to a file that has been reported as changed
                // I don't want to do that and probably cant....
                //using (var fs = File.OpenWrite(fileName))
                //{
                //    fs.Close();
                //}
                var ext = Path.GetExtension(fileName).ToLower().Substring(1);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    return false;
                }
                return _notableFiles.Contains(ext);
            }
            catch (Exception)
            {
                //no write access, other app not done
                return false;
            }
        }
        #endregion
    }
}

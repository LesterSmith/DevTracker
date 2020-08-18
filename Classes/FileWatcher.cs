using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using BusinessObjects;
using DataHelpers;
using System.Diagnostics;
using System.Text.RegularExpressions;
namespace DevTracker.Classes
{
    public enum FWType
    {
        Created = 0,
        Changed = 1,
        Deleted = 2,
        Renamed = 3
    }

    public class FileWatcher : IDisposable
    {
        #region class members
        private const string _pipe = "|";
        //public FileSystemWatcher fileWatcher1;
        //private FileSystemWatcher fileWatcher2; 
        #endregion

        #region ..ctor
        public FileWatcher()
        {
            try
            {
                // determine what we have to watch on this machine
                // if database is not setup, watch the C:\ drive
                var hlpr = new DHMisc(string.Empty);
                List<ConfigOption> dirList = hlpr.GetConfigOptions(AppWrapper.AppWrapper.FileWatcherDirecory);
                var o = dirList.Find(x => x.Value.StartsWith(Environment.MachineName));

                string[] drives;
                if (o == null)
                {
                    Watch(@"C:\");
                }
                else
                {
                    string s = o.Value.Substring(o.Value.IndexOf("|") + 1);
                    drives = s.Split(',');
                    for (int i = 0; i < drives.Length; i++)
                    {
                        Watch(drives[i]);
                    }
                }

                #region single filewatcher setup no longer used
                //fileWatcher1 = new FileSystemWatcher
                //{
                //    Path = @"C:\",
                //    Filter = "*.*",
                //    NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                //    IncludeSubdirectories = true
                //};

                //fileWatcher1.Changed +=
                //      new FileSystemEventHandler(fileSystemWatcher1_Changed);
                //fileWatcher1.Created +=
                //    new FileSystemEventHandler(fileSystemWatcher1_Created);
                //fileWatcher1.Deleted +=
                //    new FileSystemEventHandler(fileSystemWatcher1_Deleted);
                //fileWatcher1.Renamed +=
                //    new RenamedEventHandler(fileSystemWatcher1_Renamed);

                //fileWatcher1.EnableRaisingEvents = true; 
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + "No FileWatcher is Running", AppWrapper.AppWrapper.ProgramError, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region events
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!File.Exists(e.FullPath) || !IsFileInteresting(e.FullPath)) return;

            var fc = GetFileChangeObject(e);
            QueueFileChangeForProcessing(fc);
        }

        private void OnRenamed(object source, RenamedEventArgs e)
        {
            if (!File.Exists(e.FullPath) || !IsFileInteresting(e.FullPath)) return;

            var fc = GetFileChangeObject(e);
            QueueFileChangeForProcessing(fc);
        }
        #endregion

        #region helper methods


        List<FileSystemWatcher> _watchers = new List<FileSystemWatcher>();

        private void QueueFileChangeForProcessing(FileChange fc)
        {
            lock (Globals.SyncLockObject)
            {
                Globals.FileChangeQueue.Enqueue(fc);
                if (!Globals.FileAnalyzerThreadRunning)
                {
                    Globals.FileAnalyzerThreadRunning = true;
                    Thread t = new Thread(new ThreadStart(ProcessQueueEntry));
                    t.Start();
                }
            }
        }

        private void ProcessQueueEntry()
        {
            _ = new FileAnalyzer();
        }
        private void Watch(string watch_folder)
        {
            // Create a new FileSystemWatcher and set its properties.
            FileSystemWatcher watcher = new FileSystemWatcher();

            // list the watcher so we can disable them on shutdown
            _watchers.Add(watcher);

            watcher.Path = watch_folder;
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */

            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }
        private FileChange GetFileChangeObject(FileSystemEventArgs e)
        {
            return new FileChange
            {
                FullPath = e.FullPath,
                CurrentApp = Globals.LastWindowEvent.AppName,
                ChangeType = GetChangeType(e.ChangeType),
                ProjectName = Globals.LastWindowEvent.DevProjectName,
                WindowsStartTime = Globals.LastWindowEvent.StartTime,
                CurrentWindowID = Globals.LastWindowEvent.ID
            };
        }

        private string GetChangeType(WatcherChangeTypes ct)
        {
            switch (ct)
            {
                case WatcherChangeTypes.Changed:
                    return "Changed";
                case WatcherChangeTypes.Created:
                    return "Created";
                case WatcherChangeTypes.Deleted:
                    return "Deleted";
                case WatcherChangeTypes.Renamed:
                    return "Renamed";
                default:
                    return "Changed";
            }
        }
        private bool IsFileInteresting(string fileName)
        {
            try
            {
                // if a background windows process like VS is saving temp files, ignore it
                if (fileName.ToLower().Contains($"\\users\\{Environment.UserName}\\") ||
                    fileName.ToLower().Substring(3).StartsWith("windows") ||
                    fileName.ToLower().Substring(3).StartsWith("program"))
                    return false;

                var ext = Path.GetExtension(fileName);
                if (!string.IsNullOrWhiteSpace(ext))
                    ext = ext.ToLower().Substring(1);
                else
                {
                    // we are interested in the git config file for it contains URL
                    if (fileName.ToLower().EndsWith(@"\.git\config"))
                        return true;
                }
                
                var o = Globals.NotableFiles.Find(x => x.Extension == ext);
                return o != null;
            }
            catch (Exception ex)
            {
                //no write access, other app not done
                return false;
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < _watchers.Count; i++)
            {
                _watchers[i].EnableRaisingEvents = false;
            }
        }
        #endregion
    }
}

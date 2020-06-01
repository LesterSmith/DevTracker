using System.Collections.Generic;
using BusinessObjects;
namespace DevTracker.Classes
{
    public enum FileSaveOption
    {
        None = 0,
        Selected = 1,
        All = 2
    }
    public static class Globals
    {
        /// <summary>
        /// When the CurrentApp = devenv, the LastWindowEvent.ProjectName will be the 
        /// DeveProject we are currently working on in the current window
        /// these two values will be picked up by the FileWatcher and passed to a new
        /// instance of the FileAnalyzer which runs as separate thread[s] from the filewatcher
        /// </summary>
        public static WindowEvent LastWindowEvent { get; set; }

        // Lock Objects
        public static DummyLockObject SyncLockObject { get; set; }
        public static DummyLockObject FilesLockObject { get; set; }
        //NOTE WinEventProcess Change
        public static Queue<WinEventProcesss> WinEventQueue { get; set; }
        // FileChange Queue
        public static Queue<FileChange> FileChangeQueue { get; set; }
        // FileAnalyzer Process Thread
        public static bool FileAnalyzerThreadRunning { get; set; }
        // WindowEvents Process Thread
        public static bool WindowEventThreadRunning { get; set; }

        //***** These variables are cached in case DB changes ****
        public static AppWrapper.AppWrapper.WindowEventType WinEventType { get; set; }
        public static List<ConfigOption> ConfigOptions { get; set; }
        public static List<IDEMatch> IDEMatches { get; set; }
        public static List<NotableApplication> NotableApplications { get; set; }
        public static List<NotableFileExtension> NotableFiles { get; set; }
        // Since we now save only development files to ProjectFiles Table,
        // the FileSaveOption had to do with FileActivity Table which is no longer used
        // So the FilesToSave property is no longer needed
        //public static FileSaveOption FilesToSave{get;set;}
        public static int CacheTimeout { get; set; }
        //NOTE: no longer cached, could grow too large && two much unneeded data
        //public static List<DevProjPath> ProjectList { get; set; }
        public static string DisplayName { get; set; }
        //*********** End of Cached Objects ****************************

        //TODO when updating cache the length of this list should be compared to the length and contents
        // of the IDEMatches and the list below should be updated only if an ideMatch changed
        // also the list<bool> should be changed to list<Unknowns> where that objecct should have
        // properties of unKnownValue and a bool.  That wwill allow us to update the new 
        public static List<MatchUnknown> Unknowns { get; set; }
        public static WindowChangeEvents WindowChangeEventHandler { get; set; }
        public static FileWatcher FileWatchr { get; set; }
    }
}

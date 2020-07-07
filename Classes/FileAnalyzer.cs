using System;
using System.IO;
using System.Text.RegularExpressions;
using BusinessObjects;
using DevProjects;
using System.Diagnostics;
using DataHelpers;
using AppWrapper;
using DevTrackerLogging;
namespace DevTracker.Classes
{
    /// <summary>
    /// We are only interested in development associated file, not just general 
    /// files.  Only files which are known to be dev files or in a dev project path
    /// determines if the file path matches a known project path and
    /// thereby denotes whether the file is of interest to us
    /// if the currentapp is an IDE we have a file being used in development
    /// That being the case we can either use the path to find an existing project
    /// in the table of VS projects or record the new project
    /// </summary>
    public class FileAnalyzer
    {
        #region ..ctor
        public FileAnalyzer()
        {
            try
            {
                FileChange fc;
                // get instance of Project/File handler
                MaintainProject mp = new MaintainProject();

                TopOfCode:

                // get a queue item
                while (true)
                {
                    // this lock is local to this machine, and protects the Queue
                    // b/c we are multithreaded and multiple cores (processing)
                    lock (Globals.SyncLockObject)
                    {
                        if (Globals.FileChangeQueue.Count.Equals(0))
                        {
                            Globals.FileAnalyzerThreadRunning = false;
                            return;
                        }

                        fc = Globals.FileChangeQueue.Peek();
                        Globals.FileChangeQueue.Dequeue();
                        break;
                    }
                }


                //NOTE: the original reason for the FileWatcher is not to record files being manipulated,
                // Rather it is to help maintain the DevProjects Table with the best information posssible
                // so that the WindowEvents class can charge the project and to report back to the windowevents
                // LastWindowEvent the project being worked on by the currentapp

                string syncID = null;
                var ext = Path.GetExtension(fc.FullPath).Replace(".", string.Empty).ToLower();
                var devPath = string.Empty;
                var devProject = string.Empty;
                var relativeFileName = string.Empty; // filename past the pathname

                // but we should check to see if the file being saved is a .xxproj file, then and only then should
                // we check for "CheckForInsertingNewProjectPath"
                var projFileObject = Globals.NotableFiles.Find(x => x.Extension == ext);
                var ideMatch = Globals.IDEMatches.Find(x => x.AppName == fc.CurrentApp);
                
                // HERE, app is ide and we are Manupulating the Project File (.xxproj)
                if (ideMatch != null
                    && ideMatch.IsIde
                    && projFileObject != null
                    && !string.IsNullOrWhiteSpace(projFileObject.IDEProjectExtension)
                    && projFileObject.IDEProjectExtension.Equals(ext))
                {
                    // yes, we are manipulating a development language project or sln file
                    // therefore the path is where the project file is saved and
                    // by definition, the filename is the name of the project
                    fc.ProjectName = Path.GetFileNameWithoutExtension(fc.FullPath);
                    devPath = Path.GetDirectoryName(fc.FullPath);

                    // if devPath not in DevProjects path insert it, else update the pathname
                    DevProjPath dpp = new DevProjPath
                    {
                        DevProjectName = fc.ProjectName,
                        DevProjectPath = devPath,
                        IDEAppName = fc.CurrentApp,
                        DatabaseProject = ideMatch.IsDBEngine,
                        CountLines = projFileObject.CountLines,
                        ProjFileExt = projFileObject.IDEProjectExtension
                    };
                    dpp.DevSLNPath = mp.FindSLNFileFromProjectPath(dpp);
                    dpp.SyncID = syncID = mp.CheckForInsertingNewProjectPath(dpp, fc.FullPath);

                    // the .sln may or may not be in the project table at the time the
                    // project is created, in fact it may never be there for a number of reasons,
                    // e.g., DLL project, etc., but a project may get a .sln at later time
                    // here we do not know whether the code above wrote to DevProjects
                    if (ext.Equals("sln"))
                    {
                        mp.UpdateSLNPathInProject(dpp);
                    }


                    //TODO: a hole in the logic when a project FOLDER is dropped, pasted
                    //NOTE: not necessarily true b/c if a project is not initially monitord
                    // by DevTracker, any files created before that and never saved again really
                    // should not be in the Project Detail report b/c there is no time charged b/c
                    // of those file anyway.  Showing file code lines for which there is no time
                    // will by definition skew the cost of a code line down...
                    // since project files (.cs,.vb, etc.) could be created before
                    // the project file (.xxproj), now that we know for sure the
                    // name and path, update all the files that may be missing them
                    //NOTE: also, other project files will be saved after this but
                    // we will not update them here until all files are are saved in the 
                    // project and the project file .xxproj get saved again which is
                    // unless we get a method to update all the files in a project .xxproj file
                    // which I just did so instead of the call below, we need to
                    // 1. Get a list of all files from the project file.
                    // 2. Loop thru the list of files from step 1.
                    // 3. Call InsertUpdateFileActivity(with fa.Filename = each file from step 2)
                    // We can do this without being concerned about time b/c this class runs on separate thread
                    // from the FileWatcher and is processing files in sequence as they are queued
                    // by FileWatcher class
                }
                else if (ideMatch != null
                            && ideMatch.IsIde
                            && projFileObject != null
                            && !string.IsNullOrWhiteSpace(projFileObject.IDEProjectExtension))
                {
                    // this is an ide, but not saving a .xxproj file, but we know what the extension of the project file is
                    // here we are addressing the issue of existing projects when this application is installed
                    // look at FileAnalyzer saving code files for a project that has xxUnknown for a path
                    // it could try to find the project name and path by enhancing GetProjectPath() to look for the
                    // project file in the path somewhere

                    // NOTE: changing logic here to something more reliable 04/17/2020
                    DevProjPath pp = mp.IsFileInADevProjectPath(fc.FullPath);
                    // if pp not null, we have a known project
                    // **** and the following lines is not doing anything...
                    // b/c the project is already in devprojects and ChkforInsert..will do nothing
                    if (pp != null)
                    {
                        fc.ProjectName = pp.DevProjectName;
                        devPath = pp.DevProjectPath;
                        syncID = mp.CheckForInsertingNewProjectPath(new DevProjPath 
                        { 
                            DevProjectName = fc.ProjectName, 
                            DevProjectPath = devPath, 
                            IDEAppName = fc.CurrentApp, 
                            DatabaseProject = ideMatch.IsDBEngine, 
                            CountLines = projFileObject.CountLines, 
                            ProjFileExt = projFileObject.IDEProjectExtension}, 
                            fc.FullPath);
                    }
                    else
                    {
                        // NOTE: if GetProjectPath returns project name and path it
                        // found the .xxproj file so we are sure that we can check for inserting
                        // a new project in DevProjects table
                        Tuple<string, string, string> tuple = mp.GetProjectFromDevFileSave(fc.FullPath, Globals.NotableFiles, ext);
                        if (tuple == null || string.IsNullOrWhiteSpace(tuple.Item1))
                        {
                            // remove next line b/c it is writing false positive problems, e.g.,
                            // a .dll not written to a project path will log an error falsely 7/1/20
                            //_ = new LogError($"FileAnalyzer Missing Data, Project: {fc.ProjectName}  Path: {devPath} FileFullPath: {fc.FullPath}", false, "FileAnalyzer.ctor");
                            goto TopOfCode;
                        }
                        else
                        {
                            //NOTE: if devenv is installing something don't let it fool
                            // us into creating a project
                            if (fc.CurrentApp == "devenv" && tuple.Item2.ToLower().Contains("program files"))
                                goto TopOfCode;

                            fc.ProjectName = tuple.Item1;
                            devPath = tuple.Item2;
                            syncID = mp.CheckForInsertingNewProjectPath(
                            new DevProjPath
                            {
                                DevProjectName = tuple.Item1,
                                DevProjectPath = tuple.Item2,
                                IDEAppName = fc.CurrentApp,
                                DatabaseProject = ideMatch.IsDBEngine,
                                CountLines = projFileObject.CountLines,
                                ProjFileExt = projFileObject.IDEProjectExtension
                            }, fc.FullPath);
                        }
                    }
                }
                else if (fc.FullPath.ToLower().EndsWith(@"\.git\config"))
                {
                    // we can try to get the project file name from git config file
                    // the config file will not get a URL until the local repo is pushed to the server
                    var url = mp.GetGitURLFromConfigFile(fc.FullPath);
                    if (string.IsNullOrWhiteSpace(url))
                        goto TopOfCode;

                    // get the repo name from the url, should be a projectname, may not be
                    var m = Regex.Match(url, @".*/(?<PrjName>.*?)\.git");
                    var trialProjName = 
                        m.Success && !string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value) 
                        ? m.Groups["PrjName"].Value 
                        : string.Empty;
                    // if the url does not contain a projectname quit
                    if (string.IsNullOrWhiteSpace(trialProjName)) goto TopOfCode;
                    
                    var exten = Globals.NotableFiles.Find(x => x.Extension == "config");
                    if (exten == null) goto TopOfCode;
                    Tuple<string, string, string> projObj = mp.GetProjectFromGitConfigSaved(fc.FullPath, Globals.NotableFiles);
                    if (projObj == null) goto TopOfCode;
                    fc.ProjectName = projObj.Item1;
                    devPath = projObj.Item2;

                    syncID = mp.CheckForInsertingNewProjectPath(
                        new DevProjPath
                        {
                            DevProjectName = projObj.Item1,
                            DevProjectPath = projObj.Item2,
                            IDEAppName = fc.CurrentApp,
                            DatabaseProject = false, // above code found vs project extension
                            CountLines = true,
                            ProjFileExt = projObj.Item3
                        }, fc.FullPath);
                    
                }
                else
                {
                    // current app is not an IDE, or we don't have a projectFileExt type, 
                    // so see if file is saved in
                    // a known current devprojectpath for this machine and user
                    DevProjPath pp = mp.IsFileInADevProjectPath(fc.FullPath);
                    if (pp == null)
                        goto TopOfCode;

                    // yes, this file is project file of a known DevProjects project
                    fc.ProjectName = pp.DevProjectName;
                    devPath = pp.DevProjectPath;
                    syncID = pp.SyncID;
                    pp.CountLines = projFileObject.CountLines;
                    // we update files only here b/c we are not calling mp.CheckForInsertingProject
                    // as we do when we have an IDE above...
                    mp.UpdateProjectFiles(pp, fc.FullPath, new DHMisc());
                }

                // NOTE: if we get here we have a file that is being updated
                // in a known development path, else we would have ignored the file before getting here
                // if devProject is not empty, update the windowevent so that the app that caused this
                // FW event can be charged to the project 
                if (!string.IsNullOrWhiteSpace(devPath))
                {
                    lock (Globals.LastWindowEvent)
                    {
                        var hlpr2 = new DHWindowEvents();
                        if (Globals.LastWindowEvent.ID == fc.CurrentWindowID &&
                            string.IsNullOrWhiteSpace(Globals.LastWindowEvent.DevProjectName))
                        {
                            Globals.LastWindowEvent.DevProjectName = fc.ProjectName;
                            Globals.LastWindowEvent.SyncID = string.IsNullOrWhiteSpace(syncID) ? string.Empty : syncID;
                        }
                        else
                        {
                            // since the ID has changed in the last window event
                            // that means the current window changed before we could get
                            // here (somewhat unlikely), so the window event that saved the 
                            // current file has already been written to database and the
                            // projName is incorrect, it should be the last row
                            // but not necessarily
                            WindowEvent we = hlpr2.GetLastWindowEventWritten(fc.CurrentWindowID);

                            if (we != null && string.IsNullOrWhiteSpace(we.DevProjectName))
                            {
                                // update the window event that was current when this file was modified
                                // with the correct proj name so work on this file gets charged
                                hlpr2.UpadateProjAndPathInWindowEvent(fc.CurrentWindowID, fc.ProjectName, string.IsNullOrWhiteSpace(syncID) ? string.Empty : syncID);
                            }
                        }
                        //update syncid in window events if syncid is known
                        if (!string.IsNullOrWhiteSpace(syncID))
                            hlpr2.UpdateWindowEventsWithSyncID(fc.ProjectName, fc.CurrentApp, syncID);
                    }
                }

                goto TopOfCode;
            }
            catch (Exception ex)
            {
                _ = new LogError(ex, false, "FileAnalyzer.ctor");
                Globals.FileAnalyzerThreadRunning = false;
            }
        }
        #endregion
    }
}

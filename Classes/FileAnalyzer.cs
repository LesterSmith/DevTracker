using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using BusinessObjects;
using DevTracker;
//using DevProjects;
using CodeCounter;
using System.Diagnostics;
using DataHelpers;

namespace DevTracker.Classes
{
    /// <summary>
    /// determines if the file path matches a known project path and
    /// thereby denotes whether the file is of interest to us
    /// if the currentapp is devenv we have a file being used by VS
    /// That being the case we can either use the path to find an existing project
    /// in the table of VS projects or record the new project
    /// </summary>
    public class FileAnalyzer
    {
        //TODO: look for ..\projectname\.git\config (no ext) being saved
        // the project is being inialized for gitHub
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
                

                // if a background windows process like VS is saving temp files, ignore it
                if (fc.FullPath.ToLower().Contains($"\\users\\{Environment.UserName}\\"))
                    goto TopOfCode;

                //NOTE: the original reason for the FileWatcher is not to record files being manipulated,
                // Rather it is to help maintain the DevProjects Table with the best information posssible
                // so that the WindowEvents class can charge the project and to report back to the windowevents
                // LastWindowEvent the project being worked on by the currentapp

                var ext = Path.GetExtension(fc.FullPath).Replace(".", string.Empty).ToLower();
                var isDevFile = false; // will be true if file is a development file
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
                    && projFileObject.IDEProjectExtension.Contains(ext))
                {
                    // yes, we are manipulating a development language project file
                    // therefore the path is where the project file is saved and
                    // by definition, the filename is the name of the project
                    fc.ProjectName = Path.GetFileNameWithoutExtension(fc.FullPath);
                    devPath = Path.GetDirectoryName(fc.FullPath);
                    isDevFile = true;
                    
                    // if devPath not in DevProjects path insert it, else update the pathname
                    mp.CheckForInsertingNewProjectPath(
                    new DevProjPath
                    {
                        DevProjectName = fc.ProjectName,
                        DevProjectPath = devPath,
                        IDEAppName = fc.CurrentApp,
                        DatabaseProject = ideMatch.IsDBEngine,
                        CountLines = projFileObject.CountLines,
                        ProjFileExt = projFileObject.Extension
                    }, fc.FullPath);

                    // since project files (.cs,.vb, etc.) could be created before
                    // the project file (.xxproj), now that we know for sure the
                    // name and path, update all the files that may be missing them
                    //NOTE: also, other project files will be saved after this but
                    // we will not update them here until all files are are saved in the 
                    // project and the project file .xxproj get saved again which is
                    //TODO: a hole in the logic when a project FOLDER is dropped, pasted
                    // unless we get a method to update all the files in a project .xxproj file
                    // which I just did so instead of the call below, we need to
                    // 1. Get a list of all files from the project file.
                    // 2. Loop thru the list of files from step 1.
                    // 3. Call InsertUpdateFileActivity(with fa.Filename = each file from step 2)
                    // We can do this without being concerned about time b/c this class runs on separate thread
                    // from the FileWatcher and is processing files in sequence as they are queued
                    // by FileWatcher class
                    // **** PUT NEW UPDATE CODE NEXT TO REPLACE --THINK IT THRU ****
                    UpdateFileActivityWithProjectNameAndPath(
                        new FileActivity
                        {
                            DevProjName = fc.ProjectName,
                            DevProjectPath = devPath,
                            Machine = Environment.MachineName,
                            Username = Environment.UserName
                        });
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
                    DevProjPath pp = IsFileInADevProjectPath(fc.FullPath);
                    // if pp not null, we have a known project
                    if (pp != null)
                    {
                        fc.ProjectName = pp.DevProjectName;
                        devPath = pp.DevProjectPath;
                        isDevFile = true;
                    }
                    else
                    {
                        // NOTE: if GetProjectPath returns project name and path it
                        // found the .xxproj file so we are sure that we can check for inserting
                        // a new project in DevProjects table
                        var projectName = string.Empty;
                        devPath = GetProjectPath(fc.FullPath, out projectName, projFileObject.IDEProjectExtension);
                        if (!string.IsNullOrWhiteSpace(projectName))
                            fc.ProjectName = projectName;

                        if (string.IsNullOrWhiteSpace(fc.ProjectName) || string.IsNullOrWhiteSpace(devPath))
                            Debug.WriteLine($"Missing Data, Project: {fc.ProjectName}  Path: {devPath}");
                        else
                        {
                            //NOTE: Moved to new class with SyncID
                            mp.CheckForInsertingNewProjectPath(
                            new DevProjPath
                            {
                                DevProjectName = fc.ProjectName,
                                DevProjectPath = devPath,
                                IDEAppName = fc.CurrentApp,
                                DatabaseProject = ideMatch.IsDBEngine,
                                CountLines = projFileObject.CountLines,
                                ProjFileExt = projFileObject.Extension
                            }, fc.FullPath);
                            isDevFile = true;
                        }
                    }
                }
                else
                {
                    //NOTE: it just occurred to me that if Explorer is copying a development folder
                    // to a new location, we are in fact creating a new project with the same
                    // name in a different location, or if whoever runs for source
                    // control to pull down a copy of an existing to this machine, we are
                    // creating a new project with the same name but for the first time on this
                    // computer, so we have to watch for the .xxproj file to hit this computer
                    // and at that time the project is no different than if it were saved by
                    // and IDE, as above, so it may be that we don't really care who saved the
                    // project .xxproj or any other project type file cs, vb, cpp, fs, ect.
                    // therefore the complex code above regarding an IDE is overkill and reall
                    // should be more concerned with the type of file being saved than we are of
                    // what application saved the file

                    // current app is not an IDE, or we don't have a projectFileExt type, 
                    // so see if file is saved in
                    // a known current devprojectpath for this machine and user
                    DevProjPath pp = IsFileInADevProjectPath(fc.FullPath);
                    if (pp == null)
                        goto TopOfCode;

                    // yes, this file is project file of a known DevProjects project
                    fc.ProjectName = pp.DevProjectName;
                    devPath = pp.DevProjectPath;
                    isDevFile = true;
                }

                // NOTE: it looks like that if we get here we have a file that is being updated
                // in a know development path, else we would have ignored the file before gettin here
                // if devProject is not empty, update the windowevent so that the app that caused this
                // FW event can be charged to the project 
                if (!string.IsNullOrWhiteSpace(devPath))
                {
                    lock (Globals.LastWindowEvent)
                    {
                        if (Globals.LastWindowEvent.ID == fc.CurrentWindowID &&
                            string.IsNullOrWhiteSpace(Globals.LastWindowEvent.DevProjectName))
                        {
                            Globals.LastWindowEvent.DevProjectName = fc.ProjectName;
                        }
                        else
                        {
                            // since the ID has changed in the last window event
                            // that means the current window changed before we could get
                            // here (somewhat unlikely), so the window event that saved the 
                            // current file has already been written to database and the
                            // projName is incorrect, it should be the last row
                            // but not necessarily
                            var hlpr2 = new DHWindowEvents();
                            WindowEvent we = hlpr2.GetLastWindowEventWritten(fc.CurrentWindowID);

                            if (we != null && string.IsNullOrWhiteSpace(we.DevProjectName))
                            {
                                // update the window event that was current when this file was modified
                                // with the correct proj name so work on this file gets charged
                                hlpr2.UpadateProjAndPathInWindowEvent(fc.CurrentWindowID, fc.ProjectName);
                            }
                        }
                    }
                }

                // chk to see if we want to save the file
                switch (Globals.FilesToSave)
                {
                    case FileSaveOption.None:
                        goto TopOfCode;
                    case FileSaveOption.All:
                        break;
                    case FileSaveOption.Selected:
                        if (!IsFileInteresting(fc.FullPath))
                            goto TopOfCode;
                        // not interested in binary fills unless part of a devproject
                        if ((ext.Equals("dll") || ext.Equals("exe")) && !isDevFile)
                            goto TopOfCode;
                        break;
                }

                // Now, if the file is a dev file and we are to count lines
                //int codeLines = 0;
                //int blankLines = 0;
                //int designerLines = 0;
                //int commentLines = 0;
                //if (projFileObject != null && projFileObject.CountLines)
                //{
                //    FileLineCounter codeCounter = new FileLineCounter(fc.FullPath);
                //    if (codeCounter.Success)
                //    {
                //        codeLines = codeCounter.numberLines;
                //        blankLines = codeCounter.numberBlankLines;
                //        commentLines = codeCounter.numberCommentsLines;
                //        designerLines = codeCounter.numberLinesInDesignerFiles;
                //    }
                //}

                // TODO: file should be recorded in ProjectFiles table not fileActivity
                var fa = new FileActivity
                {
                    Machine = Environment.MachineName,
                    DevProjName = fc.ProjectName,
                    Filename = fc.FullPath,
                    Username = Environment.UserName,
                    FileLength = new FileInfo(fc.FullPath).Length,
                    LastAction = fc.ChangeType,
                    CreatedBy = Environment.UserName,
                    UpdatedBy = Environment.UserName,
                    LastUpdate = DateTime.Now,
                    DevProjectPath = devPath,
                    UpdateCount = 1,
                    //CodeLines = codeLines,
                    //CommentLines = commentLines,
                    //BlankLines = blankLines,
                    //DesignerLines = designerLines
                };

                var hlpr = new DHFileWatcher();
                var rows = hlpr.InsertUpdateFileActivity(fa);
                goto TopOfCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Globals.FileAnalyzerThreadRunning = false;
            }
        }

        /// <summary>
        /// This method updates the projectname
        /// </summary>
        /// <param name="fileActivity"></param>
        /// <returns></returns>
        private int UpdateFileActivityWithProjectNameAndPath(FileActivity fileActivity)
        {
            var hlpr = new DHMisc();
            return hlpr.UpdateFileActivityWithProjectData(fileActivity);
        }
        #endregion

        #region public methods
        #endregion

        #region private methods
        /// <summary>
        /// Two things to do here
        /// 1) if this file is being saved by VS
        ///    we want to insert the project name into
        ///    the DevProjName table if not extant
        ///    and the project path, user, in the project Path table if not extant
        /// 2) Initially, we are interested only in files saved by VS or
        ///    they are saved by another app in a known VS projectPath
        ///    insert/update the file activity table if the file is in 
        ///    a known project path
        /// </summary>
        private void CheckForInsertingNewProjectPath(DevProjPath projPath)
        {
            var hlpr = new DataHelpers.DHFileWatcher();
            var rows = hlpr.CheckForInsertingNewProjectPath(projPath);
        }

        private DevProjPath IsFileInADevProjectPath(string fullPath)
        {
            var hlpr = new DataHelpers.DHFileWatcher();
            return hlpr.IsFileInDevPrjPath(fullPath);
        }

        private bool IsFileInteresting(string fileName)
        {
            try
            {
                var ext = Path.GetExtension(fileName).ToLower().Substring(1);
                if (string.IsNullOrWhiteSpace(ext))
                {
                    return false;
                }
                var o = Globals.NotableFiles.Find(x => x.Extension == ext);
                return o != null;
            }
            catch (Exception)
            {
                //no write access, other app not done
                return false;
            }
        }

        /// <summary>
        /// "C:\VS2019 Projects\DataLayerUtility\DataLayerUtility\Forms\frmMain.cs"
        /// the obvious fallacy here is that the path could be 
        /// c:\VS2019 Projects\Web Projects\MyWebApp1 Sln Folder\MyWebApp1\Web Pages\Web Page1.aspx
        /// and this method will return "c:\VS2019 Projects\Web Projects" as the ProjectPath
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        //public string GetProjectPath(string fileName, bool isIDE)
        //{
        //    const string slash = @"\";

        //    var idxLast = FullPath.LastIndexOf(slash);
        //    var fullPath = FullPath;

        //    // strip the filename and last slash if extant
        //    if (fullPath.Length > idxLast + 1)
        //        fullPath = fullPath.Substring(0, idxLast);

        //    if (!isIDE /*CurrentApp != "devenv" && CurrentApp != "Code"*/)
        //    {
        //        return string.Empty;
        //    }
        //    /* Debug only stop filewatcher so we can debug */
        //    //Globals.FileWatchr.fileWatcher1.EnableRaisingEvents = false;
        //    /* take out line above for runnint */

        //    // now we have the fullPath like 
        //    //C:\VS2019 Projects\DataLayerUtility\DataLayerUtility\Forms
        //    // since this is an ide, strip all paths from the end != ProjectPath
        //    var name = string.Empty;
        //    while (true)
        //    {
        //        idxLast = fullPath.LastIndexOf(slash);
        //        if (idxLast < 0)
        //            break;
        //        name = fullPath.Substring(idxLast + 1);
        //        if (name.ToLower() == ProjectName.ToLower())
        //            return fullPath;
        //        fullPath = fullPath.Substring(0, idxLast);
        //    }
        //    return fullPath;
        //}

        /// <summary>
        /// TODO: this must be tested
        /// This method attempts to get the project path by looking for 
        /// </summary>
        /// <param name="out projectName"></param>
        /// <param name="projFileExt"></param>
        /// <returns></returns>
        private string GetProjectPath(string fileFullPath, out string projectName, string projFileExt)
        {
            if (string.IsNullOrWhiteSpace(Path.GetFileName(fileFullPath)))
            {
                projectName = string.Empty;
                return string.Empty;
            }

            var fullPath = fileFullPath;
            var dirName = string.Empty;

            while (!string.IsNullOrWhiteSpace(fullPath))
            {
                fullPath = Path.GetDirectoryName(fullPath);
                if (string.IsNullOrWhiteSpace(fullPath))
                {
                    projectName = string.Empty;
                    return string.Empty;
                }

                dirName = new DirectoryInfo(fullPath).Name;
                var prj = Globals.ProjectList.Find(x => x.DevProjectName.ToLower() == dirName.ToLower());

                // we think we have a project path
                if (prj != null)
                {
                    projectName = dirName;
                    // we think we have a project path, chek for a xxproj file in the path
                    // projFileExt can have multiple delimited extensions
                    // mainly b/c c++ proj extension changed in later version
                    string[] extensions = projFileExt.Split('|');
                    for (int i = 0; i < extensions.Length; i++)
                    {
                        var projFile = projectName + "." + extensions[i];
                        if (File.Exists(Path.Combine(fullPath, projFile)))
                        {
                            projectName = dirName;
                            return fullPath;
                        }
                    }
                    continue;
                }
            }

            projectName = string.Empty;
            return string.Empty; // did not find a project file in the supposed project directory
        }

        #endregion
    }
}

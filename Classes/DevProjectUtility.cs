using System;
using System.Collections.Generic;
using System.Linq;
using BusinessObjects;
using DataHelpers;

namespace DevTracker.Classes
{
    //public enum ProjectStatus
    //{
    //    Exists = 0,
    //    Created = 1
    //}
    /// <summary>
    /// This class adds new DevProjects and helps maintain ProjectSync Table
    /// The whole reason for the ProjectSync table is that different users
    /// could create projects with the same project name but they are 
    /// unrelated, although this is unlikely unless in the case of default
    /// project names like ConsoleApplication1, ect.
    /// W/o a sync table we can't separate the effort in reports
    /// NOTE: the assummption is that the likelihood of multiple developers
    /// adding the same DevProjectName which is unrelated to previous 
    /// instances of the same project name, at the exact same time, is
    /// very unlikely and not worthy of trying some locking mechanism or
    /// doing all the logic in a stored proc.  The complexity of keeping 
    /// the tables in sync is too great to attempt in a sproc w\o cursors
    /// which I try to avoid at all cost.
    /// </summary>
    //public static class DevProjectUtility
    //{
    //    #region public methods
    //    /// <summary>
    //    /// If this is the first instance of the DevProjectName in
    //    /// DevProjects, add it to DevProjects and ProjectSync table
    //    /// If the DevProjectName is already in DevProjects and there are 
    //    /// multiple instances in ProjectSync, link the new one to the one 
    //    /// with the most instances in DevProjects.
    //    /// 
    //    /// NOTE: Programatically inserting a new ProjectName that is a duplicate
    //    /// of an existing ProjectName in DevProjects will cause a problem that only
    //    /// can be resolved manually by a manager or admin, but this class at least
    //    /// gives us a foundation to build a correction tab in the Options Form.
    //    /// TODO: More code will probably be needed when that tab is developed shortly
    //    /// </summary>
    //    /// <param name="newProj"></param>
    //    /// <returns></returns>
    //    public static void CheckForInsertingNewProjectPath(DevProjPath newProj)
    //    {
    //        // first we want to know if there is a project by the name in newProj object
    //        var hlpr = new DHMisc();
    //        var hlpr2 = new DHFileWatcher();

    //        lock (Globals.FilesLockObject)
    //        {
    //            int rows = 0;
    //            // get list of all projects by this name regardless of user/machine
    //            List<DevProjPath> projList = hlpr.GetDevProjectByName(newProj.DevProjectName);

    //            if (projList.Count < 1)
    //            {
    //                // projectname does not exist yet, just enter it
    //                // first insert the project in the ProjectSync Table
    //                var syncID = Guid.NewGuid().ToString();
    //                var projID = Guid.NewGuid().ToString();
    //                var currDate = DateTime.Now;

    //                ProjectSync ps = new ProjectSync
    //                {
    //                    ID = syncID,
    //                    DevProjectName = newProj.DevProjectName,
    //                    DevProjectID = projID,
    //                    Machine = Environment.MachineName,
    //                    UserName = Environment.UserName,
    //                    CreatedDate = currDate
    //                };

    //                rows = hlpr.InsertProjectSyncObject(ps);

    //                // now insert the project into DevProjects
    //                newProj.CreatedDate = currDate;
    //                newProj.SyncID = syncID;

    //                rows = hlpr2.CheckForInsertingNewProjectPath(newProj);

    //            }
    //            else
    //            {
    //                // there is a project in DevProjects by this name
    //                // we have no way of knowing whether this is a project created
    //                // by as a totally new project or a first pull down of an
    //                // existing project from source control
    //                // we need to know the count of projects for this name by syncid
    //                // odds are that it's another developer pulling down the existing project
    //                // than that the devloper is creating another new project with the same name
    //                // as the existing one, at least that is this author's assumption
    //                // If the latter is true, it's chance; if the former it's collaboration:)
    //                // programatically I will go with collaboration rather than chance......
    //                // therefore at the end of the following loop I will use the syncID that 
    //                // occurs the most times within the projects with this name
    //                var sid = GetMostUsedSyncID(projList);
    //                newProj.SyncID = sid.SID;
    //                rows = hlpr2.CheckForInsertingNewProjectPath(newProj);
    //            }
    //        }
    //    }

    //    public static SyncID GetMostUsedSyncID(List<DevProjPath> projList)
    //    {
    //        List<SyncID> sids = new List<SyncID>();
    //        sids.Add(new SyncID { SID = projList[0].SyncID, Count = 0 });

    //        string lastSid = projList[0].SyncID;
    //        int ctr = 0;
    //        foreach (var devPP in projList)
    //        {
    //            if (devPP.SyncID.Equals(lastSid))
    //            {
    //                sids[ctr].Count++;
    //                continue;
    //            }

    //            ctr++;
    //            lastSid = devPP.SyncID;
    //            sids.Add(new SyncID { SID = devPP.SyncID, Count = 1 });
    //        }

    //        // now we sort the List<SIDS> to get the most popular
    //        List<SyncID> sortedSids = sids.OrderBy(o => o.Count).ToList();
    //        return sortedSids[0];
    //    }

    //    /// <summary>
    //    /// Probably one time usage
    //    /// </summary>
    //    public static void PopulateSyncTableFromDevProjects()
    //    {
    //        var hlpr = new DHMisc();
    //        List<DevProjPath> projects = hlpr.GetDevProjects();
    //        foreach (var project in projects)
    //        {

    //        }

    //    }
    //#endregion
    //}

    //public class SyncID
    //{
    //    public int Count { get; set; }
    //    public string SID { get; set; }
    //}
}

using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using BusinessObjects;
using DataHelpers;
using AppWrapper;
using DevTrackerLogging;
using DevProjects;
namespace DevTracker.Classes
{
    /// <summary>
    /// return tuple(devProjectName, IDEMatchObject, bool denoting write window or not)
    /// it has no access to syncId for project
    /// </summary>
    public class CheckForProjectName
    {
        /// <summary>
        /// Get project name from a window title using known IDE regexes
        /// </summary>
        /// <param name="title"></param>
        /// <param name="accessDenied"></param>
        /// <param name="_currentApp"></param>
        /// <param name="writeDB"></param>
        /// <returns>Tuple(devProjName, ideMatchObject, true to write windowevent, possible syncId)</returns>
        public Tuple<string, IDEMatch, bool, string> GetProjectName(string title, bool accessDenied, string _currentApp, bool writeDB)
        {
            IDEMatch foundIde = null;
            string devPrjName = string.Empty;
            bool doWriteDB = writeDB;
            string syncId = string.Empty;

            foreach (var ide in Globals.IDEMatches)
            {
                if (!accessDenied && _currentApp.ToLower() == ide.AppName.ToLower())
                {
                    var pat = ide.Regex;
                    var m = Regex.Match(title, pat, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                    devPrjName = string.Empty;
                    if (m.Success && m.Groups[ide.RegexGroupName] != null &&
                        !string.IsNullOrWhiteSpace(m.Groups[ide.RegexGroupName].Value))
                    {
                        // we found the ide match we are looking for
                        foundIde = ide;

                        // if we are concatinating two fields in ssms to get server.dbname
                        if (!string.IsNullOrWhiteSpace(ide.ProjNameConcat))
                        {
                            string[] concats = ide.ProjNameConcat.Split('|');
                            for (var i = 0; i < concats.Length; i++)
                            {
                                if (!string.IsNullOrWhiteSpace(m.Groups[concats[i]].Captures[0].ToString()))
                                    devPrjName += (i > 0 ? ide.ConcatChar : string.Empty) + m.Groups[concats[i]].Captures[0];
                            }
                        }
                        else
                            devPrjName = m.Groups[ide.RegexGroupName].Value;

                        if (devPrjName.StartsWith("Microsoft") && _currentApp == "ssms")
                            _ = new LogError($"CheckForProjectName, SSMS Invalid Project: 'Microsoft' from Title: { title}", false, "CheckForProjectName.GetProjectName");

                        if (!string.IsNullOrWhiteSpace(ide.ProjNameReplaces))
                        {
                            string[] replaces = ide.ProjNameReplaces.Split('|');
                            foreach (string s in replaces)
                            {
                                devPrjName = devPrjName.Replace(s, string.Empty).Trim();
                            }
                        }

                        // how do I know it is not the one we wan
                        // NOTE: new logic for IDEMatch objects that have AlternateProjName
                        // if it is not null, replace devPrjName with it b/c altho we found a project name
                        // it is not one we want, so make it what we want (probably the same as the unknown value)
                        // so that it will be updated when we find the projname we want...
                        // e.g., ssms has master set so DBname = Server.master in table but new logic will be correctable
                        // e.g. ssms "not connected" can now have its own match object and will get set to unknow until
                        //       user connects to a database which they will have to do in order to do anything in ssms
                        // but there is no need to run an update if the devPrjName == AlternateProjName b/c it is already
                        // that in database; we have to wait til we get a valid project name
                        if (!string.IsNullOrWhiteSpace(foundIde.AlternateProjName))
                        {
                            devPrjName = foundIde.AlternateProjName;
                        }
                        else
                        {
                            // update project name in windowevents that were written with projname unknown
                            if (!string.IsNullOrWhiteSpace(devPrjName))
                                UpdateUnknownProjectNameForIDEMatch(devPrjName, ide.AppName, ide.UnknownValue, Environment.MachineName, Environment.UserName);
                        }

                        // since we have found and processed the ide that we wanted, get out
                        doWriteDB = true;
                        goto EndOfGenericCode;
                    }
                    else /// if (m.Success && string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value))
                    {
                        // ide has no project open yet set as unknown
                        devPrjName = ide.UnknownValue;
                        doWriteDB = true;
                        continue;  // loop to see if another IDEMatch row will get the projectname
                    }
                }
            }
            // if at this point we did not find an idematch just an unknown window
            EndOfGenericCode:

            // if devProjectName not set yet
            // see if a known project name is being worked on by a non IDE
            // check to see if the window title contains a known project name
            if (string.IsNullOrWhiteSpace(devPrjName))
            {
                Tuple<string, string> prjObject = IsProjectInNonIDETitle(title);
                if (prjObject != null)
                {
                    devPrjName = prjObject.Item1;
                    syncId = prjObject.Item2;
                }
            }

            // see if we are interested in recording this window
            // NOTE: we may always want to write the window to DB b/c the company may want to know every app being used
            // especially if we want to run the Developer(user) Detail Report
            // if doWriteDB set, then we already know to write this window b/c of ideMatch found
            if (!doWriteDB)
            {
                var appConfig = Globals.ConfigOptions.Find(x => x.Name == "RECORDAPPS");
                if (appConfig != null)
                {
                    switch (appConfig.Value)
                    {
                        case "A":
                            doWriteDB = true;
                            break;
                        case "S":
                            var interestingApp = Globals.NotableApplications.Find(o => o.AppName.ToLower() == _currentApp.ToLower());
                            doWriteDB = (interestingApp != null);
                            break;
                    }
                }
            }

            // explorer is Windows and project explorer but project explorer does not always have a title
            // time spent there is really not interesting so ignore it
            if (_currentApp.ToLower() == "explorer" && string.IsNullOrWhiteSpace(title))
                doWriteDB = false; //  forget 

            return Tuple.Create(devPrjName, foundIde, doWriteDB, syncId);
        }

        private void UpdateUnknownProjectNameForIDEMatch(string devProjectName, string appName, string unknownKey, string machineName, string userName)
        {
            var hlpr = new DHWindowEvents(AppWrapper.AppWrapper.DevTrkrConnectionString);
            hlpr.UpdateUnknownProjectNameForIDEMatch(devProjectName, appName, unknownKey, machineName, userName);
        }
        public Tuple<string, string> IsProjectInNonIDETitle(string title)
        {
            var hlpr = new DHMisc();
            var projects = hlpr.GetDevProjects();
            var prjObject = projects.Find(x => title.Contains(x.DevProjectName));
            return prjObject == null ? null : Tuple.Create(prjObject.DevProjectName, prjObject.SyncID);
        }
    }
}

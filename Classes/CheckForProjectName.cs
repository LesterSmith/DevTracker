using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Diagnostics;
using BusinessObjects;
using DataHelpers;

namespace DevTracker.Classes
{
    /// <summary>
    /// This class created b/c this code needed the GetProjectName code besides
    /// WindowEvents class
    /// </summary>
    public class CheckForProjectName
    {
        public string GetProjectName(string title, ref bool accessDenied, string _currentApp, out IDEMatch ideMatchObject, ref bool writeDB)
        {
            IDEMatch foundIde = null;
            string devPrjName = string.Empty;
            foreach (var ide in Globals.IDEMatches)
            {
                if (!accessDenied && _currentApp.ToLower() == ide.AppName)
                {
                    var pat = ide.Regex;
                    var m = Regex.Match(title, pat, RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
                    devPrjName = string.Empty;
                    if (m.Success && m.Groups[ide.RegexGroupName] != null &&
                        !string.IsNullOrWhiteSpace(m.Groups[ide.RegexGroupName].Value))
                    {
#if DEBUG
                        if (ide.AppName == "ssms")
                            Debug.WriteLine($"ssms: {title}");
#endif
                        // we found the ide match we are looking for
                        ideMatchObject = ide;
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


                        if (!string.IsNullOrWhiteSpace(ide.ProjNameReplaces))
                        {
                            string[] replaces = ide.ProjNameReplaces.Split('|');
                            foreach (string s in replaces)
                            {
                                devPrjName = devPrjName.Replace(s, string.Empty).Trim();
                            }
                        }

                        // NOTE: new logic for IDEMatch objects that have AlternateProjName
                        // if it is not null, replace devPrjName with it b/c altho we found a project name
                        // it is not one we want, so make it what we want (probably the same as the unknown value)
                        // so that it will be updated when we find the projname we want...
                        // e.g., ssms has master set so DBname = Server.master in table but new logic will be correctable
                        // e.g. ssms "not connected" can now have its own match object and will get set to unknow until
                        //       user connects to a database which they will have to do in order to do anything in ssms
                        // but there is no need to run an update if the devPrjName == AlternateProjName b/c it is already
                        // that in database; we have to wait til we get a valid project name
                        if (!string.IsNullOrWhiteSpace(ideMatchObject.AlternateProjName))
                        {
                            devPrjName = ideMatchObject.AlternateProjName;
                        }
                        else
                        {
                            // update project name in windowevents that were written with projname unknown
                            if (!string.IsNullOrWhiteSpace(devPrjName))
                                UpdateUnknownProjectNameForIDEMatch(devPrjName, ide.AppName, ide.UnknownValue, Environment.MachineName, Environment.UserName);
                        }

                        // since we have found and processed the ide that we wanted, get out
                        writeDB = true;
                        goto EndOfGenericCode;
                    }
                    else /// if (m.Success && string.IsNullOrWhiteSpace(m.Groups["PrjName"].Value))
                    {
                        // ide has no project open yet set as unknown
                        devPrjName = ide.UnknownValue;
                        writeDB = true;
                        continue;  // loop to see if another IDEMatch row will get the projectname
                    }
                }
            }
            // if at this point we did not find an idematch just an unknown window
            EndOfGenericCode:

            // if we are writing this window, and devProjectName not set yet
            // see if a known project name is being worked on by a non IDE
            if (writeDB)
            {
                // check to see if the window title contains a known project name
                if (string.IsNullOrWhiteSpace(devPrjName))
                {
                    var s = IsProjectInNonIDETitle(title);
                    if (!string.IsNullOrWhiteSpace(s))
                        devPrjName = s;
                }
            }

            // see if we are interested in recording this window
            // NOTE: we may always want to write the window to DB b/c the company may want to know every app being used
            // especially if we want to run the Developer(user) Detail Report
            // if writeDB set, then we already know to write this window b/c of ideMatch found
            if (!writeDB)
            {
                var appConfig = Globals.ConfigOptions.Find(x => x.Name == "RECORDAPPS");
                if (appConfig != null)
                {
                    switch (appConfig.Value)
                    {
                        case "A":
                            writeDB = true;
                            break;
                        case "S":
                            var interestingApp = Globals.NotableApplications.Find(o => o.AppName.ToLower() == _currentApp.ToLower());
                            writeDB = (interestingApp != null);
                            break;
                    }
                }
            }

            // explorer is Windows and project explorer but project explorer does not always have a title
            // time spent there is really not interesting so ignore it
            if (_currentApp.ToLower() == "explorer" && string.IsNullOrWhiteSpace(title))
                writeDB = false; //  forget 

            ideMatchObject = foundIde;
            return devPrjName;
        }

        private void UpdateUnknownProjectNameForIDEMatch(string devProjectName, string appName, string unknownKey, string machineName, string userName)
        {
            var hlpr = new DHWindowEvents(AppWrapper.AppWrapper.DevTrkrConnectionString);
            hlpr.UpdateUnknownProjectNameForIDEMatch(devProjectName, appName, unknownKey, machineName, userName);
        }
        private string IsProjectInNonIDETitle(string title)
        {
            var prjObject = Globals.ProjectList.Find(x => title.Contains(x.DevProjectName));
            return prjObject != null ? prjObject.DevProjectName : string.Empty;
        }
    }
}

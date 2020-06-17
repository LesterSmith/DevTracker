using System;
using System.Collections.Generic;
using System.Windows.Forms;
using DataHelpers;
using BusinessObjects;
using DevTracker.Classes;
using AppWrapper;
namespace DevTracker.Forms
{
    public enum AddOrUpdate
    {
        Add,
        Update
    }
    public partial class Options : Form
    {
        #region ..ctor
        public Options()
        {
            InitializeComponent();
        }

        #endregion

        #region public and private members
        private bool _isDirty = false;
        List<NotableApplication> AppList = new List<NotableApplication>();
        List<IDEMatch> IDEMatches { get; set; }
        List<NotableFileExtension> NotableFiles { get; set; }
        List<ConfigOption> ConfigOptions { get; set; }
        List<DevProjPath> DevProjects { get; set; }
        private AddOrUpdate _addUpdate = AddOrUpdate.Add;
        #endregion

        #region form load
        private void Options_Load(object sender, EventArgs e)
        {
            try
            {
                Startup.SetupCachedDatabaseData();
                LoadConfigOptions(); // must be first, others depend on ConfigOptions
                LoadGeneralOptions();
                LoadApplications();
                LoadFiles();
                LoadPermissions();
                LoadIDEMatches();
                LoadDevProjects();
                EnableDisableControls(false, "tabGeneral");
                _isDirty = false;
            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }
        #endregion

        #region button events
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                _addUpdate = AddOrUpdate.Add;
                switch (tabControl1.SelectedTab.Name)
                {
                    case "tabGeneral":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);

                        break;
                    case "tabApplications":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);
                        txtAppFriendlyName.Clear();
                        txtAppName.Clear();
                        lbApplications.Enabled = false;
                        break;
                    case "tabFiles":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);
                        txtExtension.Clear();
                        txtProjectFileExtension.Clear();
                        lbFileExtensions.Enabled = false;
                        break;
                    case "tabPermissions":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);

                        break;
                    case "tabMatchObjects":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);
                        txtMatchDescription.Clear();
                        txtMatchRegex.Clear();
                        txtMatchGroupName.Clear();
                        txtMatchUnknownValue.Clear();
                        txtMatchAppName.Clear();
                        txtMatchProjNameReplaces.Clear();
                        txtMatchProjNameConcat.Clear();
                        txtMatchSequence.Clear();
                        txtMatchAlternateProjectName.Clear();
                        txtMatchConcatChar.Clear();
                        chkMatchIsIDE.Checked = false;
                        chkIMatchsDBEngine.Checked = false;
                        lbIDEMatches.Enabled = false;
                        break;
                    case "tabConfigOptions":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);
                        txtConfigOptionName.Clear();
                        txtConfigOptionValue.Clear();
                        txtConfigOptionDescription.Clear();
                        lbConfigOptions.Enabled = false;
                        break;
                    case "tabDevProjects":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);
                        txtDevProjectName.Clear();
                        txtDevProjectPath.Clear();
                        txtDevProjectITProjectID.Clear();
                        txtDevProjectIDEAppName.Clear();
                        txtDevProjectMachine.Clear();
                        txtDevProjectUserName.Clear();
                        txtDevProjectCreatedDate.Clear();
                        txtDevProjectCompletedDate.Clear();
                        lbDevProjects.Enabled = false;
                        break;
                }
                btnCancel.Enabled = true;
                btnSave.Enabled = true;

            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                _addUpdate = AddOrUpdate.Update;
                switch (tabControl1.SelectedTab.Name)
                {
                    case "tabGeneral":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);

                        break;
                    case "tabApplications":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);
                        lbApplications.Enabled = false;
                        break;
                    case "tabFiles":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);
                        lbFileExtensions.Enabled = false;
                        break;
                    case "tabPermissions":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);

                        break;
                    case "tabMatchObjects":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);
                        lbIDEMatches.Enabled = false;
                        break;
                    case "tabConfigOptions":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);
                        lbConfigOptions.Enabled = false;
                        break;
                    case "tabDevProjects":
                        EnableDisableControls(true, tabControl1.SelectedTab.Name);
                        lbDevProjects.Enabled = false;
                        break;
                }
                btnCancel.Enabled = true;
                btnSave.Enabled = true;
            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = MessageBox.Show("Are you sure you want to delete the selected record?", "Confirm Delete", MessageBoxButtons.OK, MessageBoxIcon.Question);
                if (dr.Equals(DialogResult.No))
                    return;
                var hlpr = new DHMisc();
                int rows;
                switch (tabControl1.SelectedTab.Name)
                {
                    case "tabGeneral":

                        break;
                    case "tabApplications":
                        rows = hlpr.DeleteApplications(AppList[lbApplications.SelectedIndex].ID);
                        LoadApplications();
                        lbApplications.SelectedIndex = 0;

                        break;
                    case "tabFiles":
                        rows = hlpr.DeleteNotableFiles(NotableFiles[lbFileExtensions.SelectedIndex].ID);
                        LoadFiles();
                        lbFileExtensions.SelectedIndex = 0;
                        LoadFilesControls();
                        break;
                    case "tabPermissions":

                        break;
                    case "tabMatchObjects":
                        rows = hlpr.DeleteProjNameMatches(IDEMatches[lbIDEMatches.SelectedIndex].ID);
                        LoadIDEMatches();
                        lbIDEMatches.SelectedIndex = 0;
                        LoadIDEMatchesControls(IDEMatches[0]);
                        break;
                    case "tabConfigOptions":
                        rows = hlpr.DeleteConfigOption(ConfigOptions[lbConfigOptions.SelectedIndex].ID);
                        LoadConfigOptions();
                        lbConfigOptions.SelectedIndex = 0;
                        LoadConfigOptionsControls(ConfigOptions[0]);
                        break;
                    case "tabDevProjects":
                        rows = hlpr.DeleteDevProjects(DevProjects[lbDevProjects.SelectedIndex].ID);
                        LoadDevProjects();
                        lbDevProjects.SelectedIndex = 0;
                        LoadDevProjectsControls(DevProjects[0]);
                        break;
                }

            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }

        private void ResetButtonsAfterSave()
        {
            SetButtonsOnTabChange();

        }
        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                DHMisc hlpr;
                if (!_isDirty)
                {
                    _errors = "You have not made any changes.";

                }
                _errors = string.Empty;
                switch (tabControl1.SelectedTab.Name)
                {
                    case "tabGeneral":
                        if (!ValidateTabGeneralForSave())
                        {
                            DisplayValidationErrors();
                            return;
                        }
                        hlpr = new DHMisc();
                        var item = ConfigOptions.Find(x => x.Name == AppWrapper.AppWrapper.CacheExpirationTime);
                        item.Value = txtCacheExpirationTime.Text;
                        hlpr.InsertUpdateConfigOptions(item);
                        item = ConfigOptions.Find(x => x.Name == AppWrapper.AppWrapper.RecordFiles);
                        item.Value = rbRecordAllFiles.Checked ? "A" : rbRecordSpecifiedFiles.Checked ? "S" : "N";
                        hlpr.InsertUpdateConfigOptions(item);
                        item = ConfigOptions.Find(x => x.Name == AppWrapper.AppWrapper.RecordApps);
                        item.Value = cbApplications.Text = cbApplications.Text.StartsWith("All") ? "A" : "S";
                        hlpr.InsertUpdateConfigOptions(item);
                        LoadConfigOptions();
                        LoadGeneralControls();
                        // since config options have been updated,
                        // reload tabConfigOption listbox
                        var c = ConfigOptions[0];
                        LoadConfigOptionsControls(c);
                        break;
                    case "tabApplications":
                        if (!ValidateTabApplicationsForSave())
                        {
                            DisplayValidationErrors();
                            return;
                        }
                        hlpr = new DHMisc();
                        var oApp = AppList[lbApplications.SelectedIndex];
                        if (_addUpdate == AddOrUpdate.Add)
                            oApp.ID = Guid.NewGuid().ToString();
                        oApp.AppName = txtAppName.Text;
                        oApp.AppFriendlyName = txtAppFriendlyName.Text;
                        hlpr.InsertUpdateNotableApplications(oApp);
                        LoadApplications();
                        lbApplications.SelectedIndex = 0;
                        lbApplications.Enabled = true;
                        SetButtonsOnTabChange();
                        break;
                    case "tabFiles":
                        if (!ValidateTabFilesForSave())
                        {
                            DisplayValidationErrors();
                            return;
                        }

                        var ext = NotableFiles[lbFileExtensions.SelectedIndex];
                        ext.Extension = txtExtension.Text;
                        ext.IDEProjectExtension = txtProjectFileExtension.Text;
                        ext.CountLines = chkCountLines.Checked;
                        if (_addUpdate == AddOrUpdate.Add)
                            ext.ID = Guid.NewGuid().ToString();
                        hlpr = new DHMisc();
                        hlpr.InsertUpdateNotableFileTypes(ext);
                        LoadFiles();
                        lbFileExtensions.SelectedIndex = 0;
                        LoadFilesControls();
                        lbFileExtensions.Enabled = true;
                        SetButtonsOnTabChange();
                        break;
                    case "tabPermissions":
                        break;
                    case "tabMatchObjects":
                        if (!ValidateTabIDEMatches())
                        {
                            DisplayValidationErrors();
                            return;
                        }

                        var ide = IDEMatches[lbIDEMatches.SelectedIndex];
                        if (_addUpdate == AddOrUpdate.Add)
                            ide.ID = Guid.NewGuid().ToString();
                        ide.AlternateProjName = txtMatchAlternateProjectName.Text;
                        ide.Regex = txtMatchRegex.Text;
                        ide.Description = txtMatchDescription.Text;
                        ide.AppName = txtMatchAppName.Text;
                        ide.RegexGroupName = txtMatchGroupName.Text;
                        ide.UnknownValue = txtMatchUnknownValue.Text;
                        ide.ProjNameConcat = txtMatchProjNameConcat.Text;
                        ide.ProjNameReplaces = txtMatchProjNameReplaces.Text;
                        if (!string.IsNullOrWhiteSpace(txtMatchSequence.Text))
                            ide.Sequence = int.Parse(txtMatchSequence.Text);
                        ide.AlternateProjName = txtMatchAlternateProjectName.Text;
                        ide.ConcatChar = txtMatchConcatChar.Text;
                        ide.IsDBEngine = chkIMatchsDBEngine.Checked;
                        ide.IsIde = chkMatchIsIDE.Checked;
                        hlpr = new DHMisc();
                        hlpr.InsertUpdateProjNameMatches(ide);
                        LoadIDEMatches();
                        lbIDEMatches.SelectedIndex = 0;
                        LoadIDEMatchesControls(IDEMatches[0]);
                        lbIDEMatches.Enabled = true;
                        SetButtonsOnTabChange();
                        break;
                    case "tabConfigOptions":
                        if (!ValidateTabConfigOptions())
                        {
                            DisplayValidationErrors();
                            return;
                        }

                        hlpr = new DHMisc();
                        var co = ConfigOptions[lbConfigOptions.SelectedIndex];
                        if (_addUpdate == AddOrUpdate.Add)
                            co.ID = Guid.NewGuid().ToString();
                        co.Name = txtConfigOptionName.Text;
                        co.Value = txtConfigOptionValue.Text;
                        co.Description = txtConfigOptionDescription.Text;
                        hlpr.InsertUpdateConfigOptions(co);
                        LoadConfigOptions();
                        lbConfigOptions.SelectedIndex = 0;
                        LoadConfigOptionsControls(ConfigOptions[0]);
                        lbConfigOptions.Enabled = true;
                        // since we have update configoptions update
                        // the tabGeneral controls as they share the same table
                        LoadGeneralControls();
                        lbApplications.Enabled = true;
                        SetButtonsOnTabChange();
                        break;
                    case "tabDevProjects":
                        if (!ValidateTabDevProjecctsForSave())
                        {
                            DisplayValidationErrors();
                            return;
                        }

                        hlpr = new DHMisc();
                        var dp = DevProjects[lbDevProjects.SelectedIndex];
                        if (_addUpdate == AddOrUpdate.Add)
                            dp.ID = Guid.NewGuid().ToString();
                        dp.DevProjectName = txtDevProjectName.Text;
                        dp.DatabaseProject = chkDevProjectDatabaseProject.Checked;
                        dp.DevProjectPath = txtDevProjectPath.Text;
                        dp.IDEAppName = txtDevProjectIDEAppName.Text;
                        dp.Machine = txtDevProjectMachine.Text;
                        dp.UserName = txtDevProjectUserName.Text;
                        DateTime date;
                        if (!string.IsNullOrWhiteSpace(txtDevProjectCompletedDate.Text) && DateTime.TryParse(txtDevProjectCreatedDate.Text, out date))
                            dp.CompletedDate = date;
                        hlpr.InsertUpdateDevProject(dp);
                        LoadDevProjects();
                        lbDevProjects.SelectedIndex = 0;
                        LoadDevProjectsControls(DevProjects[0]);
                        lbIDEMatches.Enabled = true;
                        SetButtonsOnTabChange();
                        break;
                }
            }
            catch (Exception ex)
            {
                Util.LogError(ex,true);
            }
        }

        private void DisplayValidationErrors()
        {
            MessageBox.Show(_errors, "Validation Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                switch (tabControl1.SelectedTab.Name)
                {
                    case "tabGeneral":
                        LoadGeneralControls();
                        break;
                    case "tabApplications":
                        LoadApplicationControls(lbApplications.SelectedIndex);
                        lbApplications.Enabled = true;
                        break;
                    case "tabFiles":
                        LoadFilesControls();
                        lbFileExtensions.Enabled = true;
                        break;
                    case "tabPermissions":


                        break;
                    case "tabMatchObjects":
                        var m = IDEMatches[lbIDEMatches.SelectedIndex];
                        LoadIDEMatchesControls(m);
                        lbIDEMatches.Enabled = true;
                        break;
                    case "tabConfigOptions":
                        var item = ConfigOptions[lbConfigOptions.SelectedIndex];
                        LoadConfigOptionsControls(item);
                        lbConfigOptions.Enabled = true;
                        break;
                    case "tabDevProjects":
                        var itemDP = DevProjects[lbDevProjects.SelectedIndex];
                        LoadDevProjectsControls(itemDP);
                        lbDevProjects.Enabled = true;
                        break;
                }
                SetButtonsOnTabChange();
                _isDirty = false;
            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }
        private void LoadGeneralControls()
        {
            // Cache Expiration Time
            txtCacheExpirationTime.Text = ConfigOptions.Find(x => x.Name == AppWrapper.AppWrapper.CacheExpirationTime).Value.ToString();
            txtCacheExpirationTime.Enabled = false;

            // File Types to Record in database
            var o = ConfigOptions.Find(x => x.Name == AppWrapper.AppWrapper.RecordFiles);
            string fileRecordType = string.Empty;
            if (o != null)
            {
                fileRecordType = o.Value.ToLower();
                switch (fileRecordType)
                {
                    case "a":
                        rbRecordAllFiles.Checked = true;
                        break;
                    case "s":
                        rbRecordSpecifiedFiles.Checked = true;
                        break;
                    case "n":
                        rbDontRecordFiles.Checked = true;
                        break;
                }
            }
            gbFileWatcher.Enabled = false;

            // Applications to monitor
            cbApplications.SelectedIndex = ConfigOptions.Find(x => x.Name == AppWrapper.AppWrapper.RecordFiles).Value == "A" ? 0 : 1;
            cbApplications.Enabled = false;
        }

        private void LoadDevProjectsControls(DevProjPath item)
        {
            txtDevProjectName.Text = item.DevProjectName;
            chkDevProjectDatabaseProject.Checked = item.DatabaseProject;
            txtDevProjectPath.Text = item.DevProjectPath;
            txtDevProjectIDEAppName.Text = item.IDEAppName;
            txtDevProjectMachine.Text = item.Machine;
            txtDevProjectUserName.Text = item.UserName;
            txtDevProjectCreatedDate.Text = $"{item.CreatedDate}";
            txtDevProjectCompletedDate.Text = $"{item.CompletedDate}";
            chkDevProjectDatabaseProject.Checked = item.DatabaseProject;
            txtDevProjectName.Enabled = false;
            chkDevProjectDatabaseProject.Enabled = false;
            txtDevProjectPath.Enabled = false;
            txtDevProjectIDEAppName.Enabled = false;
            txtDevProjectITProjectID.Enabled = false;
            txtDevProjectMachine.Enabled = false;
            txtDevProjectUserName.Enabled = false;
            txtDevProjectCreatedDate.Enabled = false;
            txtDevProjectCompletedDate.Enabled = false;
            chkDevProjectDatabaseProject.Enabled = false;
        }
        private void LoadConfigOptionsControls(ConfigOption item)
        {
            txtConfigOptionDescription.Text = item.Description;
            txtConfigOptionName.Text = item.Name;
            txtConfigOptionValue.Text = item.Value;
            txtConfigOptionDescription.Enabled = false;
            txtConfigOptionName.Enabled = false;
            txtConfigOptionValue.Enabled = false;
        }

        private void LoadApplicationControls(int idx)
        {
            txtAppFriendlyName.Text = AppList[idx].AppFriendlyName;
            txtAppName.Text = AppList[idx].AppName;
            txtAppFriendlyName.Enabled = false;
            txtAppName.Enabled = false;
        }

        private void LoadIDEMatchesControls(IDEMatch m)
        {
            txtMatchRegex.Text = m.Regex;
            txtMatchGroupName.Text = m.RegexGroupName;
            txtMatchUnknownValue.Text = m.UnknownValue;
            txtMatchAppName.Text = m.AppName;
            txtMatchProjNameReplaces.Text = m.ProjNameReplaces;
            txtMatchProjNameConcat.Text = m.ProjNameConcat; ;
            txtMatchDescription.Text = m.Description;
            txtMatchConcatChar.Text = m.ConcatChar;
            chkIMatchsDBEngine.Checked = m.IsDBEngine;
            chkMatchIsIDE.Checked = m.IsIde;
            txtMatchSequence.Text = m.Sequence.ToString();
            txtMatchAlternateProjectName.Text = m.AlternateProjName;
            txtMatchRegex.Enabled = false;
            txtMatchGroupName.Enabled = false;
            txtMatchUnknownValue.Enabled = false;
            txtMatchAppName.Enabled = false;
            txtMatchProjNameReplaces.Enabled = false;
            txtMatchProjNameConcat.Enabled = false;
            txtMatchDescription.Enabled = false;
            txtMatchConcatChar.Enabled = false;
            chkIMatchsDBEngine.Enabled = false;
            chkMatchIsIDE.Enabled = false;
            txtMatchSequence.Enabled = false;
            txtMatchAlternateProjectName.Enabled = false;
        }

        private void LoadFilesControls()
        {
            txtExtension.Text = NotableFiles[lbFileExtensions.SelectedIndex].Extension;
            txtProjectFileExtension.Text = NotableFiles[lbFileExtensions.SelectedIndex].IDEProjectExtension;
            txtExtension.Enabled = false;
            txtProjectFileExtension.Enabled = false;
            chkCountLines.Checked = NotableFiles[lbFileExtensions.SelectedIndex].CountLines;
        }
        private void btnClose_Click(object sender, EventArgs e)
        {
            if (_isDirty)
            {
                DialogResult dr = MessageBox.Show("You have unsaved changes on your form.  Click Yes to close without saving or No to cancel the close.", "Unsaved Changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dr == DialogResult.Yes)
                    this.Close();
                else
                    return;
            }
            this.Close();
        }
        private void btnClose_Click_1(object sender, EventArgs e)
        {
            if (_isDirty && DirtyMsg(tabControl1.SelectedTab.Name))
                return;
            Close();
        }
        #endregion

        #region tab change events
        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (_isDirty)
            {
                if (!DirtyMsg(current.Text))
                    return;

                e.Cancel = true;
            }
        }

        #endregion

        #region tabGeneral events

        #endregion

        #region private helper methods
        /// <summary>
        /// returns true if user elects to cancel the close and then save
        /// </summary>
        /// <param name="tabName"></param>
        /// <returns></returns>
        private bool DirtyMsg(string tabName)
        {
            DialogResult dr = MessageBox.Show($"You have changed controls on {tabName}.  Click OK to lose changes or Cancel to allow saving of the data.", "Changes Not Saved", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            return dr == DialogResult.Cancel;
        }

        private void LoadDevProjects()
        {
            var hlpr = new DHMisc(string.Empty);
            DevProjects = hlpr.GetDevProjects();
            lbDevProjects.Items.Clear();
            foreach (var proj in DevProjects)
            {
                lbDevProjects.Items.Add($"{proj.DevProjectName} - {proj.UserName} / {proj.Machine}");
            }
            lbDevProjects.SelectedIndex = 0;
            SetButtonsOnTabChange();
        }
        private void LoadConfigOptions()
        {
            var hlpr = new DHMisc(string.Empty);
            ConfigOptions = hlpr.GetConfigOptions();

            lbConfigOptions.Items.Clear();

            foreach (var option in ConfigOptions)
            {
                lbConfigOptions.Items.Add(option.Description);
            }
            lbConfigOptions.SelectedIndex = 0;
            SetButtonsOnTabChange();
        }
        private void LoadIDEMatches()
        {
            var hlpr = new DHMisc();
            IDEMatches = hlpr.GetProjectNameMatches();
            lbIDEMatches.Items.Clear();
            foreach (var m in IDEMatches)
            {
                lbIDEMatches.Items.Add($"{m.AppName} - {m.Description}");
            }
            lbIDEMatches.SelectedIndex = 0;
            SetButtonsOnTabChange();
        }
        private void LoadPermissions()
        {
            var hlpr = new DHMisc(string.Empty);

            SetButtonsOnTabChange();
        }

        private void LoadFiles()
        {
            var hlpr = new DHMisc();
            List<NotableFileExtension> files = hlpr.GetNotableFileExtensions();
            NotableFiles = hlpr.GetNotableFileExtensionsList();
            lbFileExtensions.Items.Clear();
            foreach (var ext in files)
                lbFileExtensions.Items.Add($"{ext.Extension} - {ext.IDEProjectExtension}");
            lbFileExtensions.SelectedIndex = 0;
            SetButtonsOnTabChange();
        }

        private void LoadGeneralOptions()
        {
            LoadGeneralControls();

            SetButtonsOnTabChange();
        }

        private void LoadApplications()
        {
            var hlpr = new DHMisc(string.Empty);
            AppList = hlpr.GetNotableApplications();
            lbApplications.Items.Clear();

            foreach (var app in AppList)
            {
                lbApplications.Items.Add($"{app.AppName} - {app.AppFriendlyName}");
            }
            lbApplications.SelectedIndex = 0;
            //LoadApplicationControls(lbApplications.SelectedIndex);
            SetButtonsOnTabChange();
        }

        private void DisableAllTabsButOne(string tabName)
        {
            switch (tabName)
            {
                case "tabGeneral":
                    EnableDisableControls(false, "tabApplications");
                    EnableDisableControls(false, "tabFiles");
                    EnableDisableControls(false, "tabPermissions");
                    EnableDisableControls(false, "tabMatchObjects");
                    EnableDisableControls(false, "tabConfigOptions");
                    EnableDisableControls(false, "tabDevProjects");
                    break;
                case "tabApplications":
                    EnableDisableControls(false, "tabGeneral");
                    EnableDisableControls(false, "tabFiles");
                    EnableDisableControls(false, "tabPermissions");
                    EnableDisableControls(false, "tabMatchObjects");
                    EnableDisableControls(false, "tabConfigOptions");
                    EnableDisableControls(false, "tabDevProjects");
                    break;
                case "tabFiles":
                    EnableDisableControls(false, "tabApplications");
                    EnableDisableControls(false, "tabGeneral");
                    EnableDisableControls(false, "tabPermissions");
                    EnableDisableControls(false, "tabMatchObjects");
                    EnableDisableControls(false, "tabConfigOptions");
                    EnableDisableControls(false, "tabDevProjects");
                    break;
                case "tabPermissions":
                    EnableDisableControls(false, "tabApplications");
                    EnableDisableControls(false, "tabGeneral");
                    EnableDisableControls(false, "tabFiles");
                    EnableDisableControls(false, "tabMatchObjects");
                    EnableDisableControls(false, "tabConfigOptions");
                    EnableDisableControls(false, "tabDevProjects");
                    break;
                case "tabMatchObjects":
                    EnableDisableControls(false, "tabApplications");
                    EnableDisableControls(false, "tabGeneral");
                    EnableDisableControls(false, "tabFiles");
                    EnableDisableControls(false, "tabPermissions");
                    EnableDisableControls(false, "tabConfigOptions");
                    EnableDisableControls(false, "tabDevProjects");
                    break;
                case "tabConfigOptions":
                    EnableDisableControls(false, "tabApplications");
                    EnableDisableControls(false, "tabGeneral");
                    EnableDisableControls(false, "tabFiles");
                    EnableDisableControls(false, "tabPermissions");
                    EnableDisableControls(false, "tabMatchObjects");
                    EnableDisableControls(false, "tabDevProjects");
                    break;
                case "tabDevProjects":
                    EnableDisableControls(false, "tabApplications");
                    EnableDisableControls(false, "tabGeneral");
                    EnableDisableControls(false, "tabFiles");
                    EnableDisableControls(false, "tabPermissions");
                    EnableDisableControls(false, "tabMatchObjects");
                    EnableDisableControls(false, "tabConfigOptions");
                    break;
            }
        }

        private void EnableDisableControls(bool enabled, string tabName, bool clearControls = false)
        {
            switch (tabName)
            {
                case "tabGeneral":
                    txtCacheExpirationTime.Enabled = enabled;
                    rbRecordAllFiles.Enabled = enabled;
                    rbDontRecordFiles.Enabled = enabled;
                    rbRecordSpecifiedFiles.Enabled = enabled;
                    gbFileWatcher.Enabled = enabled;
                    if (clearControls)
                        ClearTabGeneralControls();
                    break;
                case "tabApplications":
                    txtAppFriendlyName.Enabled = enabled;
                    txtAppName.Enabled = enabled;
                    if (clearControls)
                        ClearTabApplicationsControls();
                    break;
                case "tabFiles":
                    txtExtension.Enabled = enabled;
                    txtProjectFileExtension.Enabled = enabled;
                    if (clearControls)
                        ClearTabFilesControls();
                    break;
                case "tabPermissions":

                    if (clearControls)
                        ClearTabPermissionsControls();
                    break;
                case "tabMatchObjects":
                    txtMatchDescription.Enabled = enabled;
                    txtMatchRegex.Enabled = enabled;
                    txtMatchGroupName.Enabled = enabled;
                    txtMatchAppName.Enabled = enabled;
                    txtMatchProjNameConcat.Enabled = enabled;
                    txtMatchSequence.Enabled = enabled;
                    txtMatchConcatChar.Enabled = enabled;
                    txtMatchAlternateProjectName.Enabled = enabled;
                    chkMatchIsIDE.Enabled = enabled;
                    chkIMatchsDBEngine.Enabled = enabled;
                    txtMatchUnknownValue.Enabled = enabled;
                    txtMatchRegex.Enabled = enabled;
                    txtMatchProjNameReplaces.Enabled = enabled;
                    if (clearControls)
                        ClearTabProjectMatchOptions();
                    break;
                case "tabConfigOptions":
                    txtConfigOptionName.Enabled = enabled;
                    txtConfigOptionValue.Enabled = enabled;
                    txtConfigOptionDescription.Enabled = enabled;
                    if (clearControls)
                        ClearTabConfigOptions();
                    break;
                case "tabDevProjects":
                    txtDevProjectName.Enabled = enabled;
                    txtDevProjectPath.Enabled = enabled;
                    txtDevProjectIDEAppName.Enabled = enabled;
                    chkDevProjectDatabaseProject.Enabled = enabled;
                    txtDevProjectIDEAppName.Enabled = enabled;
                    txtDevProjectMachine.Enabled = enabled;
                    txtDevProjectUserName.Enabled = enabled;
                    txtDevProjectCreatedDate.Enabled = enabled;
                    txtDevProjectCompletedDate.Enabled = enabled;
                    txtDevProjectITProjectID.Enabled = enabled;
                    if (clearControls)
                        ClearDevProjectsControls();
                    break;
            }
        }

        private void ClearDevProjectsControls()
        {
            txtDevProjectName.Clear();
            txtDevProjectPath.Clear();
            txtDevProjectITProjectID.Clear();
            txtDevProjectIDEAppName.Clear();
            txtDevProjectMachine.Clear();
            txtDevProjectUserName.Clear();
            txtDevProjectCompletedDate.Clear();
            txtDevProjectCreatedDate.Clear();
        }

        private void ClearTabConfigOptions()
        {
            txtConfigOptionName.Clear();
            txtConfigOptionValue.Clear();
            txtConfigOptionDescription.Clear();
        }

        private void ClearTabProjectMatchOptions()
        {
            txtMatchDescription.Clear();
            txtMatchRegex.Clear();
            txtMatchGroupName.Clear();
            txtMatchUnknownValue.Clear();
            txtMatchAppName.Clear();
            txtMatchProjNameReplaces.Clear();
            txtMatchConcatChar.Clear();
            txtMatchProjNameConcat.Clear();
            txtMatchSequence.Clear();
            txtMatchAlternateProjectName.Clear();

        }

        private void ClearTabPermissionsControls()
        {
            // nothing yet
        }

        private void ClearTabFilesControls()
        {
            txtExtension.Clear();
            txtProjectFileExtension.Clear();
        }

        private void ClearTabApplicationsControls()
        {
            txtAppFriendlyName.Clear();
            txtAppName.Clear();
        }

        private void ClearTabGeneralControls()
        {
            // nothing to clear yet
        }

        private void ClearIDEMatchesControls()
        {
            txtMatchRegex.Clear();
            txtMatchGroupName.Clear();
            txtMatchUnknownValue.Clear();
            txtMatchAppName.Clear();
            txtMatchProjNameReplaces.Clear();
            txtMatchProjNameConcat.Clear();
            txtMatchConcatChar.Clear();
            txtMatchDescription.Clear();
            txtMatchSequence.Clear();
            txtMatchAlternateProjectName.Clear();
            chkIMatchsDBEngine.Checked = false;
            chkMatchIsIDE.Checked = false;
        }


        private void SetButtonsOnTabChange()
        {
            btnAdd.Enabled = true;
            btnUpdate.Enabled = true;
            btnDelete.Enabled = true;
            btnSave.Enabled = false;
            btnCancel.Enabled = false;
        }

        private string _errors = string.Empty;
        private int _number = 0;
        private bool ValidateTabGeneralForSave()
        {
            if (!int.TryParse(txtCacheExpirationTime.Text, out _number))
            {
                _errors += "Cache Timeout must be greater than zero.";
            }
            return string.IsNullOrWhiteSpace(_errors);
        }
        private bool ValidateTabApplicationsForSave()
        {
            if (string.IsNullOrWhiteSpace(txtAppFriendlyName.Text) || string.IsNullOrWhiteSpace(txtAppName.Text))
            {
                _errors += "Both App Name and Friendly Name must be entered.";
            }
            return string.IsNullOrWhiteSpace(_errors);
        }

        private bool ValidateTabFilesForSave()
        {
            if (string.IsNullOrWhiteSpace(txtExtension.Text))
            {
                _errors += "The extension must be entered, and the project file extension for any project type file such as 'vb' or 'cs', etc.";
            }
            return string.IsNullOrWhiteSpace(_errors);
        }

        private bool ValidateTabIDEMatches()
        {
            if (string.IsNullOrWhiteSpace(txtMatchRegex.Text)
                || string.IsNullOrWhiteSpace(txtMatchDescription.Text)
                || string.IsNullOrWhiteSpace(txtMatchGroupName.Text)
                || string.IsNullOrWhiteSpace(txtMatchUnknownValue.Text))
            {
                _errors += "Regex, Description, Group Name, Unknown Value must be entered.";
                if (!string.IsNullOrWhiteSpace(txtMatchSequence.Text) && !txtMatchSequence.Text.IsNumeric(out _number))
                    _errors += $"If used, Sequence must be numeric.{Environment.NewLine}";
            }
            return string.IsNullOrWhiteSpace(_errors);
        }

        private bool ValidateTabConfigOptions()
        {
            if (string.IsNullOrWhiteSpace(txtConfigOptionDescription.Text) || string.IsNullOrWhiteSpace(txtConfigOptionName.Text) || string.IsNullOrWhiteSpace(txtConfigOptionValue.Text))
            {
                _errors += $"Name, Value, and Description must be entered{Environment.NewLine}";
            }
            return string.IsNullOrWhiteSpace(_errors);
        }
        private bool ValidateTabDevProjecctsForSave()
        {
            if (string.IsNullOrWhiteSpace(txtDevProjectUserName.Text)
                || string.IsNullOrWhiteSpace(txtDevProjectPath.Text)
                || string.IsNullOrWhiteSpace(txtDevProjectIDEAppName.Text)
                || string.IsNullOrWhiteSpace(txtDevProjectMachine.Text) 
                || string.IsNullOrWhiteSpace(txtDevProjectUserName.Text) )
            {
                _errors += $"Project Name, Path, AppName, Machine, UserName, CreatedDate must be entered.";
            }
            DateTime date;
            if (!string.IsNullOrWhiteSpace(txtDevProjectCompletedDate.Text) && DateTime.TryParse(txtDevProjectCompletedDate.Text, out date))
                _errors += $"{Environment.NewLine}CompletedDate is invalid.";

            return string.IsNullOrWhiteSpace(_errors);
        }
        #endregion

        #region form and control event handlers

        private void lbApplications_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (lbApplications.SelectedItem == null)
                    return;

                var p = lbApplications.SelectedIndex;
                txtAppName.Text = AppList[p].AppName;
                txtAppFriendlyName.Text = AppList[p].AppFriendlyName;
                txtAppFriendlyName.Enabled = false;
                txtAppName.Enabled = false;
                _isDirty = false;
            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }


        private void txtCacheExpirationTime_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {

            try
            {
                switch (tabControl1.SelectedTab.Name)
                {
                    case "tabGeneral":
                        DisableAllTabsButOne("tabGeneral");
                        break;
                    case "tabApplications":
                        DisableAllTabsButOne("tabApplications");
                        lbApplications.SelectedIndex = 0;
                        break;
                    case "tabFiles":
                        DisableAllTabsButOne("tabFiles");
                        lbFileExtensions.SelectedIndex = 0;
                        break;
                    case "tabPermissions":
                        DisableAllTabsButOne("tabPermissions");

                        break;
                    case "tabMatchObjects":
                        DisableAllTabsButOne("tabMatchOptions");
                        lbIDEMatches.SelectedIndex = 0;

                        break;
                    case "tabConfigOptions":
                        DisableAllTabsButOne("tabConfigOptions");
                        lbConfigOptions.SelectedIndex = 0;
                        break;
                    case "tabDevProjects":
                        DisableAllTabsButOne("tabDevProjects");
                        lbDevProjects.SelectedIndex = 0;
                        break;
                }
                _isDirty = false;

            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }

        private void lbIDEMatches_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var m = IDEMatches[lbIDEMatches.SelectedIndex];
                LoadIDEMatchesControls(m);
                _isDirty = false;
            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }

        private void lbFileExtensions_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LoadFilesControls();
                _isDirty = false;
            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }
        private void lbConfigOptions_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var item = ConfigOptions[lbConfigOptions.SelectedIndex];
                LoadConfigOptionsControls(item);
                _isDirty = false;
            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }

        private void lbDevProjects_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var item = DevProjects[lbDevProjects.SelectedIndex];
                LoadDevProjectsControls(item);
                _isDirty = false;
            }
            catch (Exception ex)
            {
                Util.LogError(ex, true);
            }
        }

        #endregion

        #region make dirty events
        private void rbRecordAllFiles_CheckedChanged(object sender, EventArgs e)
        {
            _isDirty = true;
        }

        private void rbRecordSpecifiedFiles_CheckedChanged(object sender, EventArgs e)
        {
            _isDirty = true;
        }

        private void rbDontRecordFiles_CheckedChanged(object sender, EventArgs e)
        {
            _isDirty = true;
        }

        private void txtAppFriendlyName_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtAppName_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtExtension_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtProjectFileExtension_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtMatchDescription_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtRegex_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtGroupName_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtUnknownValue_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtMatchAppName_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtProjNameReplaces_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtProjNameConcat_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtSequence_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtAlternateProjectName_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtConcatChar_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void chkIsIDE_CheckedChanged(object sender, EventArgs e)
        {
            _isDirty = true;
        }

        private void chkIsDBEngine_CheckedChanged(object sender, EventArgs e)
        {
            _isDirty = true;
        }

        private void txtConfigOptionName_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtConfigOptionValue_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        private void txtConfigOptionDescription_KeyUp(object sender, KeyEventArgs e)
        {
            _isDirty = true;
        }

        #endregion

    }
}

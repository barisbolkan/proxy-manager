using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.WCFReference.Interop;
using ProxyMgr.ProxyManager.Utilities;
using ProxyMgr.ProxyManager.ViewModel;
using ProxyMgr.ProxyManager.Views;

namespace ProxyMgr.ProxyManager
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidProxyManagerPkgString)]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids.SolutionExists)]
    public sealed class ProxyManagerPackage : Package
    {
        #region Fields
        /// <summary>
        /// Backing field for <see cref="LogWriter"/>
        /// </summary>
        private OutputWindowWriter logWriter;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the logging operation to OutputPane to log
        /// </summary>
        private OutputWindowWriter LogWriter
        {
            get
            {
                if (logWriter == null)
                    logWriter = new OutputWindowWriter(this);

                return logWriter;
            }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public ProxyManagerPackage()
        { }
        #endregion

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            // Signal base
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;

            if (null != mcs)
            {
                // Create all comands in the package
                CommandID addCommandID = new CommandID(GuidList.guidProxyManagerCmdSet, (int)PkgCmdIDList.addServiceProxy);
                CommandID addNewCommandID = new CommandID(GuidList.guidProxyManagerCmdSet, (int)PkgCmdIDList.addNewServiceProxy);
                CommandID configureCommandID = new CommandID(GuidList.guidProxyManagerCmdSet, (int)PkgCmdIDList.configureServiceProxy);

                // Add menu commands
                OleMenuCommand addNewMenuCommand = new OleMenuCommand(AddMenuItemClicked, addNewCommandID);
                addNewMenuCommand.BeforeQueryStatus += menuCommand_BeforeQueryStatus;
                mcs.AddCommand(addNewMenuCommand);
                OleMenuCommand configureMenuCommand = new OleMenuCommand(ConfigureMenuItemClicked, configureCommandID);
                configureMenuCommand.BeforeQueryStatus += menuCommand_BeforeQueryStatus;
                mcs.AddCommand(configureMenuCommand);
                mcs.AddCommand(new OleMenuCommand(AddMenuItemClicked, addCommandID));
            }
        }

        /// <summary>
        /// Executes when right clicked to a folder item in the project
        /// </summary>
        /// <param name="sender">Owner of the event</param>
        /// <param name="e">Event arguments</param>
        private void menuCommand_BeforeQueryStatus(object sender, EventArgs e)
        {
            // get the menu that fired the event
            var command = sender as OleMenuCommand;

            // Define handles
            IVsHierarchy hierarchy = null;
            uint itemId = VSConstants.VSITEMID_NIL;

            // Get services
            IVsSolution solution = Package.GetGlobalService(typeof(SVsSolution)) as IVsSolution;

            // Set all visibility to false
            command.Visible = false;
            command.Enabled = false;

            // Get selected hierarchy from solution
            // Check if any hierarchy selected or not
            if ((hierarchy = solution.GetSelectedHierarchy(out itemId)) == null)
                return;

            // Get selected project item
            ProjectItem selectedProjectItem = hierarchy.GetSelectedProjectItem(itemId);

            /*
             * Now we have selected project item
             * So check its name and directory location
             */
            uint commandId = (uint)command.CommandID.ID;
            string selectionItemName = selectedProjectItem.Name;
            string projectPathDirectoryName = Path.GetDirectoryName(selectedProjectItem.ContainingProject.FullName);

            // Get WCF services
            IVsWCFReferenceGroupCollection serviceReferences = this.GetWCFServices(hierarchy);

            if (serviceReferences.Count() > 0)
            {
                if (commandId == PkgCmdIDList.configureServiceProxy)
                {
                    IVsWCFReferenceGroup serviceReferenceItem = serviceReferences.GetReferenceGroupByName(selectionItemName, selectionItemName);

                    if (serviceReferenceItem != null)
                    {
                        command.Enabled = true;
                        command.Visible = true;
                    }
                }
                else if (commandId == PkgCmdIDList.addNewServiceProxy && selectionItemName.Equals(ProxyMgrConstants.PackageFolderName))
                {
                    command.Visible = true;
                    command.Enabled = true;
                }
            }
        }
        #endregion

        #region Command Events
        /// <summary>
        /// This event has fired when Add Menu Item clicked
        /// </summary>
        private void AddMenuItemClicked(object sender, EventArgs e)
        {
            this.ShowProxyEntry(new ProxyEntryInformation());
        }

        /// <summary>
        /// This event has fired when Configure Menu Item clicked
        /// </summary>
        private void ConfigureMenuItemClicked(object sender, EventArgs e)
        {
            // Get current solution
            IVsSolution solution = GetService(typeof(IVsSolution)) as IVsSolution;
            uint itemId = VSConstants.VSITEMID_NIL;
            IVsHierarchy hierarchy = solution.GetSelectedHierarchy(out itemId);
            ProjectItem selectedProjectItem = hierarchy.GetSelectedProjectItem(itemId);

            // Get paths and file names...
            string selectedItemName = selectedProjectItem.Name;
            string fullpath = Path.GetDirectoryName(selectedProjectItem.Properties.Item("FullPath").Value.ToString());
            string svcMapFilePath = string.Concat(fullpath, Path.DirectorySeparatorChar, selectedItemName, ProxyMgrConstants.SvcmapFileExtension);
            ProxyEntryInformation context = null;

            // Read svcmap file
            if (File.Exists(svcMapFilePath))
            {
                using (System.IO.FileStream file = File.Open(svcMapFilePath, FileMode.Open, FileAccess.Read))
                {
                    context = file.Deserialize<ProxyEntryInformation>();
                }
            }

            // Check context
            if (context == null)
            {
                LogWriter.WriteLine(string.Format("[ WARNING ] {0} file not found.", svcMapFilePath));
                context = new ProxyEntryInformation() { ServiceName = selectedItemName, ShowMissingMap = false };
            }

            context.IsAddContext = false;

            // Show input entry window
            this.ShowProxyEntry(context);
        }
        #endregion

        #region Events
        private void dialog_OnProxyEntered(object sender, ProxyEntryInformation pInfo)
        {
            // Get active project
            DTE dte = (DTE)GetService(typeof(DTE));
            Project activeProject = (dte.ActiveSolutionProjects as Array).GetValue(0) as EnvDTE.Project;

            // Get current solution
            IVsSolution solution = GetService(typeof(IVsSolution)) as IVsSolution;
            // Get WCF Service references
            uint itemId = VSConstants.VSITEMID_NIL;
            IVsWCFReferenceGroupCollection serviceReferences = this.GetWCFServices(solution.GetSelectedHierarchy(out itemId));

            // Create project file variables
            string languageFileExtension = GetLanguageFileExtension(activeProject);
            string projectFile = activeProject.FullName;    // C:\MySampleApp\MySampleApp\MySampleApp.csproj
            string generatedCodeFilePath = string.Format(@"{0}\{1}\{2}\{2}.proxy.{3}", Path.GetDirectoryName(projectFile), ProxyMgrConstants.PackageFolderName, pInfo.ServiceName, languageFileExtension); //C:\MySampleApp\MySampleApp\Service Proxies\Input\
            string svcmapFilePath = string.Format(@"{0}\{1}{2}", Path.GetDirectoryName(generatedCodeFilePath), pInfo.ServiceName, ProxyMgrConstants.SvcmapFileExtension);
            List<string> itemsToCheckout = new List<string>(2);
            FileStream svcMapStream = null;

            // Log 
            LogWriter.WriteLine(@"****************** PROXY GENERATION STARTED: " + pInfo.ServiceAddress + " ******************");
            LogWriter.WriteLine("[ OK ] State: " + (pInfo.IsAddContext ? "Add" : "Configure"));
            LogWriter.WriteLine("[ OK ] CodeFilePath: " + generatedCodeFilePath);
            LogWriter.WriteLine("[ OK ] MapFilePath : " + svcmapFilePath);

            // if intended service not exists
            if (serviceReferences.GetReferenceGroupByName(pInfo.ServiceName, pInfo.ServiceName) == null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(generatedCodeFilePath)))
                    Directory.CreateDirectory(Path.GetDirectoryName(generatedCodeFilePath));

                svcMapStream = File.Create(svcmapFilePath);
                this.CheckOutItems(dte, projectFile);
            }
            else
            {
                if (pInfo.IsAddContext)
                {
                    // Service proxy exists
                    ShowError("[ FAIL ] Intended Service Reference name is exists. Please enter another name for proxy!");
                    return;
                }

                if (!File.Exists(svcmapFilePath))
                {
                    // Something went worng error
                    ShowError("[ FAIL ] Service mapping file is missing. Check path: " + svcmapFilePath);
                    return;
                }

                this.CheckOutItems(dte, svcmapFilePath, generatedCodeFilePath);
                svcMapStream = File.Open(svcmapFilePath, FileMode.Truncate, FileAccess.Write);
            }

            using (svcMapStream)
                svcMapStream.Serialize(pInfo);

            // Generate code
            this.GenerateServiceProxy(activeProject, pInfo);

            if (pInfo.IsAddContext)
                this.ManipulateProjectFile(projectFile, (serviceReferences.Count() > 0), languageFileExtension, pInfo.ServiceName);

            // Save the project!!!
            activeProject.Save();

            // Reload project
            this.Reload(activeProject);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Checks out the item if item is in source control
        /// </summary>
        /// <param name="dte">Current dte</param>
        /// <param name="items">item names to checked out</param>
        private void CheckOutItems(DTE dte, params string[] items)
        {
            // Check if source control exists and checkout item if it is not checked out
            if (dte.SourceControl != null)
                foreach (string item in items)
                    if (dte.SourceControl.IsItemUnderSCC(item) && !dte.SourceControl.IsItemCheckedOut(item))
                        dte.SourceControl.CheckOutItem(item);
        }

        private bool GenerateServiceProxy(Project activeProject, ProxyEntryInformation pInfo)
        {
            string languageFileExtension = this.GetLanguageFileExtension(activeProject);
            string generatedCodeFilePath = string.Format(@"{0}\{1}\{2}\{2}.proxy.{3}", Path.GetDirectoryName(activeProject.FullName), ProxyMgrConstants.PackageFolderName, pInfo.ServiceName, languageFileExtension); //C:\MySampleApp\MySampleApp\Service Proxies\Input\
            string arguments = string.Format(ProxyMgrConstants.SvcutilCommandArgumentTemplate, (pInfo.GenerateClient ? "" : "/sc"), pInfo.ServiceAddress, generatedCodeFilePath, languageFileExtension, "Auto", activeProject.Name, pInfo.ServiceName);
            string executionWarning = null;
            bool operationIsSuccess = false;

            // Execute external process
            if (this.ExecuteProcessWithArguments(ProxyMgrConstants.SvcUtilPath, arguments, out executionWarning))
            {
                operationIsSuccess = true;

                // Get newly added project file
                // When ProcjectItems.AddFromFile method called it adds Compile tag in project file
                // We need this because we are going to look up to interface is created or not!
                ProjectItem generatedCodeProjectItem = activeProject.ProjectItems.AddFromFile(generatedCodeFilePath);

                // We have to check generated code file because it may contain service interface or not
                // If it doesn't contain service interface then we  have to generate the code again with XmlSerializer
                if (generatedCodeProjectItem.FileCodeModel.CodeElements.FindInterface() == null)
                {
                    // Log all project file variables
                    LogWriter.WriteLine("[ WARNING ] Something wrong with service wsdl. Error occured while trying to generate code with 'serializer:Auto' flag. Now changing to XmlSerializer.");
                    arguments = string.Format(ProxyMgrConstants.SvcutilCommandArgumentTemplate, (pInfo.GenerateClient ? "" : "/sc"), pInfo.ServiceAddress, generatedCodeFilePath, languageFileExtension, "XmlSerializer", activeProject.Name, pInfo.ServiceName);

                    // Generate code again!!!
                    if (!this.ExecuteProcessWithArguments(ProxyMgrConstants.SvcUtilPath, arguments, out executionWarning))
                    {
                        operationIsSuccess = false;
                        // Log that something wrong!!!
                        LogWriter.WriteLine("[ FAIL ] Something went wrong while generating service proxy.");
                    }
                }

                // Add as warning
                if (!string.IsNullOrWhiteSpace(executionWarning))
                    new ErrorListProvider(this).Tasks.Add(new ErrorTask { Category = TaskCategory.User, ErrorCategory = TaskErrorCategory.Warning, Text = executionWarning });
            }

            return operationIsSuccess;
        }

        /// <summary>
        /// Executes the given executable in the given path with the given arguments
        /// </summary>
        /// <param name="commandPath">Executable path</param>
        /// <param name="arguments">Arguments</param>
        /// <returns>returns true if execution is success otherwise returns false</returns>
        private bool ExecuteProcessWithArguments(string commandPath, string arguments, out string warningInformation)
        {
            // Create process for svcutil.exe
            // Set values of the process
            ProcessStartInfo svcutilProc = new ProcessStartInfo();
            svcutilProc.CreateNoWindow = true;
            svcutilProc.UseShellExecute = false;
            svcutilProc.FileName = commandPath;
            svcutilProc.WindowStyle = ProcessWindowStyle.Hidden;
            svcutilProc.Arguments = arguments;
            svcutilProc.RedirectStandardError = true;
            warningInformation = null;

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (System.Diagnostics.Process exeProcess = System.Diagnostics.Process.Start(svcutilProc))
                {
                    // Wait for execution
                    exeProcess.WaitForExit();

                    // Get errors if exists
                    string warning = exeProcess.StandardError.ReadToEnd();

                    // Check operation state
                    if (exeProcess.ExitCode != 0)
                        throw new Exception(warning);

                    LogWriter.WriteLine(string.Format("[ {0} ] Process execution completed.", !string.IsNullOrWhiteSpace(warning) ? "WARNING" : "OK"));
                    warningInformation = warning;

                    // If came to here everything is fine
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error.
                LogWriter.WriteLine("[ FAIL ] Process execution failure! Exception info: " + ex.ToString());
                LogWriter.WriteLine("[ FAIL ] Process call arguments: " + arguments);

                return false;
            }
        }

        /// <summary>
        /// Shows the input etry window with the given model
        /// </summary>
        /// <param name="model">Model to bind to</param>
        private void ShowProxyEntry(ProxyEntryInformation model)
        {
            ProxyEntry dialog = new ProxyEntry(model);
            // Attach event
            dialog.OnProxyEntered += dialog_OnProxyEntered;
            // Show window
            dialog.ShowModal();
        }

        /// <summary>
        /// Shows error message with the given message
        /// </summary>
        /// <param name="message">Message to show as error</param>
        private void ShowError(string message)
        {
            // Show a Message Box to prove we were here
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            int result;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                        0,
                        ref clsid,
                        "Error",
                        message,
                        string.Empty,
                        0,
                        OLEMSGBUTTON.OLEMSGBUTTON_OK,
                        OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                        OLEMSGICON.OLEMSGICON_CRITICAL,
                        0,        // false
                        out result));
        }

        /// <summary>
        /// Retrieves the code language of the given Project
        /// </summary>
        /// <param name="project">Project</param>
        /// <returns>File extension of the project</returns>
        private string GetLanguageFileExtension(Project project)
        {
            CodeModel cm = project.CodeModel;
            string fileExtension = "cs";

            switch (cm.Language)
            {
                case CodeModelLanguageConstants.vsCMLanguageMC:
                case CodeModelLanguageConstants.vsCMLanguageCSharp:
                    fileExtension = "cs";
                    break;
                case CodeModelLanguageConstants.vsCMLanguageVB:
                    fileExtension = "vb";
                    break;
            }

            return fileExtension;
        }

        /// <summary>
        /// Manipulates the given project file for adding wcf service metadata storages
        /// </summary>
        /// <param name="projectFile">Current active project file</param>
        /// <param name="addServiceRefMetadata"></param>
        /// <param name="languageFileExtension"></param>
        /// <param name="name"></param>
        private void ManipulateProjectFile(string projectFile, bool addServiceRefMetadata, string languageFileExtension, string name)
        {
            string metadataDirectory = string.Concat(ProxyMgrConstants.PackageFolderName, Path.DirectorySeparatorChar); // Service Proxies\
            string metadataStorageDirectory = string.Concat(metadataDirectory, name, Path.DirectorySeparatorChar);    // Service Proxies\Input\
            string generatedCodeFile = string.Format(ProxyMgrConstants.GeneratedCodeFileNameTemplate, name, languageFileExtension);  // input.proxy.cs
            string dependUponPath = string.Concat(metadataStorageDirectory, string.Concat(name, ProxyMgrConstants.SvcmapFileExtension));

            var project = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.LoadProject(projectFile);

            // Check for WCFMetadata 
            // Meaning that if any service reference added to project file or not
            // <WCFMetadata></WCFMetadata>
            if (!addServiceRefMetadata)
            {
                // Create metadata in project file
                project.AddItem(ProxyMgrConstants.ProjectFileSvcItemName, metadataDirectory);

                // Add needed references
                project.AddItem(ProxyMgrConstants.ReferenceTagName, ProxyMgrConstants.SerializationAssemblyNamespace);
                project.AddItem(ProxyMgrConstants.ReferenceTagName, ProxyMgrConstants.ServiceModelAssemblyNamespace);
            }

            // Get collection
            project.AddItem(ProxyMgrConstants.ProjectFileSvcStorageItemName, metadataStorageDirectory);

            // Find newly added file
            foreach (var item in project.GetItems(ProxyMgrConstants.CompileTagName))
            {
                if (item.EvaluatedInclude.Equals(string.Concat(metadataStorageDirectory, generatedCodeFile)))
                {
                    item.SetMetadataValue(ProxyMgrConstants.AutoGenerateTagName, ProxyMgrConstants.AutoGenerationTagValue);
                    item.SetMetadataValue(ProxyMgrConstants.DesignTimeTagName, ProxyMgrConstants.DesignTimeTagValue);
                    item.SetMetadataValue(ProxyMgrConstants.DependentUponTagName, string.Concat(name, ProxyMgrConstants.SvcmapFileExtension));
                }
            }

            project.AddItem(ProxyMgrConstants.NoneTagName, dependUponPath, new List<KeyValuePair<string, string>>() { 
                new KeyValuePair<string, string>(ProxyMgrConstants.GeneratorTagName, ProxyMgrConstants.GeneratorTagValue) 
            });

            // Change project type and save
            Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.UnloadProject(project);
        }

        /// <summary>
        /// GEts the WCF service references added to selected hierarchy
        /// </summary>
        /// <param name="hierarchy"></param>
        /// <returns></returns>
        private IVsWCFReferenceGroupCollection GetWCFServices(IVsHierarchy hierarchy)
        {
            // Get service reference infos
            IVsWCFReferenceManagerFactory wcfReferenceManagerFactory = GetService(typeof(SVsWCFReferenceManagerFactory)) as IVsWCFReferenceManagerFactory;
            IVsWCFReferenceManager serviceManager = wcfReferenceManagerFactory.GetReferenceManager(hierarchy);

            // return...
            return serviceManager.GetReferenceGroupCollection();
        }

        /// <summary>
        /// Reloads the given project file in the given solution
        /// </summary>
        /// <param name="dte">Current development environment</param>
        /// <param name="solutionName">Solution name which project belongs</param>
        /// <param name="projectName">Project file which needs reload</param>
        private void Reload(Project project)
        {
            try
            {
                DTE dte = GetService(typeof(DTE)) as DTE;
                string projectName = ((dte.ActiveSolutionProjects as Array).GetValue(0) as Project).Name;

                // Select the project
                ((EnvDTE80.DTE2)dte).ToolWindows.SolutionExplorer.UIHierarchyItems.GetHierarchyItem(projectName).Select(vsUISelectionType.vsUISelectionTypeSelect);

                // Unlooad & load
                dte.ExecuteCommand(ProxyMgrConstants.ProjectUnloadCommand);
                dte.ExecuteCommand(ProxyMgrConstants.ProjectReloadCommand);

                // Select References folder
                // This is very strange because after reload i guess hierarchy is changing. 
                // Because of that we cant get a reference of selected hierarchy, we have to retrieve it again.
                ((EnvDTE80.DTE2)dte).ToolWindows.SolutionExplorer.UIHierarchyItems.GetHierarchyItem(projectName).UIHierarchyItems.GetHierarchyItem("Service References").Select(vsUISelectionType.vsUISelectionTypeSelect);

                LogWriter.WriteLine("[ OK ] Project  reloaded.");
            }
            catch (Exception ex)
            {
                LogWriter.WriteLine("[ FAIL ] Project reload failed! Exception: " + ex.ToString());
            }
        }
        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
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
                {
                    logWriter = new OutputWindowWriter(this);
                }

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
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }
        #endregion

        #region Package Members
        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            // Log
            LogWriter.WriteLine("[OK] Intializing Proxy Manager...");

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

                // Log
                LogWriter.WriteLine("[OK] Menu items and commands created.");
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
            this.ShowProxyEntry(new ProxyEntryViewModel());
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
            ProxyEntryInfo map = null;

            // Read svcmap file
            if (File.Exists(svcMapFilePath))
            {
                using (System.IO.FileStream file = File.Open(svcMapFilePath, FileMode.Open, FileAccess.Read))
                {
                    map = file.Deserialize<ProxyEntryInfo>();
                }
            }

            // Log
            LogWriter.WriteLine(string.Format((map == null ? "{0} file not found." : "{0} file found."), svcMapFilePath));

            // Show input entry window
            this.ShowProxyEntry(new ProxyEntryViewModel()
            {
                GenerateClient = map == null ? false : map.GenerateClient,
                IsAddContext = false,
                ServiceAddress = (map == null ? null : map.Url),
                ServiceName = selectedItemName,
                UseXmlSerializer = map == null ? false : map.UseXmlSerializer,
                ShowMissingMap = (map == null ? true : (string.IsNullOrWhiteSpace(map.Url) ? true : false))
            });
        }
        #endregion

        #region Events
        private void dialog_OnProxyEntered(object sender, ProxyEntryInfo e)
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
            bool wcfMetadataExists = serviceReferences.Count() > 0;
            string languageFileExtension = GetLanguageFileExtension(activeProject);
            string projectFile = activeProject.FullName;    // C:\MySampleApp\MySampleApp\MySampleApp.csproj
            string metadataDirectory = string.Concat(ProxyMgrConstants.PackageFolderName, Path.DirectorySeparatorChar); // Service Proxies\
            string metadataStorageDirectory = string.Concat(metadataDirectory, e.Name, Path.DirectorySeparatorChar);    // Service Proxies\Input\
            string generatedCodeFile = string.Format(ProxyMgrConstants.GeneratedCodeFileNameTemplate, e.Name, languageFileExtension);  // input.proxy.cs
            string generatedCodeFilePath = string.Format(ProxyMgrConstants.GeneratedCodeFilePathTemplate, Path.GetDirectoryName(projectFile) + Path.DirectorySeparatorChar, metadataStorageDirectory, generatedCodeFile); //C:\MySampleApp\MySampleApp\Service Proxies\Input\input.proxy.cs
            string arguments = string.Format(ProxyMgrConstants.SvcutilCommandArgumentTemplate, (e.GenerateClient ? "" : "/sc"), e.Url, generatedCodeFilePath, languageFileExtension, e.UseXmlSerializer ? "XmlSerializer" : "DataContractSerializer", activeProject.Name, e.Name);
            string depentUponPath = string.Concat(metadataStorageDirectory, string.Concat(e.Name, ProxyMgrConstants.SvcmapFileExtension));
            string svcmapFilePath = string.Concat(Path.GetDirectoryName(projectFile), Path.DirectorySeparatorChar, depentUponPath);

            // Log all project file variables
            LogWriter.WriteLine("Project File: " + projectFile);
            LogWriter.WriteLine("Metadata Directory: " + metadataDirectory);
            LogWriter.WriteLine("Metadata Storage Directory: " + metadataStorageDirectory);
            LogWriter.WriteLine("Generated Code File: " + generatedCodeFile);
            LogWriter.WriteLine("Generated Code File Path: " + generatedCodeFilePath);
            LogWriter.WriteLine("Svcmap File Path: " + svcmapFilePath);
            LogWriter.WriteLine("WCF Metadata Exists: " + wcfMetadataExists);

            // Execute external process
            if (this.ExecuteProcessWithArguments(ProxyMgrConstants.SvcUtilPath, arguments))
            {
                // Load the project file
                var project = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.LoadProject(projectFile);

                // Check for WCFMetadata 
                // Meaning that if any service reference added to project file or not
                // <WCFMetadata></WCFMetadata>
                if (!wcfMetadataExists)
                {
                    // Create metadata in project file
                    project.AddItem(ProxyMgrConstants.ProjectFileSvcItemName, metadataDirectory);

                    // Add needed references
                    project.AddItem(ProxyMgrConstants.ReferenceTagName, ProxyMgrConstants.SerializationAssemblyNamespace);
                    project.AddItem(ProxyMgrConstants.ReferenceTagName, ProxyMgrConstants.ServiceModelAssemblyNamespace);
                }

                FileStream stream = null;

                // Now we can add service storage directory
                if (serviceReferences.GetReferenceGroupByName(e.Name, e.Name) == null)
                {
                    // Get collection
                    project.AddItem(ProxyMgrConstants.ProjectFileSvcStorageItemName, metadataStorageDirectory);
                    stream = File.Create(svcmapFilePath);

                    // Add genereted file to folder
                    project.AddItem(ProxyMgrConstants.CompileTagName, string.Concat(metadataStorageDirectory, generatedCodeFile), new List<KeyValuePair<string, string>>() { 
                        new KeyValuePair<string, string>(ProxyMgrConstants.AutoGenerateTagName, ProxyMgrConstants.AutoGenerationTagValue),
                        new KeyValuePair<string, string>(ProxyMgrConstants.DesignTimeTagName, ProxyMgrConstants.DesignTimeTagValue),
                        new KeyValuePair<string, string>(ProxyMgrConstants.DependentUponTagName, string.Concat(e.Name, ".svcmap"))
                    });
                    project.AddItem(ProxyMgrConstants.NoneTagName, depentUponPath, new List<KeyValuePair<string, string>>() { 
                        new KeyValuePair<string, string>(ProxyMgrConstants.GeneratorTagName, ProxyMgrConstants.GeneratorTagValue)
                    });
                }
                else
                {
                    stream = File.Open(svcmapFilePath, FileMode.Truncate, FileAccess.Write);
                }

                using (stream)
                {
                    stream.Serialize(e);
                }

                // Change project type and save
                Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.UnloadProject(project);
                activeProject.Save();
                project.ReevaluateIfNecessary();

                // Reload project
                this.Reload(activeProject);
            }
            else
            {
                this.ShowError("There is an error occured while creating the proxy. Please check for 'Output Window/Proxy Generation' for details.");
            }
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Executes the given executable in the given path with the given arguments
        /// </summary>
        /// <param name="commandPath">Executable path</param>
        /// <param name="arguments">Arguments</param>
        /// <returns>returns true if execution is success otherwise returns false</returns>
        private bool ExecuteProcessWithArguments(string commandPath, string arguments)
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

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (System.Diagnostics.Process exeProcess = System.Diagnostics.Process.Start(svcutilProc))
                {
                    // Wait for execution
                    exeProcess.WaitForExit();

                    // Get errors if exists
                    string error = exeProcess.StandardError.ReadToEnd();

                    // Check operation state
                    if (exeProcess.ExitCode != 0)
                        throw new Exception(error);

                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        LogWriter.WriteLine("[OK] Process execution completed with warnings.");
                        // Add as warning
                        new ErrorListProvider(this).Tasks.Add(new ErrorTask
                        {
                            Category = TaskCategory.User,
                            ErrorCategory = TaskErrorCategory.Warning,
                            Text = error
                        });
                    }
                    else
                    {
                        LogWriter.WriteLine("[OK] Process execution completed successfully.");
                    }

                    // If came to here everything is fine
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log error.
                LogWriter.WriteLine("[FAIL] Process execution failure! Exception info: " + ex.ToString());
                LogWriter.WriteLine("[FAIL] Process call arguments: " + arguments);

                return false;
            }
        }

        /// <summary>
        /// Shows the input etry window with the given model
        /// </summary>
        /// <param name="model">Model to bind to</param>
        private void ShowProxyEntry(ProxyEntryViewModel model)
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
                string projectPathToSelect = string.Concat(Path.GetFileNameWithoutExtension(dte.Solution.FullName), Path.DirectorySeparatorChar, project.Name);
                string projectItemPathToSelect = string.Concat(projectPathToSelect, Path.DirectorySeparatorChar, ProxyMgrConstants.PackageFolderName);

                // Activate 
                dte.Windows.Item(EnvDTE.Constants.vsWindowKindSolutionExplorer).Activate();
                // Select the project
                ((EnvDTE80.DTE2)dte).ToolWindows.SolutionExplorer.GetItem(projectPathToSelect).Select(vsUISelectionType.vsUISelectionTypeSelect);

                // Unlooad & load
                dte.ExecuteCommand(ProxyMgrConstants.ProjectUnloadCommand);
                dte.ExecuteCommand(ProxyMgrConstants.ProjectReloadCommand);

                // Select References folder
                ((EnvDTE80.DTE2)dte).ToolWindows.SolutionExplorer.GetItem(projectItemPathToSelect).Select(vsUISelectionType.vsUISelectionTypeSelect);

                LogWriter.WriteLine("[OK] Project references selected and reloaded.");
            }
            catch (Exception ex)
            {
                LogWriter.WriteLine("[FAIL] Project reload failed! Exception: " + ex.ToString());
            }
        }
        #endregion
    }
}
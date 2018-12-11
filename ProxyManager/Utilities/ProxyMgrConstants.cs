namespace ProxyMgr.ProxyManager.Utilities
{
    public class ProxyMgrConstants
    {
        /// <summary>
        /// Represents the folder name for proxies
        /// </summary>
        public const string PackageFolderName = "Service References";

        /// <summary>
        /// Path for the svcutil.exe to generate the service proxy
        /// </summary>
        public const string SvcUtilPath = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\svcutil.exe";

        /// <summary>
        /// Path for the svcutil.exe to generate the service proxy
        /// </summary>
        public const string SvcUtilPathWin10 = @"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.1A\bin\NETFX 4.5.1 Tools\SvcUtil.exe";

        public static string[] ReusableLibraries = new string[] { 
            @"System.dll"
        };

        /// <summary>
        /// To show folder different from others, this is needed to include to project file
        /// </summary>
        public const string ProjectFileSvcItemName = "WCFMetadata";

        /// <summary>
        /// To show folder different from others, this is needed to include to project file
        /// </summary>
        public const string ProjectFileSvcStorageItemName = "WCFMetadataStorage";

        /// <summary>
        /// In order to not to fail when build, this assembly needed to added to references
        /// </summary>
        public const string SerializationAssemblyNamespace = "System.Runtime.Serialization";

        /// <summary>
        /// In order to not to fail when build, this assembly needed to added to references
        /// </summary>
        public const string ServiceModelAssemblyNamespace = "System.ServiceModel";

        /// <summary>
        /// Template for the name of the code file
        /// </summary>
        public const string GeneratedCodeFileNameTemplate = "{0}.proxy.{1}";

        /// <summary>
        /// To generate the code, this is needed to pass to svcutil as arguments
        /// </summary>
        public const string SvcutilCommandArgumentTemplate = @"/t:code {0} /out:""{1}"" {2} {3} /n:*,{4}.{5}";

        /// <summary>
        /// Compile Tag name for project file
        /// </summary>
        public const string CompileTagName = "Compile";

        /// <summary>
        /// AutoGen Tag name for project file
        /// </summary>
        public const string AutoGenerateTagName = "AutoGen";

        /// <summary>
        /// DesignTime Tag name for project file
        /// </summary>
        public const string DesignTimeTagName = "DesignTime";

        /// <summary>
        /// DependentUpon Tag name for project file
        /// </summary>
        public const string DependentUponTagName = "DependentUpon";

        /// <summary>
        /// None Tag name for project file
        /// </summary>
        public const string NoneTagName = "None";

        /// <summary>
        /// Generator Tag name for project file
        /// </summary>
        public const string GeneratorTagName = "Generator";

        /// <summary>
        /// Reference Tag name for project file
        /// </summary>
        public const string ReferenceTagName = "Reference";

        /// <summary>
        /// Autogeneration tag value for project file
        /// </summary>
        public const string AutoGenerationTagValue = "True";

        /// <summary>
        /// DesignTime tag value for project file
        /// </summary>
        public const string DesignTimeTagValue = "True";

        /// <summary>
        /// Generator tag value for project file
        /// </summary>
        public const string GeneratorTagValue = "Proxy Manager";

        /// <summary>
        /// Command name of the project unload
        /// </summary>
        public const string ProjectUnloadCommand = "Project.UnloadProject";

        /// <summary>
        /// Command name of the project reload
        /// </summary>
        public const string ProjectReloadCommand = "Project.ReloadProject";

        /// <summary>
        /// File extension of the svcmap file
        /// </summary>
        public const string SvcmapFileExtension = ".svcmap";
    }
}

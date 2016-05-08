// PkgCmdID.cs
// MUST match PkgCmdID.h

namespace ProxyMgr.ProxyManager
{
    static class PkgCmdIDList
    {
        /// <summary>
        /// Represents the command id for service proxy add operation
        /// </summary>
        public const uint addServiceProxy = 0x103;

        /// <summary>
        /// Represents the command id for service proxy configuration operation
        /// </summary>
        public const uint configureServiceProxy = 0x105;

        /// <summary>
        /// Represents the command id for service proxy add operation 
        /// </summary>
        public const uint addNewServiceProxy = 0x106;
    };
}
// Guids.cs
// MUST match guids.h
using System;

namespace ProxyMgr.ProxyManager
{
    static class GuidList
    {
        public const string guidProxyManagerPkgString = "a3fed674-7310-4b52-a3a9-fa6b537029d0";
        public const string guidProxyManagerCmdSetString = "ba0f8559-96f4-485f-83de-7d64c0b2571c";

        public static readonly Guid guidProxyManagerCmdSet = new Guid(guidProxyManagerCmdSetString);
    };
}
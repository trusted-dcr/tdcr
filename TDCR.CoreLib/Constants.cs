using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TDCR.CoreLib
{
    public static class Constants
    {
        private static string systemDataDir = Environment.GetEnvironmentVariable(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)? "LocalAppData" : "Home");

        public const uint Version = 1;
        public static string DataDir = Path.Combine(systemDataDir, "tdcr");
        public const uint GetPeerAmount = 100;
    }
}

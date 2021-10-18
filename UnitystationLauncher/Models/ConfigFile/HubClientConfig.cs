using System;
using System.Runtime.InteropServices;

namespace UnitystationLauncher.Models.ConfigFile
{
    [Serializable]
    public class HubClientConfig
    {
        public int? BuildNumber { get; set; }
        public string? WinUrl { get; set; }
        public string? OsxUrl { get; set; }
        public string? LinuxUrl { get; set; }
        public string? DailyMessage { get; set; }

        public string? GetDownloadUrl()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? WinUrl :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OsxUrl :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? LinuxUrl :
                null;
        }
    }
}
using System;
using System.Runtime.InteropServices;

namespace UnitystationLauncher.Models
{
    [Serializable]
    public class HubClientConfig
    {
        public int? BuildNumber { get; }
        public string? WinUrl { get; }
        public string? OsxUrl { get; }
        public string? LinuxUrl { get; }
        public string? DailyMessage { get; }

        public string? GetDownloadUrl()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? WinUrl :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OsxUrl :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? LinuxUrl :
                null;
        }
    }
}
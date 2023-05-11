using System;
using System.Runtime.InteropServices;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

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

        public string? GetDownloadUrl(IEnvironmentService environmentService)
        {
            return environmentService.GetCurrentEnvironment() switch
            {
                CurrentEnvironment.WindowsStandalone => WinUrl,
                CurrentEnvironment.MacOsStandalone => OsxUrl,
                CurrentEnvironment.LinuxStandalone
                    or CurrentEnvironment.LinuxFlatpak => LinuxUrl,
                _ => null
            };
        }
    }
}
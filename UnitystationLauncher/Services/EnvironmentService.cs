using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class EnvironmentService : IEnvironmentService
{
    private readonly CurrentEnvironment _currentEnvironment;
    private readonly string _userdataDirectory;

    public EnvironmentService()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _currentEnvironment = CurrentEnvironment.WindowsStandalone;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _currentEnvironment = CurrentEnvironment.MacOsStandalone;
        }
        else
        {
            _currentEnvironment = File.Exists("/.flatpak-info") 
                ? CurrentEnvironment.LinuxFlatpak 
                : CurrentEnvironment.LinuxStandalone;
        }

        _userdataDirectory = _currentEnvironment switch
        {
            CurrentEnvironment.WindowsStandalone =>
                $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/StationHub",
            CurrentEnvironment.LinuxFlatpak => 
                $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.var/app/org.unitystation.StationHub",
            // Linux and Mac Standalone
            _ => $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.local/share/StationHub"
        };
    }
    
    public CurrentEnvironment GetCurrentEnvironment()
    {
        return _currentEnvironment;
    }

    public string GetUserdataDirectory()
    {
        return _userdataDirectory;
    }
    
    public bool ShouldDisableUpdateCheck()
    {
        return _currentEnvironment == CurrentEnvironment.LinuxFlatpak;
    }
}
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class EnvironmentService : IEnvironmentService
{
    private readonly bool _isWindows;
    private readonly bool _isFlatpak;
    private readonly bool _isSteam;
    private readonly string _userdataDirectory;

    public EnvironmentService()
    {
        _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        _isFlatpak = !_isWindows && File.Exists("/.flatpak-info");
        _isSteam = File.Exists($"{Environment.CurrentDirectory}/.steam");

        if (_isWindows)
        {
            _userdataDirectory =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/StationHub";
        }
        else if (_isFlatpak && _isSteam)
        {
            // TODO: Where should this go?
            _userdataDirectory = Environment.CurrentDirectory;
        }
        else if (_isFlatpak)
        {
            _userdataDirectory =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.var/app/org.unitystation.StationHub";
        }
        else
        {
            _userdataDirectory =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}/.local/share/StationHub";
        }
    }

    public string GetUserdataDirectory()
    {
        return _userdataDirectory;
    }

    public bool IsRunningOnWindows()
    {
        return _isWindows;
    }

    public bool ShouldDisableUpdateCheck()
    {
        return _isFlatpak || _isSteam;
    }
}
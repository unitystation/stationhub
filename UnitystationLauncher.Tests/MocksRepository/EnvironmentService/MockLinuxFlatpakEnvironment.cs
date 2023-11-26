using System.Diagnostics;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.EnvironmentService;

public class MockLinuxFlatpakEnvironment : IEnvironmentService
{
    public CurrentEnvironment GetCurrentEnvironment()
    {
        return CurrentEnvironment.LinuxFlatpak;
    }

    public string GetUserdataDirectory()
    {
        return "/home/unitTester/UnitystationLauncher";
    }

    public bool ShouldDisableUpdateCheck()
    {
        return true;
    }

    public ProcessStartInfo? GetGameProcessStartInfo(string executable, string arguments)
    {
        throw new NotImplementedException();
    }
}

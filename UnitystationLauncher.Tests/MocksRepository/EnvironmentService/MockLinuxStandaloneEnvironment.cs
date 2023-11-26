using System.Diagnostics;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.EnvironmentService;

public class MockLinuxStandaloneEnvironment : IEnvironmentService
{
    public CurrentEnvironment GetCurrentEnvironment()
    {
        return CurrentEnvironment.LinuxStandalone;
    }

    public string GetUserdataDirectory()
    {
        return "/home/unitTester/UnitystationLauncher";
    }

    public bool ShouldDisableUpdateCheck()
    {
        return false;
    }

    public ProcessStartInfo? GetGameProcessStartInfo(string executable, string arguments)
    {
        throw new NotImplementedException();
    }
}
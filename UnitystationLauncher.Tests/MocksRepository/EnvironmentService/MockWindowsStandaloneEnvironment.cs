using System.Diagnostics;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.EnvironmentService;

public class MockWindowsStandaloneEnvironment : IEnvironmentService
{
    public CurrentEnvironment GetCurrentEnvironment()
    {
        return CurrentEnvironment.WindowsStandalone;
    }

    public string GetUserdataDirectory()
    {
        return "C:/Users/UnitTester/UnitystationLauncher";
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
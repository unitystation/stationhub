using Moq;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository;

public static class MockEnvironmentService
{
    public static IEnvironmentService GetLinuxStandaloneEnvironment()
    {
        Mock<IEnvironmentService> mock = new();
        mock.Setup(x => x.GetCurrentEnvironment()).Returns(CurrentEnvironment.LinuxStandalone);
        mock.Setup(x => x.GetUserdataDirectory()).Returns("/home/unitTester/UnitystationLauncher");
        mock.Setup(x => x.ShouldDisableUpdateCheck()).Returns(false);
        return mock.Object;
    }

    public static IEnvironmentService GetLinuxFlatpakEnvironment()
    {
        Mock<IEnvironmentService> mock = new();
        mock.Setup(x => x.GetCurrentEnvironment()).Returns(CurrentEnvironment.LinuxFlatpak);
        mock.Setup(x => x.GetUserdataDirectory()).Returns("/home/unitTester/UnitystationLauncher");
        mock.Setup(x => x.ShouldDisableUpdateCheck()).Returns(true);
        return mock.Object;
    }

    public static IEnvironmentService GetWindowsStandaloneEnvironment()
    {
        Mock<IEnvironmentService> mock = new();
        mock.Setup(x => x.GetCurrentEnvironment()).Returns(CurrentEnvironment.WindowsStandalone);
        mock.Setup(x => x.GetUserdataDirectory()).Returns("C:/Users/UnitTester/UnitystationLauncher");
        mock.Setup(x => x.ShouldDisableUpdateCheck()).Returns(false);
        return mock.Object;
    }

    public static IEnvironmentService GetMacStandaloneEnvironment()
    {
        Mock<IEnvironmentService> mock = new();
        mock.Setup(x => x.GetCurrentEnvironment()).Returns(CurrentEnvironment.MacOsStandalone);
        mock.Setup(x => x.GetUserdataDirectory()).Returns("/home/unitTester/UnitystationLauncher");
        mock.Setup(x => x.ShouldDisableUpdateCheck()).Returns(false);
        return mock.Object;
    }
}
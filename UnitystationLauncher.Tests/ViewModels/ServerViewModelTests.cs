using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Tests.ViewModels;

public static class ServerViewModelTests
{
    #region ServerViewModelTests.ctor

    [Fact]
    public static void ServerViewModelTests_ShouldFetchPingTimes()
    {
        ServerViewModel serverViewModel = new ServerViewModel(Server, InstallationService, PingService)
    }
    #endregion
}
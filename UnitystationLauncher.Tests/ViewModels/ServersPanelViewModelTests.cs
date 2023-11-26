using UnitystationLauncher.Services.Interface;
using UnitystationLauncher.Tests.MocksRepository.InstallationService;
using UnitystationLauncher.Tests.MocksRepository.PingService;
using UnitystationLauncher.Tests.MocksRepository.ServerService;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Tests.ViewModels;

public static class ServersPanelViewModelTests
{
    #region ServersPanelViewModel.ctor
    [Fact]
    public static void ServersPanelViewModel_ShouldFetchServers()
    {
        IInstallationService mockInstallationService = new MockNoActiveDownloads();
        IPingService mockPingService = new MockPingStaticPingTime(5);
        IServerService mockServerService = new MockRandomServers(1, 20);

        ServersPanelViewModel serversPanelViewModel = new(mockInstallationService, mockPingService, mockServerService);
        serversPanelViewModel.ServerViews.Should().NotBeEmpty();
    }

    [Fact]
    public static void ServersPanelViewModel_ShouldHandleExceptionInServerService()
    {
        IInstallationService mockInstallationService = new MockNoActiveDownloads();
        IPingService mockPingService = new MockPingStaticPingTime(5);
        IServerService mockServerService = new MockServersThrowsException();

        Func<ServersPanelViewModel> act = () => new(mockInstallationService, mockPingService, mockServerService);
        act.Should().NotThrow();
        act.Invoke().ServerViews.Should().NotBeNull().And.BeEmpty();
    }
    #endregion
}
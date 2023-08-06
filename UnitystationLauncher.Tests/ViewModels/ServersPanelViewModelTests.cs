using UnitystationLauncher.Services.Interface;
using UnitystationLauncher.Tests.MocksRepository;
using UnitystationLauncher.ViewModels;
using Xunit.Sdk;

namespace UnitystationLauncher.Tests.ViewModels;

public static class ServersPanelViewModelTests
{
    #region ServersPanelViewModel.ctor
    [Fact]
    public static void ServersPanelViewModel_ShouldFetchServers()
    {
        IInstallationService mockInstallationService = MockInstallationService.NoActiveDownloads();
        IPingService mockPingService = MockPingService.StaticPingTime(5);
        IServerService mockServerService = MockServerService.RandomServersRange(1, 20);

        ServersPanelViewModel serversPanelViewModel = new(mockInstallationService, mockPingService, mockServerService);
        serversPanelViewModel.ServerViews.Should().NotBeEmpty();
    }

    [Fact]
    public static void ServersPanelViewModel_ShouldHandleExceptionInServerService()
    {
        IInstallationService mockInstallationService = MockInstallationService.NoActiveDownloads();
        IPingService mockPingService = MockPingService.StaticPingTime(5);
        IServerService mockServerService = MockServerService.ThrowsException();

        Func<ServersPanelViewModel> act = () => new(mockInstallationService, mockPingService, mockServerService);
        act.Should().NotThrow();
        act.Invoke().ServerViews.Should().NotBeNull().And.BeEmpty();
    }
    #endregion
}
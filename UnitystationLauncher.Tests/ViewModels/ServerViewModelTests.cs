using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;
using UnitystationLauncher.Tests.MocksRepository;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Tests.ViewModels;

public static class ServerViewModelTests
{
    #region ServerViewModel.ctor
    [Fact]
    public static void ServerViewModel_ShouldFetchPingTimes()
    {
        Server server = new("UnitTestStation", 0, "127.0.0.1", 12345);
        IInstallationService mockInstallationService = MockInstallationService.NoActiveDownloads();
        IPingService mockPingService = MockPingService.StaticPingTime(5);
        
        ServerViewModel serverViewModel = new(server, mockInstallationService, mockPingService);

        serverViewModel.Ping.Should().Be("5ms");
    }
    
    [Fact]
    public static void ServerViewModel_ShouldHandleNullPingTime()
    {
        Server server = new("UnitTestStation", 0, "127.0.0.1", 12345);
        IInstallationService mockInstallationService = MockInstallationService.NoActiveDownloads();
        IPingService mockPingService = MockPingService.NullPingTime();

        Action act = () => _ = new ServerViewModel(server, mockInstallationService, mockPingService);

        act.Should().NotThrow();
    }
    
    [Fact]
    public static void ServerViewModel_ShouldHandleExceptionFromPingService()
    {
        Server server = new("UnitTestStation", 0, "127.0.0.1", 12345);
        IInstallationService mockInstallationService = MockInstallationService.NoActiveDownloads();
        IPingService mockPingService = MockPingService.ThrowsException();

        Func<ServerViewModel> act = () => new(server, mockInstallationService, mockPingService);

        act.Should().NotThrow();
        act.Invoke().Ping.Should().Be("Error");
    }
    #endregion
    
    #region ServerViewModel.LaunchGame
    [Fact]
    public static void LaunchGame_ShouldHandleNullInstallation()
    {
        Server server = new("UnitTestStation", 0, "127.0.0.1", 12345);
        IInstallationService mockInstallationService = MockInstallationService.NoActiveDownloads();
        IPingService mockPingService = MockPingService.StaticPingTime(5);
        
        ServerViewModel serverViewModel = new(server, mockInstallationService, mockPingService);
        
        Action act = () => serverViewModel.LaunchGame();
        act.Should().NotThrow();
    }
    #endregion
}
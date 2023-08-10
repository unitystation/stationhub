using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Tests.MocksRepository;

namespace UnitystationLauncher.Tests.Models.ConfigFile;

public static class HubClientConfigTests
{
    private const string WindowsUrl = "windows";
    private const string MacUrl = "mac";
    private const string LinuxUrl = "linux";

    private static readonly HubClientConfig _hubClientConfig = new()
    {
        WinUrl = WindowsUrl,
        OsxUrl = MacUrl,
        LinuxUrl = LinuxUrl,
        BuildNumber = 0,
        DailyMessage = "Unit Tests are nice"
    };

    #region HubClientConfig.GetDownloadUrl
    [Fact]
    public static void GetDownloadUrl_ShouldReturnLinuxUrlWhenPlatformIsLinux()
    {
        _hubClientConfig.GetDownloadUrl(MockEnvironmentService.GetLinuxStandaloneEnvironment()).Should().Be(LinuxUrl);
    }

    [Fact]
    public static void GetDownloadUrl_ShouldReturnLinuxUrlWhenPlatformIsLinuxFlatpak()
    {
        _hubClientConfig.GetDownloadUrl(MockEnvironmentService.GetLinuxFlatpakEnvironment()).Should().Be(LinuxUrl);
    }

    [Fact]
    public static void GetDownloadUrl_ShouldReturnWindowsUrlWhenPlatformIsWindows()
    {
        _hubClientConfig.GetDownloadUrl(MockEnvironmentService.GetWindowsStandaloneEnvironment()).Should().Be(WindowsUrl);
    }

    [Fact]
    public static void GetDownloadUrl_ShouldReturnMacUrlWhenPlatformIsMac()
    {
        _hubClientConfig.GetDownloadUrl(MockEnvironmentService.GetMacStandaloneEnvironment()).Should().Be(MacUrl);
    }
    #endregion
}
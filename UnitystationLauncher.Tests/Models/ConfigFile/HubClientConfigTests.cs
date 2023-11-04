using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Tests.MocksRepository.EnvironmentService;

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
        _hubClientConfig.GetDownloadUrl(new MockLinuxStandaloneEnvironment()).Should().Be(LinuxUrl);
    }

    [Fact]
    public static void GetDownloadUrl_ShouldReturnLinuxUrlWhenPlatformIsLinuxFlatpak()
    {
        _hubClientConfig.GetDownloadUrl(new MockLinuxFlatpakEnvironment()).Should().Be(LinuxUrl);
    }

    [Fact]
    public static void GetDownloadUrl_ShouldReturnWindowsUrlWhenPlatformIsWindows()
    {
        _hubClientConfig.GetDownloadUrl(new MockWindowsStandaloneEnvironment()).Should().Be(WindowsUrl);
    }

    [Fact]
    public static void GetDownloadUrl_ShouldReturnMacUrlWhenPlatformIsMac()
    {
        _hubClientConfig.GetDownloadUrl(new MockMacOsStandaloneEnvironment()).Should().Be(MacUrl);
    }
    #endregion
}
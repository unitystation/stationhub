using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Tests.MocksRepository;
using UnitystationLauncher.Tests.MocksRepository.EnvironmentService;

namespace UnitystationLauncher.Tests.Models.Api;

public static class ServerTests
{
    private const string WindowsUrl = "windows";
    private const string MacUrl = "mac";
    private const string LinuxUrl = "linux";

    private static readonly Server _server = new("Unit Test", 0, "127.0.0.1", 0)
    {
        WinDownload = WindowsUrl,
        OsxDownload = MacUrl,
        LinuxDownload = LinuxUrl
    };

    #region Server.GetDownloadUrl
    [Fact]
    public static void GetDownloadUrl_ShouldReturnLinuxUrlWhenPlatformIsLinux()
    {
        _server.GetDownloadUrl(new MockLinuxStandaloneEnvironment()).Should().Be(LinuxUrl);
    }

    [Fact]
    public static void GetDownloadUrl_ShouldReturnLinuxUrlWhenPlatformIsLinuxFlatpak()
    {
        _server.GetDownloadUrl(new MockLinuxFlatpakEnvironment()).Should().Be(LinuxUrl);
    }

    [Fact]
    public static void GetDownloadUrl_ShouldReturnWindowsUrlWhenPlatformIsWindows()
    {
        _server.GetDownloadUrl(new MockWindowsStandaloneEnvironment()).Should().Be(WindowsUrl);
    }

    [Fact]
    public static void GetDownloadUrl_ShouldReturnMacUrlWhenPlatformIsMac()
    {
        _server.GetDownloadUrl(new MockMacOsStandaloneEnvironment()).Should().Be(MacUrl);
    }
    #endregion

    #region Server.HasTrustedUrlSource
    [Fact]
    public static void HasTrustedUrlSource_ShouldReturnFalseWhenNull()
    {
        Server server = new("Unit Test", 0, "127.0.0.1", 0)
        {
            WinDownload = null,
            OsxDownload = null,
            LinuxDownload = null
        };

        server.HasTrustedUrlSource.Should().BeFalse();
    }

    [Theory]
    [InlineData("http://myshadyunitystationdownload.test")]
    [InlineData("http://127.0.0.1")]
    [InlineData("http://unitystationfile.b-cdn.net")]
    [InlineData(" ")]
    [InlineData("")]
    public static void HasTrustedUrlSource_ShouldReturnFalseWhenUntrusted(string url)
    {
        Server server = new("Unit Test", 0, "127.0.0.1", 0)
        {
            WinDownload = $"{url}/Windows",
            OsxDownload = $"{url}/Mac",
            LinuxDownload = $"{url}/Linux"
        };

        server.HasTrustedUrlSource.Should().BeFalse();
    }

    [Fact]
    public static void HasTrustedUrlSource_ShouldReturnTrueWhenTrusted()
    {
        Server server = new("Unit Test", 0, "127.0.0.1", 0)
        {
            WinDownload = "https://unitystationfile.b-cdn.net/Windows",
            OsxDownload = "https://unitystationfile.b-cdn.net/Mac",
            LinuxDownload = "https://unitystationfile.b-cdn.net/Linux"
        };

        server.HasTrustedUrlSource.Should().BeTrue();
    }
    #endregion

    #region Server.HasValidDomainName
    [Fact]
    public static void HasValidDomainName_ShouldReturnFalseWhenNull()
    {
#pragma warning disable CS8625
        Server server = new("Unit Test", 0, null, 0);
#pragma warning restore CS8625
        server.HasValidDomainName.Should().BeFalse();
    }

    [Theory]
    [InlineData("some invalid url <>@#*$(&")]
    [InlineData(" ")]
    [InlineData("")]
    public static void HasValidDomainName_ShouldReturnFalseWhenInvalid(string serverIp)
    {
        Server server = new("Unit Test", 0, serverIp, 0);
        server.HasValidDomainName.Should().BeFalse();
    }

    [Fact]
    public static void HasValidDomainName_ShouldReturnTrueWhenValid()
    {
        Server server = new("Unit Test", 0, "na1.unitystation.org", 0);
        server.HasValidDomainName.Should().BeTrue();
    }
    #endregion

    #region Server.HasValidIpAddress
    [Fact]
    public static void HasValidIpAddress_ShouldReturnFalseWhenNull()
    {
#pragma warning disable CS8625
        Server server = new("Unit Test", 0, null, 0);
#pragma warning restore CS8625
        server.HasValidIpAddress.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData(" ")]
    [InlineData("")]
    public static void HasValidIpAddress_ShouldReturnFalseWhenInvalid(string serverIp)
    {
        Server server = new("Unit Test", 0, serverIp, 0);
        server.HasValidIpAddress.Should().BeFalse();
    }

    [Fact]
    public static void HasValidIpAddress_ShouldReturnTrueWhenValid()
    {
        Server server = new("Unit Test", 0, "127.0.0.1", 0);
        server.HasValidIpAddress.Should().BeTrue();
    }
    #endregion
}
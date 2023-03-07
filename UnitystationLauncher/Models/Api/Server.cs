using System;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.Models.Api
{
    public class Server
    {
        public Server(string forkName, int buildVersion, string serverIp, int serverPort)
        {
            ForkName = forkName;
            BuildVersion = buildVersion;
            ServerIp = serverIp;
            ServerPort = serverPort;
        }

        public string? ServerName { get; set; }
        public string ForkName { get; }
        public int BuildVersion { get; }
        public string? CurrentMap { get; set; }
        public string? GameMode { get; set; }
        public string? InGameTime { get; set; }
        public int? PlayerCount { get; set; }
        public string ServerIp { get; }
        public int ServerPort { get; }
        public string? WinDownload { get; set; }
        public string? OsxDownload { get; set; }
        public string? LinuxDownload { get; set; }
        public (string, int) ForkAndVersion => (ForkName, BuildVersion);

        public string? DownloadUrl =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? WinDownload :
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OsxDownload :
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? LinuxDownload :
            throw new InvalidOperationException("Failed to detect OS");

        public bool HasTrustedUrlSource
        {
            get
            {
                const string trustedHost = "unitystationfile.b-cdn.net";
                var urls = new[] { WinDownload, OsxDownload, LinuxDownload };
                foreach (var url in urls)
                {
                    if (string.IsNullOrEmpty(url))
                    {
                        return false;
                    }
                    var uri = new Uri(url);
                    if (uri.Scheme != "https" || uri.Host != trustedHost)
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        public bool HasValidDomainName => Uri.CheckHostName(ServerIp) == UriHostNameType.Dns;

        public bool HasValidIpAddress => IPAddress.TryParse(ServerIp, out _);

        public string InstallationName => ForkName + BuildVersion;

        public string InstallationPath => Path.Combine(Config.InstallationsPath, InstallationName);

        public override bool Equals(object? obj)
        {
            return obj is Server server &&
                   ServerIp == server.ServerIp &&
                   ServerPort == server.ServerPort;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(ServerIp);
            hash.Add(ServerPort);
            return hash.ToHashCode();
        }
    }
}

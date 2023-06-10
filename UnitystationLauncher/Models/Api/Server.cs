using System;
using System.IO;
using System.Net;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Models.Api
{
    [Serializable]
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

        public string? GetDownloadUrl(IEnvironmentService environmentService)
        {
            return environmentService.GetCurrentEnvironment() switch
            {
                CurrentEnvironment.WindowsStandalone => WinDownload,
                CurrentEnvironment.MacOsStandalone => OsxDownload,
                CurrentEnvironment.LinuxStandalone
                    or CurrentEnvironment.LinuxFlatpak => LinuxDownload,
                _ => null
            };
        }

        public bool HasTrustedUrlSource
        {
            get
            {
                const string trustedHost = "unitystationfile.b-cdn.net";
                string?[] urls = { WinDownload, OsxDownload, LinuxDownload };
                foreach (string? url in urls)
                {
                    if (string.IsNullOrEmpty(url))
                    {
                        return false;
                    }

                    Uri uri = new(url);
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

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace UnitystationLauncher.Models
{
    public class Server
    {
        public string? ServerName { get; set;}
        public string ForkName { get; set;}
        public int BuildVersion { get; set;}
        public string? CurrentMap { get; set;}
        public string? GameMode { get; set;}
        public string? IngameTime { get; set;}
        public int? PlayerCount { get; set;}
        public string ServerIP { get; set;}
        public int ServerPort { get; set;}
        public string? WinDownload { get; set;}
        public string? OSXDownload { get; set;}
        public string? LinuxDownload { get; set; }
        public (string, int) Key => (ForkName, BuildVersion);

        public string DownloadUrl
        {
            get
            {
                return 
                    RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? WinDownload :
                    RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSXDownload :
                    RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? LinuxDownload :
                    throw new Exception("Failed to detect OS");
            }
        }

        public string InstallationName => ForkName + BuildVersion;

        public string? Description => "BuildVersion: " + BuildVersion + " - Map: " + CurrentMap + " - Gamemode: " + GameMode + " - Time: " + IngameTime;

        public string InstallationPath => Path.Combine(Config.InstallationsPath, InstallationName);

        public override bool Equals(object? obj)
        {
            return obj is Server server &&
                   ServerIP == server.ServerIP &&
                   ServerPort == server.ServerPort;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(ServerIP);
            hash.Add(ServerPort);
            return hash.ToHashCode();
        }
    }
}
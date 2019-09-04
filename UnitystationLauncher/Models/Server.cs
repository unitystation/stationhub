using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ReactiveUI;

namespace UnitystationLauncher.Models
{
    public class Server
    {
        const string InstallationFolder = "Installations";
        public Server()
        {
            // var canDownload = this.WhenAnyValue(
            //     x => Path.Combine(InstallationFolder, ForkName + BuildVersion), 
            //     x => !File.Exists(x));

            Download = ReactiveCommand.Create(DowloadImp);
        }

        public string? ServerName { get; set;}
        public string? ForkName { get; set;}
        public int BuildVersion { get; set;}
        public string? CurrentMap { get; set;}
        public string? GameMode { get; set;}
        public string? IngameTime { get; set;}
        public int PlayerCount { get; set;}
        public string? ServerIP { get; set;}
        public int ServerPort { get; set;}
        public string? WinDownload { get; set;}
        public string? OSXDownload { get; set;}
        public string? LinuxDownload { get; set;}

        public ReactiveCommand<Unit, Unit> Download { get; }

        public void DowloadImp()
        {
            var installationName = ForkName + BuildVersion;
            var installationPath = Path.Combine(InstallationFolder, installationName);
            if(Directory.Exists(installationPath))
            {
                return;
            }
            var downloadUrl =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? WinDownload :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSXDownload :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? LinuxDownload : 
                throw new Exception("OS could not be detected");

            DowloadAsync(downloadUrl, installationPath);
        }

        public async Task DowloadAsync(string downloadUrl, string installationPath)
        {
            var webRequest = WebRequest.Create(downloadUrl);
            var webResponse = await webRequest.GetResponseAsync();
            var responseStream = webResponse.GetResponseStream();

            var archive = new ZipArchive(responseStream);
            archive.ExtractToDirectory(installationPath);
        }

        public override bool Equals(object obj)
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
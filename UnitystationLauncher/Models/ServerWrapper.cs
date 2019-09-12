using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using ReactiveUI;
using Serilog;
using System.Linq;
using Mono.Unix;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace UnitystationLauncher.Models
{
    public class ServerWrapper : Server
    {
        const string InstallationFolder = "Installations";
        private static readonly FileSystemWatcher fileWatcher = new FileSystemWatcher(InstallationsPath) { EnableRaisingEvents = true, IncludeSubdirectories = true };
        private static readonly IObservable<Unit> observableFile = Observable.Merge(
                Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(
                    h => fileWatcher.Created += h,
                    h => fileWatcher.Created -= h)
                .Select(e => Unit.Default),
                Observable.Return(Unit.Default))
                .ObserveOn(SynchronizationContext.Current)
                .Delay(TimeSpan.FromMilliseconds(100))
                .Do(o => Log.Debug("File refresh"));

        private static string InstallationsPath => Path.Combine(Environment.CurrentDirectory, InstallationFolder);
        private string InstallationPath => Path.Combine(InstallationsPath, ForkName + BuildVersion);

        public ServerWrapper(Server server)
        {

            this.ServerName = server.ServerName;
            this.ForkName = server.ForkName;
            this.BuildVersion = server.BuildVersion;
            this.CurrentMap = server.CurrentMap;
            this.GameMode = server.GameMode;
            this.IngameTime = server.IngameTime;
            this.PlayerCount = server.PlayerCount;
            this.ServerIP = server.ServerIP;
            this.ServerPort = server.ServerPort;
            this.WinDownload = server.WinDownload;
            this.OSXDownload = server.OSXDownload;
            this.LinuxDownload = server.LinuxDownload;

            if (!Directory.Exists(InstallationsPath))
            {
                Directory.CreateDirectory(InstallationsPath);
            }

            var canDownload = observableFile
                .Select(e => !Directory.Exists(InstallationPath));

            var canStart = observableFile
                .Select(e => 
                    Directory.Exists(InstallationPath) && 
                    FindExecutable(InstallationPath) != null);

            Download = ReactiveCommand.Create(DowloadImp, canDownload);
            Start = ReactiveCommand.Create(StartImp, canStart);
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
        }

        public ReactiveCommand<Unit, Unit> Download { get; }

        public ReactiveCommand<Unit, Unit> Start { get; }

        public void DowloadImp()
        {
            Log.Information("Download requested...");
            var installationName = ForkName + BuildVersion;
            var installationPath = Path.Combine(InstallationFolder, installationName);
            Log.Information("Installation path: \"{Path}\"", installationName);
            if (Directory.Exists(installationPath))
            {
                Log.Information("Installation path already occupied");
                return;
            }
            var downloadUrl =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || true ? WinDownload :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? OSXDownload :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? LinuxDownload :
                throw new Exception("OS could not be detected");

            Log.Information("Download URL: \"{URL}\"", downloadUrl);

            if (downloadUrl is null)
            {
                throw new Exception("OS download is null");
            }

            DowloadAsync(downloadUrl, installationPath);
        }

        public async Task DowloadAsync(string downloadUrl, string installationPath)
        {
            Log.Information("Download started...");
            var webRequest = WebRequest.Create(downloadUrl);
            var webResponse = await webRequest.GetResponseAsync();
            var responseStream = webResponse.GetResponseStream();
            Log.Information("Download connection established...");

            var archive = new ZipArchive(responseStream);
            archive.ExtractToDirectory(installationPath);
            Log.Information("Download completed");
        }

        private void StartImp()
        {
            var exe = FindExecutable(InstallationPath);
            if(exe != null)
            {
                Process.Start(exe);
            }
        }

        public string? FindExecutable(string path)
        {
            if(!Directory.Exists(path))
            {
                return null;
            }
            var files = Directory.EnumerateFiles(path);
            string? exe = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || true)
            {
                exe = files.SingleOrDefault(s => Regex.IsMatch(Path.GetFileName(s), @".*station\.exe"));
            }
            else
            {
                exe = files.SingleOrDefault(s => (new UnixFileInfo(s).FileAccessPermissions & FileAccessPermissions.UserExecute) > 0);
            }

            return exe;
        }

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
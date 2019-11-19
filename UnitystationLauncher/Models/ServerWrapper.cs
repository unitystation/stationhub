using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Serilog;
using System.Diagnostics;
using System.Reactive.Subjects;
using Avalonia;

namespace UnitystationLauncher.Models
{
    public class ServerWrapper : Server
    {
        private AuthManager authManager;
        public ServerWrapper(Server server, AuthManager authManager)
        {
            this.authManager = authManager;
            ServerName = server.ServerName;
            ForkName = server.ForkName;
            BuildVersion = server.BuildVersion;
            CurrentMap = server.CurrentMap;
            GameMode = server.GameMode;
            IngameTime = server.IngameTime;
            PlayerCount = server.PlayerCount;
            ServerIP = server.ServerIP;
            ServerPort = server.ServerPort;
            WinDownload = server.WinDownload;
            OSXDownload = server.OSXDownload;
            LinuxDownload = server.LinuxDownload;

            if (!Directory.Exists(Config.InstallationsPath))
            {
                Directory.CreateDirectory(Config.InstallationsPath);
            }

            CanPlay.Subscribe(x => SetButtonText(x));
            CanPlay.Value = ClientInstalled;
            Start = ReactiveCommand.Create(StartImp, null);
        }

        public Reactive.Bindings.ReactiveProperty<bool> CanPlay { get; } = new Reactive.Bindings.ReactiveProperty<bool>();
        public Reactive.Bindings.ReactiveProperty<string> ButtonText { get; } = new Reactive.Bindings.ReactiveProperty<string>();
        public Subject<int> Progress { get; set; } = new Subject<int>();

        public ReactiveCommand<Unit, Unit> Start { get; }

        private void SetButtonText(bool canPlay)
        {
            if (canPlay)
            {
                ButtonText.Value = "PLAY";
            } else
            {
                ButtonText.Value = "DOWNLOAD";
            }
        }

        public async void DownloadAsync()
        {
            Log.Information("Download requested...");
            Log.Information("Installation path: \"{Path}\"", InstallationPath);

            if (Directory.Exists(InstallationPath))
            {
                Log.Information("Installation path already occupied");
                return;
            }

            Log.Information("Download URL: \"{URL}\"", DownloadUrl);

            if (DownloadUrl is null)
            {
                throw new Exception("OS download is null");
            }

            Log.Information("Download started...");
            var webRequest = WebRequest.Create(DownloadUrl);
            var webResponse = await webRequest.GetResponseAsync();
            var responseStream = webResponse.GetResponseStream();
            Log.Information("Download connection established");
            using var progStream = new ProgressStream(responseStream);
            var length = webResponse.ContentLength;
            progStream.Progress
                .Select(p => (int)(p * 100 / length))
                .DistinctUntilChanged()
                .Subscribe(p =>
                {
                    Progress.OnNext(p);
                    Log.Information("Progress: {prog}", p);
                });

            await Task.Run(() =>
            {
                Log.Information("Extracting...");
                var archive = new ZipArchive(progStream);
                archive.ExtractToDirectory(InstallationPath);
                Log.Information("Download completed");
            });
        }

        bool ClientInstalled
        {
            get
            {
               return Directory.Exists(InstallationPath) &&
                    Installation.FindExecutable(InstallationPath) != null;
            }
        }

        private void StartImp()
        {
            if (CanPlay.Value)
            {
                var exe = Installation.FindExecutable(InstallationPath);
                if (exe != null)
                {
                    Application.Current.MainWindow.WindowState = Avalonia.Controls.WindowState.Minimized;
                    var process = new Process();
                    process.StartInfo.FileName = exe;
                    process.StartInfo.Arguments =
                        $"--server {ServerIP} --port {ServerPort} --refreshtoken {authManager.CurrentRefreshToken} --uid {authManager.UID}";
                    process.Start();
                }
            } else
            {
                //DO DOWNLOAD
            }
        }
    }
}
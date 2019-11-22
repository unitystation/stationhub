using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Serilog;
using System.Diagnostics;
using System.Reactive.Subjects;
using Avalonia;
using System.Threading;
using Humanizer.Bytes;
using System.Net.NetworkInformation;
using ReactiveUI;

namespace UnitystationLauncher.Models
{
    public class ServerWrapper : Server
    {
        private AuthManager authManager;
        private InstallationManager installManager;
        private CancellationTokenSource cancelSource;
        public ServerWrapper(Server server, AuthManager authManager, 
            InstallationManager installManager)
        {
            this.authManager = authManager;
            this.installManager = installManager;
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

            Start = ReactiveCommand.Create(StartImp, null);
            Ping pingSender = new Ping();
            pingSender.PingCompleted += new PingCompletedEventHandler(PingCompletedCallback);
            pingSender.SendAsync(ServerIP, 7);
            CheckIfCanPlay();
        }

        public BehaviorSubject<bool> CanPlay { get; set; } = new BehaviorSubject<bool>(false);
        public BehaviorSubject<bool> IsDownloading { get; private set; } = new BehaviorSubject<bool>(false);
        public BehaviorSubject<string> ButtonText { get; private set; } = new BehaviorSubject<string>("");
        public BehaviorSubject<string> DownloadProgText { get; private set; } = new BehaviorSubject<string>("");
        public BehaviorSubject<string> RoundTrip { get; private set; } = new BehaviorSubject<string>("");
        public BehaviorSubject<int> Progress { get; set; } = new BehaviorSubject<int>(0);
        public ReactiveCommand<Unit, Unit> Start { get; }

        public void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            //// If an error occurred, display the exception to the user.  
            if (e.Error != null)
            {
                Log.Information("Ping failed:");
                Log.Information(e.Error.ToString());
                return;
            }
            var tripTime = e.Reply.RoundtripTime;
            if(tripTime == 0)
            {
                RoundTrip.OnNext("null");
            }
            else
            {
                RoundTrip.OnNext($"{e.Reply.RoundtripTime}ms");
            }   
        }

        public void CheckIfCanPlay()
        {
            CanPlay.OnNext(ClientInstalled);
            if (CanPlay.Value)
            {
                ButtonText.OnNext("PLAY");
            }
            else
            {
                ButtonText.OnNext("DOWNLOAD");
            }
        }

        public async Task DownloadAsync(CancellationToken cancelToken)
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
                Log.Error("OS download is null");
                return;
            }

            Log.Information("Download started...");
            var webRequest = WebRequest.Create(DownloadUrl);
            var webResponse = await webRequest.GetResponseAsync();
            var responseStream = webResponse.GetResponseStream();
            Log.Information("Download connection established");
            using var progStream = new ProgressStream(responseStream);
            var length = webResponse.ContentLength;
            var maxFileSize = ByteSize.FromBytes(length);
            progStream.Progress
                .Select(p => (int)(p * 100 / length))
                .DistinctUntilChanged()
                .Subscribe(p =>
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        progStream.Inner.Dispose();
                        Progress.OnNext(0);
                        return;
                    }
                    var downloadedAmt = (int)((float)maxFileSize.Megabytes * ((float)p / 100f));
                    DownloadProgText.OnNext($" {downloadedAmt} / {(int)maxFileSize.Megabytes} MB");
                    Progress.OnNext(p);
                    Log.Information("Progress: {prog}", p);
                });

            await Task.Run(() =>
            {
                Log.Information("Extracting...");
                try
                {
                    var archive = new ZipArchive(progStream);
                    archive.ExtractToDirectory(InstallationPath);
                    Log.Information("Download completed");
                }
                catch
                {
                    Log.Information("Extracting stopped");
                }
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

        private async void StartImp()
        {
            if (IsDownloading.Value)
            {
                cancelSource.Cancel();
                if (Directory.Exists(InstallationPath))
                {
                    Directory.Delete(InstallationPath);
                }
                Log.Information("User cancelled download");
                return;
            }

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
            }
            else
            {
                //DO DOWNLOAD
                cancelSource = new CancellationTokenSource();
                ButtonText.OnNext("CANCEL");
                DownloadProgText.OnNext("Connecting..");
                IsDownloading.OnNext(true);
                await DownloadAsync(cancelSource.Token);
                IsDownloading.OnNext(false);
                CanPlay.OnNext(ClientInstalled);
                CheckIfCanPlay();
                installManager.TryAutoRemove();
            }
        }
    }
}
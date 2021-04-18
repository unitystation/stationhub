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
using Reactive.Bindings;
using System.Threading;
using Humanizer.Bytes;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
#if FLATPAK
using System.Text.RegularExpressions;
#endif
using Avalonia.Controls.ApplicationLifetimes;

namespace UnitystationLauncher.Models
{
    public class ServerWrapper : Server, IDisposable
    {
        private readonly AuthManager _authManager;
        private CancellationTokenSource? _cancelSource;
        private bool _isDownloading;
        public ReactiveProperty<bool> CanPlay { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> IsDownloading { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<bool> IsSelected { get; } = new ReactiveProperty<bool>();
        public ReactiveProperty<string> ButtonText { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> DownloadProgText { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> RoundTrip { get; } = new ReactiveProperty<string>();
        public Subject<int> Progress { get; set; } = new Subject<int>();
        public ReactiveUI.ReactiveCommand<Unit, Unit> Start { get; }
        // Ping does not work in sandboxes so we have to reconstruct its functionality in that case.
        // Surprisingly, this is basically what that does. Looks for your system's ping tool and parses its output.
#if FLATPAK
	    private readonly Process _pingSender;
#else
        private readonly Ping _pingSender;
#endif


        public ServerWrapper(Server server, AuthManager authManager) : base(server.ForkName, server.BuildVersion,
            server.ServerIp, server.ServerPort)
        {
            _authManager = authManager;
#if FLATPAK
	        _pingSender = new Process();
#else
            _pingSender = new Ping();
            _pingSender.PingCompleted += PingCompletedCallback;
#endif
            UpdateDetails(server);

            if (!Directory.Exists(Config.InstallationsPath))
            {
                Directory.CreateDirectory(Config.InstallationsPath);
            }

            CanPlay.Subscribe(x => OnCanPlayChange(x));
            CheckIfCanPlay();
            Start = ReactiveUI.ReactiveCommand.CreateFromTask(StartImp);
        }

        public void UpdateDetails(Server server)
        {
            ServerName = server.ServerName;
            CurrentMap = server.CurrentMap;
            GameMode = server.GameMode;
            InGameTime = server.InGameTime;
            PlayerCount = server.PlayerCount;
            WinDownload = server.WinDownload;
            OsxDownload = server.OsxDownload;
            LinuxDownload = server.LinuxDownload;
#if FLATPAK
            _pingSender.StartInfo.UseShellExecute = false;
            _pingSender.StartInfo.RedirectStandardOutput = true;
            _pingSender.StartInfo.RedirectStandardError = true;
            _pingSender.StartInfo.FileName = "ping";
            _pingSender.StartInfo.Arguments = $"{ServerIp} -c 1";
            _pingSender.Start();
            StreamReader reader = _pingSender.StandardOutput;
            string e = reader.ReadToEnd();
            Regex pingReg = new Regex(@"time=(.*?)\ ");
            var pingTrunc = pingReg.Match(e);
            var pingOut = pingTrunc.Groups[1].ToString();
            RoundTrip.Value = $"{pingOut}ms";
            _pingSender.WaitForExit();
#else
            _pingSender.SendAsync(ServerIp, 7);
#endif
        }

        public void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            // If an error occurred, display the exception to the user.  
            if (e.Error != null)
            {
                Log.Error(e.Error, "Ping failed");
                return;
            }

            var tripTime = e.Reply.RoundtripTime;
            if (tripTime == 0)
            {
                RoundTrip.Value = "null";
            }
            else
            {
                RoundTrip.Value = $"{e.Reply.RoundtripTime}ms";
            }
        }

        public void CheckIfCanPlay()
        {
            CanPlay.Value = ClientInstalled;

            if (_isDownloading)
            {
                return;
            }

            OnCanPlayChange(CanPlay.Value);
        }

        private void OnCanPlayChange(bool canPlay)
        {
            if (_isDownloading)
            {
                return;
            }

            if (canPlay)
            {
                ButtonText.Value = "PLAY";
            }
            else
            {
                ButtonText.Value = "DOWNLOAD";
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

            Log.Information("Download URL: \"{Url}\"", DownloadUrl);

            if (DownloadUrl is null)
            {
                Log.Error("OS download is null");
                return;
            }

            _isDownloading = true;
            Log.Information("Download started...");
            var webRequest = WebRequest.Create(DownloadUrl);
            var webResponse = await webRequest.GetResponseAsync();
            await using var responseStream = webResponse.GetResponseStream();
            if (responseStream == null)
            {
                Log.Error("Could not download from server");
                return;
            }

            Log.Information("Download connection established");
            await using var progStream = new ProgressStream(responseStream);
            var length = webResponse.ContentLength;
            var maxFileSize = ByteSize.FromBytes(length);

            progStream.Progress
                .Select(p => (int)(p * 100 / length))
                .DistinctUntilChanged()
                .Subscribe(p =>
                {
                    if (cancelToken.IsCancellationRequested)
                    {
                        Progress.OnNext(0);
                        _isDownloading = false;
                        return;
                    }

                    var downloadedAmt = (int)((float)maxFileSize.Megabytes * (p / 100f));
                    DownloadProgText.Value = $" {downloadedAmt} / {(int)maxFileSize.Megabytes} MB";
                    Progress.OnNext(p);
                    Log.Information("Progress: {Percentage}", p);
                });

            await Task.Run(() =>
            {
                Log.Information("Extracting...");
                try
                {
                    var archive = new ZipArchive(progStream);
                    archive.ExtractToDirectory(InstallationPath, true);

                    Log.Information("Download completed");
                    Installation.MakeExecutableExecutable(InstallationPath);
                    _isDownloading = false;
                    CheckIfCanPlay();
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

        private async Task StartImp()
        {
            if (IsDownloading.Value)
            {
                CancelDownload();
                return;
            }

            if (CanPlay.Value)
            {
                StartClient();
            }
            else
            {
                await DownloadClient();
            }
        }

        private async Task DownloadClient()
        {
            //DO DOWNLOAD
            _cancelSource = new CancellationTokenSource();
            ButtonText.Value = "CANCEL";
            DownloadProgText.Value = "Connecting..";
            IsDownloading.Value = true;
            await DownloadAsync(_cancelSource.Token);
            IsDownloading.Value = false;
            CheckIfCanPlay();
        }

        private void StartClient()
        {
            var exe = Installation.FindExecutable(InstallationPath);
            if (exe == null)
            {
                return;
            }

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.MainWindow.WindowState = Avalonia.Controls.WindowState.Minimized;
            }

            ProcessStartInfo startInfo;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                startInfo = new ProcessStartInfo("/bin/bash",
                    $"-c \" open -a '{exe}' --args --server {ServerIp} --port {ServerPort} --refreshtoken {_authManager.CurrentRefreshToken} --uid {_authManager.Uid}; \"");
                Log.Information("Start osx | linux");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo = new ProcessStartInfo("/bin/bash",
                    $"-c \" '{exe}' --args --server {ServerIp} --port {ServerPort} --refreshtoken {_authManager.CurrentRefreshToken} --uid {_authManager.Uid}; \"");
                Log.Information("Start osx | linux");
            }
            else
            {
                startInfo = new ProcessStartInfo(exe,
                    $"--server {ServerIp} --port {ServerPort} --refreshtoken {_authManager.CurrentRefreshToken} --uid {_authManager.Uid}");
            }

            startInfo.UseShellExecute = false;
            var process = new Process();
            process.StartInfo = startInfo;

            process.Start();
        }

        private void CancelDownload()
        {
            _cancelSource?.Cancel();
            if (Directory.Exists(InstallationPath))
            {
                Directory.Delete(InstallationPath);
            }

            Log.Information("User cancelled download");
        }

        public void Dispose()
        {
            _cancelSource?.Dispose();
            _pingSender.Dispose();
        }
    }
}
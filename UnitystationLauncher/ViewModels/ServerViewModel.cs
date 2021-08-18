using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Humanizer.Bytes;
using Reactive.Bindings;
using Serilog;
using UnitystationLauncher.Models;
using System.Text.RegularExpressions;


namespace UnitystationLauncher.ViewModels
{
    public class ServerViewModel : ViewModelBase, IDisposable
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
        
	private readonly Ping _pingSender;
	private readonly Process _pingSenderFallback;

        public ServerViewModel(Server server, AuthManager authManager)
        {
            Server = server;
            _authManager = authManager;
	    _pingSenderFallback = new Process();
            _pingSender = new Ping();
            _pingSender.PingCompleted += PingCompletedCallback;
            UpdateDetails(server);

            if (!Directory.Exists(Config.InstallationsPath))
            {
                Directory.CreateDirectory(Config.InstallationsPath);
            }

            CanPlay.Subscribe(x => OnCanPlayChange(x));
            CheckIfCanPlay();
            Start = ReactiveUI.ReactiveCommand.CreateFromTask(StartImp);
        }

        public Server Server { get; }

        public void UpdateDetails(Server server)
        {
            Server.ServerName = server.ServerName;
            Server.CurrentMap = server.CurrentMap;
            Server.GameMode = server.GameMode;
            Server.InGameTime = server.InGameTime;
            Server.PlayerCount = server.PlayerCount;
            Server.WinDownload = server.WinDownload;
            Server.OsxDownload = server.OsxDownload;
            Server.LinuxDownload = server.LinuxDownload;
#if FLATPAK
            _pingSenderFallback.StartInfo.UseShellExecute = false;
            _pingSenderFallback.StartInfo.RedirectStandardOutput = true;
            _pingSenderFallback.StartInfo.RedirectStandardError = true;
            _pingSenderFallback.StartInfo.FileName = "ping";
            _pingSenderFallback.StartInfo.Arguments = $"{Server.ServerIp} -c 1";
            _pingSenderFallback.Start();
            StreamReader reader = _pingSenderFallback.StandardOutput;
            string e = reader.ReadToEnd();
            Regex pingReg = new Regex(@"time=(.*?)\ ");
            var pingTrunc = pingReg.Match(e);
            var pingOut = pingTrunc.Groups[1].ToString();
            RoundTrip.Value = $"{pingOut}ms";
            _pingSenderFallback.WaitForExit();
#else
            _pingSender.SendAsync(Server.ServerIp, 7);
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

        public string? DownloadUrl => Server.DownloadUrl;

        public string InstallationPath => Server.InstallationPath;

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
                    $"-c \" open -a '{exe}' --args --server {Server.ServerIp} --port {Server.ServerPort} --refreshtoken {_authManager.CurrentRefreshToken} --uid {_authManager.Uid}; \"");
                Log.Information("Start osx | linux");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                startInfo = new ProcessStartInfo("/bin/bash",
                    $"-c \" '{exe}' --args --server {Server.ServerIp} --port {Server.ServerPort} --refreshtoken {_authManager.CurrentRefreshToken} --uid {_authManager.Uid}; \"");
                Log.Information("Start osx | linux");
            }
            else
            {
                startInfo = new ProcessStartInfo(exe,
                    $"--server {Server.ServerIp} --port {Server.ServerPort} --refreshtoken {_authManager.CurrentRefreshToken} --uid {_authManager.Uid}");
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

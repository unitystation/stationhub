using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Humanizer.Bytes;
using ReactiveUI;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Mono.Unix;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class HubUpdateViewModel : ViewModelBase, IDisposable
    {
        private CancellationTokenSource? _cancelSource;
        private readonly Lazy<LoginViewModel> _loginVm;
        private readonly Config _config;
        private string? _updateTitle;
        private string? _updateMessage;
        private string? _buttonMessage;
        private string? _downloadMessage;
        private bool _installButtonVisible;
        private bool _downloadBarVisible;
        private bool _restartButtonVisible;
        private readonly Process _thisProcess;

        public ReactiveCommand<Unit, Unit> BeginDownload { get; }
        public ReactiveCommand<Unit, Unit> RestartHub { get; }
        public ReactiveCommand<Unit, ViewModelBase> Cancel { get; }
        public Subject<int> Progress { get; set; } = new Subject<int>();

        public bool InstallButtonVisible
        {
            get => _installButtonVisible;
            set => this.RaiseAndSetIfChanged(ref _installButtonVisible, value);
        }

        public bool DownloadBarVisible
        {
            get => _downloadBarVisible;
            set => this.RaiseAndSetIfChanged(ref _downloadBarVisible, value);
        }

        public bool RestartButtonVisible
        {
            get => _restartButtonVisible;
            set => this.RaiseAndSetIfChanged(ref _restartButtonVisible, value);
        }

        public string? UpdateTitle
        {
            get => _updateTitle;
            set => this.RaiseAndSetIfChanged(ref _updateTitle, value);
        }

        public string? UpdateMessage
        {
            get => _updateMessage;
            set => this.RaiseAndSetIfChanged(ref _updateMessage, value);
        }

        public string? ButtonMessage
        {
            get => _buttonMessage;
            set => this.RaiseAndSetIfChanged(ref _buttonMessage, value);
        }

        public string? DownloadMessage
        {
            get => _downloadMessage;
            set => this.RaiseAndSetIfChanged(ref _downloadMessage, value);
        }

        public HubUpdateViewModel(Lazy<LoginViewModel> loginVm, Config config)
        {
            _loginVm = loginVm;
            _config = config;
            BeginDownload = ReactiveCommand.CreateFromTask(UpdateHub);
            RestartHub = ReactiveCommand.Create(RestartApp);
            Cancel = ReactiveCommand.Create(CancelInstall);

            UpdateTitle = "Update Required";
            UpdateMessage = $"An update is required before you can continue.\n\rPlease install" +
                $" the latest version by clicking the button below:";

            ButtonMessage = $"Update Hub";
            _thisProcess = Process.GetCurrentProcess();

            InstallButtonVisible = true;
            DownloadBarVisible = false;
            RestartButtonVisible = false;
        }

        public async Task UpdateHub()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                GiveAllOwnerPermissions(Config.UnixExeFullPath);
            }

            _cancelSource = new CancellationTokenSource();
            await TryUpdate(_cancelSource.Token);
        }

        async Task TryUpdate(CancellationToken cancelToken)
        {
            Directory.CreateDirectory(Config.TempFolder);

            GiveAllOwnerPermissions(Config.TempFolder);

            InstallButtonVisible = false;
            DownloadBarVisible = true;
            UpdateTitle = "Downloading...";

            Log.Information("Download started...");
            var downloadUrl = (await _config.GetServerHubClientConfig()).GetDownloadUrl();
            if (downloadUrl == null)
            {
                Log.Error("DownloadUrl is null");
                return;
            }

            var webRequest = WebRequest.Create(downloadUrl);
            var webResponse = await webRequest.GetResponseAsync();
            await using var responseStream = webResponse.GetResponseStream();
            if (responseStream == null)
            {
                Log.Error("Failed to establish download connection");
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
                        progStream.Inner.Dispose();
                        Progress.OnNext(0);
                        return;
                    }
                    var downloadedAmt = (int)((float)maxFileSize.Kilobytes * (p / 100f));
                    DownloadMessage = $" {downloadedAmt} / {(int)maxFileSize.Kilobytes} KB";
                    Progress.OnNext(p);
                    Log.Information("Progress: {Progress}", p);
                });

            await Task.Run(() =>
            {
                Log.Information("Extracting...");
                try
                {
                    var archive = new ZipArchive(progStream);
                    archive.ExtractToDirectory(Config.TempFolder, true);

                    Log.Information("Download completed");
                }
                catch
                {
                    Log.Information("Extracting stopped");
                }
            });

            DownloadComplete();
        }

        private void DownloadComplete()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                        RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                GiveAllOwnerPermissions(Config.TempFolder);
            }

            DownloadBarVisible = false;
            RestartApp();
        }

        private void OnExit(object? sender, EventArgs e)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string argument = "-c \" sleep 1; cp -a {0}/. {1}; rm -rf {0}; {2}";

                ProcessStartInfo info = new ProcessStartInfo();
                info.Arguments = string.Format(argument, Regex.Escape(Config.TempFolder), Regex.Escape(Config.RootFolder), Regex.Escape(Config.UnixExeFullPath));
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.CreateNoWindow = true;
                info.FileName = "/bin/bash";
                Process.Start(info);
            }
            else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string argument = "/C echo \"{2}\" & Choice /C Y /N /D Y /T 1 & xcopy /Y \"{0}\" \"{1}\" & rmdir /q /s \"{0}\" & echo \"{3}\" & \"{4}\"";
                ProcessStartInfo info = new ProcessStartInfo();
                info.Arguments = string.Format(argument, Config.TempFolder, Config.RootFolder,
                    "Updating hub please wait..", "Update complete.", Config.WinExeFullPath);
                info.WindowStyle = ProcessWindowStyle.Normal;
                info.UseShellExecute = false;
                info.FileName = "cmd";
                Process.Start(info);
            }

            _thisProcess.Kill();
        }

        ViewModelBase CancelInstall()
        {
            _cancelSource?.Cancel();
            return _loginVm.Value;
        }

        void RestartApp()
        {

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Exit += OnExit;
                desktopLifetime.Shutdown();
            }
        }

        private void GiveAllOwnerPermissions(string path)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return;
            }
            new UnixFileInfo(path).FileAccessPermissions |= FileAccessPermissions.UserReadWriteExecute;
        }

        public void Dispose()
        {
            _cancelSource?.Dispose();
            _thisProcess.Dispose();
        }
    }
}

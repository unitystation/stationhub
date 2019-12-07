using Avalonia;
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
using System.Threading;
using System.Threading.Tasks;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class HubUpdateViewModel : ViewModelBase
    {
        private CancellationTokenSource cancelSource;
        private readonly Lazy<LoginViewModel> loginVM;
        private string updateTitle;
        private string updateMessage;
        private string buttonMessage;
        private string downloadMessage;
        private bool installButtonVisible;
        private bool downloadBarVisible;
        private bool restartButtonVisible;
        public HubUpdateViewModel(Lazy<LoginViewModel> loginVM)
        {
            this.loginVM = loginVM;
            BeginDownload = ReactiveCommand.Create(UpdateHub);
            RestartHub = ReactiveCommand.Create(RestartApp);
            Cancel = ReactiveCommand.Create(CancelInstall);

            UpdateTitle = "Update Required";
            UpdateMessage = $"An update is required before you can continue.\n\rPlease install" +
                $" the latest version by clicking the button below:";

            ButtonMessage = $"Update Hub";

            InstallButtonVisible = true;
            DownloadBarVisible = false;
            RestartButtonVisible = false;
        }

        public ReactiveCommand<Unit, Unit> BeginDownload { get; }
        public ReactiveCommand<Unit, Unit> RestartHub { get; }
        public ReactiveCommand<Unit, ViewModelBase> Cancel { get; }
        public Subject<int> Progress { get; set; } = new Subject<int>();

        public bool InstallButtonVisible
        {
            get => installButtonVisible;
            set => this.RaiseAndSetIfChanged(ref installButtonVisible, value);
        }

        public bool DownloadBarVisible
        {
            get => downloadBarVisible;
            set => this.RaiseAndSetIfChanged(ref downloadBarVisible, value);
        }

        public bool RestartButtonVisible
        {
            get => restartButtonVisible;
            set => this.RaiseAndSetIfChanged(ref restartButtonVisible, value);
        }

        public string UpdateTitle
        {
            get => updateTitle;
            set => this.RaiseAndSetIfChanged(ref updateTitle, value);
        }

        public string UpdateMessage
        {
            get => updateMessage;
            set => this.RaiseAndSetIfChanged(ref updateMessage, value);
        }

        public string ButtonMessage
        {
            get => buttonMessage;
            set => this.RaiseAndSetIfChanged(ref buttonMessage, value);
        }

        public string DownloadMessage
        {
            get => downloadMessage;
            set => this.RaiseAndSetIfChanged(ref downloadMessage, value);
        }

        public void UpdateHub()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Config.SetPermissions(Config.UnixExeFullPath);
            }

            cancelSource = new CancellationTokenSource();
            TryUpdate(cancelSource.Token);
        }

        async Task TryUpdate(CancellationToken cancelToken)
        {
            InstallButtonVisible = false;
            DownloadBarVisible = true;
            UpdateTitle = "Downloading...";
            RenameCurrentFiles();
            Log.Information("Download started...");
            var webRequest = WebRequest.Create(Config.serverHubClientConfig.GetDownloadURL());
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
                    DownloadMessage = $" {downloadedAmt} / {(int)maxFileSize.Megabytes} MB";
                    Progress.OnNext(p);
                    Log.Information("Progress: {prog}", p);
                });

            await Task.Run(() =>
            {
                Log.Information("Extracting...");
                try
                {
                    var archive = new ZipArchive(progStream);
                    archive.ExtractToDirectory(Config.RootFolder, true);

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
                Config.SetPermissions(Config.UnixExeFullPath);
            }

            DownloadBarVisible = false;
            RestartButtonVisible = true;
            UpdateTitle = "Download complete!\n\rClick the restart button to continue:";
        }

        private void OnExit(object? sender, EventArgs e)
        {
            Log.Information("Attempt to start new client");
            var startInfo = new ProcessStartInfo(Path.Combine(Config.GetHubExecutable()));
            var process = new Process();
            process.StartInfo = startInfo;
            process.Start();
        }

        void RenameCurrentFiles(bool reverseNames = false)
        {
            try
            {
                var from = "";
                var to = "";

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    from = reverseNames ? Config.WinExeOldFullPath : Config.WinExeFullPath;
                    to = reverseNames ? Config.WinExeFullPath : Config.WinExeOldFullPath;
                }
                else
                {
                    from = reverseNames ? Config.UnixExeOldFullPath : Config.UnixExeFullPath;
                    to = reverseNames ? Config.UnixExeFullPath : Config.UnixExeOldFullPath;
                }

                File.Move(from, to);
            }
            catch (Exception e)
            {
                Log.Error("Error: " + e.Message);
            }
        }

        ViewModelBase CancelInstall()
        {
            cancelSource.Cancel();
            RenameCurrentFiles(true);
            return loginVM.Value;
        }

        void RestartApp()
        {
            Application.Current.OnExit += OnExit;
            Application.Current.Exit();
        }
    }
}

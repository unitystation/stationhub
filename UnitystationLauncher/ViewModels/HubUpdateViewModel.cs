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
        private Process thisProcess;

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
            Process[] myProcesses = Process.GetProcessesByName("StationHub");
            thisProcess = myProcesses[0];

            InstallButtonVisible = true;
            DownloadBarVisible = false;
            RestartButtonVisible = false;
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
            Directory.CreateDirectory(Config.TempFolder);
            Console.WriteLine("Config temp folder: " + Config.TempFolder);
            Config.SetPermissions(Config.TempFolder);

            InstallButtonVisible = false;
            DownloadBarVisible = true;
            UpdateTitle = "Downloading...";

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
                    var downloadedAmt = (int)((float)maxFileSize.Kilobytes * ((float)p / 100f));
                    DownloadMessage = $" {downloadedAmt} / {(int)maxFileSize.Kilobytes} KB";
                    Progress.OnNext(p);
                    Log.Information("Progress: {prog}", p);
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
                Config.SetPermissions(Config.TempFolder);
            }

            DownloadBarVisible = false;
            //RestartButtonVisible = true;
            //UpdateTitle = "Download complete!\n\rClick the restart button to continue:";
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

            thisProcess.Kill();
        }

        ViewModelBase CancelInstall()
        {
            cancelSource.Cancel();
            return loginVM.Value;
        }

        void RestartApp()
        {

            if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Exit += OnExit;
                desktopLifetime.Shutdown();
            }
        }
    }
}

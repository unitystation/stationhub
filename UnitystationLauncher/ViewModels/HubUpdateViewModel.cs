using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Mono.Unix;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Exceptions;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.ViewModels
{
    public class HubUpdateViewModel : ViewModelBase, IDisposable
    {
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

        public ReactiveCommand<Unit, LauncherViewModel> Skip { get; }
        public ReactiveCommand<Unit, LauncherViewModel> Ignore { get; }

        private readonly Lazy<LauncherViewModel> _launcherVm;
        private readonly Config _config;
        private string? _updateTitle;
        private string? _updateMessage;
        private readonly Process _thisProcess;
        private readonly int _newVersion;

        public HubUpdateViewModel(Lazy<LauncherViewModel> launcherVm, Config config)
        {
            _launcherVm = launcherVm;
            _config = config;
            Ignore = ReactiveCommand.CreateFromTask(IgnoreUpdate);
            Skip = ReactiveCommand.Create(SkipUpdate);

            UpdateTitle = "Launcher Update Available";

            _newVersion = GetUpdateVersion().Result;

            UpdateMessage = $"Launcher version {_newVersion} is available.\n"
                            + $"Current version: {Config.CurrentBuild}";
            _thisProcess = Process.GetCurrentProcess();
        }

        private async Task<int> GetUpdateVersion()
        {
            HubClientConfig? hubClientConfig = await _config.GetServerHubClientConfigAsync();
            return hubClientConfig?.BuildNumber ?? 0;
        }

        private void Update()
        {
            ProcessStartInfo psi = new()
            {
                FileName = LinkUrls.LauncherReleasesUrl,
                UseShellExecute = true
            };

            Process.Start(psi);

            // Need to just wait a small amount of time, otherwise we will exit before that is finished opening
            Thread.Sleep(100);

            Exit();
        }

        private void Exit()
        {
            _thisProcess.Kill();
        }

        private LauncherViewModel SkipUpdate()
        {
            return _launcherVm.Value;
        }

        private async Task<LauncherViewModel> IgnoreUpdate()
        {
            Preferences preferences = await _config.GetPreferencesAsync();
            preferences.IgnoreVersionUpdate = _newVersion;
            return _launcherVm.Value;
        }

        public void Dispose()
        {
            _thisProcess.Dispose();
        }
    }
}
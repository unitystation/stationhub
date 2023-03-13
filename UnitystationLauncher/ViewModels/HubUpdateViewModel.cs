using ReactiveUI;
using System;
using System.Diagnostics;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Services.Interface;

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
        private string? _updateTitle;
        private string? _updateMessage;
        private readonly Process _thisProcess;
        private readonly int _newVersion;
        private readonly IHubService _hubService;
        private readonly IPreferencesService _preferencesService;

        public HubUpdateViewModel(Lazy<LauncherViewModel> launcherVm, IHubService hubService, IPreferencesService preferencesService)
        {
            _launcherVm = launcherVm;
            _hubService = hubService;
            _preferencesService = preferencesService;
            Ignore = ReactiveCommand.Create(IgnoreUpdate);
            Skip = ReactiveCommand.Create(SkipUpdate);

            UpdateTitle = "Launcher Update Available";

            _newVersion = GetUpdateVersion().Result;

            UpdateMessage = $"Launcher version {_newVersion} is available.\n"
                            + $"Current version: {AppInfo.CurrentBuild}";
            _thisProcess = Process.GetCurrentProcess();
        }

        private async Task<int> GetUpdateVersion()
        {
            HubClientConfig? hubClientConfig = await _hubService.GetServerHubClientConfigAsync();
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
            const int millisecondsToWait = 100;
            Thread.Sleep(millisecondsToWait);

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

        private LauncherViewModel IgnoreUpdate()
        {
            Preferences preferences = _preferencesService.GetPreferences();
            preferences.IgnoreVersionUpdate = _newVersion;
            return _launcherVm.Value;
        }

        public void Dispose()
        {
            _thisProcess.Dispose();
        }
    }
}
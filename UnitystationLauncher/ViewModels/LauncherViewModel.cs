using ReactiveUI;
using System.Reactive;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using Serilog;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MsBox.Avalonia.Base;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ViewModels
{
    public class LauncherViewModel : ViewModelBase
    {
        private readonly Lazy<HubUpdateViewModel> _hubUpdateVm;
        private readonly IHubService _hubService;
        private readonly IPreferencesService _preferencesService;
        private readonly IEnvironmentService _environmentService;
        private readonly Lazy<LoginViewModel> _logoutVm;
        private readonly ITTSService _ITTSService;
        private readonly AuthService _authService;
        public ReactiveCommand<Unit, Unit> OpenMainSite { get; }
        public ReactiveCommand<Unit, Unit> OpenPatreon { get; }
        public ReactiveCommand<Unit, Unit> OpenDiscordInvite { get; }

        private PanelBase[] _panels;
        public PanelBase[] Panels
        {
            get => _panels;
            set => this.RaiseAndSetIfChanged(ref _panels, value);
        }

        private ViewModelBase? _selectedPanel;
        public ViewModelBase? SelectedPanel
        {
            get => _selectedPanel;
            set => this.RaiseAndSetIfChanged(ref _selectedPanel, value);
        }
        public ReactiveCommand<Unit, LoginViewModel> Logout { get; }
        public ReactiveCommand<Unit, HubUpdateViewModel> ShowUpdateView { get; }

        public LauncherViewModel(
            Lazy<HubUpdateViewModel> hubUpdateVm,
            NewsPanelViewModel newsPanel,
            ServersPanelViewModel serversPanel,
            InstallationsPanelViewModel installationsPanel,
            PreferencesPanelViewModel preferencesPanel,
            IHubService hubService,
            IPreferencesService preferencesService,
            IEnvironmentService environmentService,
            IGameCommunicationPipeService gameCommunicationPipeService,
            ITTSService ITTSService,
            Lazy<LoginViewModel> logoutVm, AuthService authService)
        {
            _authService = authService;
            _logoutVm = logoutVm;
            _hubUpdateVm = hubUpdateVm;
            _hubService = hubService;
            _preferencesService = preferencesService;
            _environmentService = environmentService;
            _ITTSService = ITTSService;
            gameCommunicationPipeService.Init();

            OpenMainSite = ReactiveCommand.Create(() => OpenLink(LinkUrls.MainSiteUrl));
            OpenPatreon = ReactiveCommand.Create(() => OpenLink(LinkUrls.PatreonUrl));
            OpenDiscordInvite = ReactiveCommand.Create(() => OpenLink(LinkUrls.DiscordInviteUrl));
            Logout = ReactiveCommand.CreateFromTask(LogoutAsync);
            _panels = GetEnabledPanels(newsPanel, serversPanel, installationsPanel, preferencesPanel);
            ShowUpdateView = ReactiveCommand.Create(ShowUpdateImp);
            SelectedPanel = serversPanel;

            RxApp.MainThreadScheduler.ScheduleAsync((_, _) => ValidateClientVersionAsync());
            RxApp.MainThreadScheduler.Schedule((_) => StartTTSIfInstalled());

        }
        private async Task<LoginViewModel> LogoutAsync()
        {
            await _authService.SignOutUserAsync();
            return _logoutVm.Value;
        }



        private static PanelBase[] GetEnabledPanels(
            NewsPanelViewModel newsPanel,
            ServersPanelViewModel serversPanel,
            InstallationsPanelViewModel installationsPanel,
            PreferencesPanelViewModel preferencesPanel)
        {
            List<PanelBase> panelBases = new();

            if (newsPanel.IsEnabled)
            {
                panelBases.Add(newsPanel);
            }

            if (serversPanel.IsEnabled)
            {
                panelBases.Add(serversPanel);
            }

            if (installationsPanel.IsEnabled)
            {
                panelBases.Add(installationsPanel);
            }

            if (preferencesPanel.IsEnabled)
            {
                panelBases.Add(preferencesPanel);
            }

            return panelBases.ToArray();
        }

        private void StartTTSIfInstalled()
        {
            _ITTSService.StartTTS();
        }

        private async Task ValidateClientVersionAsync()
        {

            HubClientConfig? hubClientConfig = await _hubService.GetServerHubClientConfigAsync();

            if (hubClientConfig == null)
            {
                Log.Error("Error: {Error}", "Unable to retrieve client config from hub.");
                return;
            }

            if (hubClientConfig.BuildNumber > AppInfo.CurrentBuild)
            {
                Log.Information("Client is old {CurrentBuild} new version is {BuildNumber}",
                    AppInfo.CurrentBuild,
                    hubClientConfig.BuildNumber);

                // I think it would still be good to log it, but this should disable the update view in the launcher.
                if (_environmentService.ShouldDisableUpdateCheck())
                {
                    return;
                }

                Preferences preferences = _preferencesService.GetPreferences();
                if (preferences.IgnoreVersionUpdate < hubClientConfig.BuildNumber)
                {
                    Observable.Return(Unit.Default).InvokeCommand(ShowUpdateView);
                }
            }
        }

        private static void OpenLink(string url)
        {
            ProcessStartInfo psi = new()
            {
                FileName = url,
                UseShellExecute = true
            };

            Process.Start(psi);
        }

        private HubUpdateViewModel ShowUpdateImp()
        {
            return _hubUpdateVm.Value;
        }

        public override void Refresh()
        {
            // Do nothing
        }

    }
}

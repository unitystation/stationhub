using ReactiveUI;
using System.Reactive;
using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using Serilog;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Services;

namespace UnitystationLauncher.ViewModels
{
    public class LauncherViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly Lazy<LoginViewModel> _logoutVm;
        private readonly Lazy<HubUpdateViewModel> _hubUpdateVm;
        private readonly Config _config;
        PanelBase[] _panels;
        ViewModelBase? _selectedPanel;

        public string Username => _authService.AuthLink?.User.DisplayName ?? "";

        public PanelBase[] Panels
        {
            get => _panels;
            set => this.RaiseAndSetIfChanged(ref _panels, value);
        }

        public ViewModelBase? SelectedPanel
        {
            get => _selectedPanel;
            set => this.RaiseAndSetIfChanged(ref _selectedPanel, value);
        }

        public ReactiveCommand<Unit, LoginViewModel> Logout { get; }
        public ReactiveCommand<Unit, HubUpdateViewModel> ShowUpdateView { get; }

        public LauncherViewModel(
            AuthService authService,
            Lazy<LoginViewModel> logoutVm,
            Lazy<HubUpdateViewModel> hubUpdateVm,
            NewsPanelViewModel newsPanel,
            ServersPanelViewModel serversPanel,
            InstallationsPanelViewModel installationsPanel,
            SettingsPanelViewModel settingsPanel,
            Config config)
        {
            _authService = authService;
            _logoutVm = logoutVm;
            _hubUpdateVm = hubUpdateVm;
            _config = config;

            _panels = GetEnabledPanels(newsPanel, serversPanel, installationsPanel, settingsPanel);
            Logout = ReactiveCommand.CreateFromTask(LogoutAsync);
            ShowUpdateView = ReactiveCommand.Create(ShowUpdateImp);
            SelectedPanel = serversPanel;

            RxApp.MainThreadScheduler.ScheduleAsync((_, _) => ValidateClientVersionAsync());
        }

        private PanelBase[] GetEnabledPanels(
            NewsPanelViewModel newsPanel,
            ServersPanelViewModel serversPanel,
            InstallationsPanelViewModel installationsPanel,
            SettingsPanelViewModel settingsPanel)
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

            if (settingsPanel.IsEnabled)
            {
                panelBases.Add(settingsPanel);
            }

            return panelBases.ToArray();
        }

        private async Task ValidateClientVersionAsync()
        {
            HubClientConfig? hubClientConfig = await _config.GetServerHubClientConfigAsync();

            if (hubClientConfig == null)
            {
                Log.Error("Error: {Error}", "Unable to retrieve client config from hub.");
                return;
            }

            if (hubClientConfig.BuildNumber > Config.CurrentBuild)
            {
                Log.Information("Client is old {CurrentBuild} new version is {BuildNumber}",
                    Config.CurrentBuild,
                    hubClientConfig.BuildNumber);

                Preferences preferences = await _config.GetPreferencesAsync();

                if (preferences.IgnoreVersionUpdate < hubClientConfig.BuildNumber)
                {
                    Observable.Return(Unit.Default).InvokeCommand(ShowUpdateView);
                }
            }
        }

        private async Task<LoginViewModel> LogoutAsync()
        {
            await _authService.SignOutUserAsync();
            Preferences prefs = await _config.GetPreferencesAsync();
            prefs.LastLogin = "";
            return _logoutVm.Value;
        }

        private HubUpdateViewModel ShowUpdateImp()
        {
            return _hubUpdateVm.Value;
        }
    }
}

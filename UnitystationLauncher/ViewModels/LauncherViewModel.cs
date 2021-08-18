using ReactiveUI;
using UnitystationLauncher.Models;
using System.Reactive;
using System;
using System.Reactive.Concurrency;
using Serilog;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace UnitystationLauncher.ViewModels
{
    public class LauncherViewModel : ViewModelBase
    {
        private readonly AuthManager _authManager;
        private readonly Lazy<LoginViewModel> _logoutVm;
        private readonly Lazy<HubUpdateViewModel> _hubUpdateVm;
        private readonly Config _config;
        PanelBase[] _panels;
        ViewModelBase? _selectedPanel;

        public string Username => _authManager.AuthLink?.User.DisplayName ?? "";

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
        public ReactiveCommand<Unit, HubUpdateViewModel> ShowUpdateReqd { get; }

        public LauncherViewModel(
            AuthManager authManager,
            ServersPanelViewModel serversPanel,
            InstallationsPanelViewModel installationsPanel,
            Lazy<LoginViewModel> logoutVm,
            Lazy<HubUpdateViewModel> hubUpdateVm,
            NewsPanelViewModel news,
            SettingsPanelViewModel settings,
            Config config)
        {
            _authManager = authManager;
            _logoutVm = logoutVm;
            _hubUpdateVm = hubUpdateVm;
            _config = config;
            _panels = new PanelBase[]
            {
                news,
                serversPanel,
                installationsPanel,
                settings
            };
            Logout = ReactiveCommand.CreateFromTask(LogoutImp);
            ShowUpdateReqd = ReactiveCommand.Create(ShowUpdateImp);
            SelectedPanel = serversPanel;

            RxApp.MainThreadScheduler.Schedule(async () => await ValidateClientVersion());
        }


        async Task ValidateClientVersion()
        {
            var clientConfig = await _config.GetServerHubClientConfig();
            if (clientConfig.BuildNumber > Config.CurrentBuild)
            {
                Log.Information("Client is old {CurrentBuild} new version is {BuildNumber}",
                    Config.CurrentBuild,
                    clientConfig.BuildNumber);
                Observable.Return(Unit.Default).InvokeCommand(ShowUpdateReqd);
            }
        }

        async Task<LoginViewModel> LogoutImp()
        {
            await _authManager.SignOutUser();
            var prefs = await _config.GetPreferences();
            prefs.LastLogin = "";
            return _logoutVm.Value;
        }

        HubUpdateViewModel ShowUpdateImp()
        {
            return _hubUpdateVm.Value;
        }
    }
}

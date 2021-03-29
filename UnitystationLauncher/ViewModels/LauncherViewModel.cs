﻿using ReactiveUI;
using UnitystationLauncher.Models;
using System.Reactive;
using System.IO;
using System;
using Newtonsoft.Json;
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

        public ReactiveCommand<Unit, Unit> Refresh { get; }

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
            Logout = ReactiveCommand.Create(LogoutImp);
            Refresh = ReactiveCommand.Create(serversPanel.OnRefresh);
            ShowUpdateReqd = ReactiveCommand.Create(ShowUpdateImp);
            SelectedPanel = serversPanel;

            ValidateClientVersion();
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

        LoginViewModel LogoutImp()
        {
            _authManager.SignOutUser();
            File.WriteAllText(Path.Combine(Path.Combine(Config.RootFolder, "prefs.json")), JsonConvert.SerializeObject(new Prefs()));
            File.Delete(Path.Combine(Config.RootFolder, "settings.json"));
            return _logoutVm.Value;
        }

        HubUpdateViewModel ShowUpdateImp()
        {
            return _hubUpdateVm.Value;
        }
    }
}

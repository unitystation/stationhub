using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        private readonly AuthService _authService;
        private readonly StateService _stateService;
        private readonly DownloadService _downloadService;

        public override string Name => "Servers";

        public ServersPanelViewModel(StateService stateService, DownloadService downloadService,
            AuthService authService)
        {
            _stateService = stateService;
            _downloadService = downloadService;
            _authService = authService;

            DownloadCommand = ReactiveCommand.CreateFromTask<ServerViewModel, Unit>(async server =>
            {
                await DownloadAsync(server.Server);
                return Unit.Default;
            });
        }

        public ReactiveCommand<ServerViewModel, Unit> DownloadCommand { get; }

        public IObservable<IReadOnlyList<ServerViewModel>> ServerList => _stateService.State
            .Select(state => state
                .SelectMany(installationState => installationState.Value.Servers
                    .Select(s => new ServerViewModel(s, installationState.Value.Installation,
                        installationState.Value.Download, _authService)))
                .ToList());

        public IObservable<bool> ServersFound => ServerList.Select(sl => sl.Any());

        private async Task DownloadAsync(Server server)
        {
            await _downloadService.DownloadAsync(server);
        }
    }
}
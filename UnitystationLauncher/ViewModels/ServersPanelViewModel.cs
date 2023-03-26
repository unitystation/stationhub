using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        private readonly StateService _stateService;
        private readonly IDownloadService _downloadService;
        private readonly IEnvironmentService _environmentService;

        public override string Name => "Servers";
        public override bool IsEnabled => true;

        public ServersPanelViewModel(StateService stateService, IDownloadService downloadService, IEnvironmentService environmentService)
        {
            _stateService = stateService;
            _downloadService = downloadService;
            _environmentService = environmentService;

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
                    .Select(s => new ServerViewModel(s, installationState.Value.Installation, installationState.Value.Download, _environmentService)))
                .ToList());

        public IObservable<bool> ServersFound => ServerList.Select(sl => sl.Any());

        private async Task DownloadAsync(Server server)
        {
            await _downloadService.DownloadAsync(server);
        }
    }
}
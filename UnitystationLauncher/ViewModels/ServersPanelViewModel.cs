using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        private readonly AuthManager _authManager;
        private readonly StateManager _stateManager;
        private readonly DownloadManager _downloadManager;

        public override string Name => "Servers";

        public ServersPanelViewModel(StateManager stateManager, DownloadManager downloadManager,
            AuthManager authManager)
        {
            _stateManager = stateManager;
            _downloadManager = downloadManager;
            _authManager = authManager;

            DownloadCommand = ReactiveCommand.CreateFromTask<ServerViewModel, Unit>(async server =>
            {
                await Download(server.Server);
                return Unit.Default;
            });
        }

        public ReactiveCommand<ServerViewModel, Unit> DownloadCommand { get; }

        public IObservable<IReadOnlyList<ServerViewModel>> ServerList => _stateManager.State
            .Select(state => state
                .SelectMany(installationState => installationState.Value.Servers
                    .Select(s => new ServerViewModel(s, installationState.Value.Installation,
                        installationState.Value.Download, _authManager)))
                .ToList());

        public IObservable<bool> ServersFound => ServerList.Select(sl => sl.Any());

        private async Task Download(Server server)
        {
            await _downloadManager.Download(server);
        }
    }
}
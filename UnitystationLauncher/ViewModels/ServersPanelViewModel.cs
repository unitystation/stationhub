using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        private readonly AuthManager _authManager;
        private readonly StateManager _stateManager;

        public override string Name => "Servers";

        public ServersPanelViewModel(StateManager stateManager, AuthManager authManager)
        {
            _stateManager = stateManager;
            _authManager = authManager;
        }

        public IObservable<IReadOnlyList<ServerViewModel>> ServerList => _stateManager.State
            .Select(state => state
                .SelectMany(installationState => installationState.Value.Servers
                    .Select(s => new ServerViewModel(s, installationState.Value.Installation, _authManager)))
                .ToList());

        public IObservable<bool> ServersFound => ServerList.Select(sl => sl.Any());
    }
}

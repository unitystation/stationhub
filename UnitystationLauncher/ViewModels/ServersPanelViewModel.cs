using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        public ServerManager ServerManager { get; }

        public override string Name => "Servers";

        public ServersPanelViewModel(ServerManager serverManager)
        {
            ServerManager = serverManager;
        }

        public IObservable<IReadOnlyList<ServerViewModel>> ServerList => ServerManager.Servers;
        public IObservable<bool> ServersFound => ServerList.Select(sl => sl.Any());
    }
}

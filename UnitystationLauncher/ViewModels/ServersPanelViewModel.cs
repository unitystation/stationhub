using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        ServerWrapper? selectedServer;
        private readonly ServerManager serverManager;

        public ServersPanelViewModel(ServerManager serverManager)
        {
            this.serverManager = serverManager;
        }

        public override string Name => "Servers";

        public IObservable<IReadOnlyList<ServerWrapper>> Servers => serverManager.Servers;

        public ServerWrapper? SelectedServer
        {
            get => selectedServer;
            set => this.RaiseAndSetIfChanged(ref selectedServer, value);
        }
    }
}

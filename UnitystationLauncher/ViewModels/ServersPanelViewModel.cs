using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        ServerWrapper? selectedServer;
        private readonly ServerManager serverManager;
        private readonly DownloadManager downloadManager;
        private readonly StateManager stateManager;

        public ServersPanelViewModel(ServerManager serverManager, DownloadManager downloadManager, StateManager stateManager)
        {
            this.serverManager = serverManager;
            this.downloadManager = downloadManager;
            this.stateManager = stateManager;

            SelectedDownload = stateManager.State
                .CombineLatest(this.Changed, (dictionary, c) => (dictionary, SelectedServer))
                .Select(d =>
                {
                    (var dict, var selected) = d;
                    var key = (selected.ForkName, selected.BuildVersion);
                    if (!dict.ContainsKey(key))
                    {
                        return dict[key].download;
                    }
                    return null;
                });

            Download = ReactiveCommand.Create(
                DoDownload, 
                this.WhenAnyValue(x => x.selectedServer).Select(s => s != null));
        }

        public override string Name => "Servers";

        public IObservable<IReadOnlyList<ServerWrapper>> Servers => serverManager.Servers;

        public ReactiveCommand<Unit, Unit> Download { get; }

        public ServerWrapper? SelectedServer
        {
            get => selectedServer;
            set => this.RaiseAndSetIfChanged(ref selectedServer, value);
        }

        public IObservable<Download?> SelectedDownload { get; }

        public void DoDownload()
        {
            downloadManager.Download(SelectedServer);
        }
    }
}

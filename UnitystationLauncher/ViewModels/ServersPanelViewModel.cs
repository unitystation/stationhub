using Avalonia.Collections;
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

        public ServersPanelViewModel(ServerManager serverManager, DownloadManager downloadManager)
        {
            this.serverManager = serverManager;
            this.downloadManager = downloadManager;

            SelectedDownload = downloadManager.Downloads.GetWeakCollectionChangedObservable()
                .CombineLatest(this.Changed, (e, c) => Unit.Default)
                .Select(d => SelectedServer == null ?
                    null :
                    downloadManager.Downloads.FirstOrDefault(d =>
                        d.ForkName == SelectedServer.ForkName &&
                        d.BuildVersion == SelectedServer.BuildVersion));

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

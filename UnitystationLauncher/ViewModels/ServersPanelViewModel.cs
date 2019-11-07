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
using System.Threading;
using System.Threading.Tasks;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        ServerWrapper? selectedServer;
        private readonly DownloadManager downloadManager;

        public ServersPanelViewModel(ServerManager serverManager, DownloadManager downloadManager, DirectoryManager directoryManager)
        {
            this.ServerManager = serverManager;
            this.downloadManager = downloadManager;

            SelectedDownload = Observable.Merge(
                this.Changed.Select(x => Unit.Default),
                downloadManager.Downloads.GetWeakCollectionChangedObservable().Select(x => Unit.Default))
                .Select(d => SelectedServer == null ?
                    null :
                    downloadManager.Downloads.FirstOrDefault(d =>
                        d.ForkName == SelectedServer.ForkName &&
                        d.BuildVersion == SelectedServer.BuildVersion));

            Download = ReactiveCommand.Create(
                DoDownload,
                this.WhenAnyValue(x => x.SelectedServer)
                    .CombineLatest(directoryManager.Directories, (s, d) => s)
                    .Select(s => s != null && downloadManager.CanDownload(s))
                    .ObserveOn(SynchronizationContext.Current));
        }

        public override string Name => "Servers";
        public ServerManager ServerManager { get; }

        public ReactiveCommand<Unit, Unit> Download { get; }

        public ServerWrapper? SelectedServer
        {
            get => selectedServer;
            set => this.RaiseAndSetIfChanged(ref selectedServer, value);
        }

        public IObservable<Download?> SelectedDownload { get; }

        public void DoDownload()
        {
            _ = downloadManager.Download(SelectedServer!).Start();
        }
    }
}

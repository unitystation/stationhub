using Avalonia.Collections;
using MoreLinq.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace UnitystationLauncher.Models
{
    using State = IReadOnlyDictionary<(string ForkName, int BuildVersion), (Installation? installation, Download? download, IReadOnlyList<ServerWrapper> servers)>;

    public class StateManager
    {
        private readonly BehaviorSubject<State> _state;

        public StateManager(ServerManager serverManager, InstallationManager installationManager, DownloadManager downloadManager)
        {
            _state = new BehaviorSubject<State>(new Dictionary<(string ForkName, int BuildVersion), (Installation? installation, Download? download, IReadOnlyList<ServerWrapper> servers)>());

            var groupedServerEvents = serverManager.Servers
                .Select(servers => servers
                    .GroupBy(s => s.Key))
                .Do(x => Log.Logger.Information("Servers changed"));

            var downloadEvents = Observable.Merge(
                downloadManager.Downloads.GetWeakCollectionChangedObservable()
                    .Select(d => downloadManager.Downloads),
                Observable.Return(downloadManager.Downloads))
                .Do(x => Log.Logger.Information("Downloads changed"));

            var installationEvvents = installationManager.Installations
                .Do(x => Log.Logger.Information("Installations changed"));

            groupedServerEvents
                .CombineLatest(installationEvvents, (servers, installations) => (servers, installations))
                .Select(d => d.servers.FullJoin(d.installations,
                        s => s.Key,
                        i => i.Key,
                        s => (s.Key, ReadOnly(s), null),
                        i => (i.Key, null, i),
                        (servers, installation) => (servers.Key, servers: ReadOnly(servers), installation)))
                .CombineLatest(downloadEvents, (join, downloads) => (join, downloads))
                .Select(x => x.join.LeftJoin(x.downloads,
                        s => s.Key,
                        d => d.Key,
                        s => (s.Key, s.servers, s.installation, null),
                        (s, download) => (s.Key, s.servers, s.installation, download)))
                .Select(x => (State)x.ToDictionary(d => d.Key, d => (d.installation, d.download, d.servers)))
                .Do(x => Log.Logger.Information("state changed"))
                .Subscribe(_state);

            State.Subscribe(x => Log.Logger.Information("State changed"));
        }

        public IObservable<State> State => _state;

        private IReadOnlyList<T> ReadOnly<T>(IEnumerable<T> items)
        {
            return items.ToArray();
        }
    }
}
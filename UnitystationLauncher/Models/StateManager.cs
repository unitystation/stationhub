using Avalonia.Collections;
using MoreLinq.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace UnitystationLauncher.Models
{
    public class StateManager
    {
        public StateManager(ServerManager serverManager, InstallationManager installationManager, DownloadManager downloadManager)
        {
            var groupedServerEvents = serverManager.Servers
                .Select(servers => servers
                    .GroupBy(s => s.ForkAndVersion))
                .Do(x => Log.Logger.Information("Servers changed"));

            var downloadEvents = Observable.Merge(
                    downloadManager.Downloads.GetWeakCollectionChangedObservable()
                        .Select(d => downloadManager.Downloads),
                    Observable.Return(downloadManager.Downloads))
                .Do(x => Log.Logger.Information("Downloads changed"));

            var installationEvvents = installationManager.Installations
                .Do(x => Log.Logger.Information("Installations changed"));

            State = groupedServerEvents
                .CombineLatest(installationEvvents, (servers, installations) => (servers, installations))
                .Select(d => d.servers.FullJoin(d.installations,
                    s => s.Key,
                    i => i.ForkAndVersion,
                    servers => new ForkInstall(null, null, servers.ToArray()),
                    installation => new ForkInstall(null, installation, new List<Server>()),
                    (servers, installation) => new ForkInstall(null, installation, servers.ToArray())))
                .CombineLatest(downloadEvents, (join, downloads) => (join, downloads))
                .Select(x => x.join.LeftJoin(x.downloads,
                    s => s.ForkAndVersion,
                    d => d.ForkAndVersion,
                    s => s,
                    (s, download) => new ForkInstall(download, s.Installation, s.Servers)))
                .Select(x => x.ToDictionary(d => d.ForkAndVersion, d => d))
                .Do(x => Log.Logger.Information("state changed"))
                .PublishLast();
        }

        public IObservable<IReadOnlyDictionary<(string ForkName, int BuildVersion), ForkInstall>> State { get; }

        public class ForkInstall
        {
            public ForkInstall(Download? download, Installation? installation, IReadOnlyList<Server> servers)
            {
                Download = download;
                Installation = installation;
                Servers = servers;
            }

            public (string, int) ForkAndVersion =>
                Download?.ForkAndVersion ??
                Installation?.ForkAndVersion ??
                Servers.FirstOrDefault()?.ForkAndVersion ??
                throw new ArgumentNullException("All parameters are null or empty");

            public Download? Download { get; }
            public Installation? Installation { get; }
            public IReadOnlyList<Server> Servers { get; }
        }
    }
}
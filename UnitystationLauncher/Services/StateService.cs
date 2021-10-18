using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Collections;
using MoreLinq.Extensions;
using Serilog;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;

namespace UnitystationLauncher.Services
{
    public class StateService
    {
        public StateService(ServerService serverService, InstallationService installationService, DownloadService downloadService)
        {
            var groupedServerEvents = serverService.Servers
                .Select(servers => servers
                    .GroupBy(s => s.ForkAndVersion));

            var downloadEvents = downloadService.Downloads.GetWeakCollectionChangedObservable()
                .Select(d => downloadService.Downloads)
                .Merge(Observable.Return(downloadService.Downloads));

            var installationEvents = installationService.Installations;

            State = groupedServerEvents
                .CombineLatest(installationEvents, (servers, installations) => (servers, installations))
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
                .Do(x => Log.Information("State changed"))
                .Replay(1)
                .RefCount();
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
                throw new ArgumentNullException();

            public Download? Download { get; }
            public Installation? Installation { get; }
            public IReadOnlyList<Server> Servers { get; }
        }
    }
}
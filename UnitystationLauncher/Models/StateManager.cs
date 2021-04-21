using Avalonia.Collections;
using MoreLinq.Extensions;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Models
{
    public class StateManager
    {
        public StateManager(ServerManager serverManager, InstallationManager installationManager, DownloadManager downloadManager)
        {
            var groupedServerEvents = serverManager.Servers
                .Select(servers => servers
                    .GroupBy(s => s.Server.ForkAndVersion));

            var downloadEvents = downloadManager.Downloads.GetWeakCollectionChangedObservable()
                .Select(d => downloadManager.Downloads)
                .Merge(Observable.Return(downloadManager.Downloads));

            var installationEvents = installationManager.Installations;

            State = groupedServerEvents
                .CombineLatest(installationEvents, (servers, installations) => (servers, installations))
                .Select(d => d.servers.FullJoin(d.installations,
                    s => s.Key,
                    i => i.ForkAndVersion,
                    servers => new ForkInstall(null, null, servers.ToArray()),
                    installation => new ForkInstall(null, installation, new List<ServerViewModel>()),
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
            public ForkInstall(Download? download, Installation? installation, IReadOnlyList<ServerViewModel> servers)
            {
                Download = download;
                Installation = installation;
                Servers = servers;
            }

            public (string, int) ForkAndVersion =>
                Download?.ForkAndVersion ??
                Installation?.ForkAndVersion ??
                Servers.FirstOrDefault()?.Server.ForkAndVersion ??
                throw new ArgumentNullException();

            public Download? Download { get; }
            public Installation? Installation { get; }
            public IReadOnlyList<ServerViewModel> Servers { get; }
        }
    }
}
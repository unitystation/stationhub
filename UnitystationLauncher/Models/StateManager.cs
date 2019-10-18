using MoreLinq.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace UnitystationLauncher.Models
{
    using State = IReadOnlyDictionary<(string ForkName, int BuildVersion), (Installation? installation, Download? download, IReadOnlyCollection<ServerWrapper> servers)>;

    public class StateManager
    {
        private readonly ServerManager serverManager;
        private readonly InstallationManager installationManager;
        private readonly DownloadManager downloadManager;
        private readonly BehaviorSubject<State> state;

        public StateManager(ServerManager serverManager, InstallationManager installationManager, DownloadManager downloadManager)
        {
            this.serverManager = serverManager;
            this.installationManager = installationManager;
            this.downloadManager = downloadManager;
            state = new BehaviorSubject<State>(new Dictionary<(string ForkName, int BuildVersion), (Installation? installation, Download? download, IReadOnlyCollection<ServerWrapper> servers)>());

            serverManager.Servers
                .Select(servers => servers
                    .GroupBy(s => (s.ForkName, s.BuildVersion)))
                .CombineLatest(installationManager.Installations, (servers, installations) => (servers, installations))
                .Select(d => d.servers.FullJoin(d.installations,
                        s => s.Key,
                        i => (i.ForkName, i.BuildVersion),
                        s => (s.Key, s.ToList().AsReadOnly(), null),
                        i => (Key: (i.ForkName, i.BuildVersion), null, i),
                        (servers, installation) => (servers.Key, servers: servers.ToList().AsReadOnly(), installation)))
                //.CombineLatest(downloadManager.Downloads, (join, downloads) => (join, downloads))
                .Select(x => x.LeftJoin(downloadManager.Downloads,
                        s => s.Key,
                        d => (d.ForkName, d.BuildVersion),
                        s => (s.Key, s.servers, s.installation, null),
                        (s, download) => (s.Key, s.servers, s.installation, download)))
                .Select(x => (State)x.ToDictionary(d => d.Key, d => (d.servers, d.installation, d.download)))
                .Subscribe(state);
        }

        public IObservable<State> State => state;
    }
}
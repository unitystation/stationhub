using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Avalonia.Collections;
using MoreLinq.Extensions;
using Serilog;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services
{
    public class StateService : IStateService
    {
        private readonly IObservable<IReadOnlyDictionary<(string ForkName, int BuildVersion), ForkInstall>> _state;

        public StateService(IServerService serverService, IInstallationService installationService, IDownloadService downloadService)
        {
            IObservable<IEnumerable<IGrouping<(string, int), Server>>> groupedServerEvents = serverService.GetServers()
                .Select(servers => servers
                    .GroupBy(s => s.ForkAndVersion));

            IAvaloniaReadOnlyList<Download> downloads = downloadService.GetDownloads();

            IObservable<IAvaloniaReadOnlyList<Download>> downloadEvents = downloads.GetWeakCollectionChangedObservable()
                .Select(d => downloads)
                .Merge(Observable.Return(downloads));

            IObservable<IReadOnlyList<Installation>> installationEvents = installationService.GetInstallations();

            _state = groupedServerEvents
                .CombineLatest(installationEvents, (servers, installations) => (servers, installations))
                .Select(d => d.servers.FullJoin(d.installations,
                    s => s.Key,
                    i => i.ForkAndVersion,
                    servers => new ForkInstall(null, null, servers.ToArray()),
                    installation => new ForkInstall(null, installation, new List<Server>()),
                    (servers, installation) => new ForkInstall(null, installation, servers.ToArray())))
                .CombineLatest(downloadEvents, (join, downloadsList) => (join, downloads: downloadsList))
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

        public IObservable<IReadOnlyDictionary<(string ForkName, int BuildVersion), ForkInstall>> GetState()
        {
            return _state;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;

namespace UnitystationLauncher.Models
{
    public class ServerManager : ReactiveObject, IDisposable
    {
        private readonly HttpClient _http;
        private readonly InstallationManager _installManager;
        private bool _refreshing;

        public ServerManager(HttpClient http, InstallationManager installManager)
        {
            _http = http;
            _installManager = installManager;
            Servers = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10))
                .SelectMany(_ => GetServerList())
                .Replay(1)
                .RefCount();
        }

        public IObservable<IReadOnlyList<Server>> Servers { get; }

        public bool Refreshing
        {
            get => _refreshing;
            set => this.RaiseAndSetIfChanged(ref _refreshing, value);
        }

        private async Task<IReadOnlyList<Server>> GetServerList()
        {
            Refreshing = true;

            var data = await _http.GetStringAsync(Config.ApiUrl);
            var serverData = JsonConvert.DeserializeObject<ServerList>(data).Servers;
            Log.Information("Server list fetched");

            var servers = new List<Server>();

            foreach (var server in serverData)
            {
                if (!server.HasTrustedUrlSource)
                {
                    Log.Warning(
                        "Server: {ServerName} has untrusted download URL and has been omitted in the server list!",
                        server.ServerName);
                    continue;
                }

                servers.Add(server);
            }

            Refreshing = false;
            return servers;
        }

        public void Dispose()
        {
            _installManager.Dispose();
        }
    }
}
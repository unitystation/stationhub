using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.Services
{
    public class ServerService : ReactiveObject, IDisposable
    {
        private readonly HttpClient _http;
        private readonly InstallationService _installService;
        private bool _refreshing;

        public ServerService(HttpClient http, InstallationService installService)
        {
            _http = http;
            _installService = installService;
            Servers = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10))
                .SelectMany(_ => GetServerListAsync())
                .Replay(1)
                .RefCount();
        }

        public IObservable<IReadOnlyList<Server>> Servers { get; }

        public bool Refreshing
        {
            get => _refreshing;
            set => this.RaiseAndSetIfChanged(ref _refreshing, value);
        }

        private async Task<IReadOnlyList<Server>> GetServerListAsync()
        {
            Refreshing = true;

            var data = await _http.GetStringAsync(Config.ApiUrl);
            var serverData = JsonConvert.DeserializeObject<ServerList>(data)?.Servers;
            Log.Information("Server list fetched");

            var servers = new List<Server>();
            if (serverData != null)
            {
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
            }

            Refreshing = false;
            return servers;
        }

        public void Dispose()
        {
            _installService.Dispose();
        }
    }
}
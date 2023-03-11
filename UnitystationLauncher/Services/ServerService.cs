using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models.Api;

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
            Servers = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(10))
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

            string data = await _http.GetStringAsync(ApiUrls.ServerListUrl);
            List<Server>? serverData = JsonSerializer.Deserialize<ServerList>(data, options: new()
            {
                IgnoreReadOnlyProperties = true,
                PropertyNameCaseInsensitive = true
            })?.Servers;

            Log.Information("Server list fetched");

            List<Server> servers = new();
            if (serverData == null)
            {
                Log.Warning("Warning: {Warning}", "Invalid response from hub, server list is null.");
            }
            else if (serverData.Count == 0)
            {
                Log.Warning("Warning: {Warning}", "No servers returned by the hub.");
            }
            else
            {
                servers.AddRange(serverData.Where(IsValidServer));
            }

            Refreshing = false;
            return servers;
        }

        private static bool IsValidServer(Server server)
        {
            if (!server.HasTrustedUrlSource)
            {
                Log.Warning("Server: {ServerName} has untrusted download URL and has been omitted in the server list!",
                    server.ServerName);
                return false;
            }

            if (server is { HasValidDomainName: false, HasValidIpAddress: false })
            {
                Log.Warning("Server: {ServerName} has an invalid IP or domain name: {Domain}",
                    server.ServerName,
                    server.ServerIp);

                return false;
            }

            return true;
        }

        public void Dispose()
        {
            _installService.Dispose();
        }
    }
}
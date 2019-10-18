using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System.Text.Json;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;

namespace UnitystationLauncher.Models{

    public class ServerManager : ReactiveObject, IDisposable
    {
        private readonly HttpClient http;
        private readonly BehaviorSubject<IReadOnlyList<ServerWrapper>> serversSubject;
        TimeSpan refreshTimeout = TimeSpan.FromSeconds(5);
        bool refreshing;

        public ServerManager(HttpClient http)
        {
            serversSubject = new BehaviorSubject<IReadOnlyList<ServerWrapper>>(new ServerWrapper[0]);
            Observable.Timer(TimeSpan.Zero, refreshTimeout)
                .SelectMany(u => GetServers().ToObservable())
                .Subscribe(serversSubject);
                
            this.http = http;
        }

        public IObservable<IReadOnlyList<ServerWrapper>> Servers => serversSubject;

        public bool Refreshing
        {
            get => refreshing;
            set => this.RaiseAndSetIfChanged(ref refreshing, value);
        }

        public TimeSpan RefreshTimeout
        {
            get => refreshTimeout;
            set => this.RaiseAndSetIfChanged(ref refreshTimeout, value);
        }

        private async Task<IReadOnlyList<ServerWrapper>> GetServers()
        {
            Refreshing = true;
            Log.Verbose("Refreshing server list...");
            var response = await http.GetStringAsync(Config.apiUrl);
            var newServers = JsonConvert.DeserializeObject<ServerList>(response).Servers;

            var newWrappedServers = newServers.Select(s => new ServerWrapper(s)).ToList();

            Refreshing = false;
            Log.Verbose("Server list refreshed");
            return newWrappedServers;
        }

        public void Dispose()
        {
            serversSubject.Dispose();
        }
    }
}
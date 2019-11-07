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
                .Do(x => Refreshing = true)
                .Do(x => Log.Verbose("Refreshing server list..."))
                .SelectMany(u => http.GetStringAsync(Config.apiUrl).ToObservable())
                .Do(x => Refreshing = false)
                .DistinctUntilChanged()
                .Select(JsonConvert.DeserializeObject<ServerList>)
                .Select(servers => servers.Servers.Select(s => new ServerWrapper(s)).ToList())
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

        public void Dispose()
        {
            serversSubject.Dispose();
        }
    }
}
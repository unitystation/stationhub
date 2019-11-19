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
        private AuthManager authManager;
        TimeSpan refreshTimeout = TimeSpan.FromMinutes(1);
        bool refreshing;

        public ServerManager(HttpClient http, AuthManager authManager)
        {
            serversSubject = new BehaviorSubject<IReadOnlyList<ServerWrapper>>(new ServerWrapper[0]);
            Observable.Timer(TimeSpan.Zero, refreshTimeout)
                .Do(x => Refreshing = true)
                .Do(x => Log.Verbose("Refreshing server list..."))
                .SelectMany(u => http.GetStringAsync(Config.apiUrl).ToObservable())
                .Do(x => Refreshing = false)
                .DistinctUntilChanged()
                .Select(JsonConvert.DeserializeObject<ServerList>)
                .Select(servers => AddTest(servers))
                .Select(servers => servers.Servers.Select(s => new ServerWrapper(s, authManager)).ToList())
                .Subscribe(serversSubject);


               
            this.http = http;
            this.authManager = authManager;
        }

        private ServerList AddTest(ServerList list)
        {
            list.Servers.Add(new Server { ServerName = "TEST", BuildVersion = 12125, ForkName="fawf", ServerIP="10.1.1.12", ServerPort=1234});
            list.Servers.Add(new Server { ServerName = "TEST2", BuildVersion = 121425, ForkName = "f3123f", ServerIP = "10.1.1.13", ServerPort = 1214 });
            list.Servers.Add(new Server { ServerName = "TEST44", BuildVersion = 25, ForkName = "dawda2", ServerIP = "10.1.1.15", ServerPort = 1324 });
            return list;
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
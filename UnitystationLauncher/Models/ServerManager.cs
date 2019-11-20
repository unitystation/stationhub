using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System.Reactive.Linq;
using Reactive.Bindings;

namespace UnitystationLauncher.Models
{
    public class ServerManager : ReactiveObject, IDisposable
    {
        private readonly HttpClient http;
        private AuthManager authManager;
        bool refreshing;

        public ServerManager(HttpClient http, AuthManager authManager)
        {
            this.http = http;
            this.authManager = authManager;
            RefreshServerList();
        }

        public async void RefreshServerList()
        {
            NoServersFound.Value = false;
            if (Refreshing) return;

            Servers.Value = new List<ServerWrapper>();
            Refreshing = true;
            Log.Verbose("Refreshing server list...");
            var data = await http.GetStringAsync(Config.apiUrl);
            var serverList = JsonConvert.DeserializeObject<ServerList>(data);
            if (serverList.Servers.Count == 0)
            {
                NoServersFound.Value = true;
            }
            else
            {
               // AddTest(serverList);
                List<ServerWrapper> updatedList = new List<ServerWrapper>();
                foreach (Server s in serverList.Servers)
                {
                    updatedList.Add(new ServerWrapper(s, authManager));
                }

                Servers.Value = updatedList;
            }
            Refreshing = false;
        }

        private ServerList AddTest(ServerList list)
        {
            list.Servers.Add(new Server { ServerName = "TEST", BuildVersion = 12125, ForkName = "fawf", ServerIP = "10.1.1.12", ServerPort = 1234 });
            list.Servers.Add(new Server { ServerName = "TEST2", BuildVersion = 121425, ForkName = "f3123f", ServerIP = "10.1.1.13", ServerPort = 1214 });
            list.Servers.Add(new Server { ServerName = "TEST44", BuildVersion = 25, ForkName = "dawda2", ServerIP = "10.1.1.15", ServerPort = 1324 });
            return list;
        }

        public ReactiveProperty<List<ServerWrapper>> Servers { get; private set; } = new ReactiveProperty<List<ServerWrapper>>();
        public ReactiveProperty<bool> NoServersFound { get; private set; } = new ReactiveProperty<bool>();
        public bool Refreshing
        {
            get => refreshing;
            set => this.RaiseAndSetIfChanged(ref refreshing, value);
        }

        public void Dispose()
        {
            Servers.Dispose();
        }
    }
}
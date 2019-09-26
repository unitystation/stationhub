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

namespace UnitystationLauncher.Models{

    public class ServerManager : ReactiveObject
    {
        private readonly HttpClient http;
        private readonly ObservableCollection<ServerWrapper> servers;
        TimeSpan refreshTimeout = TimeSpan.FromSeconds(5);
        bool refreshing;

        public ServerManager(HttpClient http)
        {
            servers = new ObservableCollection<ServerWrapper>();
            Servers = new ReadOnlyObservableCollection<ServerWrapper>(servers);
            this.http = http;
            UpdateServers();
        }

        public ReadOnlyObservableCollection<ServerWrapper> Servers { get; }

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

        private async void UpdateServers()
        {
            while (true)
            {
                Refreshing = true;
                Log.Verbose("Refreshing server list...");
                var response = await http.GetStringAsync(Config.apiUrl);
                var newServers = JsonConvert.DeserializeObject<ServerList>(response).Servers;

                foreach (var deletedServer in Servers.Except(newServers).ToArray())
                {
                    servers.Remove((ServerWrapper)deletedServer);
                }

                foreach (var newServer in newServers.Except(Servers).ToArray())
                {
                    servers.Add(new ServerWrapper(newServer));
                }

                Refreshing = false;
                Log.Verbose("Server list refreshed");

                await Task.Delay(refreshTimeout);
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using Reactive.Bindings;

namespace UnitystationLauncher.Models
{
    public class ServerManager : ReactiveObject, IDisposable
    {
        private readonly HttpClient http;
        private InstallationManager installManager;
        private AuthManager authManager;
        bool refreshing;

        public ServerManager(HttpClient http, AuthManager authManager, InstallationManager installManager)
        {
            this.http = http;
            this.authManager = authManager;
            this.installManager = installManager;
            installManager.InstallListChange = RefreshInstalledStates;
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
                List<ServerWrapper> updatedList = new List<ServerWrapper>();
                foreach (Server s in serverList.Servers)
                {
                    updatedList.Add(new ServerWrapper(s, authManager, installManager));
                }

                Servers.Value = updatedList;
            }
            Refreshing = false;
        }

        public ReactiveProperty<List<ServerWrapper>> Servers { get; private set; } = new ReactiveProperty<List<ServerWrapper>>();
        public ReactiveProperty<bool> NoServersFound { get; private set; } = new ReactiveProperty<bool>();
        public bool Refreshing
        {
            get => refreshing;
            set => this.RaiseAndSetIfChanged(ref refreshing, value);
        }

        public void RefreshInstalledStates()
        {
            foreach(ServerWrapper wrapper in Servers.Value)
            {
                wrapper.CheckIfCanPlay();
            }
        }

        public void Dispose()
        {
            Servers.Dispose();
        }
    }
}
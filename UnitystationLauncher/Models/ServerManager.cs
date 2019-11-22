using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Serilog;

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
            NoServersFound = false;
            if (Refreshing) return;

            Servers = new List<ServerWrapper>();
            Refreshing = true;
            Log.Verbose("Refreshing server list...");
            var data = await http.GetStringAsync(Config.apiUrl);
            var serverList = JsonConvert.DeserializeObject<ServerList>(data);
            if (serverList.Servers.Count == 0)
            {
                NoServersFound = true;
            }
            else
            {
                List<ServerWrapper> updatedList = new List<ServerWrapper>();
                foreach (Server s in serverList.Servers)
                {
                    updatedList.Add(new ServerWrapper(s, authManager, installManager));
                }

                Servers = updatedList;
            }
            Refreshing = false;
        }

        [Reactive]
        public List<ServerWrapper> Servers { get; set; } = new List<ServerWrapper>();
        [Reactive]
        public bool NoServersFound { get; private set; }
        public bool Refreshing
        {
            get => refreshing;
            set => this.RaiseAndSetIfChanged(ref refreshing, value);
        }

        public void RefreshInstalledStates()
        {
            foreach(ServerWrapper wrapper in Servers)
            {
                wrapper.CheckIfCanPlay();
            }
        }

        public void Dispose() { }
    }
}
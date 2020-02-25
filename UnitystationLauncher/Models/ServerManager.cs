using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using Reactive.Bindings;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Models
{
    public class ServerManager : ReactiveObject, IDisposable
    {
        private readonly HttpClient http;
        private InstallationManager installManager;
        private AuthManager authManager;
        bool refreshing;
        public Action onRefresh;

        public ReactiveProperty<List<ServerWrapper>> Servers { get; private set; } = new ReactiveProperty<List<ServerWrapper>>();
        public ReactiveProperty<bool> NoServersFound { get; private set; } = new ReactiveProperty<bool>();
        public bool Refreshing
        {
            get => refreshing;
            set => this.RaiseAndSetIfChanged(ref refreshing, value);
        }

        public ServerManager(HttpClient http, AuthManager authManager, InstallationManager installManager)
        {
            this.http = http;
            this.authManager = authManager;
            this.installManager = installManager;
            Servers.Value = new List<ServerWrapper>();
            installManager.InstallListChange = RefreshInstalledStates;
            RefreshServerList();
        }

        public async void RefreshServerList()
        {
            NoServersFound.Value = false;
            if (Refreshing) return;

            var newList = new List<ServerWrapper>();
            Refreshing = true;
            
            if(onRefresh != null)
            {
                onRefresh.Invoke();
            }

            var data = await http.GetStringAsync(Config.apiUrl);
            var serverList = JsonConvert.DeserializeObject<ServerList>(data);

            if (serverList.Servers.Count == 0)
            {
                NoServersFound.Value = true;
            }
            else
            {
                foreach (Server s in serverList.Servers)
                {
                    var index = Servers.Value.FindIndex(x => x.ServerIP == s.ServerIP);
                    if (index != -1)
                    {
                        Servers.Value[index].UpdateDetails(s);
                        newList.Add(Servers.Value[index]);
                    }
                    else
                    {
                        newList.Add(new ServerWrapper(s, authManager, installManager));
                    }
                }
            }

            Refreshing = false;
            Servers.Value = new List<ServerWrapper>(); //for some reason you need to do this
            Servers.Value = newList;

            RefreshInstalledStates();
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
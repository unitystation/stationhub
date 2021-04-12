using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using Reactive.Bindings;

namespace UnitystationLauncher.Models
{
    public class ServerManager : ReactiveObject, IDisposable
    {
        private readonly HttpClient _http;
        private readonly InstallationManager _installManager;
        private readonly AuthManager _authManager;
        bool _refreshing;

        public ReactiveProperty<List<ServerWrapper>> Servers { get; private set; } = new ReactiveProperty<List<ServerWrapper>>();
        public ReactiveProperty<bool> NoServersFound { get; private set; } = new ReactiveProperty<bool>();
        public bool Refreshing
        {
            get => _refreshing;
            set => this.RaiseAndSetIfChanged(ref _refreshing, value);
        }

        public ServerManager(HttpClient http, AuthManager authManager, InstallationManager installManager)
        {
            _http = http;
            _authManager = authManager;
            _installManager = installManager;
            Servers.Value = new List<ServerWrapper>();
            installManager.Installations
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => RefreshInstalledStates());
            RxApp.MainThreadScheduler.Schedule(async () => await RefreshServerList());
        }

        public async Task RefreshServerList()
        {
            NoServersFound.Value = false;
            if (Refreshing)
            {
                return;
            }

            var newList = new List<ServerWrapper>();
            Refreshing = true;

            var data = await _http.GetStringAsync(Config.ApiUrl);
            var serverList = JsonConvert.DeserializeObject<ServerList>(data);

            if (serverList.Servers.Count == 0)
            {
                NoServersFound.Value = true;
            }
            else
            {
                foreach (Server s in serverList.Servers)
                {
                    var index = Servers.Value.FindIndex(x => x.ServerIp == s.ServerIp);
                    if (index != -1)
                    {
                        Servers.Value[index].UpdateDetails(s);
                        newList.Add(Servers.Value[index]);
                    }
                    else
                    {
                        newList.Add(new ServerWrapper(s, _authManager));
                    }
                }
            }

            Refreshing = false;
            Servers.Value = newList;

            RefreshInstalledStates();
        }

        public void RefreshInstalledStates()
        {
            foreach (ServerWrapper wrapper in Servers.Value)
            {
                wrapper.CheckIfCanPlay();
            }
        }

        public void Dispose()
        {
            _installManager.Dispose();
            Servers.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using Reactive.Bindings;
using Serilog;

namespace UnitystationLauncher.Models
{
    public class ServerManager : ReactiveObject, IDisposable
    {
        private readonly HttpClient _http;
        private readonly InstallationManager _installManager;
        private readonly AuthManager _authManager;
        private readonly IDisposable _refreshInstalledStatesSub;
        private bool _refreshing;

        public ServerManager(HttpClient http, AuthManager authManager, InstallationManager installManager)
        {
            _http = http;
            _authManager = authManager;
            _installManager = installManager;
            Servers = Observable.Timer(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(10))
                .SelectMany(_ => GetServerList())
                .Replay(1)
                .RefCount();

            _refreshInstalledStatesSub = installManager.Installations
                .CombineLatest(Servers, (installations, servers) => servers)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(RefreshInstalledStates);
        }

        public IObservable<IReadOnlyList<ServerWrapper>> Servers { get; }
        public ReactiveProperty<bool> NoServersFound { get; } = new ReactiveProperty<bool>();
        public bool Refreshing
        {
            get => _refreshing;
            set => this.RaiseAndSetIfChanged(ref _refreshing, value);
        }

        private List<ServerWrapper> _oldServerList = new List<ServerWrapper>();
        public async Task<IReadOnlyList<ServerWrapper>> GetServerList()
        {
            NoServersFound.Value = false;
            if (Refreshing)
            {
                return _oldServerList;
            }

            var newServerList = new List<ServerWrapper>();
            Refreshing = true;

            var data = await _http.GetStringAsync(Config.ApiUrl);
            Log.Information("Server list fetched");
            var serverList = JsonConvert.DeserializeObject<ServerList>(data);

            if (serverList.Servers.Count == 0)
            {
                NoServersFound.Value = true;
            }
            else
            {
                foreach (Server s in serverList.Servers)
                {
                    var index = _oldServerList.FindIndex(x => x.ServerIp == s.ServerIp);
                    if (index != -1)
                    {
                        _oldServerList[index].UpdateDetails(s);
                        newServerList.Add(_oldServerList[index]);
                    }
                    else
                    {
                        newServerList.Add(new ServerWrapper(s, _authManager));
                    }
                }
            }

            _oldServerList = newServerList;

            Refreshing = false;
            return newServerList;
        }

        void RefreshInstalledStates(IReadOnlyList<ServerWrapper> serverList)
        {
            foreach (ServerWrapper wrapper in serverList)
            {
                wrapper.CheckIfCanPlay();
            }
        }

        public void Dispose()
        {
            _installManager.Dispose();
            _refreshInstalledStatesSub.Dispose();
        }
    }
}
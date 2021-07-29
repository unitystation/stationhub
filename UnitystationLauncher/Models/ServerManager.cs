using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.ViewModels;

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

        public IObservable<IReadOnlyList<ServerViewModel>> Servers { get; }

        public bool Refreshing
        {
            get => _refreshing;
            set => this.RaiseAndSetIfChanged(ref _refreshing, value);
        }

        private List<ServerViewModel> _oldServerList = new List<ServerViewModel>();

        private async Task<IReadOnlyList<ServerViewModel>> GetServerList()
        {
            if (Refreshing)
            {
                return _oldServerList;
            }

            var newServerList = new List<ServerViewModel>();
            Refreshing = true;

            var data = await _http.GetStringAsync(Config.ApiUrl);
            Log.Information("Server list fetched");
            var serverList = JsonConvert.DeserializeObject<ServerList>(data);

            foreach (Server s in serverList.Servers)
            {
                var index = _oldServerList.FindIndex(x => x.Server.ServerIp == s.ServerIp);
                if (index != -1)
                {
                    _oldServerList[index].UpdateDetails(s);
                    newServerList.Add(_oldServerList[index]);
                }
                else
                {
                    if (!HasTrustedUrlSource(s))
                    {
                        Log.Warning($"Server: {s.ServerName} has untrusted download URL and has been omitted in " +
                                    "the server list!");
                        continue;
                    }
                    newServerList.Add(new ServerViewModel(s, _authManager));
                }
            }

            _oldServerList = newServerList;

            Refreshing = false;
            return newServerList;
        }

        private bool HasTrustedUrlSource(Server server)
        {
            const string trustedHost = "unitystationfile.b-cdn.net";
            var urls = new[] { server.WinDownload, server.OsxDownload, server.LinuxDownload };
            foreach (var url in urls)
            {
                if (string.IsNullOrEmpty(url))
                {
                    return false;
                }
                var uri = new Uri(url);
                if (uri.Scheme != "https" && uri.Host != trustedHost)
                {
                    return false;
                }
            }
            return true;
        }

        void RefreshInstalledStates(IReadOnlyList<ServerViewModel> serverList)
        {
            foreach (ServerViewModel wrapper in serverList)
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
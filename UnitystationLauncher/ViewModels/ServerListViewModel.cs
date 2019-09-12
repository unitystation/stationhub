using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class ServerListViewModel : ViewModelBase
    {
        ServerWrapper[] servers;
        ServerWrapper? selectedServer;
        int refreshFrequency = 5000;
        bool refreshing;

        public ServerListViewModel()
        {
            UpdateServers();
        }

        public ServerWrapper[] Servers
        {
            get => servers;
            set => this.RaiseAndSetIfChanged(ref servers, value);
        }

        public ServerWrapper? SelectedServer
        {
            get => selectedServer;
            set => this.RaiseAndSetIfChanged(ref selectedServer, value);
        }

        public bool Refreshing
        {
            get => refreshing;
            set => this.RaiseAndSetIfChanged(ref refreshing, value);
        }

        private async Task UpdateServers()
        {
            using var httpClient = new HttpClient();
            while (true)
            {
                Refreshing = true;
                Log.Debug("Refreshing server list...");
                var response = await httpClient.GetStringAsync("https://api.unitystation.org/serverlist");
                var servers = JsonConvert.DeserializeObject<ServerList>(response).Servers;

                if (!(Servers?.SequenceEqual(servers) ?? false))
                {
                    Servers = servers.Select(s => new ServerWrapper(s)).ToArray();
                    Log.Debug("Server list changed");
                }
                Refreshing = false;
                Log.Debug("Server list refreshed");

                await Task.Delay(refreshFrequency);
            }
        }
    } 
}
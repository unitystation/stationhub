using Newtonsoft.Json;
using ReactiveUI;
using Serilog;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        readonly HttpClient http;
        ServerWrapper[] servers;
        ServerWrapper? selectedServer;
        int refreshFrequency = 5000;
        bool refreshing;

        public ServersPanelViewModel(HttpClient http)
        {
            this.http = http;
            UpdateServers();
        }

        public override string Name => "Servers";
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

        private async void UpdateServers()
        {
            while (true)
            {
                Refreshing = true;
                Log.Verbose("Refreshing server list...");
                var response = await http.GetStringAsync(Config.apiUrl);
                var servers = JsonConvert.DeserializeObject<ServerList>(response).Servers;

                if (!(Servers?.SequenceEqual(servers) ?? false))
                {
                    Servers = servers.Select(s => new ServerWrapper(s)).ToArray();
                    Log.Debug("Server list changed");
                }
                Refreshing = false;
                Log.Verbose("Server list refreshed");

                await Task.Delay(refreshFrequency);
            }
        }
    }
}

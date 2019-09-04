using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ReactiveUI;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class ServerListViewModel : ViewModelBase
    {
        Server[] servers;
        Server selectedServer;
        int refreshFrequency = 5000;
        bool refreshing;

        public ServerListViewModel()
        {
            UpdateServers();
        }

        public Server[] Servers
        {
            get => servers;
            set => this.RaiseAndSetIfChanged(ref servers, value);
        }

        public Server SelectedServer
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
            using (var httpClient = new HttpClient())
            {
                while(true)
                {
                    Refreshing = true;
                    var response = await httpClient.GetStringAsync("https://api.unitystation.org/serverlist");
                    var servers = JsonConvert.DeserializeObject<ServerList>(response).Servers;
                    Refreshing = false;
                    if(!(Servers?.SequenceEqual(servers) ??false)){
                        Servers = servers;
                    }

                    await Task.Delay(refreshFrequency);
                }
            } 
        }
    } 
}
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
        ServerWrapper? selectedServer;

        public ServersPanelViewModel(ServerManager serverManager)
        {
            ServerManager = serverManager;
        }

        public override string Name => "Servers";
        
        public ServerManager ServerManager { get; }

        public ServerWrapper? SelectedServer
        {
            get => selectedServer;
            set => this.RaiseAndSetIfChanged(ref selectedServer, value);
        }
    }
}

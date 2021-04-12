using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UnitystationLauncher.Models;
using ReactiveUI;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        public ServerManager ServerManager { get; }
        public NewsViewModel NewsViewModel { get; }

        public override string Name => "Servers";

        public ServersPanelViewModel(
            ServerManager serverManager,
            NewsViewModel newsViewModel)
        {
            ServerManager = serverManager;
            NewsViewModel = newsViewModel;
            Observable.Timer(TimeSpan.FromSeconds(10))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(async _ => await ServerManager.RefreshServerList());
        }

        public async Task OnRefresh()
        {
            await ServerManager.RefreshServerList();
            await NewsViewModel.GetPullRequests();
        }
    }
}

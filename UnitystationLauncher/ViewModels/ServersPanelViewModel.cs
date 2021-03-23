using System;
using UnitystationLauncher.Models;
using System.Timers;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        public ServerManager ServerManager { get; }
        public NewsViewModel NewsViewModel { get; }

        Timer _stateTimer;

        public override string Name => "Servers";

        public ServersPanelViewModel(
            ServerManager serverManager,
            NewsViewModel newsViewModel)
        {
            ServerManager = serverManager;
            NewsViewModel = newsViewModel;
            _stateTimer = new Timer(10000);
            _stateTimer.Elapsed += OnTimedEvent;
        }

        public void OnRefresh()
        {
            ServerManager.RefreshServerList();
            NewsViewModel.GetPullRequests();
        }

        public void OnFocused()
        {
            _stateTimer.AutoReset = true;
            _stateTimer.Enabled = true;
        }

        public void OnUnFocused()
        {
            _stateTimer.Enabled = false;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            ServerManager.RefreshServerList();
        }
    }
}

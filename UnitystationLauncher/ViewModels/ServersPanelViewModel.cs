using ReactiveUI;
using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using UnitystationLauncher.Models;
using System.Reactive;
using System.Timers;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        ServerWrapper? selectedServer;
        public ServerManager ServerManager { get; }
        public NewsViewModel NewsViewModel { get; }

        Timer stateTimer;

        public override string Name => "Servers";
        public override IBitmap Icon => new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>()
            .Open(new Uri("avares://StationHub/Assets/servericon.png")));


        public ServerWrapper? SelectedServer
        {
            get => selectedServer;
            set {
                if (value == null)
                {
                    return;
                }

                this.RaiseAndSetIfChanged(ref selectedServer, value);
                foreach(var s in ServerManager.Servers.Value)
                {
                    if(value != s)
                    {
                        s.IsSelected.Value = false;
                    } else
                    {
                        s.IsSelected.Value = true;
                    }
                }
            }
        }

        public ReactiveCommand<Unit, Unit> Refresh { get; }

        public ServersPanelViewModel(
            ServerManager serverManager,
            NewsViewModel newsViewModel)
        {
            this.ServerManager = serverManager;
            this.NewsViewModel = newsViewModel;

            Refresh = ReactiveCommand.Create(OnRefresh, null);
        }

        private void OnRefresh()
        {
            ServerManager.RefreshServerList();
            NewsViewModel.GetPullRequests();
        }

        public void OnFocused()
        {
            stateTimer = new Timer(10000);
            stateTimer.Elapsed += OnTimedEvent;
            stateTimer.AutoReset = true;
            stateTimer.Enabled = true;
        }

        public void OnUnFocused()
        {
            stateTimer.Dispose();
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            ServerManager.RefreshServerList();
        }
    }
}

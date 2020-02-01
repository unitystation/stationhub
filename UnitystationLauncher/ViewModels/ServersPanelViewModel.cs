using ReactiveUI;
using System;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using UnitystationLauncher.Models;
using System.Reactive;

namespace UnitystationLauncher.ViewModels
{
    public class ServersPanelViewModel : PanelBase
    {
        ServerWrapper? selectedServer;
        public ServerManager ServerManager { get; }
        public ServersPanelViewModel(
            ServerManager serverManager)
        {
            this.ServerManager = serverManager;
            Refresh = ReactiveCommand.Create(ServerManager.RefreshServerList, null);
        }

        public override string Name => "Servers";
        public override IBitmap Icon => new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>()
            .Open(new Uri("avares://StationHub/Assets/servericon.png")));
        

        public ServerWrapper? SelectedServer
        {
            get => selectedServer;
            set => this.RaiseAndSetIfChanged(ref selectedServer, value);
        }

        public ReactiveCommand<Unit, Unit> Refresh { get; }
    }
}

using Avalonia;
using Avalonia.Platform;
using ReactiveUI;
using Avalonia.Media.Imaging;
using System;
using System.Reactive;
using System.Diagnostics;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher.ViewModels
{
    public class NewsPanelViewModel : PanelBase
    {
        public override string Name => "News";
        
        public override bool IsEnabled => true;

        ViewModelBase _changelog;

        Bitmap _backGroundImage;

        public ReactiveCommand<Unit, Unit> OpenMainSite { get; }
        public ReactiveCommand<Unit, Unit> OpenPatreon { get; }
        public ReactiveCommand<Unit, Unit> OpenGameIssues { get; }
        public ReactiveCommand<Unit, Unit> OpenLauncherIssues { get; }
        public ReactiveCommand<Unit, Unit> OpenDiscordInvite { get; }

        public Bitmap BackGroundImage
        {
            get => _backGroundImage;
            set => this.RaiseAndSetIfChanged(ref _backGroundImage, value);
        }

        public ViewModelBase Changelog
        {
            get => _changelog;
            set => this.RaiseAndSetIfChanged(ref _changelog, value);
        }

        public NewsPanelViewModel(ChangelogViewModel changelog)
        {
            _changelog = changelog;
            
            OpenMainSite = ReactiveCommand.Create(() => OpenLink(Config.MainSiteUrl));
            OpenPatreon = ReactiveCommand.Create(() => OpenLink(Config.PatreonUrl));
            OpenGameIssues = ReactiveCommand.Create(() => OpenLink(Config.GameIssuesUrl));
            OpenLauncherIssues = ReactiveCommand.Create(() => OpenLink(Config.LauncherIssuesUrl));
            OpenDiscordInvite = ReactiveCommand.Create(() => OpenLink(Config.DiscordInviteUrl));

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            _backGroundImage = new Bitmap(assets?.Open(new Uri("avares://StationHub/Assets/bgnews.png")));
        }

        private static void OpenLink(string url)
        {
            ProcessStartInfo psi = new()
            {
                FileName = url,
                UseShellExecute = true
            };
            
            Process.Start(psi);
        }
    }
}

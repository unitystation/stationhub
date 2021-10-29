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
        public override int Width => 60;

        ViewModelBase _changelog;

        Bitmap _backGroundImage;

        public ReactiveCommand<Unit, Unit> OpenSite { get; }
        public ReactiveCommand<Unit, Unit> OpenSupport { get; }
        public ReactiveCommand<Unit, Unit> OpenReport { get; }

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
            OpenSite = ReactiveCommand.Create(OpenUriSite);
            OpenReport = ReactiveCommand.Create(OpenUriReport);
            OpenSupport = ReactiveCommand.Create(OpenUriSupport);

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            _backGroundImage = new Bitmap(assets.Open(new Uri("avares://StationHub/Assets/bgnews.png")));
        }

        private void OpenUriSite()
        {
            var psi = new ProcessStartInfo
            {
                FileName = Config.SiteUrl,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void OpenUriReport()
        {
            var psi = new ProcessStartInfo
            {
                FileName = Config.ReportUrl,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        private void OpenUriSupport()
        {
            var psi = new ProcessStartInfo
            {
                FileName = Config.SupportUrl,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

    }
}

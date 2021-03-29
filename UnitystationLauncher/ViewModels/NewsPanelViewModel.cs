using Avalonia;
using Avalonia.Platform;
using ReactiveUI;
using Avalonia.Media.Imaging;
using System;
using System.Reactive;
using System.Diagnostics;
using UnitystationLauncher.Models;
using System.Collections.Generic;
using System.IO;

namespace UnitystationLauncher.ViewModels
{
    public class NewsPanelViewModel : PanelBase
    {
        public override string Name => "News";

        ViewModelBase _news;

        Bitmap _backGroundImage;

        public ReactiveCommand<Unit, Unit> OpenSite { get; }
        public ReactiveCommand<Unit, Unit> OpenSupport { get; }
        public ReactiveCommand<Unit, Unit> OpenReport { get; }

        public Bitmap BackGroundImage
        {
            get => _backGroundImage;
            set => this.RaiseAndSetIfChanged(ref _backGroundImage, value);
        }

        public ViewModelBase News
        {
            get => _news;
            set => this.RaiseAndSetIfChanged(ref _news, value);
        }

        public NewsPanelViewModel(NewsViewModel news)
        {
            _news = news;
            OpenSite = ReactiveCommand.Create(OpenUriSite);
            OpenReport = ReactiveCommand.Create(OpenUriReport);
            OpenSupport = ReactiveCommand.Create(OpenUriSupport);

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            _backGroundImage = new Bitmap(assets.Open(new Uri("avares://StationHub/Assets/bgnews.png")));
        }

        public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive)
        {
            List<String> filesFound = new List<String>();
            var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (var filter in filters)
            {
                filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
            }
            return filesFound.ToArray();
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

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

        ViewModelBase news;

        Bitmap backGroundImage;

        private List<string> Videos;

        public ReactiveCommand<Unit,Unit> OpenSite { get; }
        public ReactiveCommand<Unit,Unit> OpenSupport { get; }
        public ReactiveCommand<Unit,Unit> OpenReport { get; }

        public ReactiveCommand<Unit,Unit> NextVideo { get; }
        public ReactiveCommand<Unit,Unit> PastVideo { get; }

        public Bitmap BackGroundImage 
        { 
            get => backGroundImage; 
            set => this.RaiseAndSetIfChanged(ref backGroundImage, value); 
        }

        public ViewModelBase News
        {
            get => news;
            set => this.RaiseAndSetIfChanged(ref news, value);
        }

        public NewsPanelViewModel(NewsViewModel news)
        {
            News = news;
            OpenSite = ReactiveCommand.Create(OpenUriSite, null);
            OpenReport = ReactiveCommand.Create(OpenUriReport, null);
            OpenSupport = ReactiveCommand.Create(OpenUriSupport, null);

            NextVideo = ReactiveCommand.Create(Next, null);
            PastVideo = ReactiveCommand.Create(Back, null);

            LoadImage(new Uri("avares://StationHub/Assets/bgnews.png"));
        }
        
        private void LoadImage(Uri url)
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            BackGroundImage = new Bitmap(assets.Open(url));
        }

        private void Next()
        {

        }

        private void Back()
        {

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

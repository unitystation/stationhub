using System;
using ReactiveUI;

namespace UnitystationLauncher.Models
{
    public class Download : ReactiveObject
    {
        public string DownloadUrl { get; }
        public string InstallPath { get; }
        public string ForkName { get; }
        public int BuildVersion { get; }

        private long _size;
        public long Size
        {
            get => _size;
            set
            {
                this.RaiseAndSetIfChanged(ref _size, value);
                this.RaisePropertyChanged(nameof(Progress));
            }
        }

        private bool _active;
        public bool Active
        {
            get => _active;
            set => this.RaiseAndSetIfChanged(ref _active, value);
        }

        private long _downloaded;
        public long Downloaded
        {
            get => _downloaded;
            set
            {
                this.RaiseAndSetIfChanged(ref _downloaded, value);
                this.RaisePropertyChanged(nameof(Progress));
            }
        }

        public int Progress => (int)(Downloaded * 100 / Math.Max(1, Size));

        public Download(string url, string installationPath, string forkName, int buildVersion)
        {
            DownloadUrl = url;
            InstallPath = installationPath;
            ForkName = forkName;
            BuildVersion = buildVersion;
        }
    }
}
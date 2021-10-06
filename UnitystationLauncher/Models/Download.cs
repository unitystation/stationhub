using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Serilog;

namespace UnitystationLauncher.Models
{
    public class Download : ReactiveObject
    {
        private readonly string _url;
        private readonly string _installationPath;
        private long _size;
        private long _downloaded;
        private bool _active;
        private int _progress;

        public Download(string url, string installationPath)
        {
            _url = url;
            _installationPath = installationPath;

            this.WhenAnyValue(d => d.Progress).DistinctUntilChanged()
                .Subscribe(p => Log.Information("Progress: {Progress}", p));
        }

        public string Url => _url;

        public string InstallationPath => _installationPath;

        public long Size
        {
            get => _size;
            set => this.RaiseAndSetIfChanged(ref _size, value);
        }

        public long Downloaded
        {
            get => _downloaded;
            set => this.RaiseAndSetIfChanged(ref _downloaded, value);
        }

        public bool Active
        {
            get => _active;
            set => this.RaiseAndSetIfChanged(ref _active, value);
        }

        public int Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public (string, int) ForkAndVersion => (ForkName, BuildVersion);
        public string ForkName => Installation.GetForkName(InstallationPath);
        public int BuildVersion => Installation.GetBuildVersion(InstallationPath);
        public string RelativeInstallationPath => Path.GetRelativePath(Environment.CurrentDirectory, InstallationPath);

        public async Task StartAsync()
        {
            Log.Information("Download requested...");
            Log.Information("Installation path: \"{Path}\"", InstallationPath);
            Log.Information("Download URL: \"{Url}\"", this?.Url);

            if (!CanStart())
            {
                return;
            }

            Active = true;
            try
            {
                Log.Information("Download started...");
                var webRequest = WebRequest.Create(Url);
                var webResponse = await webRequest.GetResponseAsync();
                await using var responseStream = webResponse.GetResponseStream();
                if (responseStream == null)
                {
                    Log.Error("Could not download from server");
                    return;
                }

                Log.Information("Download connection established");
                await using var progStream = new ProgressStream(responseStream);
                Size = webResponse.ContentLength;

                progStream.Progress
                    .Select(p => (int)(p * 100 / Size))
                    .DistinctUntilChanged()
                    .Do(p => Log.Information("Progress: {Percentage}", p))
                    .Subscribe(p => { Progress = p; });

                await Task.Run(() =>
                {
                    Log.Information("Extracting...");
                    try
                    {
                        var archive = new ZipArchive(progStream);
                        archive.ExtractToDirectory(InstallationPath, true);

                        Log.Information("Download completed");
                        Installation.MakeExecutableExecutable(InstallationPath);
                    }
                    catch
                    {
                        Log.Information("Extracting stopped");
                    }
                });
            }
            finally
            {
                Active = false;
            }
        }

        public bool CanStart()
        {
            if (Directory.Exists(InstallationPath))
            {
                Log.Information("Installation path already occupied");
                return false;
            }

            return true;
        }
    }
}
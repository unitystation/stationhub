using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Humanizer;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Infrastructure;

namespace UnitystationLauncher.Models
{
    public class Download : ReactiveObject
    {
        private readonly string _url;
        private readonly string _installationPath;
        private long _size;
        private bool _active;
        private int _progress;

        public Download(string url, string installationPath)
        {
            _url = url;
            _installationPath = installationPath;
        }

        public string Url => _url;

        public string InstallationPath => _installationPath;

        public long Size
        {
            get => _size;
            set => this.RaiseAndSetIfChanged(ref _size, value);
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

        public async Task StartAsync(HttpClient http)
        {
            Log.Information("Download requested, Installation Path '{Path}', Url '{Url}'", InstallationPath, Url);

            if (!CanStart())
            {
                return;
            }

            Active = true;
            try
            {
                Log.Information("Download started...");
                var request = await http.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead);
                await using var responseStream = await request.Content.ReadAsStreamAsync();
                if (responseStream == null)
                {
                    Log.Error("Could not download from server");
                    return;
                }

                Log.Information("Download connection established");
                await using var progStream = new ProgressStream(responseStream);
                Size = request.Content.Headers.ContentLength ??
                       throw new NullReferenceException(nameof(request.Content.Headers.ContentLength));

                using var logProgDisposable = LogProgress(progStream);

                progStream.Progress
                    .Select(p => (int)(p * 100 / Size))
                    .DistinctUntilChanged()
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

        private IDisposable LogProgress(ProgressStream progStream)
        {
            var lastPosition = 0L;
            var lastTime = DateTime.Now;

            return progStream.Progress
                .Subscribe(pos =>
                {
                    var time = DateTime.Now;
                    var deltaPos = pos - lastPosition;
                    var deltaTime = time - lastTime;
                    var deltaPercentage = deltaPos * 100 / Size;
                    var percentage = pos * 100 / Size;

                    if (pos != Size)
                    {
                        if (deltaPercentage < 25 && deltaTime.TotalSeconds < .25)
                        {
                            return;
                        }
                    }

                    var speed = deltaPos.Bytes().Per(deltaTime);
                    Log.Information("Progress: {ProgressPercent}%, Download speed = {DownloadSpeed}",
                        percentage,
                        speed.Humanize("#.##"));

                    lastPosition = pos;
                    lastTime = time;
                });
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
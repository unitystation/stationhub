using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Humanizer;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Exceptions;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Models
{
    public class Download : ReactiveObject
    {
        private readonly string _url;
        private readonly string _installationPath;
        private long _size;
        private bool _active;
        private long _downloaded;
        private readonly IPreferencesService _preferencesService;
        private readonly IEnvironmentService _environmentService;

        public Download(string url, string installationPath, IPreferencesService preferencesService, IEnvironmentService environmentService)
        {
            _url = url;
            _installationPath = installationPath;
            _preferencesService = preferencesService;
            _environmentService = environmentService;
        }

        public string Url => _url;

        public string InstallationPath => _installationPath;

        public long Size
        {
            get => _size;
            set
            {
                this.RaiseAndSetIfChanged(ref _size, value);
                this.RaisePropertyChanged(nameof(Progress));
            }
        }

        public bool Active
        {
            get => _active;
            set => this.RaiseAndSetIfChanged(ref _active, value);
        }

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

        public (string, int) ForkAndVersion => (ForkName, BuildVersion);
        public string ForkName => Installation.GetForkName(InstallationPath, _preferencesService);
        public int BuildVersion => Installation.GetBuildVersion(InstallationPath, _preferencesService);

        // TODO: Move HTTP calls here to the download service, needed for unit testing
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
                HttpResponseMessage request = await http.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead);
                await using Stream responseStream = await request.Content.ReadAsStreamAsync();
                if (responseStream == null)
                {
                    Log.Error("Could not download from server");
                    return;
                }

                Log.Information("Download connection established");
                await using var progStream = new ProgressStream(responseStream);
                Size = request.Content.Headers.ContentLength ??
                       throw new ContentLengthNullException(Url);

                using var logProgDisposable = LogProgress(progStream);

                using var progDisposable = progStream.Progress
                    .Subscribe(p => { Downloaded = p; });

                await Task.Run(() =>
                {
                    Log.Information("Extracting...");
                    try
                    {
                        var archive = new ZipArchive(progStream);
                        // TODO: Enable extraction cancelling
                        archive.ExtractToDirectory(InstallationPath, true);

                        Log.Information("Download completed");
                        Installation.MakeExecutableExecutable(InstallationPath, _environmentService);
                    }
                    catch
                    {
                        Log.Information("Extracting stopped");
                    }
                });
            }
            catch (Exception e)
            {
                Log.Error(e, "Failed to download Url '{Url}'", Url);
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
                    Log.Information("Progress: {ProgressPercent}%, Speed = {DownloadSpeed}",
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
using Serilog;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;

namespace UnitystationLauncher.Models
{
    public class Download
    {
        public string Url { get; }
        public string InstallationPath { get; }
        public Subject<int> Progress { get; set; } = new Subject<int>();
        public float Speed { get; set; }
        public long Downloaded { get; set; }
        public long Size { get; set; }
        public long Time { get; set; }

        public Download(string url, string installationPath)
        {
            Url = url;
            InstallationPath = installationPath;
        }

        public (string, int) Key => (ForkName, BuildVersion);
        public string ForkName => Installation.GetForkName(InstallationPath);
        public int BuildVersion => Installation.GetBuildVersion(InstallationPath);
        public string RelativeInstallationPath => Path.GetRelativePath(Environment.CurrentDirectory, InstallationPath);

        public async Task Start()
        {
            Log.Information("Download requested...");
            Log.Information("Installation path: \"{Path}\"", InstallationPath);

            if (Directory.Exists(InstallationPath))
            {
                Log.Information("Installation path already occupied");
                return;
            }

            Log.Information("Download URL: \"{Url}\"", Url);

            Log.Information("Download started...");
            var webRequest = WebRequest.Create(Url);
            var webResponse = await webRequest.GetResponseAsync();
            var responseStream = webResponse.GetResponseStream();
            if (responseStream == null)
            {
                Log.Error("Could not get responseStream");
                return;
            }
            Log.Information("Download connection established");
            await using var progStream = new ProgressStream(responseStream);
            var length = webResponse.ContentLength;
            progStream.Progress
                .Select(p => (int)(p * 100 / length))
                .DistinctUntilChanged()
                .Subscribe(p =>
                {
                    Progress.OnNext(p);
                    Log.Information("Progress: {Prog}", p);
                });

            await Task.Run(() =>
            {
                Log.Information("Extracting...");
                var archive = new ZipArchive(progStream);

                archive.ExtractToDirectory(InstallationPath);
                Log.Information("Download completed");
            });
        }
    }
}

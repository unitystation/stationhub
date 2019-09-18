using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using UnitystationLauncher.Infrastructure;

namespace UnitystationLauncher.Models
{
    class Download
    {
        public string Url { get; }
        public string InstallationPath { get; }
        public Subject<int> Progress { get; set; }
        public float Speed { get; set; }
        public long Downloaded { get; set; }
        public long Size { get; set; }
        public long Time { get; set; }

        public Download(string url, string installationPath)
        {
            Url = url;
            InstallationPath = installationPath;
        }

        public async Task Start()
        {
            Log.Information("Download requested...");
            Log.Information("Installation path: \"{Path}\"", InstallationPath);

            if (Directory.Exists(InstallationPath))
            {
                Log.Information("Installation path already occupied");
                return;
            }

            Log.Information("Download URL: \"{URL}\"", Url);

            if (Url is null)
            {
                throw new Exception("OS download is null");
            }

            Log.Information("Download started...");
            var webRequest = WebRequest.Create(Url);
            var webResponse = await webRequest.GetResponseAsync();
            var responseStream = webResponse.GetResponseStream();
            Log.Information("Download connection established");
            using var progStream = new ProgressStream(responseStream);
            var length = webResponse.ContentLength;
            progStream.Progress
                .Select(p => (int)(p * 100 / length))
                .DistinctUntilChanged()
                .Subscribe(p => {
                    Progress.OnNext(p);
                    Log.Information("Progress: {prog}", p);
                });

            await Task.Run(() =>
            {
                Log.Information("Extracting...");
                var archive = new ZipArchive(progStream);
                archive.ExtractToDirectory(InstallationPath);
                Log.Information("Download completed");
            });
        }

        public async Task Cancel() { }

        public async Task Stop() => throw new NotSupportedException($"Stopping is not supported, try {nameof(Cancel)} instead");
    }
}

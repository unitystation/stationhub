using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace UnitystationLauncher.Models
{
    public class Download
    {
        readonly HttpClient http;
        public string Url { get; }
        public string InstallationPath { get; }
        public Subject<int>? Progress { get; set; }
        public float Speed { get; set; }
        public long Downloaded { get; set; }
        public long Size { get; set; }
        public long Time { get; set; }

        public Download(string url, string installationPath, HttpClient http)
        {
            Url = url;
            InstallationPath = installationPath;
            this.http = http;
            Progress = new Subject<int>();
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

            Log.Information("Download URL: \"{URL}\"", Url);

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
                SetPermissions(InstallationPath);
            });
        }

        public async Task Cancel() { }

        public async Task Stop() => throw new NotSupportedException($"Stopping is not supported, try {nameof(Cancel)} instead");

        private void SetPermissions(string path)
        {
            try
            {
                ProcessStartInfo startInfo;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    startInfo = new ProcessStartInfo("chmod", $"744 {path}");
                    startInfo.UseShellExecute = true;
                    var process = new Process();
                    process.StartInfo = startInfo;

                    process.Start();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "An exception occurred when setting the permissions");
            }
        }
    }
}

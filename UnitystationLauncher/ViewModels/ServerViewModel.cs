using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Reactive.Bindings;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Models;

#if FLATPAK
using System.Text.RegularExpressions;
#endif

namespace UnitystationLauncher.ViewModels
{
    public class ServerViewModel : ViewModelBase, IDisposable
    {
        private bool _downloading;
        private readonly AuthManager _authManager;

        // Ping does not work in sandboxes so we have to reconstruct its functionality in that case.
        // Surprisingly, this is basically what that does. Looks for your system's ping tool and parses its output.
#if FLATPAK
	    private readonly Process _pingSender;
#else
        private readonly Ping _pingSender;
#endif

        public ServerViewModel(Server server, Installation? installation, AuthManager authManager)
        {
            Server = server;
            Installation = installation;
            _authManager = authManager;
#if FLATPAK
	        _pingSender = new Process();
#else
            _pingSender = new Ping();
            _pingSender.PingCompleted += PingCompletedCallback;
#endif
            UpdateDetails(server);

            if (!Directory.Exists(Config.InstallationsPath))
            {
                Directory.CreateDirectory(Config.InstallationsPath);
            }
        }

        public Server Server { get; }
        public Installation? Installation { get; set; }
        public ReactiveProperty<string> RoundTrip { get; } = new ReactiveProperty<string>();
        public Subject<int> Progress { get; set; } = new Subject<int>();

        public bool Installed => Installation != null;

        private string? DownloadUrl => Server.DownloadUrl;

        private string InstallationPath => Server.InstallationPath;

        public bool Downloading
        {
            get => _downloading;
            set => this.RaiseAndSetIfChanged(ref _downloading, value);
        }

        public void UpdateDetails(Server server)
        {
            Server.ServerName = server.ServerName;
            Server.CurrentMap = server.CurrentMap;
            Server.GameMode = server.GameMode;
            Server.InGameTime = server.InGameTime;
            Server.PlayerCount = server.PlayerCount;
            Server.WinDownload = server.WinDownload;
            Server.OsxDownload = server.OsxDownload;
            Server.LinuxDownload = server.LinuxDownload;
#if FLATPAK
            _pingSender.StartInfo.UseShellExecute = false;
            _pingSender.StartInfo.RedirectStandardOutput = true;
            _pingSender.StartInfo.RedirectStandardError = true;
            _pingSender.StartInfo.FileName = "ping";
            _pingSender.StartInfo.Arguments = $"{Server.ServerIp} -c 1";
            _pingSender.Start();
            StreamReader reader = _pingSender.StandardOutput;
            string e = reader.ReadToEnd();
            Regex pingReg = new Regex(@"time=(.*?)\ ");
            var pingTrunc = pingReg.Match(e);
            var pingOut = pingTrunc.Groups[1].ToString();
            RoundTrip.Value = $"{pingOut}ms";
            _pingSender.WaitForExit();
#else
            _pingSender.SendAsync(Server.ServerIp, 7);
#endif
        }

        private void PingCompletedCallback(object sender, PingCompletedEventArgs e)
        {
            // If an error occurred, display the exception to the user.  
            if (e.Error != null)
            {
                Log.Error(e.Error, "Ping failed");
                return;
            }

            var tripTime = e.Reply.RoundtripTime;
            RoundTrip.Value = tripTime == 0 ? "null" : $"{e.Reply.RoundtripTime}ms";
        }

        public async Task Download()
        {
            Log.Information("Download requested...");
            Log.Information("Installation path: \"{Path}\"", InstallationPath);

            if (Directory.Exists(InstallationPath))
            {
                Log.Information("Installation path already occupied");
                return;
            }

            Log.Information("Download URL: \"{Url}\"", DownloadUrl);

            if (DownloadUrl is null)
            {
                Log.Error("OS download is null");
                return;
            }

            Downloading = true;
            Log.Information("Download started...");
            var webRequest = WebRequest.Create(DownloadUrl);
            var webResponse = await webRequest.GetResponseAsync();
            await using var responseStream = webResponse.GetResponseStream();
            if (responseStream == null)
            {
                Log.Error("Could not download from server");
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
                    Log.Information("Progress: {Percentage}", p);
                });

            await Task.Run(() =>
            {
                Log.Information("Extracting...");
                try
                {
                    var archive = new ZipArchive(progStream);
                    archive.ExtractToDirectory(InstallationPath, true);

                    Log.Information("Download completed");
                    Installation.MakeExecutableExecutable(InstallationPath);
                    Downloading = false;
                }
                catch
                {
                    Log.Information("Extracting stopped");
                }
            });
        }

        public void Start()
        {
            Installation?.Start(IPAddress.Parse(Server.ServerIp), (short)Server.ServerPort, _authManager.CurrentRefreshToken, _authManager.Uid);
        }

        public void Dispose()
        {
            _pingSender.Dispose();
        }
    }
}
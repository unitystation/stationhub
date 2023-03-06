using System;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Humanizer;
using Reactive.Bindings;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services;

namespace UnitystationLauncher.ViewModels
{
    public class ServerViewModel : ViewModelBase
    {
        public Server Server { get; }
        public Installation? Installation { get; set; }
        public Download? Download { get; set; }
        public ReactiveProperty<string> RoundTrip { get; }
        public IObservable<string> DownloadedAmount { get; }
        public IObservable<string> DownloadSize { get; }
        public bool Installed => Installation != null;
        public IObservable<bool> Downloading =>
            Download?.WhenAnyValue(d => d.Active) ?? Observable.Return(false);

        private readonly AuthService _authService;

        public ServerViewModel(Server server, Installation? installation, Download? download, AuthService authService)
        {
            Server = server;
            Installation = installation;
            Download = download;
            _authService = authService;
            RoundTrip = new();

            try
            {
#if FLATPAK
                Task.Run(FlatpakGetPingTime);
#else
                using Ping ping = new();
                ping.PingCompleted += PingCompletedCallback;
                ping.SendAsync(Server.ServerIp, null);
#endif
            }
            catch (ArgumentException e)
            {
                Log.Error("Error: {Error}", $"Invalid IP address when trying to ping server: {e.Message}");
                RoundTrip.Value = "Error";
            }

            DownloadedAmount = (
                    Download?.WhenAnyValue(d => d.Downloaded)
                    ?? Observable.Return(0L)
                )
                .Select(p => p.Bytes().ToString("# MB"));

            DownloadSize = (
                    Download?.WhenAnyValue(d => d.Size)
                    ?? Observable.Return(0L)
                )
                .Select(p => p.Bytes().ToString("# MB"));
        }

        private void PingCompletedCallback(object _, PingCompletedEventArgs eventArgs)
        {
            // If an error occurred, display the exception to the user.  
            if (eventArgs.Error != null)
            {
                Log.Error(eventArgs.Error, "Ping failed");
                return;
            }

            long? tripTime = eventArgs.Reply?.RoundtripTime;
            RoundTrip.Value = tripTime.HasValue ? $"{tripTime.Value}ms" : "null";
        }

        public void LaunchGame()
        {
            Installation?.LaunchWithArgs(Server.ServerIp, (short)Server.ServerPort,
                _authService.CurrentRefreshToken, _authService.Uid);
        }

        // Ping does not work in the Flatpak sandbox so we have to reconstruct its functionality in that case.
        // Surprisingly, this is basically what that does. Looks for your system's ping tool and parses its output.
        private async Task FlatpakGetPingTime()
        {
            using Process pingSender = new()
            {
                StartInfo = new()
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    FileName = "ping",
                    Arguments = $"{Server.ServerIp} -c 1"
                }
            };

            pingSender.Start();
            StreamReader reader = pingSender.StandardOutput;
            string pingRawOutput = await reader.ReadToEndAsync();
            Match matchedPingOutput = new Regex(@"time=(.*?)\ ").Match(pingRawOutput);
            string pingOut = matchedPingOutput.Groups[1].ToString();
            RoundTrip.Value = $"{pingOut}ms";
            await pingSender.WaitForExitAsync();
        }
    }
}

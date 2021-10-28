using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using Humanizer;
using Reactive.Bindings;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services;

#if FLATPAK
using System.Text.RegularExpressions;
#endif

namespace UnitystationLauncher.ViewModels
{
    public class ServerViewModel : ViewModelBase, IDisposable
    {
        private readonly AuthService _authService;

        // Ping does not work in sandboxes so we have to reconstruct its functionality in that case.
        // Surprisingly, this is basically what that does. Looks for your system's ping tool and parses its output.
#if FLATPAK
	    private readonly Process _pingSender;
#else
        private readonly Ping _pingSender;
#endif

        public ServerViewModel(Server server, Installation? installation, Download? download, AuthService authService)
        {
            Server = server;
            Installation = installation;
            Download = download;
            _authService = authService;
#if FLATPAK
	        _pingSender = new Process();
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
            _pingSender = new Ping();
            _pingSender.PingCompleted += PingCompletedCallback;
            _pingSender.SendAsync(Server.ServerIp, 7);
#endif
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

        public Server Server { get; }
        public Installation? Installation { get; set; }
        public Download? Download { get; set; }
        public ReactiveProperty<string> RoundTrip { get; } = new ReactiveProperty<string>();

        public bool Installed => Installation != null;

        public IObservable<bool> Downloading => Download?.WhenAnyValue(d => d.Active) ?? Observable.Return(false);

        public IObservable<string> DownloadedAmount { get; }
        public IObservable<string> DownloadSize { get; }

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

        public void Start()
        {
            Installation?.Start(IPAddress.Parse(Server.ServerIp), (short)Server.ServerPort,
                _authService.CurrentRefreshToken, _authService.Uid);
        }

        public void Dispose()
        {
            _pingSender.Dispose();
        }
    }
}
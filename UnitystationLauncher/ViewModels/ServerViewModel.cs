using System;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Reactive.Bindings;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;
using ReactiveCommand = ReactiveUI.ReactiveCommand;

namespace UnitystationLauncher.ViewModels;

public class ServerViewModel : ViewModelBase
{
    public Server Server { get; set; }

    public string Ping { get; internal set; }

    public Download? Download { get; set; }

    public string DownloadSize => (Download?.Size ?? 0L).Bytes().ToString("# MB");

    public string DownloadedAmount => (Download?.Downloaded ?? 0L).Bytes().ToString("# MB");

    public Installation? Installation => _installationService.GetInstallation(Server.ForkName, Server.BuildVersion);

    public bool ShowDownloadButton => Installation == null && !ShowDownloadProgress;

    public bool ShowDownloadProgress => Download?.Active ?? false;

    public bool ShowStartButton => Installation != null;

    private readonly IInstallationService _installationService;
    private readonly IEnvironmentService _environmentService;

    public ServerViewModel(Server server, IEnvironmentService environmentService, IInstallationService installationService)
    {
        Server = server;
        Ping = string.Empty;

        _installationService = installationService;
        _environmentService = environmentService;

        Download = _installationService.GetInProgressDownload(server.ForkName, server.BuildVersion);

        GetPingTime();
    }

    private void GetPingTime()
    {
        if (Server.HasValidDomainName || Server.HasValidIpAddress)
        {
            if (_environmentService.GetCurrentEnvironment() == CurrentEnvironment.LinuxFlatpak)
            {
                Task.Run(FlatpakGetPingTime);
            }
            else
            {
                try
                {
                    using Ping ping = new();
                    ping.PingCompleted += PingCompletedCallback;
                    ping.SendAsync(Server.ServerIp, null);
                }
                catch (ArgumentException e)
                {
                    Log.Error("Error: {Error}", $"Invalid IP address when trying to ping server: {e.Message}");
                    Ping = "Error";
                }
            }
        }
        else
        {
            Log.Error("Error: {Error}", $"Server '{Server.ServerName}' has an invalid ip address, skipping ping...");
            Ping = "Bad IP";
        }
    }

    private void PingCompletedCallback(object _, PingCompletedEventArgs eventArgs)
    {
        // If an error occurred, display the exception to the user.  
        if (eventArgs.Error != null)
        {
            Log.Error("Ping failed: {Error}", eventArgs.Error);
            return;
        }

        long? tripTime = eventArgs.Reply?.RoundtripTime;
        Ping = tripTime.HasValue ? $"{tripTime.Value}ms" : "null";
        this.RaisePropertyChanged(nameof(Ping));
    }

    public void LaunchGame()
    {
        if (Installation == null)
        {
            Log.Warning("No installation for server.");
            return;
        }

        _installationService.StartInstallation(Installation.InstallationId, Server.ServerIp, (short)Server.ServerPort);
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
        Ping = $"{pingOut}ms";
        this.RaisePropertyChanged(nameof(Ping));
        await pingSender.WaitForExitAsync();
    }

    public override void Refresh()
    {
        GetPingTime();
        RefreshDownloadingStatus();
        this.RaisePropertyChanged(nameof(Server));
        this.RaisePropertyChanged(nameof(Server.ServerName));
        this.RaisePropertyChanged(nameof(Server.PlayerCount));
        this.RaisePropertyChanged(nameof(Server.BuildVersion));
        this.RaisePropertyChanged(nameof(Server.CurrentMap));
        this.RaisePropertyChanged(nameof(Server.GameMode));
        this.RaisePropertyChanged(nameof(Server.InGameTime));
    }

    private void RefreshDownloadingStatus()
    {
        this.RaisePropertyChanged(nameof(Download));
        this.RaisePropertyChanged(nameof(ShowDownloadButton));
        this.RaisePropertyChanged(nameof(ShowDownloadProgress));
        this.RaisePropertyChanged(nameof(DownloadSize));
        this.RaisePropertyChanged(nameof(DownloadedAmount));

        // Refresh the UI for this server more often while it is downloading.
        if (ShowDownloadProgress)
        {
            RxApp.MainThreadScheduler.Schedule(DateTimeOffset.Now.AddMilliseconds(200), RefreshDownloadingStatus);
        }
        else
        {
            this.RaisePropertyChanged(nameof(Installation));
            this.RaisePropertyChanged(nameof(ShowStartButton));
        }
    }
}


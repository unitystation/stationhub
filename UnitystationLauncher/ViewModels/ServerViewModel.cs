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
    private readonly IPingService _pingService;

    public ServerViewModel(Server server, IInstallationService installationService, IPingService pingService)
    {
        Server = server;
        Ping = string.Empty;

        _installationService = installationService;
        _pingService = pingService;

        Download = _installationService.GetInProgressDownload(server.ForkName, server.BuildVersion);

        RxApp.MainThreadScheduler.ScheduleAsync(GetPing);
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

    private async Task GetPing(IScheduler scheduler, CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        try
        {
            Ping = await _pingService.GetPing(Server);
        }
        catch (Exception e)
        {
            Ping = "Error";
            Log.Error($"Error while trying to ping: {e.Message}");
        }

        this.RaisePropertyChanged(nameof(Ping));
    }

    public override void Refresh()
    {
        RxApp.MainThreadScheduler.ScheduleAsync(GetPing);
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


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ViewModels;

public class ServersPanelViewModel : PanelBase
{
    public override string Name => "Servers";
    public override bool IsEnabled => true;

    public ReactiveCommand<ServerViewModel, Unit> DownloadCommand { get; }

    public bool ServersFound => ServerViews.Any();

    public ObservableCollection<ServerViewModel> ServerViews { get; init; } = new();

    private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(10);

    private readonly IInstallationService _installationService;
    private readonly IPingService _pingService;
    private readonly IServerService _serverService;

    public ServersPanelViewModel(IInstallationService installationService, IPingService pingService,
        IServerService serverService)
    {
        _installationService = installationService;
        _pingService = pingService;
        _serverService = serverService;

        DownloadCommand = ReactiveCommand.Create<ServerViewModel, Unit>(server =>
        {
            _ = DownloadServer(server.Server);
            return Unit.Default;
        });

        InitializeServersList();
    }

    private void InitializeServersList()
    {
        Log.Information("Initializing servers list...");
        RxApp.MainThreadScheduler.ScheduleAsync((_, _) => RefreshServersList());

        Log.Information("Scheduling periodic refresh for servers list...");

        // Why can you not just run async methods with this?? Instead we have to do this ugly thing
        RxApp.TaskpoolScheduler.SchedulePeriodic(_refreshInterval,
            () => { RxApp.MainThreadScheduler.ScheduleAsync((_, _) => RefreshServersList()); });
    }

    private async Task RefreshServersList()
    {
        List<Server> servers;
        try
        {
            servers = await _serverService.GetServersAsync();
        }
        catch (Exception e)
        {
            servers = new();
            Log.Error($"Error while fetching servers list: {e.Message}");
        }

        AddNewServers(servers);
        RemoveDeletedServers(servers);
        Refresh();

        Log.Debug("Servers list has been refreshed.");
    }

    private void AddNewServers(List<Server> servers)
    {
        foreach (Server server in servers)
        {
            ServerViewModel? viewModel = ServerViews.FirstOrDefault(viewModel =>
                viewModel.Server.ServerIp == server.ServerIp
                && viewModel.Server.ServerPort == server.ServerPort);

            if (viewModel == null)
            {
                ServerViews.Add(new(server, _installationService, _pingService));
            }
            else
            {
                viewModel.Server = server;
            }
        }
    }

    private void RemoveDeletedServers(List<Server> servers)
    {
        for (int i = ServerViews.Count - 1; i >= 0; i--)
        {
            ServerViewModel viewModel = ServerViews[i];
            Server? server = servers.FirstOrDefault(server =>
                server.ServerIp == viewModel.Server.ServerIp
                && server.ServerPort == viewModel.Server.ServerPort);

            if (server == null)
            {
                ServerViews.Remove(viewModel);
            }
        }
    }

    private async Task DownloadServer(Server server)
    {
        (Download? download, string downloadFailReason) = await _installationService.DownloadInstallationAsync(server);

        if (download == null)
        {
            _ = MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.Ok, "Problem downloading server",
                downloadFailReason).ShowAsync();
            return;
        }

        foreach (ServerViewModel viewModel in
                 ServerViews.Where(viewModel => viewModel.Server.ForkName == download.ForkName
                                                && viewModel.Server.BuildVersion == download.BuildVersion))
        {
            viewModel.Download = download;
            viewModel.Refresh();
        }

        this.RaisePropertyChanged(nameof(ServerViews));
    }

    public override void Refresh()
    {
        this.RaisePropertyChanged(nameof(ServersFound));
        this.RaisePropertyChanged(nameof(ServerViews));

        foreach (ServerViewModel viewModel in ServerViews)
        {
            viewModel.Refresh();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using MsBox.Avalonia.Base;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Models.ConfigFile;
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
    private readonly IPreferencesService? _preferencesService;
    private readonly IEnvironmentService? _environmentService;

    public ServersPanelViewModel(IInstallationService installationService, IPingService pingService,
        IServerService serverService, IPreferencesService? preferencesService,IEnvironmentService? environmentService   )
    {
        _installationService = installationService;
        _pingService = pingService;
        _serverService = serverService;
        _preferencesService = preferencesService;
        _environmentService = environmentService;


        DownloadCommand = ReactiveCommand.Create<ServerViewModel, Unit>(server =>
        {
            _ = DownloadServer(server.Server);
            return Unit.Default;
        });

        InitializeServersList();
      
    }

    public async Task CheckNewUser()
    {
        if (_environmentService == null || _preferencesService == null) return; //is Tests
        
        if (_environmentService.GetCurrentEnvironment() == CurrentEnvironment.MacOsStandalone)
        {
            return;
        } 
        
        if (_preferencesService.GetPreferences().TTSEnabled == null)
        {
            IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.YesNo,
                "Local TTS?", " would you like local TTS ( Character voices from other players Will not be present if you say no ) To be installed on your computer (2Gb Storage space and 500mb download)? It will download the first time you download a build It may take awhile to extract  ");

            string response = await msgBox.ShowAsync();
            if (response.Equals(MessageBoxResults.Yes))
            {
                SaveChoiceTTS(true);
            }
            else
            {
                SaveChoiceTTS(false);
            }
        }
    }
        
    private void SaveChoiceTTS(bool? val)
    {
        if (_environmentService == null || _preferencesService == null) return; //is Tests
        Preferences prefs = _preferencesService.GetPreferences();
        prefs.TTSEnabled = val;
    }
    
    private void InitializeServersList()
    {
        Log.Information("Initializing servers list...");
        RxApp.MainThreadScheduler.ScheduleAsync((_, _) => RefreshServersList());

        Log.Information("Scheduling periodic refresh for servers list...");
        RxApp.MainThreadScheduler.ScheduleAsync((_, _) => CheckNewUser());
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
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MessageBox.Avalonia.BaseWindows.Base;
using Serilog;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationsPanelViewModel : PanelBase
    {
        public override string Name => "Installations";
        public override bool IsEnabled => true;

        string? _buildNum;
        public string? BuildNum
        {
            get => _buildNum;
            set => this.RaiseAndSetIfChanged(ref _buildNum, value);
        }

        private bool _autoRemove;
        public bool AutoRemove
        {
            get => _autoRemove;
            set => this.RaiseAndSetIfChanged(ref _autoRemove, value);
        }

        public ObservableCollection<InstallationViewModel> InstallationViews { get; init; } = new();

        private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(10);
        private readonly IPreferencesService _preferencesService;
        private readonly IInstallationService _installationService;

        public InstallationsPanelViewModel(IInstallationService installationService, IPreferencesService preferencesService)
        {
            _installationService = installationService;
            _preferencesService = preferencesService;

            BuildNum = $"Hub Build Num: {AppInfo.CurrentBuild}";

            this.WhenAnyValue(p => p.AutoRemove)
                .Select(_ => Observable.FromAsync(OnAutoRemoveChangedAsync))
                .Concat()
                .Subscribe();

            UpdateFromPreferences();
            InitializeInstallationsList();
        }

        private void UpdateFromPreferences()
        {
            Preferences prefs = _preferencesService.GetPreferences();
            AutoRemove = prefs.AutoRemove;
        }

        private async Task OnAutoRemoveChangedAsync()
        {
            if (AutoRemove)
            {
                IMsBoxWindow<string> msgBox = MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.YesNo,
                    "Are you sure?", "This will remove older installations from disk. Proceed?");

                string response = await msgBox.Show();
                if (response.Equals(MessageBoxResults.Yes))
                {
                    SaveChoice();
                }
                else
                {
                    AutoRemove = false;
                }
            }
            else
            {
                SaveChoice();
            }
        }

        private void InitializeInstallationsList()
        {
            Log.Information("Initializing installations list...");
            List<Installation> installations = _installationService.GetInstallations();

            foreach (Installation installation in installations)
            {
                InstallationViews.Add(new(installation, _installationService));
            }

            Log.Information("Scheduling periodic refresh for installations list...");
            RxApp.MainThreadScheduler.SchedulePeriodic(_refreshInterval, RefreshInstallationsList);
        }

        private void RefreshInstallationsList()
        {
            List<Installation> installations = _installationService.GetInstallations();

            // Add new
            foreach (Installation installation in installations)
            {
                InstallationViewModel? viewModel = InstallationViews.FirstOrDefault(view => view.Installation.InstallationId == installation.InstallationId);

                if (viewModel == null)
                {
                    InstallationViews.Add(new(installation, _installationService));
                }
            }

            // Remove old
            for (int i = 0; i < InstallationViews.Count; i++)
            {
                InstallationViewModel viewModel = InstallationViews[i];
                Installation? installation = installations.FirstOrDefault(inst => inst.InstallationId == viewModel.Installation.InstallationId);

                if (installation == null)
                {
                    InstallationViews.Remove(viewModel);
                    i--;
                }
            }

            Refresh();
            Log.Debug("Installations list has been refreshed.");
        }

        private void SaveChoice()
        {
            Preferences prefs = _preferencesService.GetPreferences();
            prefs.AutoRemove = AutoRemove;
        }

        public override void Refresh()
        {
            this.RaisePropertyChanged(nameof(InstallationViews));
        }
    }
}
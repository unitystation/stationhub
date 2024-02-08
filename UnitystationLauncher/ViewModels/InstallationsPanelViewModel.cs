using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MsBox.Avalonia.Base;
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

        private string? _buildNum;
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

        private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(2);
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
                IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.YesNo,
                    "Are you sure?", "This will remove older installations from disk. Proceed?");

                string response = await msgBox.ShowAsync();
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

            AddNewInstallations(installations);
            RemoveDeletedInstallations(installations);
            Refresh();

            Log.Debug("Installations list has been refreshed.");
        }

        private void AddNewInstallations(List<Installation> installations)
        {
            foreach (Installation installation in installations)
            {
                InstallationViewModel? viewModel = InstallationViews.FirstOrDefault(view => view.Installation.InstallationId == installation.InstallationId);

                if (viewModel == null)
                {
                    InstallationViews.Add(new(installation, _installationService));
                }
            }
        }

        private void RemoveDeletedInstallations(List<Installation> installations)
        {
            for (int i = InstallationViews.Count - 1; i >= 0; i--)
            {
                InstallationViewModel viewModel = InstallationViews[i];
                Installation? installation = installations.FirstOrDefault(inst => inst.InstallationId == viewModel.Installation.InstallationId);

                if (installation == null)
                {
                    InstallationViews.Remove(viewModel);
                }
            }
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
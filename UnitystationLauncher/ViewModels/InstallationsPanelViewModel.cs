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
using UnitystationLauncher.Services;
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

        private bool? _TTSEnabled;
        
        public bool? TTSEnabled
        {
            get => _TTSEnabled;
            set => this.RaiseAndSetIfChanged(ref _TTSEnabled, value);
        }

        public ObservableCollection<InstallationViewModel> InstallationViews { get; init; } = new();

        private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(2);
        private readonly IPreferencesService _preferencesService;
        private readonly IInstallationService _installationService;
        private readonly IEnvironmentService _environmentService;
        private readonly ITTSService _ttsService;
        
        public InstallationsPanelViewModel(IInstallationService installationService,
            IPreferencesService preferencesService, 
            IEnvironmentService environmentService,
            ITTSService ttsService
            )
        {
            _installationService = installationService;
            _preferencesService = preferencesService;
            _environmentService = environmentService;

            _ttsService = ttsService;
            
            BuildNum = $"Hub Build Num: {AppInfo.CurrentBuild}";

            UpdateFromPreferences();
            
            this.WhenAnyValue(p => p.AutoRemove)
                .Select(_ => Observable.FromAsync(OnAutoRemoveChangedAsync))
                .Concat()
                .Subscribe();

            this.WhenAnyValue(p => p.TTSEnabled)
                .Select(_ => Observable.FromAsync(OnTTSChange))
                .Concat()
                .Subscribe();
            
         
            InitializeInstallationsList();
        }

        private void UpdateFromPreferences()
        {
            Preferences prefs = _preferencesService.GetPreferences();
            AutoRemove = prefs.AutoRemove;
            TTSEnabled = prefs.TTSEnabled;
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
        
        private async Task OnTTSChange()
        {
            if (_environmentService.GetCurrentEnvironment() == CurrentEnvironment.MacOsStandalone)
            {
                IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.Ok,
                    " Mac Support is sad ",
                    " Sadly TTS is unsupported on Mac, If you'd like to contribute this, feel free to shoot a message on the Discord. ");
                string response = await msgBox.ShowAsync();
                return;
            }

            
            if (TTSEnabled is false)
            {
                IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.YesNo,
                    "Are you sure?", "This will disable Character voices (TTS). Proceed? (If yes It can take a little bit to delete So be patient)");

                string response = await msgBox.ShowAsync();
                if (response.Equals(MessageBoxResults.Yes))
                {
                    TTSEnabled = false;
                    SaveChoiceTTS();
                }
                else
                {
                    TTSEnabled = true;
                    SaveChoiceTTS();
                }
            }
            else
            {
                SaveChoiceTTS();
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

        private void SaveChoiceTTS()
        {
            Preferences prefs = _preferencesService.GetPreferences();
            prefs.TTSEnabled = TTSEnabled;

            if (TTSEnabled == false)
            {
                _ttsService.StopTTS();
                string installationBasePath = _preferencesService.GetPreferences().InstallationPath;
                var LocalVersion = System.IO.Path.Combine(installationBasePath, "tts");
                if (System.IO.Directory.Exists(LocalVersion))
                {
                    System.IO.Directory.Delete(LocalVersion, true);
                }
            }
            
        }
        
        public override void Refresh()
        {
            this.RaisePropertyChanged(nameof(InstallationViews));
        }
    }
}
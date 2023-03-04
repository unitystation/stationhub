using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Models;
using MessageBox.Avalonia.DTO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Services;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationsPanelViewModel : PanelBase
    {
        public override string Name => "Installations";
        public override bool IsEnabled => true;

        private readonly InstallationService _installationService;
        private readonly Config _config;
        string? _buildNum;
        private bool _autoRemove;

        public InstallationsPanelViewModel(InstallationService installationService, Config config)
        {
            _installationService = installationService;
            _config = config;

            BuildNum = $"Hub Build Num: {Config.CurrentBuild}";

            this.WhenAnyValue(p => p.AutoRemove)
                .Select(_ => Observable.FromAsync(OnAutoRemoveChangedAsync))
                .Concat()
                .Subscribe();

            RxApp.MainThreadScheduler.ScheduleAsync((scheduler, ct) => UpdateFromPreferencesAsync());
        }

        public IObservable<IReadOnlyList<InstallationViewModel>> Installations => _installationService.Installations
            .Select(installations => installations
                .Select(installation => new InstallationViewModel(installation)).ToList());

        public string? BuildNum
        {
            get => _buildNum;
            set => this.RaiseAndSetIfChanged(ref _buildNum, value);
        }

        public bool AutoRemove
        {
            get => _autoRemove;
            set => this.RaiseAndSetIfChanged(ref _autoRemove, value);
        }

        async Task UpdateFromPreferencesAsync()
        {
            var prefs = await _config.GetPreferencesAsync();
            AutoRemove = prefs.AutoRemove;
            _installationService.AutoRemove = prefs.AutoRemove;
        }

        private async Task OnAutoRemoveChangedAsync()
        {
            if (AutoRemove)
            {
                var msgBox = MessageBoxManager.GetMessageBoxCustomWindow(new MessageBoxCustomParams
                {
                    SystemDecorations = Avalonia.Controls.SystemDecorations.BorderOnly,
                    WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterScreen,
                    ContentHeader = "Are you sure?",
                    ContentMessage = "This will remove older installations from disk. Proceed?",
                    ButtonDefinitions = new[]
                        {new ButtonDefinition {Name = "Cancel"}, new ButtonDefinition {Name = "Confirm"}}
                });

                var response = await msgBox.Show();
                if (response.Equals("Confirm"))
                {
                    await SaveChoiceAsync();
                }
                else
                {
                    AutoRemove = false;
                }
            }
            else
            {
                await SaveChoiceAsync();
            }
        }

        async Task SaveChoiceAsync()
        {
            var prefs = await _config.GetPreferencesAsync();
            prefs.AutoRemove = AutoRemove;
            _installationService.AutoRemove = AutoRemove;
        }
    }
}
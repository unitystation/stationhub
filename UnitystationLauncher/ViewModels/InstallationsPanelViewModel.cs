using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Models;
using MessageBox.Avalonia.DTO;
using System.Reactive.Linq;
using System.Threading.Tasks;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Services;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationsPanelViewModel : PanelBase
    {
        public override string Name => "Installations";
        public override bool IsEnabled => true;

        string? _buildNum;
        private bool _autoRemove;

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
        }

        public IObservable<IReadOnlyList<InstallationViewModel>> Installations => _installationService.GetInstallations()
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

        private void UpdateFromPreferences()
        {
            Preferences prefs = _preferencesService.GetPreferences();
            AutoRemove = prefs.AutoRemove;
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

        private void SaveChoice()
        {
            Preferences prefs = _preferencesService.GetPreferences();
            prefs.AutoRemove = AutoRemove;
        }
    }
}
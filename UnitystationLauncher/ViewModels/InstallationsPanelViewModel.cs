using ReactiveUI;
using System;
using System.Collections.Generic;
using UnitystationLauncher.Models;
using MessageBox.Avalonia;
using MessageBox.Avalonia.Models;
using MessageBox.Avalonia.DTO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationsPanelViewModel : PanelBase
    {
        public override string Name => "Installations";
        private InstallationManager _installationManager;
        private Installation? _selectedInstallation;
        private Config _config;
        string? _buildNum;
        private bool _autoRemove;

        public InstallationsPanelViewModel(InstallationManager installationManager, Config config)
        {
            _installationManager = installationManager;
            _config = config;

            BuildNum = $"Hub Build Num: {Config.CurrentBuild}";

            CheckBoxClick = ReactiveUI.ReactiveCommand.CreateFromTask(OnCheckBoxClick);

            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var prefs = await _config.GetPreferences();
                AutoRemove = prefs.AutoRemove;
                installationManager.AutoRemove = prefs.AutoRemove;
            });
        }

        public IObservable<IReadOnlyList<Installation>> Installations => _installationManager.Installations;
        public ReactiveCommand<Unit, Unit> CheckBoxClick { get; }

        public Installation? SelectedInstallation
        {
            get => _selectedInstallation;
            set => this.RaiseAndSetIfChanged(ref _selectedInstallation, value);
        }

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

        //This is a Reactive Command action as confirmation needs to happen with the
        //message box.
        async Task OnCheckBoxClick()
        {
            if (AutoRemove)
            {
                var msgBox = MessageBoxManager.GetMessageBoxCustomWindow(new MessageBoxCustomParams
                {
                    Style = MessageBox.Avalonia.Enums.Style.None,
                    Icon = MessageBox.Avalonia.Enums.Icon.None,
                    ShowInCenter = true,
                    ContentHeader = "Are you sure?",
                    ContentMessage = "This will remove older installations from disk. Proceed?",
                    ButtonDefinitions = new[]
                        {new ButtonDefinition {Name = "Cancel"}, new ButtonDefinition {Name = "Confirm"}}
                });

                var response = await msgBox.Show();
                if (response.Equals("Confirm"))
                {
                    await SaveChoice();
                }
                else
                {
                    AutoRemove = false;
                }
            }
            else
            {
                await SaveChoice();
            }
        }

        async Task SaveChoice()
        {
            var prefs = await _config.GetPreferences();
            prefs.AutoRemove = AutoRemove;
            _installationManager.AutoRemove = AutoRemove;
        }
    }
}
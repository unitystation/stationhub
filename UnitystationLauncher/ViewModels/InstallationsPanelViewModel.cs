﻿using ReactiveUI;
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
                .Subscribe(async v => await OnAutoRemoveChanged());

            RxApp.MainThreadScheduler.Schedule(async () =>
            {
                var prefs = await _config.GetPreferences();
                AutoRemove = prefs.AutoRemove;
                installationService.AutoRemove = prefs.AutoRemove;
            });
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

        private async Task OnAutoRemoveChanged()
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
            _installationService.AutoRemove = AutoRemove;
        }
    }
}
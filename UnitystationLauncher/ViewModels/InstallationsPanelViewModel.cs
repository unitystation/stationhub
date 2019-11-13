using ReactiveUI;
using System;
using System.Collections.Generic;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationsPanelViewModel : PanelBase
    {
        public override string Name => "Installations";

        private InstallationManager installationManager;

        private Installation? selectedInstallation;

        public InstallationsPanelViewModel(InstallationManager installationManager)
        {
            this.installationManager = installationManager;
        }

        public IObservable<IReadOnlyList<Installation>> Installations => installationManager.Installations;

        public Installation? SelectedInstallation
        {
            get => selectedInstallation;
            set => this.RaiseAndSetIfChanged(ref selectedInstallation, value);
        }
    }
}

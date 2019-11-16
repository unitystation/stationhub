using ReactiveUI;
using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationsPanelViewModel : PanelBase
    {
        public override string Name => "Installations";
        public override IBitmap Icon => new Bitmap(AvaloniaLocator.Current.GetService<IAssetLoader>()
            .Open(new Uri("avares://UnitystationLauncher/Assets/archiveicon.png")));
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

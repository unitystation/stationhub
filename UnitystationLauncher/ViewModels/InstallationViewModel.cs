using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using MsBox.Avalonia.Base;
using ReactiveUI;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationViewModel : ViewModelBase
    {
        public Installation Installation { get; }
        public ReactiveCommand<Unit, Unit> LaunchCommand { get; set; }
        public ReactiveCommand<Unit, Unit> UninstallCommand { get; set; }

        private readonly IInstallationService _installationService;

        public InstallationViewModel(Installation installation, IInstallationService installationService)
        {
            _installationService = installationService;
            Installation = installation;
            LaunchCommand = ReactiveCommand.Create(LaunchInstallation);
            UninstallCommand = ReactiveCommand.Create(DeleteInstallation); ;
        }

        private void LaunchInstallation()
        {
            _installationService.StartInstallation(Installation.InstallationId);
        }

        private void DeleteInstallation()
        {
            _installationService.DeleteInstallation(Installation.InstallationId);
        }

        public override void Refresh()
        {
            // Do nothing
        }
        
    }
}
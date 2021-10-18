using System.Reactive;
using ReactiveUI;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationViewModel : ViewModelBase
    {
        public InstallationViewModel(Installation installation)
        {
            Installation = installation;
            LaunchCommand = ReactiveCommand.Create(Installation.Start);
            UninstallCommand = ReactiveCommand.CreateFromTask(Installation.DeleteAsync);
        }

        public Installation Installation { get; }
        public ReactiveCommand<Unit, Unit> LaunchCommand { get; set; }
        public ReactiveCommand<Unit, Unit> UninstallCommand { get; set; }
    }
}
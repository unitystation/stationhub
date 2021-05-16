using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class InstallationViewModel : ViewModelBase
    {
        public InstallationViewModel(Installation installation)
        {
            Installation = installation;
        }

        public Installation Installation { get; }
    }
}
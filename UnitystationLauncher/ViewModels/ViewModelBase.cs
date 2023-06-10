using ReactiveUI;

namespace UnitystationLauncher.ViewModels
{
    public abstract class ViewModelBase : ReactiveObject
    {
        public abstract void Refresh();
    }
}

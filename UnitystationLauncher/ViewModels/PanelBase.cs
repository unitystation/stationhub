namespace UnitystationLauncher.ViewModels
{
    public abstract class PanelBase : ViewModelBase
    {
        public abstract string Name { get; }
        
        public abstract bool IsEnabled { get; }
    }
}

using System.Reactive;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace UnitystationLauncher.Views
{
    public class SecurityPanelView : UserControl
    {
        //public ReactiveCommand<Unit, Unit> OnClickCommand { get; }
        public SecurityPanelView()
        {
            //OnClickCommand = ReactiveCommand.Create(() => { DoThing(); });
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        

    }
}

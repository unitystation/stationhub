using System.Reactive;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace UnitystationLauncher.Views
{
    public class SecurityPanelView : UserControl
    {
        public SecurityPanelView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        

    }
}

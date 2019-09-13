using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnitystationLauncher.Views
{
    public class InstallationsPanelView : UserControl
    {
        public InstallationsPanelView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

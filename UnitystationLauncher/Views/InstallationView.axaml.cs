using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnitystationLauncher.Views
{
    public class InstallationView : UserControl
    {
        public InstallationView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
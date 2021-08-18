using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnitystationLauncher.Views
{
    public class ServersPanelView : UserControl
    {
        public ServersPanelView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}

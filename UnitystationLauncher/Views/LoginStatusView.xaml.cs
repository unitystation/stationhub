using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnitystationLauncher.Views
{
    public class LoginStatusView : UserControl
    {
        public LoginStatusView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
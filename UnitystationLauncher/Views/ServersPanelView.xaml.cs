using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using UnitystationLauncher.ViewModels;

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

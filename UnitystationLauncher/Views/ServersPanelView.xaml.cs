using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Views
{
    public class ServersPanelView : UserControl
    {
        ServersPanelViewModel vm;
        public ServersPanelView()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            AttachedToVisualTree += ServersListAttachedToVisualTree;
            DetachedFromVisualTree += ServersListDetachedFromVisualTree;
            vm = (ServersPanelViewModel)this.DataContext;
        }

        private void ServersListAttachedToVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            vm = (ServersPanelViewModel)this.DataContext;
            vm.OnFocused();
        }

        private void ServersListDetachedFromVisualTree(object sender, VisualTreeAttachmentEventArgs e)
        {
            vm.OnUnFocused();
        }
    }
}

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Views
{
    public class PreferencesPanelView : UserControl
    {
        public PreferencesPanelView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // I would like for this to be an async method, but I cannot due to Avalonia not supporting that on events
        private void ChangeInstallationFolder_OnClick(object? sender, RoutedEventArgs eventArgs)
        {
            if (DataContext is PreferencesPanelViewModel viewModel)
            {
                Task.Run(async () =>
                {
                    Window mainWindow = ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).MainWindow;
                    OpenFolderDialog dialog = new();
                    string? result = await dialog.ShowAsync(mainWindow);
                    
                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        viewModel.SetInstallationPath(result);
                    }
                });
            }
        }
    }
}

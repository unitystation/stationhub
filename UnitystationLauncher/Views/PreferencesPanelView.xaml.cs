using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;
using Serilog;
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

        private void ChangeInstallationFolder_OnClick(object? sender, RoutedEventArgs eventArgs)
        {
            RxApp.MainThreadScheduler.ScheduleAsync((_, _) => ChangeInstallationFolder());
        }

        private async Task ChangeInstallationFolder()
        {
            if (Application.Current != null
                && Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime applicationLifetime
                && DataContext is PreferencesPanelViewModel viewModel)
            {
                // This dialog needs to be in the View, not in the ViewModel.
                OpenFolderDialog dialog = new();
                string? result = await dialog.ShowAsync(applicationLifetime.MainWindow);

                if (!string.IsNullOrWhiteSpace(result))
                {
                    await viewModel.SetInstallationPathAsync(result);
                }
                else
                {
                    Log.Warning("Invalid path");
                }
            }
        }
    }
}

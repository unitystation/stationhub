using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Views;

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

    // This dialog needs to be in the View, not in the ViewModel.
    private async Task ChangeInstallationFolder()
    {
        if (Application.Current != null
            && Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime applicationLifetime
            && DataContext is PreferencesPanelViewModel viewModel
            && TopLevel.GetTopLevel(this) is TopLevel topLevel)
        {
            // Start async operation to open the dialog.
            IReadOnlyList<IStorageFolder> selectedFolder = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
            {
                Title = "Select Folder",
                AllowMultiple = false,
                SuggestedStartLocation = await topLevel.StorageProvider.TryGetFolderFromPathAsync(viewModel.InstallationPath)
            });

            if (selectedFolder.Count > 0)
            {
                string? result = selectedFolder[0].TryGetLocalPath();

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

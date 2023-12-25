using System.Threading.Tasks;
using MsBox.Avalonia.Base;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ViewModels
{
    public class PreferencesPanelViewModel : PanelBase
    {
        public override string Name => "Preferences";

        public override bool IsEnabled => true;

        private string _installationPath;
        public string InstallationPath
        {
            get => _installationPath;
            set => this.RaiseAndSetIfChanged(ref _installationPath, value);
        }

        private readonly IPreferencesService _preferencesService;
        private readonly IInstallationService _installationService;

        public PreferencesPanelViewModel(IPreferencesService preferencesService, IInstallationService installationService)
        {
            _preferencesService = preferencesService;
            _installationService = installationService;

            _installationPath = _preferencesService.GetPreferences().InstallationPath;
        }

        public async Task SetInstallationPathAsync(string path)
        {
            (bool isValidPath, string invalidReason) = _installationService.IsValidInstallationBasePath(path);
            if (isValidPath)
            {
                IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(
                    MessageBoxButtons.YesNo,
                    string.Empty,
                    $"Would you like to move your old installations to the new location?\n "
                     + $"New installation path: {path}");

                string response = await msgBox.ShowAsync();
                Log.Information($"Move installations? {response}");
                if (response.Equals(MessageBoxResults.Yes))
                {
                    bool success = _installationService.MoveInstallations(path);
                    if (!success)
                    {
                        await MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.Ok, "Error moving installation",
                            "Could not move existing install.").ShowAsync();

                        return;
                    }
                }

                _preferencesService.GetPreferences().InstallationPath = InstallationPath = path;
                Log.Information($"Installation directory changed to: {path}");
            }
            else
            {
                Log.Warning($"Invalid directory as installation path, ignoring change: {path}");
                await MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.Ok, "Invalid installation path", invalidReason).ShowAsync();
            }
        }

        public override void Refresh()
        {
            // Do nothing
        }
    }
}

using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using HarfBuzzSharp;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.ConfigFile;
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

        private bool? _invalidInstallationPath;
        public bool InvalidInstallationPath
        {
            get => _invalidInstallationPath ?? false;
            set => this.RaiseAndSetIfChanged(ref _invalidInstallationPath, value);
        }
        
        private bool? _restartClientPrompt;
        public bool RestartClientPrompt
        {
            get => _restartClientPrompt ?? false;
            set => this.RaiseAndSetIfChanged(ref _restartClientPrompt, value);
        }
        
        private readonly IPreferencesService _preferencesService;
        
        public PreferencesPanelViewModel(IPreferencesService preferencesService)
        {
            _preferencesService = preferencesService;

            _installationPath = _preferencesService.GetPreferences().InstallationPath;
        }

        public void SetInstallationPath(string path)
        {
            if (Directory.GetDirectories(path).Length == 0 && Directory.GetFiles(path).Length == 0 && path.All(char.IsAscii))
            {
                Log.Information("Installation directory changed to:\n{newDir}", path);
                _preferencesService.GetPreferences().InstallationPath = path;
                InstallationPath = _preferencesService.GetPreferences().InstallationPath;
                RestartClientPrompt = true;
                InvalidInstallationPath = false;
            }
            else
            {
                Log.Warning("Invalid directory as installation path, ignoring change:\n{newDir}", path);
                RestartClientPrompt = false;
                InvalidInstallationPath = true;
            }
        }
    }
}

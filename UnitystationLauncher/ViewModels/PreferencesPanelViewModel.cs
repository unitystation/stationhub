using System;
using System.Threading.Tasks;
using System.Reactive.Linq;
using MsBox.Avalonia.Base;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Infrastructure;
using UnitystationLauncher.Services.Interface;
using UnitystationLauncher.Models.Enums;
using UnitystationLauncher.Models.ConfigFile;
using DynamicData.Binding;

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
        private bool _autoRemove;
        public bool AutoRemove
        {
            get => _autoRemove;
            set => this.RaiseAndSetIfChanged(ref _autoRemove, value);
        }
        // for a future overhaul of TTS
        // private string TTSPath;
        // public string TTSText
        // { // exp ? true : false
        //     get { return string.IsNullOrEmpty(TTSPath) ? "Not Installed." : $"Is Installed at {TTSPath}"; }
        // }
        // public string TTSButtonText
        // {
        //     get { return string.IsNullOrEmpty(TTSPath) ? "Install" : "Uninstall"; }
        // }
        private bool? _TTSEnabled;

        public bool? TTSEnabled
        {
            get => _TTSEnabled;
            set => this.RaiseAndSetIfChanged(ref _TTSEnabled, value);
        }

        private readonly IPreferencesService _preferencesService;
        private readonly IInstallationService _installationService;
        private readonly IEnvironmentService _environmentService;
        private readonly ITTSService _ttsService;

        public PreferencesPanelViewModel(
            IPreferencesService preferencesService,
            IInstallationService installationService,
            IEnvironmentService environmentService,
            ITTSService ttsService)
        {
            _preferencesService = preferencesService;
            _installationService = installationService;
            _environmentService = environmentService;
            _ttsService = ttsService;

            Preferences preferences = _preferencesService.GetPreferences();
            _installationPath = preferences.InstallationPath;
            _autoRemove = preferences.AutoRemove;
            _TTSEnabled = preferences.TTSEnabled;
            this.WhenAnyValue(p => p.AutoRemove)
                .Select(_ => Observable.FromAsync(OnAutoRemoveChangedAsync))
                .Concat()
                .Subscribe();

            this.WhenAnyValue(p => p.TTSEnabled)
                .Select(_ => Observable.FromAsync(OnTTSChangedAsync))
                .Concat()
                .Subscribe();
        }

        public async Task OnAutoRemoveChangedAsync()
        {
            if (AutoRemove)
            {
                IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.YesNo,
                    "Are you sure?", "This will remove older installations from disk. Proceed?");

                string response = await msgBox.ShowAsync();
                AutoRemove = response.Equals(MessageBoxResults.Yes);
            }
            _preferencesService.GetPreferences().AutoRemove = AutoRemove;
        }

        public async Task OnTTSChangedAsync()
        {
            if (_environmentService.GetCurrentEnvironment() == CurrentEnvironment.MacOsStandalone)
            {
                IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.Ok,
                    " MacOS is unsupported ",
                    " Text-To-Speech is not supported on MacOS. If you would like to contribute, talk to us on the discord. ");
                await msgBox.ShowAsync(); //should this even be awaited?
                return;
            }


            if (TTSEnabled is false)
            {
                IMsBox<string> msgBox = MessageBoxBuilder.CreateMessageBox(MessageBoxButtons.YesNo,
                    "Are you sure?", "This will disable Character voices and delete the TTS System. This may take a While.");
                string response = await msgBox.ShowAsync();
                TTSEnabled = response.Equals(MessageBoxResults.No);
            }
            _preferencesService.GetPreferences().TTSEnabled = TTSEnabled;

            if (TTSEnabled == false)
            {
                _ttsService.StopTTS();
                string installationBasePath = _preferencesService.GetPreferences().InstallationPath; //yucky
                var LocalVersion = System.IO.Path.Combine(installationBasePath, "tts");
                if (System.IO.Directory.Exists(LocalVersion))
                {
                    System.IO.Directory.Delete(LocalVersion, true);
                }
            }
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

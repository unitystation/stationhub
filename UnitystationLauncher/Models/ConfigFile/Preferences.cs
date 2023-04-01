using ReactiveUI;

namespace UnitystationLauncher.Models.ConfigFile
{
    public class Preferences : ReactiveObject
    {
        private bool _autoRemove = true;
        private int _ignoreVersionUpdate;
        private string _installationPath = string.Empty;

        public bool AutoRemove
        {
            get => _autoRemove;
            set => this.RaiseAndSetIfChanged(ref _autoRemove, value);
        }

        public int IgnoreVersionUpdate
        {
            get => _ignoreVersionUpdate;
            set => this.RaiseAndSetIfChanged(ref _ignoreVersionUpdate, value);
        }

        public string InstallationPath
        {
            get => _installationPath;
            set => this.RaiseAndSetIfChanged(ref _installationPath, value);
        }
    }
}
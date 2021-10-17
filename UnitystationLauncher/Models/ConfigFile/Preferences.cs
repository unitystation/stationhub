using ReactiveUI;

namespace UnitystationLauncher.Models.ConfigFile
{
    public class Preferences : ReactiveObject
    {
        private bool _autoRemove = true;
        private string? _lastLogin = "";

        public bool AutoRemove
        {
            get => _autoRemove;
            set => this.RaiseAndSetIfChanged(ref _autoRemove, value);
        }

        public string? LastLogin
        {
            get => _lastLogin;
            set => this.RaiseAndSetIfChanged(ref _lastLogin, value);
        }
    }
}
using System;
using System.IO;
using System.Reactive;
using Newtonsoft.Json;
using ReactiveUI;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly Lazy<SignUpViewModel> _signUpVm;
        private readonly Lazy<ForgotPasswordViewModel> _forgotVm;
        private readonly Lazy<LoginStatusViewModel> _loginStatusVm;
        private readonly AuthManager _authManager;
        string _email = "";
        string _password = "";

        public LoginViewModel(
            Lazy<LoginStatusViewModel> loginStatusVm,
            Lazy<SignUpViewModel> signUpVm, 
            Lazy<ForgotPasswordViewModel> forgotVm,
            AuthManager authManager)
        {
            _authManager = authManager;
            _signUpVm = signUpVm;
            _loginStatusVm = loginStatusVm;
            _forgotVm = forgotVm;

            var possibleCredentials = this.WhenAnyValue(
                x => x.Email,
                x => x.Password,
                (u, p) =>
                    !string.IsNullOrWhiteSpace(u) &&
                    !string.IsNullOrWhiteSpace(p));

            Login = ReactiveCommand.Create(
                UserLogin,
                possibleCredentials);

            Create = ReactiveCommand.Create(
                UserCreate);

            ForgotPw = ReactiveCommand.Create(
                ForgotPass);

            CheckForLastLogin();
        }

        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public ReactiveCommand<Unit, LoginStatusViewModel> Login { get; }
        public ReactiveCommand<Unit, SignUpViewModel> Create { get; }
        public ReactiveCommand<Unit, ForgotPasswordViewModel> ForgotPw { get; }

        public LoginStatusViewModel UserLogin()
        {
            _authManager.LoginMsg = new LoginMsg
            {
                Email = Email,
                Pass = Password
            };

            SaveLoginEmail();

            return _loginStatusVm.Value;
        }
        
        public SignUpViewModel UserCreate()
        {
            return _signUpVm.Value;
        }

        public ForgotPasswordViewModel ForgotPass()
        {
            return _forgotVm.Value;
        }

        void CheckForLastLogin()
        {
            if (File.Exists(Config.RootFolder + "prefs.json"))
            {
                var prefs = JsonConvert.DeserializeObject<Prefs>(File.ReadAllText(Path.Combine(Config.RootFolder, "prefs.json")));
                Email = prefs.LastLogin ?? "";
            }
        }

        void SaveLoginEmail()
        {
            var data = "";
            if (File.Exists(Config.RootFolder + "prefs.json"))
            {
                data = File.ReadAllText(Path.Combine(Config.RootFolder, "prefs.json"));
                var prefs = JsonConvert.DeserializeObject<Prefs>(data);
                prefs.LastLogin = _email;
                data = JsonConvert.SerializeObject(prefs);
            }
            else
            {
                data = JsonConvert.SerializeObject(new Prefs { AutoRemove = false, LastLogin = _email });
            }
            File.WriteAllText(Path.Combine(Config.RootFolder, "prefs.json"), data);
        }
    }
}

using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using ReactiveUI;
using UnitystationLauncher.Models.ConfigFile;
using UnitystationLauncher.Services;

namespace UnitystationLauncher.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly Lazy<SignUpViewModel> _signUpVm;
        private readonly Lazy<ForgotPasswordViewModel> _forgotVm;
        private readonly Lazy<LoginStatusViewModel> _loginStatusVm;
        private readonly AuthService _authService;
        private readonly Config _config;
        string _email = "";
        string _password = "";

        public LoginViewModel(
            Lazy<LoginStatusViewModel> loginStatusVm,
            Lazy<SignUpViewModel> signUpVm,
            Lazy<ForgotPasswordViewModel> forgotVm,
            AuthService authService,
            Config config)
        {
            _authService = authService;
            _config = config;
            _signUpVm = signUpVm;
            _loginStatusVm = loginStatusVm;
            _forgotVm = forgotVm;

            var possibleCredentials = this.WhenAnyValue(
                x => x.Email,
                x => x.Password,
                (u, p) =>
                    !string.IsNullOrWhiteSpace(u) &&
                    !string.IsNullOrWhiteSpace(p));

            Login = ReactiveCommand.CreateFromTask(
                UserLogin,
                possibleCredentials);

            Create = ReactiveCommand.Create(
                UserCreate);

            ForgotPw = ReactiveCommand.Create(
                ForgotPass);

            RxApp.MainThreadScheduler.Schedule(async () => await CheckForLastLogin());
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

        public async Task<LoginStatusViewModel> UserLogin()
        {
            _authService.LoginMsg = new LoginMsg
            {
                Email = Email,
                Pass = Password
            };

            await SaveLoginEmail();

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

        async Task CheckForLastLogin()
        {
            Email = (await _config.GetPreferences()).LastLogin ?? "";
        }

        async Task SaveLoginEmail()
        {
            var prefs = await _config.GetPreferences();
            prefs.LastLogin = _email;
        }
    }
}

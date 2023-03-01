using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Services;

namespace UnitystationLauncher.ViewModels
{
    public class LoginStatusViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly Lazy<LauncherViewModel> _launcherVm;
        private readonly LoginViewModel _loginVm;
        private string? _failedMessage;
        private bool _isFailedVisible;
        private bool _isResendEmailVisible;
        private bool _resendClicked;
        private bool _isWaitingVisible;

        public LoginStatusViewModel(AuthService authService, Lazy<LauncherViewModel> launcherVm,
            LoginViewModel loginVm)
        {
            IsFailedVisible = false;
            IsResendEmailVisible = false;
            ResendClicked = false;
            _authService = authService;
            _loginVm = loginVm;
            _launcherVm = launcherVm;

            var hasAlreadyResent = this.WhenAnyValue(
                x => x.ResendClicked,
                (r) => !r);

            ResendEmail = ReactiveCommand.Create(OnResend, hasAlreadyResent);

            GoBack = ReactiveCommand.Create(GoBackToLogin);

            OpenLauncher = ReactiveCommand.Create(SignInComplete);

            if (!authService.AttemptingAutoLogin)
            {
                RxApp.MainThreadScheduler.ScheduleAsync((scheduler, ct) => UserLoginAsync());
            }
            else
            {
                IsWaitingVisible = true;
            }
        }

        public bool IsFailedVisible
        {
            get => _isFailedVisible;
            set => this.RaiseAndSetIfChanged(ref _isFailedVisible, value);
        }

        public bool IsResendEmailVisible
        {
            get => _isResendEmailVisible;
            set => this.RaiseAndSetIfChanged(ref _isResendEmailVisible, value);
        }

        public string? FailedMessage
        {
            get => _failedMessage;
            set => this.RaiseAndSetIfChanged(ref _failedMessage, value);
        }

        public bool ResendClicked
        {
            get => _resendClicked;
            set => this.RaiseAndSetIfChanged(ref _resendClicked, value);
        }

        public bool IsWaitingVisible
        {
            get => _isWaitingVisible;
            set => this.RaiseAndSetIfChanged(ref _isWaitingVisible, value);
        }

        public ReactiveCommand<Unit, LoginViewModel> GoBack { get; }
        public ReactiveCommand<Unit, Unit> ResendEmail { get; }
        public ReactiveCommand<Unit, LauncherViewModel> OpenLauncher { get; }

        public async Task UserLoginAsync()
        {
            bool signInSuccess = true;
            ResendClicked = false;
            IsResendEmailVisible = false;
            IsWaitingVisible = true;

            if (string.IsNullOrEmpty(_authService.LoginMsg?.Email) ||
                string.IsNullOrEmpty(_authService.LoginMsg.Pass))
            {
                Log.Error("Login failed");
                FailedMessage = "Login failed.\r\n" +
                                "Check your email and password\r\n" +
                                "and try again.";
                return;
            }

            try
            {
                _authService.AuthLink = await _authService.SignInWithEmailAndPasswordAsync(_authService.LoginMsg.Email,
                    _authService.LoginMsg.Pass);
            }
            catch (Exception e)
            {
                Log.Error(e, "Login failed");
                FailedMessage = "Login failed.\r\n" +
                                "Check your email and password\r\n" +
                                "and try again.";
                signInSuccess = false;
            }

            if (signInSuccess)
            {
                var user = await _authService.GetUpdatedUserAsync();

                if (!user.IsEmailVerified)
                {
                    FailedMessage = "Email not yet verified.\r\n" +
                                    "Please click on the activation link sent to your\r\n" +
                                    "email address. Alternatively you can request another verification\r\n" +
                                    "email by clicking the resend button below.";
                    signInSuccess = false;
                    IsResendEmailVisible = true;
                }
            }

            _authService.LoginMsg = null;

            IsWaitingVisible = false;
            if (!signInSuccess)
            {
                IsFailedVisible = true;
                return;
            }

            _authService.SaveAuthSettings();

            Observable.Start(() => { }).InvokeCommand(this, vm => vm.OpenLauncher);
        }

        public void OnResend()
        {
            _authService.ResendVerificationEmail();
            ResendClicked = true;
            FailedMessage = "A new verification email has been sent to:\r\n" +
                            $"{_authService.AuthLink?.User.Email ?? "{ no email }"}\r\n" +
                            $"Please activate your account by clicking the link\r\n" +
                            $"in the email and try again.";
        }

        public LoginViewModel GoBackToLogin()
        {
            return _loginVm;
        }
        public LauncherViewModel SignInComplete()
        {
            return _launcherVm.Value;
        }
    }
}
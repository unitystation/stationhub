using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using DynamicData.Binding;
using Firebase.Auth;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly AuthManager authManager;
        private readonly Lazy<LauncherViewModel> launcherVM;
        private readonly Lazy<SignUpViewModel> signUpVM;
        string? email;
        string? password;
        private string failedMessage;
        private bool isFormVisible;
        private bool isFailedVisible;
        private bool isResendEmailVisible;
        private bool resendClicked = false;
        private bool isWaitingVisible;

        public LoginViewModel(AuthManager authManager, Lazy<LauncherViewModel> launcherVM,
            Lazy<SignUpViewModel> signUpVM)
        {
            IsFormVisible = true;
            IsFailedVisible = false;
            IsResendEmailVisible = false;
            ResendClicked = false;
            this.authManager = authManager;
            this.signUpVM = signUpVM;
            this.launcherVM = launcherVM;
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

            var hasAlreadyResent = this.WhenAnyValue(
                x => x.ResendClicked,
                (r) => !r);

            ResendEmail = ReactiveCommand.Create(OnResend, hasAlreadyResent);

            GoBack = ReactiveCommand.Create(ShowLoginForm);
        }

        public string? Email
        {
            get => email;
            set => this.RaiseAndSetIfChanged(ref email, value);
        }

        public string? Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);
        }

        public bool IsFormVisible
        {
            get => isFormVisible;
            set => this.RaiseAndSetIfChanged(ref isFormVisible, value);
        }

        public bool IsFailedVisible
        {
            get => isFailedVisible;
            set => this.RaiseAndSetIfChanged(ref isFailedVisible, value);
        }

        public bool IsResendEmailVisible
        {
            get => isResendEmailVisible;
            set => this.RaiseAndSetIfChanged(ref isResendEmailVisible, value);
        }

        public string FailedMessage
        {
            get => failedMessage;
            set => this.RaiseAndSetIfChanged(ref failedMessage, value);
        }

        public bool ResendClicked
        {
            get => resendClicked;
            set => this.RaiseAndSetIfChanged(ref resendClicked, value);
        }

        public bool IsWaitingVisible
        {
            get => isWaitingVisible;
            set => this.RaiseAndSetIfChanged(ref isWaitingVisible, value);
        }

        public ReactiveCommand<Unit, LauncherViewModel?> Login { get; }
        public ReactiveCommand<Unit, SignUpViewModel?> Create { get; }
        public ReactiveCommand<Unit, Unit> GoBack { get; }
        public ReactiveCommand<Unit, Unit> ResendEmail { get; }

        public async Task<LauncherViewModel?> UserLogin()
        {
            bool signInSuccess = true;
            ResendClicked = false;
            IsResendEmailVisible = false;
            ShowSigningInScreen();
            try
            {
                authManager.AuthLink = await authManager.SignInWithEmailAndPasswordAsync(email!, password!);
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
                var user = await authManager.GetUpdatedUser();

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

            IsWaitingVisible = false;
            if (!signInSuccess)
            {
                IsFailedVisible = true;
                return null;
            }

            authManager.Store();

            return launcherVM.Value;
        }

        public void ShowSigningInScreen()
        {
            IsFormVisible = false;
            IsWaitingVisible = true;
        }
        
        public SignUpViewModel? UserCreate()
        {
            return signUpVM.Value;
        }

        public void OnResend()
        {
            authManager.ResendVerificationEmail();
            ResendClicked = true;
            FailedMessage = "A new verification email has been sent to:\r\n" +
                            $"{authManager.AuthLink.User.Email}\r\n" +
                            $"Please activate your account by clicking the link\r\n" +
                            $"in the email and try again.";
        }

        public void ShowLoginForm()
        {
            IsFailedVisible = false;
            IsFormVisible = true;
        }
    }
}
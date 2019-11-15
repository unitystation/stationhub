using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Interactivity;
using DynamicData.Binding;
using Firebase.Auth;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class LoginStatusViewModel : ViewModelBase
    {
        private readonly AuthManager authManager;
        private readonly Lazy<LauncherViewModel> launcherVM;
        private readonly LoginViewModel loginVM;
        private string failedMessage;
        private bool isFailedVisible;
        private bool isResendEmailVisible;
        private bool resendClicked;
        private bool isWaitingVisible;

        public LoginStatusViewModel(AuthManager authManager, Lazy<LauncherViewModel> launcherVM,
            LoginViewModel loginVM)
        {
            IsFailedVisible = false;
            IsResendEmailVisible = false;
            ResendClicked = false;
            this.authManager = authManager;
            this.loginVM = loginVM;
            this.launcherVM = launcherVM;

            var hasAlreadyResent = this.WhenAnyValue(
                x => x.ResendClicked,
                (r) => !r);

            ResendEmail = ReactiveCommand.Create(OnResend, hasAlreadyResent);

            GoBack = ReactiveCommand.Create(GoBackToLogin);
            
            OpenLauncher = ReactiveCommand.Create(SignInComplete);

            if (!authManager.AttemptingAutoLogin)
            {
                UserLogin();
            }
            else
            {
                IsWaitingVisible = true;
            }
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
        
        public ReactiveCommand<Unit, LoginViewModel> GoBack { get; }
        public ReactiveCommand<Unit, Unit> ResendEmail { get; }
        public ReactiveCommand<Unit, LauncherViewModel> OpenLauncher { get; }

        public async void UserLogin()
        {
            bool signInSuccess = true;
            ResendClicked = false;
            IsResendEmailVisible = false;
            IsWaitingVisible = true;
            
            if (string.IsNullOrEmpty(authManager.LoginMsg.Email) ||
                string.IsNullOrEmpty(authManager.LoginMsg.Pass))
            {
                Log.Error("Login failed");
                FailedMessage = "Login failed.\r\n" +
                                "Check your email and password\r\n" +
                                "and try again.";
                return;
            }
            
            try
            {
                authManager.AuthLink = await authManager.SignInWithEmailAndPasswordAsync(authManager.LoginMsg.Email,
                    authManager.LoginMsg.Pass);
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

            authManager.LoginMsg = null;
            
            IsWaitingVisible = false;
            if (!signInSuccess)
            {
                IsFailedVisible = true;
                return;
            }

            authManager.Store();

            Observable.Start(() => {}).InvokeCommand(this, vm => vm.OpenLauncher);
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

        public LoginViewModel GoBackToLogin()
        {
            return loginVM;
        }
        public LauncherViewModel? SignInComplete()
        {
            return launcherVM.Value;
        }
    }
}
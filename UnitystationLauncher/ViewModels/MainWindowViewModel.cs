using System;
using ReactiveUI;
using System.Reactive.Linq;
using Serilog;
using System.Threading;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        ViewModelBase content;
        private Lazy<LauncherViewModel> launcherVM;
        private Lazy<LoginStatusViewModel> loginStatusVM;
        private AuthManager authManager;
        private LoginViewModel loginVM;

        public MainWindowViewModel(LoginViewModel loginVM, Lazy<LoginStatusViewModel> loginStatusVM, Lazy<LauncherViewModel> launcherVM,
            AuthManager authManager)
        {
            this.loginStatusVM = loginStatusVM;
            this.loginVM = loginVM;
            this.authManager = authManager;
            this.launcherVM = launcherVM;
            Content = loginVM;
            authManager.AttemptingAutoLogin = false;
            CheckForExistingUser();
        }

        public ViewModelBase Content
        {
            get => content;
            private set
            {
                this.RaiseAndSetIfChanged(ref content, value);
                ContentChanged();
            }
        }

        void CheckForExistingUser()
        {
            if (authManager.AuthLink != null)
            {
                authManager.AttemptingAutoLogin = true;
                Content = loginStatusVM.Value;
                AttemptAuthRefresh();
            }
        }

        async void AttemptAuthRefresh()
        {
            var refreshToken = new RefreshToken
            {
                userID = authManager.AuthLink.User.LocalId,
                refreshToken = authManager.AuthLink.RefreshToken
            };

            var token = await authManager.GetCustomToken(refreshToken, authManager.AuthLink.User.Email);

            if (string.IsNullOrEmpty(token))
            {
                Log.Error("Login failed");
                Content = loginVM;
                authManager.AttemptingAutoLogin = false;
                return;
            }

            try
            {
                authManager.AuthLink = await authManager.SignInWithCustomToken(token);
            }
            catch (Exception e)
            {
                Log.Error(e, "Login failed");
                Content = loginVM;
                authManager.AttemptingAutoLogin = false;
                return;
            }

            var user = await authManager.GetUpdatedUser();
            if (!user.IsEmailVerified)
            {
                Content = loginVM;
                authManager.AttemptingAutoLogin = false;
                return;
            }
            authManager.AttemptingAutoLogin = false;
            authManager.Store();
            Content = launcherVM.Value;
        }

        private void ContentChanged()
        {
            SubscribeToVM(Content switch
            {
                LoginViewModel loginVM => Observable.Merge(
                    loginVM.Login.Select(vm => (ViewModelBase)vm),
                    loginVM.Create.Select(vm => (ViewModelBase)vm),
                    loginVM.ForgotPW.Select(vm => (ViewModelBase)vm)),

                LoginStatusViewModel loginStatusVM => Observable.Merge(
                    loginStatusVM.GoBack.Select(vm => (ViewModelBase)vm),
                    loginStatusVM.OpenLauncher.Select(vm => (ViewModelBase)vm)),

                LauncherViewModel launcherVM => Observable.Merge(
                    launcherVM.Logout.Select(vm => (ViewModelBase)vm),
                    launcherVM.ShowUpdateReqd.Select(vm => (ViewModelBase)vm)),

                SignUpViewModel signUpViewModel => Observable.Merge(
                    signUpViewModel.Cancel,
                    signUpViewModel.DoneButton),

                HubUpdateViewModel hubUpdateViewModel => Observable.Merge(
                    hubUpdateViewModel.Cancel),

                ForgotPasswordViewModel forgotPasswordViewModel => Observable.Merge(
                    forgotPasswordViewModel.DoneButton),


                _ => throw new ArgumentException($"ViewModel type is not handled and will never be able to change")
            });
        }

        private void SubscribeToVM(IObservable<ViewModelBase?> observable)
        {
            observable
                .SkipWhile(vm => vm == null)
                .Take(1)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(vm => Content = vm);
        }
    }
}

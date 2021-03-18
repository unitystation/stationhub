using System;
using ReactiveUI;
using System.Reactive.Linq;
using Serilog;
using System.Threading;
using UnitystationLauncher.Models;
using System.Reactive;
using Avalonia.Media;

namespace UnitystationLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        ViewModelBase content;
        private Lazy<LauncherViewModel> launcherVM;
        private Lazy<LoginStatusViewModel> loginStatusVM;
        private AuthManager authManager;
        private LoginViewModel loginVM;

        private Geometry maximizeIcon;
        private string maximizeToolTip;

        public Geometry MaximizeIcon
        {
            get => maximizeIcon;
            set => this.RaiseAndSetIfChanged(ref maximizeIcon, value);
        }

        public string MaximizeToolTip
        {
            get => maximizeToolTip;
            set => this.RaiseAndSetIfChanged(ref maximizeToolTip, value);
        }

        public ReactiveCommand<Unit,Unit> CommandMaximizee { get; }

        public MainWindowViewModel(LoginViewModel loginVM, Lazy<LoginStatusViewModel> loginStatusVM, Lazy<LauncherViewModel> launcherVM,
            AuthManager authManager)
        {
            this.loginStatusVM = loginStatusVM;
            this.loginVM = loginVM;
            this.authManager = authManager;
            this.launcherVM = launcherVM;
            Content = loginVM;
            authManager.AttemptingAutoLogin = false;
            MaximizeIcon = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
            MaximizeToolTip = "Maximize";
            CommandMaximizee = ReactiveCommand.Create(Maximize, null);
            CheckForExistingUser();
        }

        private void Maximize()
        {
            switch (MaximizeToolTip)
            {
                case "Maximize":
                    MaximizeIcon = Geometry.Parse("M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
                    MaximizeToolTip = "Restore Down";
                    break;
                case "Restore Down":
                    MaximizeIcon = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                    MaximizeToolTip = "Maximize";
                    break;
            }
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

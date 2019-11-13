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
        private AuthManager authManager;
        private LoginViewModel loginVM;

        public MainWindowViewModel(LoginViewModel loginVM, Lazy<LauncherViewModel> launcherVM,
            AuthManager authManager)
        {
            this.loginVM = loginVM;
            this.authManager = authManager;
            this.launcherVM = launcherVM;
            Content = loginVM;
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
                loginVM.ShowSigningInScreen();
                AttemptAuthRefresh();
            }
        }

        async void AttemptAuthRefresh()
        {
            try
            {
                authManager.AuthLink = await authManager.AuthLink.GetFreshAuthAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, "Login failed");
                loginVM.ShowLoginForm();
                return;
            }

            var user = await authManager.GetUpdatedUser();
            if (!user.IsEmailVerified)
            {
                loginVM.ShowLoginForm();
                return;
            }
            
            authManager.Store();
            Content = launcherVM.Value;
        }

        private void ContentChanged()
        {
            SubscribeToVM(Content switch
            {
                LoginViewModel loginVM => Observable.Merge(
                    loginVM.Login.Select(vm => (ViewModelBase) vm),
                    loginVM.Create.Select(vm => (ViewModelBase) vm)),
                
                LauncherViewModel launcherVM => 
                    launcherVM.Logout,
                
                SignUpViewModel signUpViewModel => Observable.Merge(
                    signUpViewModel.Cancel,
                    signUpViewModel.DoneButton),
                
                _  => throw new ArgumentException($"ViewModel type is not handled and will never be able to change")
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

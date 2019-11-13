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

        public MainWindowViewModel(LoginViewModel loginVM, Lazy<LauncherViewModel> launcherVM,
            AuthManager authManager)
        {
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
                return;
            }
            
            authManager.Store();
            Content = launcherVM.Value;
        }

        private void ContentChanged()
        {
            switch(Content)
            {
                case LoginViewModel loginVM:
                    SubscribeToVM(loginVM.Login);
                    SubscribeToVM(loginVM.Create);
                    break;
                case LauncherViewModel launcherVM:
                    SubscribeToVM(launcherVM.Logout);
                    break;
                case SignUpViewModel signUpViewModel:
                    SubscribeToVM(signUpViewModel.Cancel);
                    break;
                    
            }
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

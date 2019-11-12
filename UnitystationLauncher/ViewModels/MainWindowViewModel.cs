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

        public MainWindowViewModel(LoginViewModel loginVM, Lazy<LauncherViewModel> launcherVM)
        {
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
            if (AuthManager.Instance.AuthLink != null)
            {
                AttemptAuthRefresh();
            }
        }

        async void AttemptAuthRefresh()
        {
            try
            {
                AuthManager.Instance.AuthLink = await AuthManager.Instance.AuthLink.GetFreshAuthAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, "Login failed");
                return;
            }
            
            AuthManager.Instance.Store();
            Content = launcherVM.Value;
        }

        private void ContentChanged()
        {
            switch(Content)
            {
                case LoginViewModel loginVM:
                    SubscribeToVM(
                        Observable.Merge(
                            loginVM.Login,
                            loginVM.Create));
                    break;
                case LauncherViewModel launcherVM:
                    SubscribeToVM(launcherVM.Logout);
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

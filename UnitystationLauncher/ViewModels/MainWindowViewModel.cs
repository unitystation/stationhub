using System;
using ReactiveUI;
using System.Reactive.Linq;
using Serilog;
using System.Threading;

namespace UnitystationLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        ViewModelBase content;

        public MainWindowViewModel(LoginViewModel loginVM)
        {
            Content = loginVM;
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

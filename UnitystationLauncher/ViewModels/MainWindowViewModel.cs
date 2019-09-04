using System;
using ReactiveUI;
using System.Reactive.Linq;

namespace UnitystationLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        ViewModelBase content;

        public MainWindowViewModel()
        {
            var login = new LoginViewModel();
            Content = login;

            Observable.Merge(
                login.Login,
                login.Create)
                .Take(1)
                .Subscribe(username =>
                {
                    Content = new LauncherViewModel(username);
                });
        }

        public ViewModelBase Content
        {
            get => content;
            private set => this.RaiseAndSetIfChanged(ref content, value);
        }
    }
}

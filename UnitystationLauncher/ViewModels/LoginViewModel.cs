using System;
using System.Reactive;
using DynamicData.Binding;
using ReactiveUI;

namespace UnitystationLauncher.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        string? username;
        string? password;

        public LoginViewModel()
        {
            var possibleCredentials = this.WhenAnyValue(
                x => x.Username,
                x => x.Password,
                (u, p) => 
                    !string.IsNullOrWhiteSpace(u) &&
                    !string.IsNullOrWhiteSpace(p));

            Login = ReactiveCommand.Create(
                UserLogin,
                possibleCredentials);

            Create = ReactiveCommand.Create(
                UserCreate,
                possibleCredentials);
        }

        public string Username
        {
            get => username;
            set => this.RaiseAndSetIfChanged(ref username, value);
        }

        public string Password
        {
            get => password;
            set => this.RaiseAndSetIfChanged(ref password, value);
        }

        public ReactiveCommand<Unit, string> Login { get; }
        public ReactiveCommand<Unit, string> Create { get; }

        public string UserLogin()
        {
            return Username;
        }

        public string UserCreate()
        {
            return Username;
        }
    }
}
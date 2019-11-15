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
        private readonly Lazy<SignUpViewModel> signUpVM;
        private readonly Lazy<LoginStatusViewModel> loginStatusVM;
        private readonly AuthManager authManager;
        string? email;
        string? password;

        public LoginViewModel(Lazy<LoginStatusViewModel> loginStatusVM,
            Lazy<SignUpViewModel> signUpVM, AuthManager authManager)
        {
            this.authManager = authManager;
            this.signUpVM = signUpVM;
            this.loginStatusVM = loginStatusVM;
            var possibleCredentials = this.WhenAnyValue(
                x => x.Email,
                x => x.Password,
                (u, p) =>
                    !string.IsNullOrWhiteSpace(u) &&
                    !string.IsNullOrWhiteSpace(p));

            Login = ReactiveCommand.Create(
                UserLogin,
                possibleCredentials);

            Create = ReactiveCommand.Create(
                UserCreate);
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

        public ReactiveCommand<Unit, LoginStatusViewModel?> Login { get; }
        public ReactiveCommand<Unit, SignUpViewModel?> Create { get; }

        public LoginStatusViewModel? UserLogin()
        {
            authManager.LoginMsg = new LoginMsg
            {
                Email = Email,
                Pass = Password
            };
            return loginStatusVM.Value;
        }
        
        public SignUpViewModel? UserCreate()
        {
            return signUpVM.Value;
        }
    }
}
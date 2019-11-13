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
    public class SignUpViewModel : ViewModelBase
    {
        private readonly AuthManager authManager;
        private readonly Lazy<LoginViewModel> loginVM;
        string? email;
        string? password;
        string? username;

        public SignUpViewModel(AuthManager authManager, Lazy<LoginViewModel> loginVM)
        {
            this.authManager = authManager;
            this.loginVM = loginVM;
            var possibleCredentials = this.WhenAnyValue(
                x => x.Email,
                x => x.Password,
                x => x.Username,
                (u, p, i) =>
                    !string.IsNullOrWhiteSpace(u) &&
                    !string.IsNullOrWhiteSpace(p) &&
                    !string.IsNullOrEmpty(i));
            
            Submit = ReactiveCommand.Create(
                UserCreate, possibleCredentials);
            
            Cancel = ReactiveCommand.Create(ReturnToLogin);
        }
        
        public string? Username
        {
            get => username;
            set => this.RaiseAndSetIfChanged(ref username, value);
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

        public ReactiveCommand<Unit, LoginViewModel?> Cancel { get; }
        public ReactiveCommand<Unit, Unit> Submit { get; }
        
        public void UserCreate()
        {
            
        }

        public LoginViewModel? ReturnToLogin()
        {
            return loginVM.Value;
        }
    }
}
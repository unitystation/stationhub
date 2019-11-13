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
        private readonly AuthManager authManager;
        private readonly Lazy<LauncherViewModel> launcherVM;
        private readonly Lazy<SignUpViewModel> signUpVM;
        string? email;
        string? password;

        public LoginViewModel(AuthManager authManager, Lazy<LauncherViewModel> launcherVM,
            Lazy<SignUpViewModel> signUpVM)
        {
            this.authManager = authManager;
            this.signUpVM = signUpVM;
            this.launcherVM = launcherVM;
            var possibleCredentials = this.WhenAnyValue(
                x => x.Email,
                x => x.Password,
                (u, p) =>
                    !string.IsNullOrWhiteSpace(u) &&
                    !string.IsNullOrWhiteSpace(p));

            Login = ReactiveCommand.CreateFromTask(
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

        public ReactiveCommand<Unit, LauncherViewModel?> Login { get; }
        public ReactiveCommand<Unit, SignUpViewModel?> Create { get; }
        
        public async Task<LauncherViewModel?> UserLogin()
        {
            try
            {
                authManager.AuthLink = await authManager.SignInWithEmailAndPasswordAsync(email!, password!);
            }
            catch (Exception e)
            {
                Log.Error(e, "Login failed");
                return null;
            }
            
            authManager.Store();
            
            return launcherVM.Value;
        }

        public SignUpViewModel? UserCreate()
        {
            return signUpVM.Value;
        }
    }
}
using System;
using System.Reactive;
using System.Threading.Tasks;
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
        string? email;
        string? password;

        public LoginViewModel(AuthManager authManager, Lazy<LauncherViewModel> launcherVM)
        {
            this.authManager = authManager;
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
                UserCreate,
                possibleCredentials);
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
        public ReactiveCommand<Unit, LauncherViewModel?> Create { get; }

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
            return launcherVM.Value;
        }

        public LauncherViewModel? UserCreate()
        {
            return null;
        }
    }
}
using System;
using System.Reactive;
using System.Threading.Tasks;
using DynamicData.Binding;
using Firebase.Auth;
using ReactiveUI;
using Serilog;

namespace UnitystationLauncher.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private const string ApiKey = "AIzaSyB7GorzPgwHYjSV4XaJoszj98tLM4_WZpE";
        private readonly FirebaseAuthProvider auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
        string? email;
        string? password;

        public LoginViewModel()
        {
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
            FirebaseAuthLink authLink;
            try
            {
                authLink = await auth.SignInWithEmailAndPasswordAsync(email, password);
            }
            catch (Exception e)
            {
                Log.Error("Login failed", e);
                return null;
            }
            return new LauncherViewModel(authLink);
        }

        public LauncherViewModel? UserCreate()
        {
            return null;
        }
    }
}
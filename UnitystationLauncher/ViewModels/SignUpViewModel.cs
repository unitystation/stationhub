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
        private string creationMessage;
        private string endButtonText;
        private bool isFormVisible;
        private bool isCreatedVisible;
        private bool creationSuccess = false;

        public SignUpViewModel(AuthManager authManager, Lazy<LoginViewModel> loginVM)
        {
            IsFormVisible = true;
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
            
            DoneButton = ReactiveCommand.Create(CreationEndButton);
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

        public bool IsFormVisible
        {
            get => isFormVisible;
            set => this.RaiseAndSetIfChanged(ref isFormVisible, value);
        }

        public bool IsCreatedVisible
        {
            get => isCreatedVisible;
            set => this.RaiseAndSetIfChanged(ref isCreatedVisible, value);
        }

        public string CreationMessage
        {
            get => creationMessage;
            set => this.RaiseAndSetIfChanged(ref creationMessage, value);
        }

        public string EndButtonText
        {
            get => endButtonText;
            set => this.RaiseAndSetIfChanged(ref endButtonText, value);
        }

        public ReactiveCommand<Unit, LoginViewModel?> Cancel { get; }
        public ReactiveCommand<Unit, LoginViewModel?> DoneButton { get; }
        public ReactiveCommand<Unit, Unit> Submit { get; }

        public async void UserCreate()
        {
            IsFormVisible = false;
            creationSuccess = true;
            
            try
            { 
                //var authLink = await authManager.CreateAccount(username, email, password);
            } catch (Exception e)
            {
                Log.Error(e, "Login failed");
                creationSuccess = false;
            }

            if (creationSuccess)
            {
                CreationMessage = $"Success! An email has been sent to {email}. \r\n" +
                                  $"Please click the link in the email to verify \r\n" +
                                  $"your account before signing in.";
                EndButtonText = "Done";
            }
            else
            {
                CreationMessage = $"Invalid email address. Please try again.";
                EndButtonText = "Back";
            }

            IsCreatedVisible = true;
        }

        public LoginViewModel? ReturnToLogin()
        {
            return loginVM.Value;
        }
        
        public LoginViewModel? CreationEndButton()
        {
            if (!creationSuccess) return null;
            return loginVM.Value;
        }
    }
}
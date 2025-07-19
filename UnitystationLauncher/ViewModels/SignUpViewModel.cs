using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Serilog;
using UnitystationLauncher.Services;
using ZstdSharp.Unsafe;

namespace UnitystationLauncher.ViewModels
{
    public class SignUpViewModel : ViewModelBase
    {
        private readonly AuthService _authService;
        private readonly Lazy<LoginViewModel> _loginVm;
        string _email = "";
        string _password = "";
        string _username = "";
        string _usernameID = "";
        private string? _creationMessage;
        private string? _endButtonText;
        private bool _isFormVisible;
        private bool _isWaitingVisible;
        private bool _isCreatedVisible;

        public string UsernameID
        {
            get => _usernameID;
            set => this.RaiseAndSetIfChanged(ref _usernameID, value);
        }
        
        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => this.RaiseAndSetIfChanged(ref _password, value);
        }

        public bool IsFormVisible
        {
            get => _isFormVisible;
            set => this.RaiseAndSetIfChanged(ref _isFormVisible, value);
        }

        public bool IsCreatedVisible
        {
            get => _isCreatedVisible;
            set => this.RaiseAndSetIfChanged(ref _isCreatedVisible, value);
        }

        public bool IsWaitingVisible
        {
            get => _isWaitingVisible;
            set => this.RaiseAndSetIfChanged(ref _isWaitingVisible, value);
        }

        public string? CreationMessage
        {
            get => _creationMessage;
            set => this.RaiseAndSetIfChanged(ref _creationMessage, value);
        }

        public string? EndButtonText
        {
            get => _endButtonText;
            set => this.RaiseAndSetIfChanged(ref _endButtonText, value);
        }

        public ReactiveCommand<Unit, LoginViewModel> Cancel { get; }
        public ReactiveCommand<Unit, LoginViewModel> DoneButton { get; }
        public ReactiveCommand<Unit, Unit> Submit { get; }

        public SignUpViewModel(AuthService authService, Lazy<LoginViewModel> loginVm)
        {
            IsFormVisible = true;
            IsWaitingVisible = false;
            IsCreatedVisible = false;
            _authService = authService;
            _loginVm = loginVm;
            var possibleCredentials = this.WhenAnyValue(
                x => x.Email,
                x => x.Password,
                x => x.Username,
                x => x.UsernameID,
                (u, p, i,m) =>
                    !string.IsNullOrWhiteSpace(u) &&
                    !string.IsNullOrWhiteSpace(p) &&
                    p.Length > 6 &&
                    !string.IsNullOrEmpty(i) &&
                    !string.IsNullOrEmpty(m));
            
            

            Submit = ReactiveCommand.CreateFromTask(
                UserCreateAsync, possibleCredentials);

            Cancel = ReactiveCommand.Create(ReturnToLogin);

            DoneButton = ReactiveCommand.Create(CreationEndButton);
        }

        public override void Refresh()
        {
            
        }

        public async Task UserCreateAsync()
        {
            IsFormVisible = false;
            var creationSuccess = true;
            IsWaitingVisible = true;

            try
            {
                await _authService.CreateAccountAsync(_usernameID, _username, _email, _password);
            }
            catch (Exception e)
            {
                Log.Error(e, "Login failed");
                creationSuccess = false;
            }

            if (creationSuccess)
            {
                CreationMessage = $"Success! An email has been sent to \r\n{_email}\r\n" +
                                  $"Please click the link in the email to verify\r\n" +
                                  $"your account before signing in.";
                EndButtonText = "Done";
            }
            else
            {
                CreationMessage = $"Something went wrong with the verification email server.\r\n" +
                    $"A reset password email has been sent to {_email} as a work around.\r\n" +
                    $"Please reset your password and try to log in.";
                _authService.SendForgotPasswordEmail(_email);
                EndButtonText = "Back";
            }

            IsWaitingVisible = false;
            IsCreatedVisible = true;
        }

        public LoginViewModel ReturnToLogin()
        {
            return _loginVm.Value;
        }

        public LoginViewModel CreationEndButton()
        {
            return _loginVm.Value;
        }
    }
}
using System;
using ReactiveUI;
using System.Reactive.Linq;
using Serilog;
using System.Threading;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using Avalonia.Media;
using UnitystationLauncher.Services;

namespace UnitystationLauncher.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly Lazy<LauncherViewModel> _launcherVm;
        private readonly Lazy<LoginStatusViewModel> _loginStatusVm;
        private readonly AuthService _authService;
        private readonly LoginViewModel _loginVm;

        ViewModelBase _content;
        private Geometry _maximizeIcon;
        private string _maximizeToolTip;

        public Geometry MaximizeIcon
        {
            get => _maximizeIcon;
            set => this.RaiseAndSetIfChanged(ref _maximizeIcon, value);
        }

        public string MaximizeToolTip
        {
            get => _maximizeToolTip;
            set => this.RaiseAndSetIfChanged(ref _maximizeToolTip, value);
        }

        public ReactiveCommand<Unit, Unit> CommandMaximizee { get; }

        public MainWindowViewModel(LoginViewModel loginVm, Lazy<LoginStatusViewModel> loginStatusVm, Lazy<LauncherViewModel> launcherVm,
            AuthService authService)
        {
            _loginStatusVm = loginStatusVm;
            _loginVm = loginVm;
            _authService = authService;
            _launcherVm = launcherVm;
            Content = _content = loginVm;
            authService.AttemptingAutoLogin = false;
            _maximizeIcon = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
            _maximizeToolTip = "Maximize";
            CommandMaximizee = ReactiveCommand.Create(Maximize);
            RxApp.MainThreadScheduler.ScheduleAsync((scheduler, ct) => CheckForExistingUserAsync());
        }

        private void Maximize()
        {
            if (MaximizeToolTip == "Maximize")
            {
                MaximizeIcon =
                    Geometry.Parse(
                        "M2048 1638h-410v410h-1638v-1638h410v-410h1638v1638zm-614-1024h-1229v1229h1229v-1229zm409-409h-1229v205h1024v1024h205v-1229z");
                MaximizeToolTip = "Restore Down";
            }
            else if (MaximizeToolTip == "Restore Down")
            {
                MaximizeIcon = Geometry.Parse("M2048 2048v-2048h-2048v2048h2048zM1843 1843h-1638v-1638h1638v1638z");
                MaximizeToolTip = "Maximize";
            }
        }

        public ViewModelBase Content
        {
            get => _content;
            private set
            {
                this.RaiseAndSetIfChanged(ref _content, value);
                ContentChanged();
            }
        }

        async Task CheckForExistingUserAsync()
        {
            if (_authService.AuthLink != null)
            {
                _authService.AttemptingAutoLogin = true;
                Content = _loginStatusVm.Value;
                await AttemptAuthRefreshAsync();
            }
        }

        async Task AttemptAuthRefreshAsync()
        {
            if (_authService.AuthLink == null)
            {
                Log.Error("Login failed");
                Content = _loginVm;
                _authService.AttemptingAutoLogin = false;
                return;
            }

            RefreshToken refreshToken = new()
            {
                UserId = _authService.AuthLink.User.LocalId,
                Token = _authService.AuthLink.RefreshToken
            };

            string token = await _authService.GetCustomTokenAsync(refreshToken);

            if (string.IsNullOrEmpty(token))
            {
                Log.Error("Login failed");
                Content = _loginVm;
                _authService.AttemptingAutoLogin = false;
                return;
            }

            try
            {
                _authService.AuthLink = await _authService.SignInWithCustomTokenAsync(token);
            }
            catch (Exception e)
            {
                Log.Error(e, "Login failed");
                Content = _loginVm;
                _authService.AttemptingAutoLogin = false;
                return;
            }

            var user = await _authService.GetUpdatedUserAsync();
            if (!user.IsEmailVerified)
            {
                Content = _loginVm;
                _authService.AttemptingAutoLogin = false;
                return;
            }
            _authService.AttemptingAutoLogin = false;
            _authService.SaveAuthSettings();
            Content = _launcherVm.Value;
        }

        private void ContentChanged()
        {
            SubscribeToVm(Content switch
            {
                LoginViewModel loginVm => Observable.Merge(
                    loginVm.Login.Select(vm => (ViewModelBase)vm),
                    loginVm.Create.Select(vm => (ViewModelBase)vm),
                    loginVm.ForgotPw.Select(vm => (ViewModelBase)vm)),

                LoginStatusViewModel loginStatusVm => Observable.Merge(
                    loginStatusVm.GoBack.Select(vm => (ViewModelBase)vm),
                    loginStatusVm.OpenLauncher.Select(vm => (ViewModelBase)vm)),

                LauncherViewModel launcherVm => Observable.Merge(
                    launcherVm.Logout.Select(vm => (ViewModelBase)vm),
                    launcherVm.ShowUpdateReqd.Select(vm => (ViewModelBase)vm)),

                SignUpViewModel signUpViewModel => Observable.Merge(
                    signUpViewModel.Cancel,
                    signUpViewModel.DoneButton),

                HubUpdateViewModel hubUpdateViewModel => Observable.Merge(
                    hubUpdateViewModel.Cancel),

                ForgotPasswordViewModel forgotPasswordViewModel => Observable.Merge(
                    forgotPasswordViewModel.DoneButton),


                _ => throw new ArgumentException($"ViewModel type is not handled and will never be able to change")
            });
        }

        private void SubscribeToVm(IObservable<ViewModelBase?> observable)
        {
            observable
                .SkipWhile(vm => vm == null)
                .Select(vm => vm!)
                .Take(1)
                .ObserveOn(SynchronizationContext.Current!)
                .Subscribe(vm => Content = vm);
        }
    }
}

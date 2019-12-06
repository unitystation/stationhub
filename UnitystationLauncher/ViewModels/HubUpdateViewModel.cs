using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class HubUpdateViewModel : ViewModelBase
    {
        private readonly Lazy<LoginViewModel> loginVM;
        private string updateMessage;
        public HubUpdateViewModel(Lazy<LoginViewModel> loginVM)
        {
            this.loginVM = loginVM;
            GoBack = ReactiveCommand.Create(GoBackToLogin);

            UpdateMessage = $"An update is required before you can continue.\n\rPlease download" +
                $" the latest version by clicking the button below." +
                $"\n\rCurrent version: ({Config.currentBuild}). Newest version: ({Config.serverHubClientConfig.buildNumber})";
        }

        public ReactiveCommand<Unit, LoginViewModel> GoBack { get; }

        public string UpdateMessage
        {
            get => updateMessage;
            set => this.RaiseAndSetIfChanged(ref updateMessage, value);
        }

        public LoginViewModel GoBackToLogin()
        {
            return loginVM.Value;
        }
    }
}

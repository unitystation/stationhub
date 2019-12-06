using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class HubUpdateViewModel : ViewModelBase
    {
        private readonly Lazy<LoginViewModel> loginVM;
        private string updateTitle;
        private string updateMessage;
        private string buttonMessage;
        private string downloadMessage;
        private bool installButtonVisible;
        private bool downloadBarVisible;
        private bool restartButtonVisible;
        public HubUpdateViewModel(Lazy<LoginViewModel> loginVM)
        {
            this.loginVM = loginVM;
            BeginDownload = ReactiveCommand.Create(UpdateHub);
            Cancel = ReactiveCommand.Create(CancelInstall);

            UpdateTitle = "Update Required";
            UpdateMessage = $"An update is required before you can continue.\n\rPlease install" +
                $" the latest version by clicking the button below:";

            ButtonMessage = $"Update Hub";

            InstallButtonVisible = true;
            DownloadBarVisible = false;
            RestartButtonVisible = false;
        }

        public ReactiveCommand<Unit, Unit> BeginDownload { get; }
        public ReactiveCommand<Unit, ViewModelBase> Cancel { get; }

        public bool InstallButtonVisible
        {
            get => installButtonVisible;
            set => this.RaiseAndSetIfChanged(ref installButtonVisible, value);
        }

        public bool DownloadBarVisible
        {
            get => downloadBarVisible;
            set => this.RaiseAndSetIfChanged(ref downloadBarVisible, value);
        }

        public bool RestartButtonVisible
        {
            get => restartButtonVisible;
            set => this.RaiseAndSetIfChanged(ref restartButtonVisible, value);
        }

        public string UpdateTitle
        {
            get => updateTitle;
            set => this.RaiseAndSetIfChanged(ref updateTitle, value);
        }

        public string UpdateMessage
        {
            get => updateMessage;
            set => this.RaiseAndSetIfChanged(ref updateMessage, value);
        }

        public string ButtonMessage
        {
            get => buttonMessage;
            set => this.RaiseAndSetIfChanged(ref buttonMessage, value);
        }

        public string DownloadMessage
        {
            get => downloadMessage;
            set => this.RaiseAndSetIfChanged(ref downloadMessage, value);
        }

        public void UpdateHub()
        {
            TryUpdate();
        }

        async Task TryUpdate()
        {
            InstallButtonVisible = false;
            DownloadBarVisible = true;
            UpdateTitle = "Downloading...";
        }

        ViewModelBase CancelInstall()
        {
            return loginVM.Value;
        }
    }
}

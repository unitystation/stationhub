using ReactiveUI;
using UnitystationLauncher.Models;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Reactive;
using System.IO;
using System.Runtime.InteropServices;
using System;
using System.Net;
using System.IO.Compression;
using System.Net.Sockets;
using Firebase.Auth;

namespace UnitystationLauncher.ViewModels
{
    public class LauncherViewModel : ViewModelBase
    {
        private readonly AuthManager authManager;
        private readonly Lazy<LoginViewModel> logoutVM;
        string username;
        ViewModelBase news;
        PanelBase[] panels;
        ViewModelBase? selectedPanel;

        public LauncherViewModel(
            AuthManager authManager, 
            ServersPanelViewModel serversPanel, 
            InstallationsPanelViewModel installationsPanel,
            NewsViewModel news,
            Lazy<LoginViewModel> logoutVM)
        {
            this.authManager = authManager;
            this.logoutVM = logoutVM;
            News = news;
            Panels = new PanelBase[]
            {
                serversPanel,
                installationsPanel
            };
            Username = this.authManager!.AuthLink.User.DisplayName;
            Logout = ReactiveCommand.Create(LogoutImp);
            SelectedPanel = serversPanel;
        }

        public string Username
        {
            get => username;
            set => this.RaiseAndSetIfChanged(ref username, value);
        }

        public ViewModelBase News
        {
            get => news;
            set => this.RaiseAndSetIfChanged(ref news, value);
        }

        public PanelBase[] Panels
        {
            get => panels;
            set => this.RaiseAndSetIfChanged(ref panels, value);
        }

        public ViewModelBase? SelectedPanel
        {
            get => selectedPanel;
            set => this.RaiseAndSetIfChanged(ref selectedPanel, value);
        }

        public ReactiveCommand<Unit, ViewModelBase> Logout { get; }

        ViewModelBase LogoutImp()
        {
            File.Delete("settings.json");
            return logoutVM.Value;
        }
    }
}

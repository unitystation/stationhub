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
        private readonly FirebaseAuthLink authLink;
        string username;
        ViewModelBase news;
        ServerListViewModel serverList;

        public LauncherViewModel(FirebaseAuthLink authLink)
        {
            this.authLink = authLink;
            this.Username = authLink.User.DisplayName;
            News = new NewsViewModel();
            ServerList = new ServerListViewModel();
            Logout = ReactiveCommand.Create(LogoutImp);
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

        public ServerListViewModel ServerList
        {
            get => serverList;
            set => this.RaiseAndSetIfChanged(ref serverList, value);
        }

        public ReactiveCommand<Unit, ViewModelBase> Logout { get; }

        ViewModelBase LogoutImp()
        {
            return new LoginViewModel();
        }
    }
}

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

namespace UnitystationLauncher.ViewModels
{
    public class LauncherViewModel : ViewModelBase
    {
        string username;
        ViewModelBase news;
        ServerListViewModel serverList;

        public LauncherViewModel(string username)
        {
            this.Username = username;
            News = new NewsViewModel();
            ServerList = new ServerListViewModel();
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
    }
}

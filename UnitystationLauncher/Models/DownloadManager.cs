using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using Avalonia.Collections;
using Serilog;

namespace UnitystationLauncher.Models
{

    public class DownloadManager
    {
        private readonly HttpClient http;
        private readonly AvaloniaList<Download> downloads;

        public DownloadManager(HttpClient http)
        {
            downloads = new AvaloniaList<Download>();
            this.http = http;
            Downloads.GetWeakCollectionChangedObservable().Subscribe(x => Log.Information("Downloads changed"));
        }

        public IAvaloniaReadOnlyList<Download> Downloads => downloads;

        public Download AddDownload(string url, string installationPath)
        {
            var download = new Download(url, installationPath, http);
            downloads.Add(download);
            return download;
        }

        public Download Download(Server server)
        {
            var download = new Download(server.DownloadUrl, server.InstallationPath, http);
            downloads.Add(download);
            return download;
        }

        public bool CanDownload(Server server)
        {
            return !Directory.Exists(server.InstallationPath);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using Avalonia.Collections;

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
            downloads.GetWeakCollectionChangedObservable();
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
    }
}
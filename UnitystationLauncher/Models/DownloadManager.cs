using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;

namespace UnitystationLauncher.Models{

    public class DownloadManager
    {
        private readonly HttpClient http;
        private readonly ObservableCollection<Download> downloads;

        public DownloadManager(HttpClient http)
        {
            downloads = new ObservableCollection<Download>();
            Downloads = new ReadOnlyObservableCollection<Download>(downloads);
            this.http = http;
        }

        public ReadOnlyObservableCollection<Download> Downloads { get; }

        public Download AddDownload(string url, string installationPath)
        {
            var download = new Download(url, installationPath, http);
            downloads.Add(download);
            return download;
        }
    }
}
using System;
using System.IO;
using Avalonia.Collections;
using Serilog;

namespace UnitystationLauncher.Models
{
    public class DownloadManager
    {
        private readonly AvaloniaList<Download> _downloads;

        public DownloadManager()
        {
            _downloads = new AvaloniaList<Download>();
            Downloads.GetWeakCollectionChangedObservable().Subscribe(x => Log.Information("Downloads changed"));
        }

        public IAvaloniaReadOnlyList<Download> Downloads => _downloads;

        public Download AddDownload(string url, string installationPath)
        {
            var download = new Download(url, installationPath);
            _downloads.Add(download);
            return download;
        }

        public Download Download(Server server)
        {
            if (server.DownloadUrl == null)
            {
                throw new ArgumentNullException(nameof(server.DownloadUrl));
            }

            var download = new Download(server.DownloadUrl, server.InstallationPath);
            _downloads.Add(download);
            return download;
        }

        public bool CanDownload(Server server)
        {
            return !Directory.Exists(server.InstallationPath);
        }
    }
}
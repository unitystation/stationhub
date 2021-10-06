using System;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<Download?> Download(Server server)
        {
            if (server.DownloadUrl == null)
            {
                throw new ArgumentNullException(nameof(server.DownloadUrl));
            }

            var download = new Download(server.DownloadUrl, server.InstallationPath);
            if (!download.CanStart())
            {
                return null;
            }

            _downloads.Remove(_downloads.FirstOrDefault(d => d.ForkAndVersion == download.ForkAndVersion));

            _downloads.Add(download);
            await download.StartAsync();
            return download;
        }

        public bool CanDownload(Server server)
        {
            if (server.DownloadUrl == null)
            {
                Log.Warning("Unable to download because DownloadUrl is null");
                return false;
            }
            var download = new Download(server.DownloadUrl, server.InstallationPath);
            return download.CanStart();
        }
    }
}
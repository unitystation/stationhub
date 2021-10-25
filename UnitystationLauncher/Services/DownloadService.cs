using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Collections;
using Serilog;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;

namespace UnitystationLauncher.Services
{
    public class DownloadService
    {
        private readonly HttpClient _http;
        private readonly AvaloniaList<Download> _downloads;

        public DownloadService(HttpClient http)
        {
            _http = http;
            _downloads = new AvaloniaList<Download>();
            Downloads.GetWeakCollectionChangedObservable().Subscribe(x => Log.Information("Downloads changed"));
        }

        public IAvaloniaReadOnlyList<Download> Downloads => _downloads;

        public async Task<Download?> DownloadAsync(Server server)
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
            await download.StartAsync(_http);
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
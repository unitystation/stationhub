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
            _downloads = new();
            Downloads.GetWeakCollectionChangedObservable().Subscribe(_ => Log.Information("Downloads changed"));
        }

        public IAvaloniaReadOnlyList<Download> Downloads => _downloads;

        public async Task DownloadAsync(Server server)
        {
            if (server.DownloadUrl == null)
            {
                throw new ArgumentNullException(nameof(server.DownloadUrl));
            }

            Download download = new(server.DownloadUrl, server.InstallationPath);
            if (!download.CanStart())
            {
                return;
            }

            Download? itemToRemove = _downloads.FirstOrDefault(d => d.ForkAndVersion == download.ForkAndVersion);
            if (itemToRemove != null)
            {
                _downloads.Remove(itemToRemove);
            }

            _downloads.Add(download);
            await download.StartAsync(_http);
        }

        public bool CanDownload(Server server)
        {
            if (server.DownloadUrl == null)
            {
                Log.Warning("Unable to download because DownloadUrl is null");
                return false;
            }
            Download download = new Download(server.DownloadUrl, server.InstallationPath);
            return download.CanStart();
        }
    }
}
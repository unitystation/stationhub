using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia.Collections;
using Serilog;
using UnitystationLauncher.Models;
using UnitystationLauncher.Models.Api;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services
{
    public class DownloadService : IDownloadService
    {
        private readonly HttpClient _http;
        private readonly AvaloniaList<Download> _downloads;
        private readonly IPreferencesService _preferencesService;

        public DownloadService(HttpClient http, IPreferencesService preferencesService)
        {
            _http = http;
            _downloads = new();
            _preferencesService = preferencesService;
            _downloads.GetWeakCollectionChangedObservable().Subscribe(_ => Log.Information("Downloads changed"));
        }

        public IAvaloniaReadOnlyList<Download> GetDownloads()
        {
            return _downloads;
        }

        public async Task DownloadAsync(Server server)
        {
            if (server.DownloadUrl == null)
            {
                throw new ArgumentNullException(nameof(server.DownloadUrl));
            }

            Download download = new(server.DownloadUrl, server.GetInstallationPath(_preferencesService.GetPreferences()), _preferencesService);
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

            Download download = new Download(server.DownloadUrl, server.GetInstallationPath(_preferencesService.GetPreferences()), _preferencesService);
            return download.CanStart();
        }
    }
}
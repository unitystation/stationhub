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
        private readonly IEnvironmentService _environmentService;

        public DownloadService(HttpClient http, IPreferencesService preferencesService, IEnvironmentService environmentService)
        {
            _http = http;
            _downloads = new();
            _preferencesService = preferencesService;
            _environmentService = environmentService;
            _downloads.GetWeakCollectionChangedObservable().Subscribe(_ => Log.Information("Downloads changed"));
        }

        public IAvaloniaReadOnlyList<Download> GetDownloads()
        {
            return _downloads;
        }

        public async Task DownloadAsync(Server server)
        {
            string? downloadUrl = server.GetDownloadUrl(_environmentService);
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                throw new OperationCanceledException("Null or empty download url");
            }

            Download download = new(downloadUrl, server.GetInstallationPath(_preferencesService.GetPreferences()), _preferencesService, _environmentService);
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
            string? downloadUrl = server.GetDownloadUrl(_environmentService);
            if (string.IsNullOrWhiteSpace(downloadUrl))
            {
                Log.Warning("Null or empty download url");
                return false;
            }

            Download download = new(downloadUrl, server.GetInstallationPath(_preferencesService.GetPreferences()), _preferencesService, _environmentService);
            return download.CanStart();
        }
    }
}
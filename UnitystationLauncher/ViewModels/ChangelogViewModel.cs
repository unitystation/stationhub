using System;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models.Api.Changelog;

namespace UnitystationLauncher.ViewModels
{
    public class ChangelogViewModel : ViewModelBase
    {
        private ObservableCollection<VersionViewModel> Versions { get; }
        private HttpClient HttpClient { get; }

        public ChangelogViewModel(HttpClient httpClient)
        {
            HttpClient = httpClient;
            Versions = new();

            RxApp.TaskpoolScheduler.ScheduleAsync(GetChangesAsync);
        }

        private async Task GetChangesAsync(IScheduler _, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            try
            {
                HttpResponseMessage response = await HttpClient.GetAsync(ApiUrls.Latest10VersionsUrl, cancellationToken);
                Log.Information("Changelog fetched");

                string apiJson = await response.Content.ReadAsStringAsync(cancellationToken);
                Changelog? changelog = JsonSerializer.Deserialize<Changelog>(apiJson);

                if (changelog == null)
                {
                    Log.Error("Error: {Error}", "Unable to read changelog!");
                    return;
                }

                if (changelog.Results != null)
                {
                    foreach (GameVersion version in changelog.Results)
                    {
                        Versions.Add(new(version));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Error: {Error}", $"Something went wrong reading from the changelog API: {e.Message}");
            }
        }
    }
}
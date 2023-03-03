using System.Collections.Generic;
using ReactiveUI;
using Octokit;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace UnitystationLauncher.ViewModels
{
    public class ChangelogViewModel : ViewModelBase
    {
        private readonly GitHubClient _client;

        ObservableCollection<PullRequestViewModel> PullRequests { get; }

        public ChangelogViewModel()
        {
            _client = new GitHubClient(new ProductHeaderValue("UnitystationCommitNews"));
            PullRequests = new();
            
            RxApp.TaskpoolScheduler.ScheduleAsync(GetPullRequestsAsync);
        }

        private async Task GetPullRequestsAsync(IScheduler scheduler, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
            
            PullRequestRequest options = new()
            {
                State = ItemStateFilter.Closed
            };
            
            ApiOptions apiOptions = new()
            {
                PageCount = 1,
                PageSize = 10,
            };

            try
            {
                IReadOnlyList<PullRequest> closedPrs = await _client.Repository.PullRequest.GetAllForRepository("unitystation", "unitystation", options, apiOptions);
                Log.Information("PR list fetched");

                foreach (PullRequest pr in closedPrs)
                {
                    if (pr.Merged)
                    {
                        PullRequests.Add(new PullRequestViewModel(pr));
                    }
                }
            }
            catch
            {
                //Rate limit has probably exceeded
            }
        }
    }
}
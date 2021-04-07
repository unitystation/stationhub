using ReactiveUI;
using Octokit;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Threading.Tasks;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels
{
    public class NewsViewModel : ViewModelBase
    {
        readonly GitHubClient _client;
        ObservableCollection<PullRequestWrapper> _pullRequests = new ObservableCollection<PullRequestWrapper>();

        public ObservableCollection<PullRequestWrapper> PullRequests
        {
            get => _pullRequests;
            set => this.RaiseAndSetIfChanged(ref _pullRequests, value);
        }

        public NewsViewModel()
        {
            _client = new GitHubClient(new ProductHeaderValue("UnitystationCommitNews"));
            RxApp.MainThreadScheduler.Schedule(async () => await GetPullRequests());
        }

        public async Task GetPullRequests()
        {
            PullRequestRequest options = new PullRequestRequest();
            ApiOptions apiOptions = new ApiOptions();
            apiOptions.PageCount = 1;
            apiOptions.PageSize = 10;
            options.State = ItemStateFilter.Closed;
            for (int i = PullRequests.Count - 1; i > 0; i--)
            {
                PullRequests.RemoveAt(i);
            }

            try
            {
                var getList = await _client.Repository.PullRequest.GetAllForRepository("unitystation", "unitystation", options, apiOptions);

                foreach (var pr in getList)
                {
                    if (pr.Merged)
                    {
                        PullRequests.Add(new PullRequestWrapper(pr));
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
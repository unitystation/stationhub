using ReactiveUI;
using Octokit;
using System.Collections.ObjectModel;
using System.Reactive.Concurrency;
using System.Threading.Tasks;

namespace UnitystationLauncher.ViewModels
{
    public class ChangelogViewModel : ViewModelBase
    {
        readonly GitHubClient _client;
        ObservableCollection<PullRequestViewModel> _pullRequests = new ObservableCollection<PullRequestViewModel>();

        public ObservableCollection<PullRequestViewModel> PullRequests
        {
            get => _pullRequests;
            set => this.RaiseAndSetIfChanged(ref _pullRequests, value);
        }

        public ChangelogViewModel()
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

            try
            {
                var closedPrs = await _client.Repository.PullRequest.GetAllForRepository("unitystation", "unitystation", options, apiOptions);

                foreach (var pr in closedPrs)
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
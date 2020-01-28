using ReactiveUI;
using Octokit;
using System.Reactive.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels{
    public class NewsViewModel : ViewModelBase
    {
        GitHubClient client;
        ObservableCollection<PullRequestWrapper> pullRequests = new ObservableCollection<PullRequestWrapper>();

        public NewsViewModel()
        {
            GetPullRequests();
        }

        async void GetPullRequests()
        {
            client = new GitHubClient(new ProductHeaderValue("UnitystationCommitNews"));
            PullRequestRequest options = new PullRequestRequest();
            ApiOptions apiOptions = new ApiOptions();
            apiOptions.PageCount = 1;
            apiOptions.PageSize = 10;
            options.State = ItemStateFilter.Closed;

            try
            {
                var getList = await client.Repository.PullRequest.GetAllForRepository("unitystation", "unitystation", options, apiOptions);

                foreach (var pr in getList)
                {
                    if (pr.Merged) PullRequests.Add(new PullRequestWrapper(pr));
                }
            } catch
            {
                //Rate limit has probably exceeded
            }
        }

        public ObservableCollection<PullRequestWrapper> PullRequests{
            get => pullRequests;
            set => this.RaiseAndSetIfChanged(ref pullRequests, value);
        }
    }
}
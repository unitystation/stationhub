using ReactiveUI;
using Octokit;
using Octokit.Reactive;
using System.Reactive.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading;

namespace UnitystationLauncher.ViewModels{
    public class NewsViewModel : ViewModelBase
    {
        ObservableGitHubClient client;
        ObservableCollection<PullRequest> pullRequests = new ObservableCollection<PullRequest>();

        public NewsViewModel()
        {
            client = new ObservableGitHubClient(new ProductHeaderValue("UnitystationCommitNews"));
            PullRequestRequest options = new PullRequestRequest();
            options.State = ItemStateFilter.Closed;

            client.Repository.PullRequest
                .GetAllForRepository("unitystation", "unitystation", options)
                .Take(10)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(pr => {
                    PullRequests.Add(pr);
                });
        }

        public ObservableCollection<PullRequest> PullRequests{
            get => pullRequests;
            set => this.RaiseAndSetIfChanged(ref pullRequests, value);
        }
    }
}
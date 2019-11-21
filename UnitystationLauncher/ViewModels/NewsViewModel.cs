using ReactiveUI;
using Octokit;
using Octokit.Reactive;
using System.Reactive.Linq;
using System;
using System.Collections.ObjectModel;
using System.Threading;
using UnitystationLauncher.Models;

namespace UnitystationLauncher.ViewModels{
    public class NewsViewModel : ViewModelBase
    {
        ObservableGitHubClient client;
        ObservableCollection<PullRequestWrapper> pullRequests = new ObservableCollection<PullRequestWrapper>();

        public NewsViewModel()
        {
            client = new ObservableGitHubClient(new ProductHeaderValue("UnitystationCommitNews"));
            PullRequestRequest options = new PullRequestRequest();
            options.State = ItemStateFilter.Closed;

            client.Repository.PullRequest
                .GetAllForRepository("unitystation", "unitystation", options)
                .Take(20)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(pr => {
                    if(pr.Merged) PullRequests.Add(new PullRequestWrapper(pr));
                });
        }

        public ObservableCollection<PullRequestWrapper> PullRequests{
            get => pullRequests;
            set => this.RaiseAndSetIfChanged(ref pullRequests, value);
        }
    }
}
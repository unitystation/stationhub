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
        ObservableCollection<Commit> commits = new ObservableCollection<Commit>();

        public NewsViewModel()
        {
            client = new ObservableGitHubClient(new ProductHeaderValue("UnitystationCommitNews"));
            client.Repository.Commit
                .GetAll("unitystation","unitystation")
                .Take(10)
                .ObserveOn(SynchronizationContext.Current)
                .Subscribe(commit =>{
                    Commits.Add(commit.Commit);
                });
        }

        public ObservableCollection<Commit> Commits{
            get => commits;
            set => this.RaiseAndSetIfChanged(ref commits, value);
        }
    }
}
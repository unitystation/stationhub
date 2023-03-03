using System;
using Octokit;

namespace UnitystationLauncher.ViewModels
{
    public class PullRequestViewModel : ViewModelBase
    {
        public string Title { get; }
        public DateTimeOffset? MergedAt { get; }
        public string Url { get; }
        public string ContributedBy { get; }
        
        public PullRequestViewModel(PullRequest pr)
        {
            Title = pr.Title;
            MergedAt = pr.MergedAt;
            Url = pr.HtmlUrl;
            ContributedBy = $"By: {pr.User.Login}";
        }
    }
}

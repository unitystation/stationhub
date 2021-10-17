using System;
using Octokit;

namespace UnitystationLauncher.ViewModels
{
    public class PullRequestViewModel : ViewModelBase
    {
        public PullRequestViewModel(PullRequest pr)
        {
            Title = pr.Title;
            MergedAt = pr.MergedAt;
            Url = pr.HtmlUrl;
        }

        public string Title { get; set; }
        public DateTimeOffset? MergedAt { get; set; }
        public string Url { get; set; }
    }
}

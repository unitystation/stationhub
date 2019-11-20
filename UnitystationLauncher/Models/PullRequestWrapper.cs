using Octokit;
using ReactiveUI;
using System.Reactive;
using System;
using System.Diagnostics;

namespace UnitystationLauncher.Models
{
    public class PullRequestWrapper : PullRequest
    {
        public PullRequestWrapper(PullRequest pr)
        {
            Title = pr.Title;
            Url = pr.HtmlUrl;
            VisitURL = ReactiveCommand.Create(LaunchURL, null);
        }

        public ReactiveCommand<Unit,Unit> VisitURL { get; }
        public void LaunchURL()
        {
            var ps = new ProcessStartInfo(Url)
            {
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(ps);
        }
    }
}

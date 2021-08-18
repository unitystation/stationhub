using Octokit;
using ReactiveUI;
using System.Diagnostics;

namespace UnitystationLauncher.Models
{
    public class PullRequestWrapper : PullRequest
    {
        public PullRequestWrapper(PullRequest pr)
        {
            Title = pr.Title;
            MergedAt = pr.MergedAt;
            Url = pr.HtmlUrl;
            ReactiveCommand.Create(LaunchUrl);
        }

        private void LaunchUrl()
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

using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Diagnostics;
using UnitystationLauncher.Constants;

namespace UnitystationLauncher.ViewModels
{
    public class NewsPanelViewModel : PanelBase
    {
        public override string Name => "News";

        public override bool IsEnabled => true;

        private ViewModelBase _currentBlogPost;
        private ViewModelBase CurrentBlogPost
        {
            get => _currentBlogPost;
            set => this.RaiseAndSetIfChanged(ref _currentBlogPost, value);
        }

        private ViewModelBase _changelog;
        public ViewModelBase Changelog
        {
            get => _changelog;
            set => this.RaiseAndSetIfChanged(ref _changelog, value);
        }

        public ReactiveCommand<Unit, Unit> OpenMainSite { get; }
        public ReactiveCommand<Unit, Unit> OpenPatreon { get; }
        public ReactiveCommand<Unit, Unit> OpenGameIssues { get; }
        public ReactiveCommand<Unit, Unit> OpenLauncherIssues { get; }
        public ReactiveCommand<Unit, Unit> OpenDiscordInvite { get; }

        private ObservableCollection<BlogPostViewModel> BlogPosts { get; }
        public ReactiveCommand<Unit, Unit> NextBlog { get; }
        public ReactiveCommand<Unit, Unit> PreviousBlog { get; }
        private int CurrentBlogPostIndex { get; set; }



        public NewsPanelViewModel(ChangelogViewModel changelog)
        {
            _changelog = changelog;

            OpenMainSite = ReactiveCommand.Create(() => OpenLink(LinkUrls.MainSiteUrl));
            OpenPatreon = ReactiveCommand.Create(() => OpenLink(LinkUrls.PatreonUrl));
            OpenGameIssues = ReactiveCommand.Create(() => OpenLink(LinkUrls.GameIssuesUrl));
            OpenLauncherIssues = ReactiveCommand.Create(() => OpenLink(LinkUrls.LauncherIssuesUrl));
            OpenDiscordInvite = ReactiveCommand.Create(() => OpenLink(LinkUrls.DiscordInviteUrl));

            BlogPosts = new();
            FetchBlogPosts();
            NextBlog = ReactiveCommand.Create(NextPost);
            PreviousBlog = ReactiveCommand.Create(PreviousPost);
            CurrentBlogPost = new BlogPostViewModel("Loading...", string.Empty, null);
            CurrentBlogPostIndex = 0;

            SetCurrentBlogPost();
        }

        private void FetchBlogPosts()
        {
            // TODO, fetch from blog post API, for now we can just hard code one.
            BlogPosts.Add(new("Coming soon!", "https://www.unitystation.org/blog", null));
        }

        private void SetCurrentBlogPost()
        {
            CurrentBlogPost = BlogPosts[CurrentBlogPostIndex];
        }

        private void NextPost()
        {
            CurrentBlogPostIndex++;
            if (CurrentBlogPostIndex >= BlogPosts.Count)
            {
                CurrentBlogPostIndex = 0;
            }

            SetCurrentBlogPost();
        }

        private void PreviousPost()
        {
            CurrentBlogPostIndex--;
            if (CurrentBlogPostIndex <= 0)
            {
                CurrentBlogPostIndex = BlogPosts.Count - 1;
            }

            SetCurrentBlogPost();
        }

        private static void OpenLink(string url)
        {
            ProcessStartInfo psi = new()
            {
                FileName = url,
                UseShellExecute = true
            };

            Process.Start(psi);
        }
    }
}

using System;
using System.Collections.Generic;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Diagnostics;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Models.Api.Changelog;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.ViewModels
{
    public class NewsPanelViewModel : PanelBase
    {
        public override string Name => "News";

        public override bool IsEnabled => true;

        private readonly IBlogService _blogService;

        public ObservableCollection<BlogPostViewModel> BlogPosts { get; }
        private BlogPostViewModel? _currentBlogPost;
        public BlogPostViewModel CurrentBlogPost
        {
            get => _currentBlogPost ?? new BlogPostViewModel("Loading...", string.Empty, string.Empty, new(), null);
            set => this.RaiseAndSetIfChanged(ref _currentBlogPost, value);
        }

        private ViewModelBase _changelog;
        public ViewModelBase Changelog
        {
            get => _changelog;
            set => this.RaiseAndSetIfChanged(ref _changelog, value);
        }

        private string _newsHeader = "News";
        public string NewsHeader
        {
            get => _newsHeader;
            set => this.RaiseAndSetIfChanged(ref _newsHeader, value);
        }

        public ReactiveCommand<Unit, Unit> NextBlog { get; }
        public ReactiveCommand<Unit, Unit> PreviousBlog { get; }
        private int CurrentBlogPostIndex { get; set; }

        public NewsPanelViewModel(ChangelogViewModel changelog, IBlogService blogService)
        {
            _changelog = changelog;
            _blogService = blogService;

            BlogPosts = new();
            FetchBlogPosts();
            NextBlog = ReactiveCommand.Create(NextPost);
            PreviousBlog = ReactiveCommand.Create(PreviousPost);
            CurrentBlogPostIndex = 0;

            SetCurrentBlogPost();
        }

        private void FetchBlogPosts()
        {
            List<BlogPost>? blogPosts = _blogService.GetBlogPosts(5);

            if (blogPosts != null)
            {
                foreach (BlogPost post in blogPosts)
                {
                    string title = post.Title;
                    string link = $"{LinkUrls.BlogBaseUrl}/{post.Slug ?? string.Empty}";
                    string summary = post.Summary ?? string.Empty;
                    string? image = post.ImageUrl ?? null;
                    DateOnly? date;

                    if (post.CreateDateTime.HasValue)
                    {
                        date = DateOnly.FromDateTime(post.CreateDateTime.Value);
                    }
                    else
                    {
                        date = null;
                    }

                    BlogPosts.Add(new(title, link, summary, date, image));
                }
            }
        }

        public void NextPost()
        {
            CurrentBlogPostIndex++;
            if (CurrentBlogPostIndex >= BlogPosts.Count)
            {
                CurrentBlogPostIndex = 0;
            }

            SetCurrentBlogPost();
        }

        public void PreviousPost()
        {
            CurrentBlogPostIndex--;
            if (CurrentBlogPostIndex < 0)
            {
                CurrentBlogPostIndex = BlogPosts.Count - 1;
            }

            SetCurrentBlogPost();
        }

        private void SetCurrentBlogPost()
        {
            NewsHeader = $"News ({CurrentBlogPostIndex + 1}/{BlogPosts.Count})";
            CurrentBlogPost = BlogPosts[CurrentBlogPostIndex];
        }

        public override void Refresh()
        {
            // Do nothing
        }
    }
}

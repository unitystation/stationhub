using FluentAssertions;
using UnitystationLauncher.Services.Interface;
using UnitystationLauncher.Tests.MocksRepository;
using UnitystationLauncher.ViewModels;
using Xunit;

namespace UnitystationLauncher.Tests.ViewModels;

public static class NewsPanelViewModelTests
{
    #region NewsPanelViewModel.ctor
    [Fact]
    public static void NewsPanelViewModel_ShouldFetchBlogPosts()
    {
        IBlogService blogService = MockBlogService.NormalBlogPosts(3);

        NewsPanelViewModel newsPanelViewModel = new(null!, blogService);
        newsPanelViewModel.BlogPosts.Count.Should().Be(3);
        newsPanelViewModel.NewsHeader.Should().Be("News (1/3)");
    }
    #endregion
    
    #region NewsPanelViewModel.NextPost
    [Fact]
    public static void NextPost_ShouldGoToNextPostWithWrapAroundAndUpdateTitle()
    {
        IBlogService blogService = MockBlogService.NormalBlogPosts(2);

        NewsPanelViewModel newsPanelViewModel = new(null!, blogService);
        newsPanelViewModel.BlogPosts.Count.Should().Be(2);
        newsPanelViewModel.NewsHeader.Should().Be("News (1/2)");
        string firstTitle = newsPanelViewModel.CurrentBlogPost.Title;

        newsPanelViewModel.NextPost();
        newsPanelViewModel.CurrentBlogPost.Title.Should().NotBe(firstTitle);
        newsPanelViewModel.NewsHeader.Should().Be("News (2/2)");
        
        newsPanelViewModel.NextPost();
        newsPanelViewModel.CurrentBlogPost.Title.Should().Be(firstTitle);
        newsPanelViewModel.NewsHeader.Should().Be("News (1/2)");
    }
    #endregion
    
    #region NewsPanelViewModel.PreviousPost
    [Fact]
    public static void PreviousPost_ShouldGoToPreviousPostWithWrapAroundAndUpdateTitle()
    {
        IBlogService blogService = MockBlogService.NormalBlogPosts(2);

        NewsPanelViewModel newsPanelViewModel = new(null!, blogService);
        newsPanelViewModel.BlogPosts.Count.Should().Be(2);
        newsPanelViewModel.NewsHeader.Should().Be("News (1/2)");
        string firstTitle = newsPanelViewModel.CurrentBlogPost.Title;

        newsPanelViewModel.PreviousPost();
        newsPanelViewModel.CurrentBlogPost.Title.Should().NotBe(firstTitle);
        newsPanelViewModel.NewsHeader.Should().Be("News (2/2)");
        
        newsPanelViewModel.PreviousPost();
        newsPanelViewModel.CurrentBlogPost.Title.Should().Be(firstTitle);
        newsPanelViewModel.NewsHeader.Should().Be("News (1/2)");
    }
    #endregion
}
using UnitystationLauncher.Services.Interface;
using UnitystationLauncher.Tests.MocksRepository.BlogService;
using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Tests.ViewModels;

public static class NewsPanelViewModelTests
{
    #region NewsPanelViewModel.ctor
    [Fact]
    public static void NewsPanelViewModel_ShouldFetchBlogPosts()
    {
        IBlogService blogService = new MockNormalBlogPosts();

        NewsPanelViewModel newsPanelViewModel = new(null!, blogService);
        newsPanelViewModel.BlogPosts.Count.Should().Be(5);
        newsPanelViewModel.NewsHeader.Should().Be("News (1/5)");
    }

    [Fact]
    public static void NewsPanelViewModel_ShouldHandleExceptionInBlogService()
    {
        IBlogService blogService = new MockBlogPostsThrowException();

        NewsPanelViewModel newsPanelViewModel = new(null!, blogService);
        newsPanelViewModel.NewsHeader.Should().Be("News (1/1)");
        newsPanelViewModel.CurrentBlogPost.Title.Should().Be("Error fetching blog posts");
    }
    #endregion

    #region NewsPanelViewModel.NextPost
    [Fact]
    public static void NextPost_ShouldGoToNextPostWithWrapAroundAndUpdateTitle()
    {
        IBlogService blogService = new MockNormalBlogPosts();

        NewsPanelViewModel newsPanelViewModel = new(null!, blogService);
        newsPanelViewModel.BlogPosts.Count.Should().Be(5);
        newsPanelViewModel.NewsHeader.Should().Be("News (1/5)");

        newsPanelViewModel.NextPost(); // 2
        newsPanelViewModel.NextPost(); // 3
        newsPanelViewModel.NextPost(); // 4
        newsPanelViewModel.NextPost(); // 5
        newsPanelViewModel.NewsHeader.Should().Be("News (5/5)");

        newsPanelViewModel.NextPost();
        newsPanelViewModel.NewsHeader.Should().Be("News (1/5)");
    }
    #endregion

    #region NewsPanelViewModel.PreviousPost
    [Fact]
    public static void PreviousPost_ShouldGoToPreviousPostWithWrapAroundAndUpdateTitle()
    {
        IBlogService blogService = new MockNormalBlogPosts();

        NewsPanelViewModel newsPanelViewModel = new(null!, blogService);
        newsPanelViewModel.BlogPosts.Count.Should().Be(5);
        newsPanelViewModel.NewsHeader.Should().Be("News (1/5)");

        newsPanelViewModel.PreviousPost();
        newsPanelViewModel.NewsHeader.Should().Be("News (5/5)");

        newsPanelViewModel.PreviousPost();
        newsPanelViewModel.NewsHeader.Should().Be("News (4/5)");
    }
    #endregion
}
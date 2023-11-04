using UnitystationLauncher.ViewModels;

namespace UnitystationLauncher.Tests.ViewModels;

public static class BlogPostViewModelTests
{
    #region BlogPostViewModel.ctor
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public static void BlogPostViewModel_ShouldHandleNullOrEmptyImage(string? postImage)
    {
        BlogPostViewModel blogPostViewModel = new(string.Empty, string.Empty, string.Empty, null, postImage);

        blogPostViewModel.PostImage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public static void BlogPostViewModel_ShouldHandleCommaSeperatedValues()
    {
        string test = "first,second,third,fourth";
        BlogPostViewModel blogPostViewModel = new(string.Empty, string.Empty, string.Empty, null, test);

        blogPostViewModel.PostImage.Should().Be("first");
    }

    [Fact]
    public static void BlogPostViewModel_ShouldSetPostImage()
    {
        BlogPostViewModel blogPostViewModel = new(string.Empty, string.Empty, string.Empty, null, "imageLinkGoesHere");

        blogPostViewModel.PostImage.Should().Be("imageLinkGoesHere");
    }

    [Fact]
    public static void BlogPostViewModel_ShouldSetPostTitle()
    {
        BlogPostViewModel blogPostViewModel = new("titleGoesHere", string.Empty, string.Empty, null, string.Empty);

        blogPostViewModel.Title.Should().Be("titleGoesHere");
    }

    [Fact]
    public static void BlogPostViewModel_ShouldSetPostLink()
    {
        BlogPostViewModel blogPostViewModel = new(string.Empty, "linkGoesHere", string.Empty, null, string.Empty);

        blogPostViewModel.PostLink.Should().Be("linkGoesHere");
    }

    [Fact]
    public static void BlogPostViewModel_ShouldSetPostDate()
    {
        BlogPostViewModel blogPostViewModel = new(string.Empty, String.Empty, string.Empty, DateOnly.Parse("2023-05-16"), string.Empty);

        blogPostViewModel.PostDate.Should().Be(DateOnly.Parse("2023-05-16"));
        blogPostViewModel.PostDateVisible.Should().BeTrue();
    }

    [Fact]
    public static void BlogPostViewModel_ShouldHidePostDateWhenNull()
    {
        BlogPostViewModel blogPostViewModel = new(string.Empty, "linkGoesHere", string.Empty, null, string.Empty);

        blogPostViewModel.PostDateVisible.Should().BeFalse();
    }

    [Fact]
    public static void BlogPostViewModel_ShouldSetPostSummary()
    {
        BlogPostViewModel blogPostViewModel = new(string.Empty, string.Empty, "summaryGoesHere", null, string.Empty);

        blogPostViewModel.PostSummary.Should().Be("summaryGoesHere");
    }
    #endregion
}
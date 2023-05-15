using System;
using FluentAssertions;
using UnitystationLauncher.ViewModels;
using Xunit;

namespace UnitystationLauncher.Tests.ViewModels;

public static class BlogPostViewModelTests
{
    #region BlogPostViewModel.ctor
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public static void BlogPostViewModel_ShouldHandleNullOrEmptyImage(string? postImage)
    {
        BlogPostViewModel blogPostViewModel = new(string.Empty, string.Empty, postImage);

        blogPostViewModel.PostImage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public static void BlogPostViewModel_ShouldHandleCommaSeperatedValues()
    {
        string test = "first,second,third,fourth";
        BlogPostViewModel blogPostViewModel = new(string.Empty, string.Empty, test);

        blogPostViewModel.PostImage.Should().Be("first");
    }

    [Fact]
    public static void BlogPostViewModel_ShouldSetPostImage()
    {
        BlogPostViewModel blogPostViewModel = new(string.Empty, string.Empty, "imageLinkGoesHere");

        blogPostViewModel.PostImage.Should().Be("imageLinkGoesHere");
    }

    [Fact]
    public static void BlogPostViewModel_ShouldSetPostTitle()
    {
        BlogPostViewModel blogPostViewModel = new("titleGoesHere", string.Empty, string.Empty);

        blogPostViewModel.Title.Should().Be("titleGoesHere");
    }

    [Fact]
    public static void BlogPostViewModel_ShouldSetPostLink()
    {
        BlogPostViewModel blogPostViewModel = new(string.Empty, "linkGoesHere", string.Empty);

        blogPostViewModel.PostLink.Should().Be("linkGoesHere");
    }
    #endregion
}
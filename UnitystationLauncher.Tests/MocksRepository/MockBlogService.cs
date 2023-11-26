using System.Collections.Generic;
using UnitystationLauncher.Models.Api.Changelog;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository;

public static class MockBlogService
{
    public static IBlogService NormalBlogPosts(int count)
    {
        List<BlogPost> blogPosts = new();

        for (int i = 0; i < count; i++)
        {
            blogPosts.Add(new()
            {
                Title = $"Test {i}",
                Slug = $"test-{i}",
                ImageUrl = $"test-{i}.png"
            });
        }

        Mock<IBlogService> mock = new();
        mock.Setup(x => x.GetBlogPosts(It.IsAny<int>()))
            .Returns(blogPosts);

        return mock.Object;
    }

    public static IBlogService ThrowsException()
    {
        Mock<IBlogService> mock = new();
        mock.Setup(x => x.GetBlogPosts(It.IsAny<int>())).Throws<Exception>();
        return mock.Object;
    }
}
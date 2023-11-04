using System.Collections.Generic;
using UnitystationLauncher.Models.Api.Changelog;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.BlogService;

public class MockNormalBlogPosts : IBlogService
{
    public List<BlogPost> GetBlogPosts(int count)
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

        return blogPosts;
    }
}
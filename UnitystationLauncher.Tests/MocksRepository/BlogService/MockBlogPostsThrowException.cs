using System.Collections.Generic;
using UnitystationLauncher.Models.Api.Changelog;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Tests.MocksRepository.BlogService;

public class MockBlogPostsThrowException : IBlogService
{
    public List<BlogPost> GetBlogPosts(int count)
    {
        throw new();
    }
}
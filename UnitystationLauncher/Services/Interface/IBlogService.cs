using System.Collections.Generic;
using UnitystationLauncher.Models.Api.Changelog;

namespace UnitystationLauncher.Services.Interface;

/// <summary>
///   Handles contacting the Blog API.
/// </summary>
public interface IBlogService
{
    /// <summary>
    ///   Gets the latest {count} blog posts.
    /// </summary>
    /// <param name="count">Number of blog posts to retrieve</param>
    /// <returns>A list of blog posts, if there are none (unlikely) or there is some problem contacting the API the list will be empty.</returns>
    public List<BlogPost> GetBlogPosts(int count);
}
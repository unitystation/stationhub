using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using UnitystationLauncher.Models.Api.Changelog;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher.Services;

public class BlogService : IBlogService
{
    private readonly HttpClient _httpClient;

    public BlogService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public List<BlogPost>? GetBlogPosts(int count)
    {
        HttpResponseMessage response = _httpClient.GetAsync(Constants.ApiUrls.LatestBlogPosts).Result;
        string blogJson = response.Content.ReadAsStringAsync().Result;
        BlogList blogList = JsonSerializer.Deserialize<BlogList>(blogJson) ?? new();

        string? nextPage = blogList.NextPage;
        while (blogList.Posts.Count < count && !string.IsNullOrEmpty(nextPage))
        {
            response = _httpClient.GetAsync(nextPage).Result;
            blogJson = response.Content.ReadAsStringAsync().Result;
            BlogList tempList = JsonSerializer.Deserialize<BlogList>(blogJson) ?? new();

            foreach (BlogPost post in tempList.Posts)
            {
                // For some reason we are getting duplicates, filter them out
                if (!blogList.Posts.Any(x => x.Title.Equals(post.Title)))
                {
                    blogList.Posts.Add(post);
                }

                if (blogList.Posts.Count == count)
                {
                    break;
                }

                nextPage = tempList.NextPage;
            }
        }


        return blogList.Posts;
    }
}
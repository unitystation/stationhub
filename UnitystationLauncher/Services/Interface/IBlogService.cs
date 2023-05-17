using System.Collections.Generic;
using System.Threading.Tasks;
using UnitystationLauncher.Models.Api.Changelog;

namespace UnitystationLauncher.Services.Interface;

public interface IBlogService
{
    public List<BlogPost>? GetBlogPosts(int count);
}
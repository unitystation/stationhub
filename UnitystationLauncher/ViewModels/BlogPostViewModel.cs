using System;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;

namespace UnitystationLauncher.ViewModels;

public class BlogPostViewModel : ViewModelBase
{
    public string Title { get; }
    public string PostLink { get; }
    
    public bool PostDateVisible { get; }
    public string PostDate { get; }

    private string? _postImage;
    public string PostImage
    {
        get => _postImage ?? "avares://StationHub/Assets/bgnews.png";
        set => this.RaiseAndSetIfChanged(ref _postImage, value);
    }

    private string? _darkenBg;
    public string DarkenBg
    {
        get => _darkenBg ?? "avares://StationHub/Assets/transparentdark.png";
        set => this.RaiseAndSetIfChanged(ref _darkenBg, value);
    }

    public BlogPostViewModel(string title, string postLink, DateOnly? postDate, string? postImage)
    {
        Title = title;
        PostLink = postLink;

        if (postDate.HasValue)
        {
            PostDate = postDate.Value.ToString("yyyy-MM-dd");
            PostDateVisible = true;
        }
        else
        {
            PostDate = new DateOnly().ToString("yyyy-MM-dd");
            PostDateVisible = false;
        }
        

        if (!string.IsNullOrWhiteSpace(postImage))
        {
            // Sometimes these come as comma seperated values, just take the first one in that case
            if (postImage.Contains(','))
            {
                postImage = postImage.Split(',').First();
            }

            PostImage = postImage;
        }
    }

    private void OpenLink()
    {
        if (string.IsNullOrWhiteSpace(PostLink))
        {
            return;
        }

        ProcessStartInfo psi = new()
        {
            FileName = PostLink,
            UseShellExecute = true
        };
        Process.Start(psi);
    }
}
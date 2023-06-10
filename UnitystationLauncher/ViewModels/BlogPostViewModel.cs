using System;
using System.Diagnostics;
using System.Linq;
using ReactiveUI;

namespace UnitystationLauncher.ViewModels;

public class BlogPostViewModel : ViewModelBase
{
    public string Title { get; }
    public string PostLink { get; }
    public string PostSummary { get; }

    public bool PostDateVisible { get; }
    public DateOnly PostDate { get; }

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

    public BlogPostViewModel(string title, string postLink, string postSummary, DateOnly? postDate, string? postImage)
    {
        Title = title;
        PostLink = postLink;
        PostSummary = postSummary;

        if (postDate.HasValue)
        {
            PostDate = postDate.Value;
            PostDateVisible = true;
        }
        else
        {
            PostDate = new();
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

    public override void Refresh()
    {
        // Do nothing
    }
}
using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace UnitystationLauncher.ViewModels;

public class BlogPostViewModel : ViewModelBase
{
    private string Title { get; }
    private string PostLink { get; }
    private Bitmap PostImage { get; }
    private Bitmap DarkenBg { get; }

    public BlogPostViewModel(string title, string postLink, Bitmap? postImage)
    {
        Title = title;
        PostLink = postLink;
        IAssetLoader assets = AvaloniaLocator.Current.GetService<IAssetLoader>()
                              ?? throw new NullReferenceException("No IAssetLoader found, check Autofac configuration.");
        // Default image in case one is not provided
        postImage ??= new(assets.Open(new("avares://StationHub/Assets/bgnews.png")));
        PostImage = postImage;
        DarkenBg = new(assets.Open(new("avares://StationHub/Assets/transparentdark.png")));
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
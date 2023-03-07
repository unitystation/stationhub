using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace UnitystationLauncher.Views;

public partial class BlogPostView : UserControl
{
    public BlogPostView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
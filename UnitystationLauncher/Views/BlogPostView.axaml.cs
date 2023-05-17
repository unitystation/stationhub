using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace UnitystationLauncher.Views;

public class BlogPostView : UserControl
{
    public bool PostSummaryVisible { get; set; } = false;

    public BlogPostView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void InputElement_OnPointerLeave(object? sender, PointerEventArgs e)
    {
        this.GetControl<Panel>("TitlePanel").IsVisible = true;
        this.GetControl<Panel>("SummaryPanel").IsVisible = false;
    }

    private void InputElement_OnPointerEnter(object? sender, PointerEventArgs e)
    {
        this.GetControl<Panel>("TitlePanel").IsVisible = false;
        this.GetControl<Panel>("SummaryPanel").IsVisible = true;
    }
}
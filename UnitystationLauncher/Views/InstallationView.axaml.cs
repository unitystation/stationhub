using System;
using System.Diagnostics;
using System.IO;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace UnitystationLauncher.Views;

public class InstallationView : UserControl
{
    public InstallationView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void HandlePathClick(object sender, PointerPressedEventArgs e)
    {
        TextBlock? block = sender as TextBlock;
        if (block?.Text == null)
        {
            return;
        }
        string path = Path.GetFullPath(block.Text);
        string protcol = GetManagerProtocl();
        Process.Start(protcol, new Uri(RemoveLastPart(path)).ToString());
    }

    private string GetManagerProtocl()
    {
        string protocol = "Explorer.exe";
        if (OperatingSystem.IsWindows())
        {
            protocol = "Explorer.exe";
        }
        else if (OperatingSystem.IsMacOS())
        {
            protocol = "open";
        }
        else if (OperatingSystem.IsLinux())
        {
            protocol = "xdg-open";
        }
        return protocol;
    }

    private string RemoveLastPart(string path)
    {
        int lastBackslashIndex = path.LastIndexOf('\\');
        return lastBackslashIndex >= 0 ? path.Substring(0, lastBackslashIndex) : path;
    }
}

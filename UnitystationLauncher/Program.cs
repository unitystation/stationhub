using System.Collections.Generic;
using Avalonia;
using Avalonia.ReactiveUI;

namespace UnitystationLauncher;
public static class Program
{
    public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            // Windows Specific
            .With(new Win32PlatformOptions
            {
                RenderingMode = new List<Win32RenderingMode>
                {
                    Win32RenderingMode.Wgl
                }
            })
            // MacOS Specific, AvaloniaNativePlatformOptions is "OSX backend options,
            // and MacOSPlatformOptions is "OSX front-end options", no idea why they decided to do it that way.
            .With(new AvaloniaNativePlatformOptions
            {
                RenderingMode = new List<AvaloniaNativeRenderingMode>
                {
                    AvaloniaNativeRenderingMode.OpenGl
                }
            })
            .With(new MacOSPlatformOptions
            {
                ShowInDock = true
            })
            .With(new MacOSPlatformOptions { ShowInDock = true })
            // Linux Specific
            .With(new X11PlatformOptions
            {
                RenderingMode = new List<X11RenderingMode>
                {
                    X11RenderingMode.Egl
                }
            })
            .LogToTrace()
            .UseReactiveUI()
            .WithInterFont();
}

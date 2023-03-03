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
            .With(new AvaloniaNativePlatformOptions { UseGpu = true, UseDeferredRendering = false }) // Common options
                                                                                                     //.With(new Win32PlatformOptions {  }) // Windows Specific
            .With(new MacOSPlatformOptions { ShowInDock = true }) // MacOS Specific
                                                                  //.With(new X11PlatformOptions {  }) // Linux Specific
            .LogToTrace()
            .UseReactiveUI();
}

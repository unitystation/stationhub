using Avalonia;
using Avalonia.ReactiveUI;

namespace UnitystationLauncher
{
    class Program
    {
        public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new X11PlatformOptions { UseGpu = true })
                .With(new AvaloniaNativePlatformOptions { UseGpu = true, UseDeferredRendering = false })
                .With(new MacOSPlatformOptions { ShowInDock = true })
                .With(new Win32PlatformOptions
                {
                    UseDeferredRendering = false
                })
                .LogToDebug()
                .UseReactiveUI(); 
    }
}

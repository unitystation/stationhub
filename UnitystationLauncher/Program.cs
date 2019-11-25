using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using UnitystationLauncher.ViewModels;
using UnitystationLauncher.Views;
using Serilog;
using Steamworks;
using System.IO;
using Serilog.Events;
using Autofac;
using AutofacSerilogIntegration;

namespace UnitystationLauncher
{
    class Program
    {
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp().Start(AppMain, args);

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .With(new X11PlatformOptions { UseGpu = true })
                .With(new AvaloniaNativePlatformOptions { UseGpu = true })
                .With(new MacOSPlatformOptions { ShowInDock = true })
                .With(new Win32PlatformOptions
                {
                    UseDeferredRendering = false,
                    AllowEglInitialization = true
                })
                .LogToDebug()
                .UseReactiveUI();

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(Path.Combine("Logs", "Launcher.log"), rollingInterval: RollingInterval.Day)
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
                .CreateLogger();

            var builder = new ContainerBuilder();
            builder.RegisterModule(new StandardModule());
            builder.RegisterLogger();

            using var container = builder.Build();

            try
            {
                SteamClient.Init(801140);
            }
            catch (System.Exception e)
            {
                // Couldn't init for some reason (steam is closed etc)
                Log.Error(e.Message);
            }

            try
            {
                var window = new MainWindow
                {
                    DataContext = container.Resolve<MainWindowViewModel>(),
                };

                window.Closing += (s, e) =>
                {
                    SteamClient.Shutdown();
                    Log.Information("Steam client closing");
                };

                app.Run(window);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "A fatal exception occured");
                throw;
            }
        }
    }
}

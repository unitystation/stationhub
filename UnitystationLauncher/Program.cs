using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using UnitystationLauncher.ViewModels;
using UnitystationLauncher.Views;
using Serilog;
using Serilog.Sinks.File;
using System.IO;
using Serilog.Events;

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

            var window = new MainWindow
            {
                DataContext = new MainWindowViewModel(),
            };

            try
            {
                app.Run(window);
            }
            catch(Exception e)
            {
                Log.Fatal(e, "A fatal exception occured");
                throw;
            }
        }
    }
}

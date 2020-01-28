using System;
using Avalonia;
using Avalonia.Logging.Serilog;
using UnitystationLauncher.ViewModels;
using UnitystationLauncher.Views;
using UnitystationLauncher.Models;
using Serilog;
using System.IO;
using Serilog.Events;
using Autofac;
using AutofacSerilogIntegration;
using System.Runtime.InteropServices;
using Avalonia.ReactiveUI;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;

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

            Log.Information($"Build Number: {Config.currentBuild}");
            CleanUpdateFiles();
            var builder = new ContainerBuilder();
            builder.RegisterModule(new StandardModule());
            builder.RegisterLogger();

            using var container = builder.Build();
            
            try
            {
                var window = new MainWindow
                {
                    DataContext = container.Resolve<MainWindowViewModel>(),
                };

                app.Run(window);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "A fatal exception occured");
                throw;
            }
        }

        //Gets rid of any old update files if they exist
        static void CleanUpdateFiles()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (File.Exists(Config.WinExeOldFullPath))
                {
                    Console.WriteLine("Delete update files!");
                    File.Delete(Config.WinExeOldFullPath);
                }
            }
            else
            {
                if (File.Exists(Config.UnixExeOldFullPath))
                {
                    Console.WriteLine("Delete update files!");
                    File.Delete(Config.UnixExeOldFullPath);
                }
            }
        }
    }
}

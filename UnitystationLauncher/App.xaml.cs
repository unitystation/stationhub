using System;
using Avalonia;
using UnitystationLauncher.ViewModels;
using UnitystationLauncher.Views;
using UnitystationLauncher.Models;
using Serilog;
using System.IO;
using Serilog.Events;
using Autofac;
using AutofacSerilogIntegration;
using System.Runtime.InteropServices;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace UnitystationLauncher
{
    public class App : Application
    {
        private IContainer container;

        public override void Initialize()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new StandardModule());
            builder.RegisterLogger();
            container = builder.Build();
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(Path.Combine("Logs", "Launcher.log"), rollingInterval: RollingInterval.Day)
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
                .CreateLogger();

            Log.Information($"Build Number: {Config.currentBuild}");
            CleanUpdateFiles();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = container.Resolve<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

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
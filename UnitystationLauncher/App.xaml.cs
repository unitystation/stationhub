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
                .WriteTo.File(Path.Combine(Config.RootFolder, "Logs", "Launcher.log"), rollingInterval: RollingInterval.Day)
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
                .CreateLogger();

            Log.Information($"Build Number: {Config.currentBuild}");

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = container.Resolve<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

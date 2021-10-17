using System;
using Avalonia;
using UnitystationLauncher.ViewModels;
using UnitystationLauncher.Views;
using Serilog;
using System.IO;
using System.Runtime.InteropServices;
using Serilog.Events;
using Autofac;
using AutofacSerilogIntegration;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Mono.Unix;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher
{
    public class App : Application
    {
        private IContainer _container = null!;

        public override void Initialize()
        {
            Directory.CreateDirectory(Config.InstallationsPath);
            GiveAllOwnerPermissions(Config.InstallationsPath);

            var builder = new ContainerBuilder();
            builder.RegisterModule(new StandardModule());
            builder.RegisterLogger();
            _container = builder.Build();
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File(Path.Combine(Config.RootFolder, "Logs", "Launcher.log"), rollingInterval: RollingInterval.Day)
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
                .CreateLogger();

            Log.Information("Build Number: {CurrentBuild}", Config.CurrentBuild);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _container.Resolve<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static void GiveAllOwnerPermissions(string path)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return;
                }

                var fileInfo = new UnixFileInfo(path);
                fileInfo.FileAccessPermissions |= FileAccessPermissions.UserReadWriteExecute;
            }
            catch (Exception e)
            {
                Log.Error(e, "An exception occurred when setting the permissions");
            }
        }
    }
}

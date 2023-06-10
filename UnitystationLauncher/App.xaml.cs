using Avalonia;
using UnitystationLauncher.ViewModels;
using UnitystationLauncher.Views;
using Serilog;
using System.IO;
using Serilog.Events;
using Autofac;
using AutofacSerilogIntegration;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using UnitystationLauncher.Constants;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher
{
    public class App : Application
    {
        private IContainer _container = null!;

        public override void Initialize()
        {
            ContainerBuilder builder = new();
            builder.RegisterModule(new StandardModule());
            builder.RegisterLogger();
            _container = builder.Build();
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            IEnvironmentService environmentService = _container.Resolve<IEnvironmentService>();
            Log.Logger = new LoggerConfiguration()
#if DEBUG
                .MinimumLevel.Verbose()
#else
                .MinimumLevel.Information()
#endif
                .WriteTo.File(Path.Combine(environmentService.GetUserdataDirectory(), "Logs", "Launcher.log"), rollingInterval: RollingInterval.Day)
                .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug)
                .CreateLogger();

            Log.Information("Build Number: {CurrentBuild}", AppInfo.CurrentBuild);
            Log.Information("Current environment: {CurrentEnvironment}", environmentService.GetCurrentEnvironment());

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = _container.Resolve<MainWindowViewModel>(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

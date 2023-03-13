using System.Net.Http;
using Autofac;
using UnitystationLauncher.Services;
using UnitystationLauncher.Services.Interface;

namespace UnitystationLauncher
{
    public class StandardModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpClient>().SingleInstance();

            // Services
            builder.RegisterType<EnvironmentService>().As<IEnvironmentService>().SingleInstance();
            builder.RegisterType<PreferencesService>().As<IPreferencesService>().SingleInstance();
            builder.RegisterType<HubService>().As<IHubService>().SingleInstance();
            builder.RegisterType<DownloadService>().SingleInstance();
            builder.RegisterType<InstallationService>().SingleInstance();
            builder.RegisterType<ServerService>().SingleInstance();
            builder.RegisterType<StateService>().SingleInstance();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("ViewModel"));
        }
    }
}
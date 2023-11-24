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
            builder.RegisterType<InstallationService>().As<IInstallationService>().SingleInstance();
            builder.RegisterType<ServerService>().As<IServerService>().SingleInstance();
            builder.RegisterType<BlogService>().As<IBlogService>().SingleInstance();
            builder.RegisterType<PingService>().As<IPingService>().SingleInstance();
            builder.RegisterType<AssemblyTypeCheckerService>().As<IAssemblyTypeCheckerService>().SingleInstance();
            builder.RegisterType<CodeScanService>().As<ICodeScanService>().SingleInstance();
            builder.RegisterType<CodeScanConfigService>().As<ICodeScanConfigService>().SingleInstance();
            builder.RegisterType<GameCommunicationPipeService>().As<IGameCommunicationPipeService>().SingleInstance();

            // View Models
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("ViewModel"));
        }
    }
}
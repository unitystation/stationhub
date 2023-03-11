using System.Net.Http;
using Autofac;
using UnitystationLauncher.Models.ConfigFile;

namespace UnitystationLauncher
{
    public class StandardModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HttpClient>().SingleInstance();
            builder.RegisterType<Config>().SingleInstance();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("Service"))
                .SingleInstance();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("ViewModel"));
        }
    }
}
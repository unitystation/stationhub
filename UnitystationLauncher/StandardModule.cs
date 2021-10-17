using System.Net.Http;
using Autofac;
using Firebase.Auth;
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

            builder.Register(c => new FirebaseConfig("AIzaSyB7GorzPgwHYjSV4XaJoszj98tLM4_WZpE")).SingleInstance();
            builder.RegisterType<FirebaseAuthProvider>().SingleInstance();
        }
    }
}
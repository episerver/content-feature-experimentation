using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using EPiServer.Web;
using Microsoft.Extensions.DependencyInjection;

namespace EPiServer.Marketing.Testing.Web.FullStackSDK
{
    [InitializableModule]
    [ModuleDependency(typeof(InitializationModule))]
    public class InitializeExperimentation : IConfigurableModule
    {
        void IConfigurableModule.ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IFullstackSDKClient, FullstackSDKClient>();
        }

        void IInitializableModule.Initialize(InitializationEngine context) { }

        void IInitializableModule.Uninitialize(InitializationEngine context) { }
    }
}
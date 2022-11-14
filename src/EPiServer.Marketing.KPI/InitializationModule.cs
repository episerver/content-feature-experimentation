using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.ServiceLocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EPiServer.Marketing.KPI
{
    [InitializableModule]
    [ModuleDependency(typeof(Shell.UI.InitializationModule))]
    [ModuleDependency(typeof(Web.InitializationModule))]
    public partial class InitializationModule : IConfigurableModule //netcore , IInitializableHttpModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            
        }

        public void Initialize(InitializationEngine context)
        {
            // throw new NotImplementedException();
        }

        public void Uninitialize(InitializationEngine context)
        {
            // throw new NotImplementedException();
        }
    }
}

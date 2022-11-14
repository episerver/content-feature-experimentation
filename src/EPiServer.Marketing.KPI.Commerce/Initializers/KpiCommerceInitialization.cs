using System;
using System.Linq;
using System.Reflection;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Framework.Localization;
using EPiServer.Framework.Localization.XmlResources;
using System.Diagnostics.CodeAnalysis;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.DependencyInjection;
using EPiServer.Shell.Modules;
using EPiServer.Shell;

namespace EPiServer.Marketing.KPI.Commerce.Initializers
{
    [ExcludeFromCodeCoverage]
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule), typeof(ShellInitialization))]
    public class KpiCommerceInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            context.Services.Configure<ProtectedModuleOptions>(options =>
            {
                if (!options.Items.Any(x => x.Name.Equals("EPiServer.Marketing.KPI.Commerce")))
                {
                    var module = new ModuleDetails
                    {
                        Name = "EPiServer.Marketing.KPI.Commerce",
                    };
                    options.Items.Add(module);
                }
            });
        }

        public void Initialize(InitializationEngine context)
        {
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}

using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Core
{
    [InitializableModule]
    [ModuleDependency(typeof(Shell.UI.InitializationModule))]
    [ModuleDependency(typeof(Web.InitializationModule))]
    public partial class InitializationModule : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var serviceProvider = context.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            context.Services.Configure<TestingOption>(configuration.GetSection(TestingOption.Section));
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

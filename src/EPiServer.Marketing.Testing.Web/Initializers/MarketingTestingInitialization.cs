using EPiServer.Core;
using EPiServer.Framework;
using EPiServer.Framework.Cache;
using EPiServer.Framework.Initialization;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Core;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.RestAPI;
using EPiServer.Marketing.Testing.Web.Evaluator;
using EPiServer.ServiceLocation;
using EPiServer.Shell.Modules;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    [ExcludeFromCodeCoverage]
    [InitializableModule]
    [ModuleDependency(typeof(EPiServer.Web.InitializationModule))]
    [ModuleDependency(typeof(Shell.UI.InitializationModule))]
    public class MarketingTestingInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var serviceProvider = context.Services.BuildServiceProvider();
            var configuration = serviceProvider.GetService<IConfiguration>();

            var configuredTimeout = configuration["EPiServer:Marketing:Testing:CacheTimeoutInMinutes"];
            int.TryParse(configuredTimeout, out int timeout);

            context.Services.Configure<TestingOption>(configuration.GetSection(TestingOption.Section));
            context.Services.AddTransient<IExperimentationFactory, DefaultExperimentationFactory>();
            context.Services.AddSingleton<IExperimentationClient, ExperimentationClient>();
            context.Services.AddTransient<IContentLockEvaluator, ABTestLockEvaluator>();
            context.Services.AddSingleton<ITestManager>(
                serviceLocator =>
                    new CachingTestManager(
                        serviceLocator.GetInstance<ISynchronizedObjectInstanceCache>(),
                        serviceLocator.GetInstance<DefaultMarketingTestingEvents>(),
                        new TestManager(),
                        timeout < 10 ? 60 : timeout
                    ));
            context.Services.AddSingleton<ITestHandler, TestHandler>();
            context.Services.Configure<ProtectedModuleOptions>(o =>
            {
                if (!o.Items.Any(x => x.Name.Equals("EPiServer.Marketing.Testing")))
                {
                    o.Items.Add(new ModuleDetails() { Name = "EPiServer.Marketing.Testing" });
                }
            });
        }

        public void Initialize(InitializationEngine context) 
        {
            ServiceLocator.Current.GetInstance<ITestManager>();
            ServiceLocator.Current.GetInstance<ITestHandler>();
        }

        public void Uninitialize(InitializationEngine context) { }
    }
}

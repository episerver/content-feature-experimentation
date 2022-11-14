using EPiServer.Framework.Hosting;
using EPiServer.Framework.Web.Resources;
using EPiServer.Reference.Commerce.Site.Features.Shared.Channels;
using EPiServer.Web.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

namespace EPiServer.DependencyInjection
{
    internal static class ServiceCollectionExtensions
    {
        /// <internal-api/>
        public static IServiceCollection AddUIMappedFileProviders(this IServiceCollection serviceCollection, string applicationRootPath, string uiSolutionRelativePath)
        {
            serviceCollection.Configure<ClientResourceOptions>(o => o.Debug = true);

            var uiSolutionFolder = Path.Combine(applicationRootPath, uiSolutionRelativePath);
            EnsureDirectory(new DirectoryInfo(Path.Combine(applicationRootPath, "modules/_protected")));
            serviceCollection.Configure<CompositeFileProviderOptions>(c =>
            {
                c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/EPiServer.Commerce.UI.Admin", string.Empty, Path.Combine(uiSolutionFolder, @"EPiServer.Commerce.UI.Admin")));
                c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/Commerce", string.Empty, Path.Combine(uiSolutionFolder, @"EPiServer.Commerce.UI")));
                c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/EPiServer.Commerce.Shell", string.Empty, Path.Combine(uiSolutionFolder, @"EPiServer.Commerce.Shell")));
                c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/EPiServer.Commerce.UI.CustomerService", string.Empty, Path.Combine(uiSolutionFolder, @"EPiServer.Commerce.UI.CustomerService")));
            });
            return serviceCollection;
        }

        public static void AddDisplayResolutions(this IServiceCollection services)
        {
            services.AddSingleton<StandardResolution>();
            services.AddSingleton<IpadHorizontalResolution>();
            services.AddSingleton<IphoneVerticalResolution>();
            services.AddSingleton<AndroidVerticalResolution>();
        }

        private static void EnsureDirectory(DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.Parent.Exists)
            {
                EnsureDirectory(directoryInfo.Parent);
            }

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
        }
    }
}

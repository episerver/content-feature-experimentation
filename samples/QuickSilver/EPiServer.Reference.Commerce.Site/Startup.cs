using EPiServer.Cms.UI.AspNetIdentity;
using EPiServer.Data;
using EPiServer.DependencyInjection;
using EPiServer.Framework.Hosting;
using EPiServer.Framework.Web.Resources;
using EPiServer.Marketing.Testing.Web.Initializers;
using EPiServer.Personalization.Commerce.Tracking;
using EPiServer.Reference.Commerce.Site.Features.Market.Services;
using EPiServer.Reference.Commerce.Site.Features.Recommendations.Services;
using EPiServer.Reference.Commerce.Site.Features.Shared.Services;
using EPiServer.Reference.Commerce.Site.Infrastructure;
using EPiServer.Reference.Commerce.Site.Infrastructure.Attributes;
using EPiServer.Reference.Commerce.Site.Infrastructure.Indexing;
using EPiServer.ServiceLocation;
using EPiServer.Tracking.Commerce;
using EPiServer.Web;
using EPiServer.Web.Hosting;
using EPiServer.Web.Routing;
using Mediachase.Commerce;
using Mediachase.Commerce.Anonymous;
using Mediachase.Commerce.Core;
using Mediachase.Commerce.Orders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace EPiServer.Reference.Commerce.Site
{
    public class Startup
    {
        private readonly IWebHostEnvironment _webHostingEnvironment;
        private readonly IConfiguration _configuration;

        public Startup(IWebHostEnvironment webHostingEnvironment, IConfiguration configuration)
        {
            _webHostingEnvironment = webHostingEnvironment;
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCmsAspNetIdentity<ApplicationUser>(o =>
            {
                if (string.IsNullOrEmpty(o.ConnectionStringOptions?.ConnectionString))
                {
                    o.ConnectionStringOptions = new ConnectionStringOptions
                    {
                        Name = "EcfSqlConnection",
                        ConnectionString = _configuration.GetConnectionString("EcfSqlConnection")
                    };
                }
            },
            null);
            //(builder) =>
            //    builder.AddTokenProvider(OptinTokenProviderOptions.TokenProviderName, typeof(OptinTokenProvider<>).MakeGenericType(builder.UserType))
            //);

            //UI
            if (_webHostingEnvironment.IsDevelopment())
            {
                services.AddUIMappedFileProviders(_webHostingEnvironment.ContentRootPath, @"..\..\..\");
                services.Configure<ClientResourceOptions>(uiOptions =>
                {
                    uiOptions.Debug = true;
                });
            }

            //Commerce
            services.AddCommerce();

            // Add detection services from 'Wangkanai.Detection' package
            services.AddDetection();

            //site specific
            services.TryAddEnumerable(Microsoft.Extensions.DependencyInjection.ServiceDescriptor.Singleton(typeof(IFirstRequestInitializer), typeof(UsersInstaller)));
            services.AddDisplayResolutions();
            services.TryAddSingleton<IRecommendationContext, RecommendationContext>();
            services.AddSingleton<ICurrentMarket, CurrentMarket>();
            services.AddSingleton<ITrackingResponseDataInterceptor, TrackingResponseDataInterceptor>();
            services.AddHttpContextOrThreadScoped<SiteContext, CustomCurrencySiteContext>();
            services.AddTransient<CatalogIndexer>();
            services.TryAddSingleton<ServiceAccessor<IContentRouteHelper>>(locator => locator.GetInstance<IContentRouteHelper>);
            services.AddEmbeddedLocalization<Startup>();
            services.Configure<MvcOptions>(o =>
            {
                o.Filters.Add(new ControllerExceptionFilterAttribute());
                o.Filters.Add(new ReadOnlyFilter());
                o.Filters.Add(new AJAXLocalizationFilterAttribute());
                o.ModelBinderProviders.Insert(0, new ModelBinderProvider());
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/util/Login";
            });

            services.Configure<OrderOptions>(o =>
            {
                o.DisableOrderDataLocalization = true;
            });

            services.AddABTesting(_configuration.GetConnectionString("EPiServerDB"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAnonymousId();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAnonymousMigrator();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "Default", pattern: "{controller}/{action}/{id?}");
                endpoints.MapControllers();
                endpoints.MapContent();
            });
        }
    }

    internal static class IntenalServiceCollectionExtensions
    {
        public static IServiceCollection AddUIMappedFileProviders(this IServiceCollection services, string applicationRootPath, string uiSolutionRelativePath)
        {
            services.Configure<ClientResourceOptions>(o => o.Debug = true);

            var uiSolutionFolder = Path.Combine(applicationRootPath, uiSolutionRelativePath);
            EnsureDictionary(new DirectoryInfo(Path.Combine(applicationRootPath, "modules/_protected")));
            services.Configure<CompositeFileProviderOptions>(c =>
            {
                c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/EPiServer.Marketing.KPI.Commerce", string.Empty, Path.Combine(uiSolutionFolder, @"src\EPiServer.Marketing.KPI.Commerce")));
                c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/EPiServer.Marketing.KPI", string.Empty, Path.Combine(uiSolutionFolder, @"src\EPiServer.Marketing.KPI")));
                c.BasePathFileProviders.Add(new MappingPhysicalFileProvider("/EPiServer/EPiServer.Marketing.Testing", string.Empty, Path.Combine(uiSolutionFolder, @"src\EPiServer.Marketing.Testing.Web")));
            });
            return services;
        }

        private static void EnsureDictionary(DirectoryInfo directoryInfo)
        {
            if (!directoryInfo.Parent.Exists)
            {
                EnsureDictionary(directoryInfo.Parent);
            }

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }
        }
    }
}

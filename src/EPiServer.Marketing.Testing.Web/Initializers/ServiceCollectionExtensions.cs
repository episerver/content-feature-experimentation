using EPiServer.Data;
using EPiServer.Marketing.KPI.Dal;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.ServiceLocation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Web.Initializers
{
    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/> related to EPiServer.Marketing.Testing
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register services EPiServer.Marketing.Testing
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException"><paramref name="services"/> is <c>null</c>.</exception>
        public static IServiceCollection AddABTesting(this IServiceCollection services, string connectionString)
        {
            
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddDbContext<KpiDatabaseContext>(
                options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        x => x.MigrationsHistoryTable("__MigrationHistory", "dbo"));
                });

            services.AddDbContext<DatabaseContext>(
                options =>
                {
                    options.UseSqlServer(
                        connectionString,
                        x => x.MigrationsHistoryTable("__MigrationHistory", "dbo"));
                });

            return services;
        }
    }
}

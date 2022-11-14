using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EPiServer.Commerce.Order;
using Mediachase.Commerce.Anonymous;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EPiServer.Reference.Commerce.Site.Infrastructure
{
    public class AnonymousMigratorMiddleware
    {
        private readonly RequestDelegate _next;
        public const string AnonymousMigrationCookieName = "AnonymousMigration";

        public AnonymousMigratorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext, IProfileMigrator profileMigrator)
        {
            HandleRequest(httpContext, profileMigrator);
            await _next?.Invoke(httpContext);
        }

        private void HandleRequest(HttpContext httpContext, IProfileMigrator profileMigrator)
        {
            if (httpContext.User.Identity != null && httpContext.User.Identity.IsAuthenticated)
            {
                if (httpContext.Features.Get<IAnonymousIdFeature>() == null)
                {
                    return;
                }

                if (httpContext.Request.Cookies.ContainsKey(AnonymousMigrationCookieName) && bool.TryParse(httpContext.Request.Cookies[AnonymousMigrationCookieName], out var anonymousMigration) && anonymousMigration)
                { 
                    var anonymousId = new Guid(httpContext.Features.Get<IAnonymousIdFeature>().AnonymousId);

                    profileMigrator.MigrateOrders(anonymousId);
                    profileMigrator.MigrateCarts(anonymousId);
                    profileMigrator.MigrateWishlists(anonymousId);

                    httpContext.Response.Cookies.Delete(AnonymousMigrationCookieName);
                }
            }
        }
    }

    public static class AnonymousMigratorMiddlewareExtensions
    {
        public static IApplicationBuilder UseAnonymousMigrator(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AnonymousMigratorMiddleware>();
        }
    }
}

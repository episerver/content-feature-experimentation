using System;
using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using Microsoft.AspNetCore.Http;

namespace EPiServer.Marketing.KPI.Common.Helpers
{
    /// <summary>
    /// This exists to allow us to mock the request for unit testing purposes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [ServiceConfiguration(ServiceType = typeof(IKpiHelper), Lifecycle = ServiceInstanceScope.Singleton)]
    public class KpiHelper : IKpiHelper
    {
        protected readonly Injected<IHttpContextAccessor> _httpContextAccessor;
        
        /// <summary>
        /// Evaluates current URL to determine if page is in a system folder context (e.g Edit, or Preview)
        /// </summary>
        /// <returns></returns>
        public virtual bool IsInSystemFolder()
        {
            return _httpContextAccessor.Service.HttpContext == null ||
                   _httpContextAccessor.Service.HttpContext.Request.Path.Value.IndexOf(Shell.Paths.ProtectedRootPath, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public string GetUrl(ContentReference contentReference)
        {
            return UrlResolver.Current.GetUrl(contentReference);
        }

        public string GetRequestPath()
        {
            return _httpContextAccessor.Service.HttpContext!=null ? _httpContextAccessor.Service.HttpContext.Request.Path : string.Empty;
        }
    }
}
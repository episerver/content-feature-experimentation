using EPiServer.Core;
using EPiServer.ServiceLocation;
using EPiServer.Shell;
using EPiServer.Web;
using EPiServer.Web.Mvc.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(IUIHelper), Lifecycle = ServiceInstanceScope.Singleton)]

    public class UIHelper : IUIHelper
    {
        private readonly Injected<IHttpContextAccessor> _httpContextAccessor;
        private IServiceProvider _serviceLocator;

        /// <summary>
        /// Default constructor
        /// </summary>
        public UIHelper()
        {
            _serviceLocator = ServiceLocator.Current;
        }

        /// <summary>
        /// unit tests should use this contructor and add needed services to the service locator as needed
        /// </summary>
        /// <param name="locator"></param>
        public UIHelper(IServiceProvider locator)
        {
            _serviceLocator = locator;
        }

        public string getConfigurationURL()
        {
            //Build EPiServer URL to the configuration page
            var requested = _httpContextAccessor.Service.HttpContext.Request.Scheme + "://" + _httpContextAccessor.Service.HttpContext.Request.Host.Value;
            string settingsUrl = GetSettingsUrlString();
            return string.Format("{0}{1}", requested, settingsUrl);
        }

        private string GetSettingsUrlString()
        {
            // out: http://{DOMAIN}/{PROTECTED_PATH}/CMS/Admin/
            var baseUrl = UIPathResolver.Instance.CombineWithUI("Admin/");

            // out: /{PROTECTED_PATH}/{MODULE_NAME}/Views/Admin/Settings.aspx
            var targetResourcePath = Paths.ToResource(typeof(UIHelper), "TestingAdministration");

            // out: http://{DOMAIN}/{PROTECTED_PATH}/CMS/Admin/?customdefaultpage=/{PROTECTED_PATH}/{MODULE_NAME}/Views/Admin/Settings.aspx
            return UriUtil.AddQueryString(baseUrl, "customdefaultpage", targetResourcePath);
        }

        public string GetAppRelativePath()
        {
            // out: /{PROTECTED_PATH}/{MODULE_NAME}/
            return Paths.ToResource(typeof(UIHelper), "");
        }

        /// <summary>
        /// Given the specified Guid, get the content data from cms
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>The icontent object if found, if not found returns a BasicContent instance with name set to ContentNotFound</returns>
        public IContent getContent(Guid guid)
        {
            IContentRepository repo = _serviceLocator.GetInstance<IContentRepository>();
            try
            {
                return repo.Get<IContent>(guid);
            } catch
            {
                return new BasicContent() { Name = "ContentNotFound" };
            }
        }

        public string getEpiUrlFromLink(ContentReference contentLink)
        {
            var urlHelper = ServiceLocator.Current.GetInstance<UrlHelper>();
            return urlHelper.ContentUrl(contentLink);

            
        }
    }
}

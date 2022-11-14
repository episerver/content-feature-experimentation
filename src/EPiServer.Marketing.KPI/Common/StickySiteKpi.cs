using EPiServer.Core;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Runtime.Caching;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Web.Routing;
using EPiServer.ServiceLocation;
using EPiServer.DataAbstraction;
using System.Linq;
using EPiServer.Marketing.KPI.Common.Helpers;
using System.Web;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace EPiServer.Marketing.KPI.Common
{
    /// <summary>
    /// Converts when a user visits the content under test and then visits any other page within the same browser session.  
    /// Results: Views are the number of visitors that visited the web page.  
    /// Conversions are the number of visitors that clicked through to any other page within the specified time.
    /// </summary>
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Markup.StickySiteConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Markup.StickySiteReadOnlyMarkup.html",
        text_id = "/kpi/stickysite_kpi/name",
        description_id = "/kpi/stickysite_kpi/description")]
    public class StickySiteKpi : Kpi
    {
        private ObjectCache _sessionCache = MemoryCache.Default;
        protected readonly Injected<IKpiHelper> _stickyHelper;
        protected readonly Injected<IHttpContextAccessor> _httpContextAccessor;
        private readonly Injected<IContentRepository> _contentRepository;
        private readonly Injected<IContentEvents> _contentEvents;
        private readonly Injected<UrlResolver> _urlResolver;

        [DataMember]
        public Guid TestContentGuid;

        /// <summary>
        /// Number of minutes until another page is visited.
        /// </summary>
        [DataMember]
        public int Timeout;

        /// <inheritdoc />
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var cookie = _httpContextAccessor.Service.HttpContext.Request.Cookies[$"SSK_{TestContentGuid}"].FromLegacyCookieString();
            var hasConverted = !_stickyHelper.Service.IsInSystemFolder() &&
                               e is ContentEventArgs eventArgs &&
                               eventArgs.Content != null &&
                               eventArgs.Content.ContentGuid != TestContentGuid &&
                               _httpContextAccessor.Service.HttpContext.Request.Path != cookie["path"] &&
                               !IsSupportingContent();

            return new KpiConversionResult() { KpiId = Id, HasConverted = hasConverted };
        }

        /// <inheritdoc />
        public override void Validate(Dictionary<string, string> responseData)
        {
            if (responseData["Timeout"] == "" || responseData["CurrentContent"] == "")
            {
                // should never happen if the markup is correct
                var errormessage = LocalizationService.Current
                    .GetString("/kpi/stickysite_kpi/config_markup/error_internal");
                throw new KpiValidationException(
                    string.Format(errormessage, "timeout=" + responseData["Timeout"] + " currentcontent=" + responseData["CurrentContent"]));
            }

            // save the kpi arguments

            var currentContent = _contentRepository.Service.Get<IContent>(new ContentReference(responseData["CurrentContent"]));
            TestContentGuid = currentContent.ContentGuid;

            bool isInt = int.TryParse(responseData["Timeout"], out Timeout);
            if (!isInt || Timeout < 1 || Timeout > 60)
            {
                throw new KpiValidationException(
                    LocalizationService.Current
                    .GetString("/kpi/stickysite_kpi/config_markup/error_invalid_timeoutvalue"));
            }

            Timeout = int.Parse(responseData["Timeout"]);
        }

        /// <inheritdoc />
        public override string UiMarkup
        {
            get
            {
                string markup = base.UiMarkup;

                var conversionLabel = LocalizationService.Current
                    .GetString("/kpi/stickysite_kpi/config_markup/conversion_label");
                return string.Format(markup, conversionLabel);
            }
        }

        /// <inheritdoc />
        public override string UiReadOnlyMarkup
        {
            get
            {
                string markup = base.UiReadOnlyMarkup;

                var conversionDescription = LocalizationService.Current
                    .GetString("/kpi/stickysite_kpi/readonly_markup/conversion_selector_description");
                conversionDescription = string.Format(conversionDescription, Timeout);
                markup = string.Format(markup, conversionDescription);
                return markup;
            }
        }

        private EventHandler<ContentEventArgs> _eh;

        /// <inheritdoc />
        public override event EventHandler EvaluateProxyEvent
        {
            add
            {
                _eh = new EventHandler<ContentEventArgs>(value);
                _contentEvents.Service.LoadedContent += _eh;
            }
            remove
            {
                _contentEvents.Service.LoadedContent -= _eh;
            }
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override void Initialize()
        {
            _contentEvents.Service.LoadedContent += AddSessionOnLoadedContent;
        }

        /// <inheritdoc />
        [ExcludeFromCodeCoverage]
        public override void Uninitialize()
        {
            _contentEvents.Service.LoadedContent -= AddSessionOnLoadedContent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddSessionOnLoadedContent(object sender, ContentEventArgs e)
        {
            var cookieKey = $"SSK_{TestContentGuid}";
            var httpContext = _httpContextAccessor.Service.HttpContext;

            if (!_stickyHelper.Service.IsInSystemFolder() && e.Content != null && e.Content.ContentGuid == TestContentGuid)
            {
                if (!httpContext.Items.ContainsKey(cookieKey) && httpContext.Request.Cookies[cookieKey] == null)
                {
                    var path = IsSupportingContent() ? AbsolutePath(httpContext.Request.Path.Value) : httpContext.Request.Path.Value;
                    var contentId = TestContentGuid.ToString();
                    var option = new CookieOptions()
                    {
                        Expires = DateTime.Now.AddMinutes(Timeout),
                        HttpOnly = true
                    };
                    var cookieValue = new Dictionary<string, string>()
                    {
                        { "path", path },
                        { "contentguid", contentId }
                    };
                    if (!CookieExists(path, contentId) && IsContentBeingLoaded(path))
                    {
                        httpContext.Response.Cookies.Append(cookieKey, cookieValue.ToLegacyCookieString(), option);
                        httpContext.Items[cookieKey] = true; // we are done for this request. 
                    }
                }
            }
        }

        public string AbsolutePath(string path)
        {
            return new Uri(new Uri(_httpContextAccessor.Service.HttpContext.Request.Scheme + "://" + _httpContextAccessor.Service.HttpContext.Request.Host.Value), path).ToString();
        }

        /// <summary>
        /// Method to determine if the loaded content method is actually being called to load the content associated with the test
        /// or if its just some other content 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private bool IsContentBeingLoaded(string path)
        {
            var cacheKey = $"SSK_{TestContentGuid}"; // use the cookie key as the cache key too. 

            HashSet<string> testcontentPaths;

            if (_sessionCache.Contains(cacheKey))
            {
                testcontentPaths = (HashSet<string>)_sessionCache.Get(cacheKey);
            }
            else
            {
                testcontentPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                _httpContextAccessor.Service.HttpContext.Items[cacheKey] = true;    // we use this flag to keep us from processing more LoadedContent calls. 

                var content = _contentRepository.Service.Get<IContent>(TestContentGuid);
                var contentUrl = _urlResolver.Service.GetUrl(content.ContentLink);
                if (contentUrl != null)
                {
                    testcontentPaths.Add(contentUrl);
                }
                else
                {
                    var parentContent = _contentRepository.Service.Get<IContent>(content.ParentLink);

                    var linkRepository = ServiceLocator.Current.GetInstance<IContentSoftLinkRepository>();
                    var referencingContentLinks = linkRepository.Load(content.ContentLink, true)
                                                                .Where(link =>
                                                                        link.SoftLinkType == ReferenceType.PageLinkReference &&
                                                                        !ContentReference.IsNullOrEmpty(link.OwnerContentLink))
                                                                .Select(link => link.OwnerContentLink)
                                                                .ToList();
                    foreach (var x in referencingContentLinks)
                    {
                        testcontentPaths.Add(UrlResolver.Current.GetUrl(x));
                    }
                }

                CacheItemPolicy policy = new CacheItemPolicy
                {
                    AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(Timeout)
                };
                _sessionCache.Add(cacheKey, testcontentPaths, policy);

                _httpContextAccessor.Service.HttpContext.Items.Remove(cacheKey);
            }

            return testcontentPaths.Contains(path);
        }

        private bool CookieExists(string path, string contentGuid)
        {
            var cookies = _httpContextAccessor.Service.HttpContext.Request.Cookies;
            if (cookies.Count > 0)
            {
                foreach (var cookieName in _httpContextAccessor.Service.HttpContext.Request.Cookies)
                {

                    if (cookieName.Key.Contains("SSK_"))
                    {
                        var cookieValue = _httpContextAccessor.Service.HttpContext.Request.Cookies[cookieName.Key].FromLegacyCookieString();

                        return cookieValue["path"] == path && cookieValue["contentguid"] == contentGuid;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the request is for an asset (such as image, css file)
        /// </summary>
        /// <returns></returns>
        private bool IsSupportingContent()
        {
            var pathExtensions = Path.GetExtension(_httpContextAccessor.Service.HttpContext.Request.Path.Value);

            return pathExtensions == ".png" ||
                   pathExtensions == ".css" ||
                   _httpContextAccessor.Service.HttpContext.Request.Path.Value.IndexOf(SystemContentRootNames.GlobalAssets, StringComparison.OrdinalIgnoreCase) > 0 ||
                   _httpContextAccessor.Service.HttpContext.Request.Path.Value.IndexOf(SystemContentRootNames.ContentAssets, StringComparison.OrdinalIgnoreCase) > 0;
        }
    }
}
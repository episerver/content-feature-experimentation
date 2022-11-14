using EPiServer.Core;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.AspNetCore.Mvc.Routing;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Web.Mvc.Html;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Web.Routing;
using System.Web;
using System.Runtime.Caching;
using EPiServer.Marketing.KPI.Common.Helpers;
using System.Linq;
using EPiServer.Web.Mvc;

namespace EPiServer.Marketing.KPI.Common
{

    /// <summary>
    /// Common KPI class that can be used to compare IContent Guid values.
    /// </summary>
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Markup.ContentComparatorConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Markup.ContentComparatorReadOnlyMarkup.html",
        text_id = "/kpi/content_comparator_kpi/name",
        description_id = "/kpi/content_comparator_kpi/description")]
    public class ContentComparatorKPI : Kpi
    {
        /// <summary>
        /// ID of the content to be tested.
        /// </summary>
        [DataMember]
        public Guid ContentGuid;
        public IContent _content;
        public List<string>  _startpagepaths = new List<string>();
        private ObjectCache _cache;
        private readonly Injected<IKpiHelper> _kpiHelper;
        private readonly Injected<IUrlResolver> _IUrlResolver;
        private readonly Injected<IContentRepository> _contentRepository;
        private readonly Injected<IContentVersionRepository> _contentVersionRepository;
        private readonly Injected<IContentEvents> _contentEvents;

        public ContentComparatorKPI()
        {
        }

        public ContentComparatorKPI(Guid contentGuid)
        {
            ContentGuid = contentGuid;
        }

    /// <inheritdoc />
    [DataMember]
        public override string UiMarkup
        {
            get
            {
                var conversionLabel = LocalizationService.Current.GetString("/kpi/content_comparator_kpi/config_markup/conversion_label");

                return string.Format(base.UiMarkup, conversionLabel);
            }
        }

        /// <inheritdoc />
        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                string markup = base.UiReadOnlyMarkup;

                if (ContentGuid != Guid.Empty)
                {
                     var conversionDescription = LocalizationService.Current.GetString("/kpi/content_comparator_kpi/readonly_markup/conversion_selector_description");

                    var conversionContent = _contentRepository.Service.Get<IContent>(ContentGuid);
                    var conversionLink = _IUrlResolver.Service.GetUrl(conversionContent.ContentLink);
                    markup = string.Format(markup, conversionDescription, conversionLink,
                        conversionContent.Name);
                }

                return markup;
            }
        }

        /// <inheritdoc />
        public override void Validate(Dictionary<string, string> responseData)
        {
            if (responseData["ConversionPage"] == "")
            {
                throw new KpiValidationException(LocalizationService.Current.GetString("/kpi/content_comparator_kpi/config_markup/error_conversionpage"));
            }
            var conversionContent = _contentRepository.Service.Get<IContent>(new ContentReference(responseData["ConversionPage"]));
            var currentContent = _contentRepository.Service.Get<IContent>(new ContentReference(responseData["CurrentContent"]));

            if (IsContentPublished(conversionContent) && !IsCurrentContent(conversionContent, currentContent))
            {
                ContentGuid = conversionContent.ContentGuid;
            }
        }

        /// <inheritdoc />
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            _cache = MemoryCache.Default;
            var retval = false;
            
            var ea = e as ContentEventArgs;
            if (ea != null)
            {
                if (_content == null)
                {                    
                    _content = _contentRepository.Service.Get<IContent>(ContentGuid);

                    if (_cache.Contains("StartPagePaths") && _cache.Get("StartPagePaths") != null)
                    {
                        _startpagepaths = _cache.Get("StartPagePaths") as List<string>;
                        if (!_startpagepaths.Contains(_kpiHelper.Service.GetUrl(ContentReference.StartPage)))
                        {
                            _startpagepaths.Add(_kpiHelper.Service.GetUrl(ContentReference.StartPage));
                            _cache.Remove("StartPagePaths");
                            _cache.Add("SiteStart", _startpagepaths, DateTimeOffset.MaxValue);
                        }
                    }
                    else
                    {
                        _startpagepaths.Add(_kpiHelper.Service.GetUrl(ContentReference.StartPage));
                        _cache.Add("SiteStart", _startpagepaths, DateTimeOffset.MaxValue);
                    }
                }

                if ( ContentReference.StartPage.ID == _content.ContentLink.ID )
                {
                    // if the target content is the start page, we also need to check 
                    // the path to make sure its not just a request for some other static
                    // resources such as css or jscript
                    retval = (_startpagepaths.Contains(_kpiHelper.Service.GetRequestPath(), StringComparer.OrdinalIgnoreCase) 
                        && ContentGuid.Equals(ea.Content.ContentGuid));
                }
                else
                {   
                    //We need to make sure the content being evaluated is the actual content being requested
                    //Addresses MAR-1226
                    retval = (_kpiHelper.Service.GetUrl(_content.ContentLink).ToLower().Trim('/') == _kpiHelper.Service.GetRequestPath().ToLower().Trim('/') 
                        && ContentGuid.Equals(ea.Content.ContentGuid));                    
                }
            }

            return new KpiConversionResult() { KpiId = Id, HasConverted = retval };
        }

        private bool IsContentPublished(IContent content)
        {
            var publishedContent = _contentVersionRepository.Service.LoadPublished(content.ContentLink);
            if (publishedContent == null)
            {
                throw new KpiValidationException(LocalizationService.Current.GetString("/kpi/content_comparator_kpi/config_markup/error_selected_notpublished"));
            }
            return true;
        }

        private bool IsCurrentContent(IContent conversionContent, IContent currentContent)
        {
            if (conversionContent.ContentLink.ID == currentContent.ContentLink.ID)
            {
                throw new KpiValidationException(LocalizationService.Current.GetString("/kpi/content_comparator_kpi/config_markup/error_selected_samepage"));
            }
            return false;
        }

        private EventHandler<ContentEventArgs> _eh;

        /// <inheritdoc />
        public override event EventHandler EvaluateProxyEvent
        {
            add {
                _eh = new EventHandler<ContentEventArgs>(value);
                _contentEvents.Service.LoadedContent += _eh;
            }
            remove {
                _contentEvents.Service.LoadedContent -= _eh;
            }
        }
    }
}

using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.ServiceLocation;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace EPiServer.Marketing.KPI.Commerce.Kpis
{
    /// <summary>
    /// Base class that contains some common functionality between commerce KPI instances.
    /// Note that it is abstract specifically so that it doesn't show up in the picker.
    /// </summary>
    [DataContract]
    public abstract class CommerceKpi : Kpi
    {
        protected readonly Injected<IContentLoader> _contentLoader;
        protected readonly Injected<ReferenceConverter> _referenceConverter;
        protected readonly Injected<IPublishedStateAssessor> _publishedStateAssessor;
        protected readonly Injected<IContentRepository> _contentRepository;

        [ExcludeFromCodeCoverage]
        protected string LocalizationSection { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public Guid ContentGuid;

        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public bool isVariant;

        /// <inheritdoc />
        public override void Validate(Dictionary<string, string> responseData)
        {
            if (responseData["ConversionProduct"] == "")
            {
                throw new KpiValidationException(LocalizationService.Current.GetString("/commercekpi/" + LocalizationSection + "/config_markup/error_conversionproduct"));
            }

            //Get the correct product id as it's represented in EPiServer Commerce
            //In this example we arbitrarily use the integer 1
            var productIdFromCommerce = responseData["ConversionProduct"].Split('_')[0];

            //We use the content link builder to get the contentlink to our product
            var productLink = _referenceConverter.Service.GetContentLink(Int32.Parse(productIdFromCommerce),
                CatalogContentType.CatalogEntry, 0);

            //Get the product using CMS API
            var content = _contentLoader.Service.Get<EntryContentBase>(productLink);
            if (!IsContentPublished(content))
            {
                throw new KpiValidationException(LocalizationService.Current.GetString("/commercekpi/" + LocalizationSection + "/config_markup/error_not_published_product"));
            }
            ContentGuid = content.ContentGuid;
            isVariant = content is VariationContent;
        }

        private bool IsContentPublished(IContent content)
        {
            return _publishedStateAssessor.Service.IsPublished(content, PagePublishedStatus.Published);
        }

        /// <inheritdoc />
        [DataMember]
        public override string UiMarkup
        {
            get
            {
                var conversionLabel = LocalizationService.Current
                    .GetString("/commercekpi/" + LocalizationSection + "/config_markup/conversion_label");
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

                var conversionDescription = LocalizationService.Current
                    .GetString("/commercekpi/" + LocalizationSection + "/readonly_markup/conversion_selector_description");

                if (!Guid.Empty.Equals(ContentGuid))
                {
                    var content = _contentRepository.Service.Get<IContent>(ContentGuid);
                    markup = string.Format(markup, conversionDescription,
                        content.ContentLink, content.Name);
                }

                return markup;
            }
        }

        private EventHandler<OrderGroupEventArgs> _eh;
        /// <summary>
        /// Setup the event that we want to be evaluated on
        /// </summary>
        public override event EventHandler EvaluateProxyEvent
        {
            add
            {
                _eh = new EventHandler<OrderGroupEventArgs>(value);
                OrderContext.Current.OrderGroupUpdated += _eh;
            }
            remove
            {
                OrderContext.Current.OrderGroupUpdated -= _eh;
            }
        }
    }
}

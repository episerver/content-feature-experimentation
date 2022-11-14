using EPiServer.Commerce.Order;
using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.KPI.Results;
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using EPiServer.Framework.Localization;
using Mediachase.Commerce.Markets;
using Mediachase.Commerce.Shared;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager.DataClass;
using System.Diagnostics.CodeAnalysis;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.KPI.Commerce.Kpis
{
    /// <summary>
    /// Tests the potential effects of content on the average cart total of participating users.  Results: The total represents the average cart total across all visitors that checked out as part of the test
    /// </summary>
    [DataContract]
    [UIMarkup(configmarkup = "EPiServer.Marketing.KPI.Commerce.Markup.AverageOrderKpiConfigMarkup.html",
        readonlymarkup = "EPiServer.Marketing.KPI.Commerce.Markup.AverageOrderKpiReadOnlyMarkup.html",
        text_id = "/commercekpi/averageorder/name",
        description_id = "/commercekpi/averageorder/description")]
    [AlwaysEvaluate]
    public class AverageOrderKpi : CommerceKpi, IFinancialKpi
    {
        private ILogger _logger;
        private readonly Injected<IMarketService> _marketService;
        private readonly Injected<IKpiManager> _kpiManager;
        private readonly Injected<IOrderGroupCalculator> _orderGroupCalculator;

        [ExcludeFromCodeCoverage]
        public AverageOrderKpi()
        {
            LocalizationSection = "averageorder";
            _logger = LogManager.GetLogger();
        }

        /// <inheritdoc />
        [DataMember]
        public override string KpiResultType
        {
            get
            {
                return typeof(KpiFinancialResult).Name.ToString();
            }
        }

        /// <inheritdoc />
        [DataMember]
        public override string UiMarkup
        {
            get
            {
                var conversionText = LocalizationService.Current
                   .GetString("/commercekpi/" + LocalizationSection + "/config_markup/conversion_label");
                return string.Format(base.UiMarkup, conversionText);

            }
        }

        /// <inheritdoc />
        [DataMember]
        public override string UiReadOnlyMarkup
        {
            get
            {
                var conversionText = LocalizationService.Current
                   .GetString("/commercekpi/" + LocalizationSection + "/readonly_markup/conversion_description");
                return string.Format(base.UiReadOnlyMarkup, conversionText);
            }
        }

        /// <summary>
        /// Kpi implentation for using the DynamicDataStore that is part of EPiServer for storing commerce related settings.
        /// </summary>
        [DataMember]
        public CommerceData PreferredFinancialFormat { get; set; }       

        /// <inheritdoc />
        public override void Validate(Dictionary<string, string> responseData)
        {
            var commerceData = _kpiManager.Service.GetCommerceSettings();

            if(commerceData == null)
            {
                var defaultMarket = _marketService.Service.GetMarket("DEFAULT");
                if(defaultMarket == null)
                {
                    throw new KpiValidationException(LocalizationService.Current.GetString("/commercekpi/averageorder/config_markup/error_defaultmarketundefined"));
;                }
            }
            else
            {
                var preferredMarket = _marketService.Service.GetMarket(commerceData.CommerceCulture);
                if(preferredMarket == null)
                {
                    throw new KpiValidationException(LocalizationService.Current.GetString("/commercekpi/averageorder/config_markup/error_undefinedmarket"));
                }
            }
        }

        /// <inheritdoc />
        public override IKpiResult Evaluate(object sender, EventArgs e)
        {
            var retval = new KpiFinancialResult() {
                KpiId = Id,
                Total = 0,
                HasConverted = false
            };

            var ordergroup = sender as IPurchaseOrder;
            if (ordergroup != null)
            {
                var orderTotal = _orderGroupCalculator.Service.GetOrderGroupTotals(ordergroup).SubTotal;
                var orderMarket = _marketService.Service.GetMarket(ordergroup.MarketId);
                var orderCurrency = orderMarket.DefaultCurrency.CurrencyCode;
                var preferredMarket = _marketService.Service.GetMarket(PreferredFinancialFormat.CommerceCulture);

                if (preferredMarket != null)
                {
                    if (orderCurrency != preferredMarket.DefaultCurrency.CurrencyCode)
                    {
                        var convertedTotal = CurrencyFormatter.ConvertCurrency(orderTotal, preferredMarket.DefaultCurrency.CurrencyCode);
                        retval.ConvertedTotal = convertedTotal.Amount;
                    }
                    else
                    {
                        retval.ConvertedTotal = orderTotal.Amount;
                    }
                    retval.HasConverted = true;
                    retval.Total = orderTotal.Amount;
                    retval.TotalMarketCulture = orderCurrency;
                    retval.ConvertedTotalCulture = preferredMarket.MarketId.Value;
                }
                else
                {
                    _logger.Error(LocalizationService.Current.GetString("/commercekpi/averageorder/config_markup/error_undefinedmarket"));
                }
                
            }
            return retval;
        }
    }
}

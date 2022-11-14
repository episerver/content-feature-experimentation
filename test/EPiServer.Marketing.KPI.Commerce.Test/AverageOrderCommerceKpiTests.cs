using EPiServer.Commerce.Order;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Commerce.Kpis;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.ServiceLocation;
using Mediachase.Commerce;
using Mediachase.Commerce.Markets;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace EPiServer.Marketing.KPI.Commerce.Test
{
    [ExcludeFromCodeCoverage]
    public class AverageOrderCommerceKpiTests : CommerceKpiTestsBase
    {
        private Guid _kpiId = Guid.Parse("c1327f8f-4063-48b0-a35a-61b9a37d3901");
        private CommerceData commerceData;
        private Currency _defaultCurrency;
        private MyLogger _logger = new MyLogger();

        private IServiceCollection Services { get; } = new ServiceCollection();

        private AverageOrderKpi GetUnitUnderTest()
        {
            commerceData = new CommerceData();
            commerceData.CommerceCulture = "English";
            commerceData.preferredFormat = new System.Globalization.NumberFormatInfo();

            _defaultCurrency = Currency.USD;

            Services.AddSingleton(_mockMarketService.Object);
            Services.AddSingleton(_mockKpiManager.Object);
            Services.AddSingleton(_mockOrderGroupCalculator.Object);
            Services.AddSingleton(_logger);

            ServiceLocator.SetScopedServiceProvider(Services.BuildServiceProvider());

            return new AverageOrderKpi();
        }

        [Fact]
        public void AverageOrderKpiResultType_Is_KpiFinancialresult()
        {
            var averageOrderKpi = GetUnitUnderTest();
            Assert.True(averageOrderKpi.KpiResultType == "KpiFinancialResult");
        }

        [Fact]
        public void AverageOrder_UIMarkup_IsRetreived_Correctly()
        {
            var averageOrder = GetUnitUnderTest();

            Assert.NotNull(averageOrder.UiMarkup);
        }

        [Fact]
        public void AverageOrdert_UIReadOnlyMarkup_IsRetrieved_Correctly()
        {
            var averageOrder = GetUnitUnderTest();

            Assert.NotNull(averageOrder.UiReadOnlyMarkup);
        }

        [Fact]
        public void AverageOrderCommerce_DefaultReturnVal_Returned_WhenOrderGroupEventArgsAndOrderGroup_AreNull()
        {
            var averageOrder = GetUnitUnderTest();
            averageOrder.Id = _kpiId;

            var returnVal = averageOrder.Evaluate(new object(), new EventArgs());

            Assert.True(returnVal.KpiId == averageOrder.Id);
            Assert.True(!returnVal.HasConverted);
        }

        [Fact]
        public void Validate_DoesNotThrowExceptions_WhenRequiredData_IsNotNull()
        {            
            Dictionary<string, string> responseData = new Dictionary<string, string> { { "a", "b" } };

            var averageOrder = GetUnitUnderTest();
            _mockKpiManager.Setup(call => call.GetCommerceSettings()).Returns(commerceData);
            _mockMarketService.Setup(call => call.GetMarket(It.IsAny<MarketId>())).Returns(_mockMarket.Object);

            averageOrder.Validate(responseData);
        }

       [Fact]
       public void Validate_DoesNotThrowException_WhenCommerceDataIsNull_And_PreferredMarketIsPopulated()
        {
            Dictionary<string, string> responseData = new Dictionary<string, string> { { "a", "b" } };

            var averageOrder = GetUnitUnderTest();
            _mockKpiManager.Setup(call => call.GetCommerceSettings()).Returns((CommerceData)null);
            _mockMarketService.Setup(call => call.GetMarket(It.IsAny<MarketId>())).Returns(_mockMarket.Object);

            averageOrder.Validate(responseData);
        }

        [Fact]
        public void Validate_ThrowsException_WhenCommerceDataAndMarket_AreNull()
        {
            Dictionary<string, string> responseData = new Dictionary<string, string> { { "a", "b" } };

            var averageOrder = GetUnitUnderTest();
            _mockKpiManager.Setup(call => call.GetCommerceSettings()).Returns((CommerceData)null);
            _mockMarketService.Setup(call => call.GetMarket(It.IsAny<MarketId>())).Returns((IMarket)null);

            Assert.Throws<KpiValidationException>(() => averageOrder.Validate(responseData));
        }

        [Fact]
        public void Validate_ThrowsException_MarketIsNull_And_CommerceDataIsPopulated()
        {
            Dictionary<string, string> responseData = new Dictionary<string, string> { { "a", "b" } };

            var averageOrder = GetUnitUnderTest();
            _mockKpiManager.Setup(call => call.GetCommerceSettings()).Returns(commerceData);
            _mockMarketService.Setup(call => call.GetMarket(It.IsAny<MarketId>())).Returns((IMarket)null);

            Assert.Throws<KpiValidationException>(() => averageOrder.Validate(responseData));
        }
    }
}

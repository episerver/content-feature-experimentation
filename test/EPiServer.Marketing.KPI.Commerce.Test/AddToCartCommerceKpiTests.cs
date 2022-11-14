﻿using EPiServer.Commerce.Catalog.ContentTypes;
using EPiServer.Framework.Cache;
using EPiServer.Marketing.KPI.Commerce.Kpis;
using Mediachase.Commerce.Catalog;
using Mediachase.Commerce.Orders;
using Moq;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using System.Collections.Generic;
using System.Globalization;
using EPiServer.Commerce.Order;
using EPiServer.Commerce.Order.Internal;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Core;
using EPiServer.Marketing.KPI.Commerce.Test.Fakes;
using EPiServer.Marketing.Testing.Test.Fakes;
using Mediachase.Commerce;
using Mediachase.Commerce.Inventory;
using Mediachase.Commerce.Markets;
using Microsoft.Extensions.DependencyInjection;
using EPiServer.ServiceLocation;

namespace EPiServer.Marketing.KPI.Commerce.Test
{
    [ExcludeFromCodeCoverage]
    public class AddToCartCommerceKpiTests : CommerceKpiTestsBase
    {
        private Guid _kpiId = Guid.Parse("c1327f8f-4063-48b0-a35a-61b9a37d3901");
        private Guid _contentGuid = Guid.Parse("cbe9f6ed-3d20-47b0-93da-0d76d5c095a7");
        private IServiceCollection Services { get; } = new ServiceCollection();

        private AddToCartKpi GetUnitUnderTest()
        {
            var synchronizedObjectInstanceCache = new Mock<ISynchronizedObjectInstanceCache>();
            _mockReferenceConverter = new Mock<ReferenceConverter>(
                new EntryIdentityResolver(synchronizedObjectInstanceCache.Object, new CatalogOptions()),
                new NodeIdentityResolver(synchronizedObjectInstanceCache.Object, new CatalogOptions()));

            Services.AddSingleton(_mockContentLoader.Object);
            Services.AddSingleton(_mockReferenceConverter.Object);
            Services.AddSingleton(_mockContentRepository.Object);
            Services.AddSingleton(_mockPublishedStateAssossor.Object);

            ServiceLocator.SetScopedServiceProvider(Services.BuildServiceProvider());

            return new AddToCartKpi();
        }

        [Fact]
        public void AddToCart_DefaultReturnVal_Returned_WhenOrderGroupEventArgsAndOrderGroup_AreNull()
        {
            AddToCartKpi addToCartKpi = GetUnitUnderTest();
            addToCartKpi.Id = _kpiId;

            var returnVal = addToCartKpi.Evaluate(new object(), new EventArgs());

            Assert.True(returnVal.KpiId == addToCartKpi.Id);
            Assert.True(!returnVal.HasConverted);
        }

        [Fact]
        public void AddToCart_DefaultReturnVal_Returned_WhenOrderGroup_IsNull()
        {
            OrderGroupEventArgs orderArgs = new OrderGroupEventArgs(1, OrderGroupEventType.Cart);

            AddToCartKpi addToCartKpi = GetUnitUnderTest();
            addToCartKpi.Id = _kpiId;

            var returnVal = addToCartKpi.Evaluate(new object(), orderArgs);

            Assert.True(returnVal.KpiId == addToCartKpi.Id);
            Assert.True(!returnVal.HasConverted);
        }

        [Fact]
        public void AddToCart_DefaultReturnVal_Returned_WhenOrderGroupEventArgs_IsNull()
        {
            AddToCartKpi addToCartKpi = GetUnitUnderTest();
            addToCartKpi.Id = _kpiId;

            var returnVal = addToCartKpi.Evaluate(_mockOrderGroup, new EventArgs());

            Assert.True(returnVal.KpiId == addToCartKpi.Id);
            Assert.True(!returnVal.HasConverted);
        }

        [Fact]
        public void AddToCart_DefaultReturnVal_Returned_WhenOrderFormsIsEmpty()
        {
            OrderGroupEventArgs orderArgs = new OrderGroupEventArgs(1, OrderGroupEventType.Cart);
            PurchaseOrder po = new PurchaseOrder(Guid.Parse("0fa0ac0c-25a0-4641-8929-f61b71f15ad2"));

            AddToCartKpi addToCartKpi = GetUnitUnderTest();
            addToCartKpi.Id = _kpiId;

            var returnVal = addToCartKpi.Evaluate(po, orderArgs);

            Assert.True(returnVal.KpiId == addToCartKpi.Id);
            Assert.True(!returnVal.HasConverted);
        }

        [Fact]
        public void AddToCart_DefaultReturnVal_Returned_WhenLineItemsIsEmpty()
        {
            OrderGroupEventArgs orderArgs = new OrderGroupEventArgs(1, OrderGroupEventType.Cart);
            PurchaseOrder po = new PurchaseOrder(Guid.Parse("0fa0ac0c-25a0-4641-8929-f61b71f15ad2"));
            OrderForm of = new OrderForm();
            OrderForm of2 = new OrderForm();
            po.OrderForms.Add(of);
            po.OrderForms.Add(of2);

            AddToCartKpi addToCartKpi = GetUnitUnderTest();
            addToCartKpi.Id = _kpiId;

            var returnVal = addToCartKpi.Evaluate(po, orderArgs);

            Assert.True(returnVal.KpiId == addToCartKpi.Id);
            Assert.True(!returnVal.HasConverted);
        }

        [Fact]
        public void AddToCart_HasConverted_IsTrue_WhenIsVariant_AndContentGuidsMatch()
        {
            var contentGuid = Guid.Parse("a94daef6-aaad-4d41-a4d9-711f2b441124");

            var catBase = new Mock<EntryContentBase>();
            catBase.SetupGet(x => x.Name).Returns("Mock Catalog Content");
            catBase.SetupGet(x => x.ContentGuid).Returns(contentGuid);

            var refer = new ContentReference() { ID = 1, WorkID = 111 };
            var orderArgs = new OrderGroupEventArgs(1, OrderGroupEventType.Cart);

            var addToCartKpi = GetUnitUnderTest();

            var orderGroup = FakeHelpers.CreateFakeOrderGroup();

            _mockReferenceConverter.Setup(call => call.GetContentLinks(It.IsAny<IEnumerable<string>>())).Returns(new Dictionary<string, ContentReference>() { { "code", refer }});
            _mockContentLoader.Setup(call => call.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<CultureInfo>())).Returns(new[] { catBase.Object });

            addToCartKpi.Id = _kpiId;
            addToCartKpi.ContentGuid = contentGuid;
            addToCartKpi.isVariant = true;

            var returnVal = addToCartKpi.Evaluate(orderGroup, orderArgs);
            Assert.True(returnVal.KpiId == addToCartKpi.Id);
            Assert.True(returnVal.HasConverted);
        }

        [Fact]
        public void Validate_ThrowsException_IfConversionProduct_IsNull()
        {
            Dictionary<string, string> responseData = new Dictionary<string, string> { { "ConversionProduct", "" } };

            var addToCartKpi = GetUnitUnderTest();

            Assert.Throws<KpiValidationException>(() => addToCartKpi.Validate(responseData));
        }

        [Fact]
        public void Validate_ThrowsException_IfContentType_IsNotCatalogEntry()
        {
            Dictionary<string, string> responseData = new Dictionary<string, string> { { "ConversionProduct", "1_100" }, { "CurrentContent", "2" } };
            Mock<CatalogContentBase> _mockContent = new Mock<CatalogContentBase>();
            _mockContent.SetupGet(prop => prop.ContentLink).Returns(new ContentReference(1));
            _mockContent.SetupGet(prop => prop.ContentType).Returns(CatalogContentType.Catalog);


            var addToCartKpi = GetUnitUnderTest();

            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(_mockContent.Object);
            _mockReferenceConverter.Setup(call => call.GetContentLink(It.IsAny<int>(), It.IsAny<CatalogContentType>(), It.IsAny<int>())).Returns(new ContentReference(1));
            _mockContentLoader.Setup(call => call.GetItems(It.IsAny<IEnumerable<ContentReference>>(), It.IsAny<CultureInfo>())).Returns(new[] { _mockContent.Object });
            Assert.Throws<KpiValidationException>(() => addToCartKpi.Validate(responseData));
        }

        [Fact]
        public void Validate_ThrowsException_WhenContentIsNotPublished()
        {
            Dictionary<string, string> responseData = new Dictionary<string, string> { { "ConversionProduct", "1_100" }, { "CurrentContent", "2" } };
            Mock<VariationContent> _mockContent = new Mock<VariationContent>();
            _mockContent.SetupGet(prop => prop.ContentLink).Returns(new ContentReference(1));
            _mockContent.SetupGet(prop => prop.ContentType).Returns(CatalogContentType.CatalogEntry);
            _mockContent.SetupGet(prop => prop.ContentGuid).Returns(_contentGuid);


            DataAbstraction.ContentVersion contentVersion = new DataAbstraction.ContentVersion(new ContentReference(1), "Test", VersionStatus.Published, DateTime.Now, "Me", "Me", 2, "en", false, false);

            var addToCartKpi = GetUnitUnderTest();

            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(_mockContent.Object);
            _mockReferenceConverter.Setup(call => call.GetContentLink(It.IsAny<int>(), It.IsAny<CatalogContentType>(), It.IsAny<int>())).Returns(new ContentReference(1));
            _mockContentLoader.Setup(call => call.Get<EntryContentBase>(It.IsAny<ContentReference>())).Returns(_mockContent.Object);
            _mockPublishedStateAssossor.Setup(x => x.IsPublished(It.IsAny<IContent>(), PublishedStateCondition.None))
                .Returns(false);
            Assert.Throws<KpiValidationException>(() => addToCartKpi.Validate(responseData));
        }

        [Fact]
        public void Validate_SetsContentGuid_And_IsVariantTrue_When_ProductIsVariationContent()
        {
            Dictionary<string, string> responseData = new Dictionary<string, string> { { "ConversionProduct", "1_100" }, { "CurrentContent", "2" } };
            Mock<VariationContent> _mockContent = new Mock<VariationContent>();
            _mockContent.SetupGet(prop => prop.ContentLink).Returns(new ContentReference(1));
            _mockContent.SetupGet(prop => prop.ContentType).Returns(CatalogContentType.CatalogEntry);
            _mockContent.SetupGet(prop => prop.ContentGuid).Returns(_contentGuid);


            DataAbstraction.ContentVersion contentVersion = new DataAbstraction.ContentVersion(new ContentReference(1), "Test", VersionStatus.Published, DateTime.Now, "Me", "Me", 2, "en", false, false);

            var addToCartKpi = GetUnitUnderTest();

            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(_mockContent.Object);
            _mockReferenceConverter.Setup(call => call.GetContentLink(It.IsAny<int>(), It.IsAny<CatalogContentType>(), It.IsAny<int>())).Returns(new ContentReference(1));
            _mockContentLoader.Setup(call => call.Get<EntryContentBase>(It.IsAny<ContentReference>())).Returns(_mockContent.Object);
            _mockPublishedStateAssossor.Setup(x => x.IsPublished(It.IsAny<IContent>(), PublishedStateCondition.None))
                .Returns(true);

            addToCartKpi.Validate(responseData);
            Assert.True(addToCartKpi.ContentGuid == _contentGuid);
            Assert.True(addToCartKpi.isVariant == true);

        }

        [Fact]
        public void Validate_SetsContentGuid_And_IsVariantFalse_WhenContentIsNotVariant()
        {
            Dictionary<string, string> responseData = new Dictionary<string, string> { { "ConversionProduct", "1_100" }, { "CurrentContent", "2" } };
            Mock<EntryContentBase> _mockContent = new Mock<EntryContentBase>();
            _mockContent.SetupGet(prop => prop.ContentLink).Returns(new ContentReference(1));
            _mockContent.SetupGet(prop => prop.ContentType).Returns(CatalogContentType.CatalogEntry);
            _mockContent.SetupGet(prop => prop.ContentGuid).Returns(_contentGuid);

            var addToCartKpi = GetUnitUnderTest();

            _mockContentRepository.Setup(call => call.Get<IContent>(It.IsAny<ContentReference>())).Returns(_mockContent.Object);
            _mockReferenceConverter.Setup(call => call.GetContentLink(It.IsAny<int>(), It.IsAny<CatalogContentType>(), It.IsAny<int>())).Returns(new ContentReference(1));
            _mockContentLoader.Setup(call => call.Get<EntryContentBase>(It.IsAny<ContentReference>())).Returns(_mockContent.Object);
            _mockPublishedStateAssossor.Setup(x => x.IsPublished(It.IsAny<IContent>(), PublishedStateCondition.None))
                .Returns(true);

            addToCartKpi.Validate(responseData);
            Assert.True(addToCartKpi.ContentGuid == _contentGuid);
            Assert.True(addToCartKpi.isVariant == false);
        }
    }
}

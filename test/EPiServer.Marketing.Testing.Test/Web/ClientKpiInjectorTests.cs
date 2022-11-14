﻿using EPiServer.Framework.Web.Resources;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web.ClientKPI;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Xunit;



namespace EPiServer.Marketing.Testing.Test.Web
{
    public class ClientKpiInjectorTests
    {
        Mock<IServiceProvider> mockServiceLocator;
        Mock<ITestingContextHelper> mockTestingHelper;
        Mock<IMarketingTestingWebRepository> mockWebRepo;        
        Mock<ILogger> mockLogger;
        Mock<IHttpContextHelper> mockHttpContextHelper;
        Mock<IKpiManager> mockKpiManager;
        Mock<IRequiredClientResourceList> mockIRequiredClientResourceList;
        public IServiceCollection Services { get; } = new ServiceCollection();
        private string FakeClientCookie()
        {
            var cookieString = "{\"41b24d0e-5e38-4214-b889-fbd9491312b8\":{\"TestId\":\"3f183552-4549-4d80-90e6-6fcbbb339909\",\"TestContentId\":\"39186dbe-598c-4576-a468-e054d374edd8\",\"TestVariantId\":\"69dac1a5-1751-4265-9f85-26c041ba2d63\",\"ShowVariant\":false,\"Viewed\":true,\"Converted\":false,\"KpiConversionDictionary\":{\"41b24d0e-5e38-4214-b889-fbd9491312b8\":false}}}";
            return cookieString;
        }

        private IMarketingTest FakeABTest()
        {
            var retTest = new ABTest()
            {
                Id = Guid.NewGuid(),
                Variants = new List<Variant> { new Variant() { Id = new Guid("69dac1a5-1751-4265-9f85-26c041ba2d63"), ItemVersion = 10 } }
            };

            return retTest;
        }

        private ClientKpiInjector GetUnitUnderTest()
        {
            mockServiceLocator = new Mock<IServiceProvider>();
            mockTestingHelper = new Mock<ITestingContextHelper>();
            mockWebRepo = new Mock<IMarketingTestingWebRepository>();
            mockLogger = new Mock<ILogger>();
            mockHttpContextHelper = new Mock<IHttpContextHelper>();
            mockKpiManager = new Mock<IKpiManager>();
            mockIRequiredClientResourceList = new Mock<IRequiredClientResourceList>();

            mockServiceLocator.Setup(sl => sl.GetService(typeof(ITestingContextHelper))).Returns(mockTestingHelper.Object);
            mockServiceLocator.Setup(sl => sl.GetService(typeof(IMarketingTestingWebRepository))).Returns(mockWebRepo.Object);
            mockServiceLocator.Setup(sl => sl.GetService(typeof(ILogger))).Returns(mockLogger.Object);
            mockServiceLocator.Setup(sl => sl.GetService(typeof(IHttpContextHelper))).Returns(mockHttpContextHelper.Object);
            mockServiceLocator.Setup(sl => sl.GetService(typeof(IKpiManager))).Returns(mockKpiManager.Object);
            mockIRequiredClientResourceList.Setup(x => x.RequireScriptInline(It.IsAny<string>())).Returns(new ClientResourceSettings(Guid.NewGuid().ToString()));

            Services.AddSingleton(mockIRequiredClientResourceList.Object);

            ServiceLocator.SetScopedServiceProvider(Services.BuildServiceProvider());

            return new ClientKpiInjector(mockServiceLocator.Object);
        }

        [Fact]
        public void ActivateClientKpis_adds_to_context_when_a_clientKpi_is_detected()
        {
            var aClientKpi = new TimeOnPageClientKpi(){Id = Guid.NewGuid()};
            var aKpiList = new List<IKpi> {aClientKpi};
            var aTestCookie = new TestDataCookie();

            var aClientKpiInjector = GetUnitUnderTest();
            mockTestingHelper.Setup(m => m.IsHtmlContentType()).Returns(true).Verifiable();

            aClientKpiInjector.ActivateClientKpis(aKpiList, aTestCookie);

            //verify the expected item name and value are set
            mockHttpContextHelper.Verify(hch => hch.SetItemValue(It.Is<string>(item => item == aClientKpi.Id.ToString()), It.IsAny<object>()),
                Times.Once(), "the item with the KPI id was not added to the HttpContext");
        }

        [Fact]
        public void ActivateClientKpis_only_adds_clientKpis_to_the_context()
        {
            var aClientKpi = new FakeClientKpi(){Id = Guid.NewGuid()};
            var aServerKpi = new FakeServerKpi(){Id = Guid.NewGuid()};
            var aKpiList = new List<IKpi> {aClientKpi, aServerKpi};
            var aTestCookie = new TestDataCookie();

            var aClientKpiInjector = GetUnitUnderTest();
            mockTestingHelper.Setup(m => m.IsHtmlContentType()).Returns(true).Verifiable();

            aClientKpiInjector.ActivateClientKpis(aKpiList, aTestCookie);

            //verify the expected item name and value are set
            mockHttpContextHelper.Verify(hch => hch.SetItemValue(It.Is<string>(item => item == aClientKpi.Id.ToString()), It.IsAny<object>()),
                Times.Once(), "only the client kpi should be set in the items collection");
        }

        [Fact]
        public void ActivateClientKpis_does_not_add_items_or_cookies_when_the_context_already_has_the_item()
        {
            var aClientKpi = new TimeOnPageClientKpi(){Id = Guid.NewGuid()};
            var aKpiList = new List<IKpi> {aClientKpi};
            var aTestCookie = new TestDataCookie();

            var aClientKpiInjector = GetUnitUnderTest();
            mockHttpContextHelper.Setup(hch => hch.HasItem(It.IsAny<string>())).Returns(true);

            aClientKpiInjector.ActivateClientKpis(aKpiList, aTestCookie);

            //verify the cookie was added only once
            mockHttpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()),
                Times.Never(), "should not do work when the item already exists in the HttpContext");
            //verify the expected item name and value are set
            mockHttpContextHelper.Verify(hch => hch.SetItemValue(It.IsAny<string>(), It.IsAny<object>()),
                Times.Never(), "should not do work when the item already exists in the HttpContext");
            mockTestingHelper.Verify(hch => hch.IsHtmlContentType(), Times.Once);
        }

        [Fact]
        public void ActivateClientKpis_does_not_add_items_or_cookies_when_ContentIsNotHtml()
        {
            var aClientKpi = new TimeOnPageClientKpi() { Id = Guid.NewGuid() };
            var aKpiList = new List<IKpi> { aClientKpi };
            var aTestCookie = new TestDataCookie();

            var aClientKpiInjector = GetUnitUnderTest();
            mockTestingHelper.Setup(th => th.IsHtmlContentType()).Returns(false);

            aClientKpiInjector.ActivateClientKpis(aKpiList, aTestCookie);

            //verify the cookie was added only once
            mockHttpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()),
                Times.Never(), "should not do work when in edit mode");
            //verify the expected item name and value are set
            mockHttpContextHelper.Verify(hch => hch.SetItemValue(It.IsAny<string>(), It.IsAny<object>()),
                Times.Never(), "should not do work when in edit mode");
            mockTestingHelper.Verify(hch => hch.IsHtmlContentType(), Times.Once);
        }

        [Fact]
        public void ActivateClientKpis_does_not_add_items_or_cookies_when_we_are_in_edit_mode()
        {
            var aClientKpi = new TimeOnPageClientKpi(){Id = Guid.NewGuid()};
            var aKpiList = new List<IKpi> {aClientKpi};
            var aTestCookie = new TestDataCookie();
            
            var aClientKpiInjector = GetUnitUnderTest();
            mockTestingHelper.Setup(th => th.IsInSystemFolder()).Returns(true);

            aClientKpiInjector.ActivateClientKpis(aKpiList, aTestCookie);

            //verify the cookie was added only once
            mockHttpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()),
                Times.Never(), "should not do work when in edit mode");
            //verify the expected item name and value are set
            mockHttpContextHelper.Verify(hch => hch.SetItemValue(It.IsAny<string>(), It.IsAny<object>()),
                Times.Never(), "should not do work when in edit mode");
            mockTestingHelper.Verify(hch => hch.IsHtmlContentType(), Times.Once);
        }

        [Fact]
        public void ActivateClientKpis_does_not_add_items_or_cookies_when_the_kpi_has_converted_already()
        {
            var aClientKpi = new TimeOnPageClientKpi(){Id = Guid.NewGuid()};
            var aKpiList = new List<IKpi> {aClientKpi};
            var aTestCookie = new TestDataCookie() { Converted = true, AlwaysEval = false };

            var aClientKpiInjector = GetUnitUnderTest();
            mockHttpContextHelper.Setup(hch => hch.HasItem(It.IsAny<string>())).Returns(true);

            aClientKpiInjector.ActivateClientKpis(aKpiList, aTestCookie);

            //verify the cookie was added only once
            mockHttpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CookieOptions>()),
                Times.Never(), "should not do work when in edit mode");
            //verify the expected item name and value are set
            mockHttpContextHelper.Verify(hch => hch.SetItemValue(It.IsAny<string>(), It.IsAny<object>()),
                Times.Never(), "should not do work when in edit mode");
            mockTestingHelper.Verify(hch => hch.IsHtmlContentType(), Times.Once);
        }

        [Fact]
        public void AppendClientKpiScript_attaches_an_ab_filter_with_the_clientkpi_script_and_removes_special_chars()
        {
            var aClientKpiInjector = GetUnitUnderTest();
            var aClientKpi = new TimeOnPageClientKpi();
            var aKpiList = new List<IKpi> { aClientKpi };
            var aFakeTest = FakeABTest();
            var aFakeCookie = FakeClientCookie();
            var clientKpis= JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(aFakeCookie);
            mockHttpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            mockServiceLocator.Setup(sl => sl.GetService(typeof(IKpiManager))).Returns(mockKpiManager.Object);
            mockHttpContextHelper.Setup(hch => hch.GetCookieValue(It.IsAny<string>())).Returns(aFakeCookie);
            mockKpiManager.Setup(km => km.Get(It.IsAny<Guid>())).Returns(aClientKpi);
            mockWebRepo.Setup(wr => wr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(aFakeTest);
            mockHttpContextHelper.Setup(hch => hch.HasItem(It.IsAny<string>())).Returns(true);
            mockTestingHelper.Setup(mth => mth.IsHtmlContentType()).Returns(true);

            aClientKpiInjector.AppendClientKpiScript(clientKpis);

            Assert.Contains(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()), aClientKpi.ClientEvaluationScript);

            mockTestingHelper.Verify(hch => hch.IsHtmlContentType(), Times.Once);
        }

        [Fact]
        public void AppendClientKpiScript_attaches_an_ab_filter_with_the_clientkpi_script()
        {
            var aClientKpiInjector = GetUnitUnderTest();
            var aClientKpi = new FakeClientKpi();
            var aKpiList = new List<IKpi> { aClientKpi };
            var aFakeTest = FakeABTest();
            var aFakeCookie = FakeClientCookie();
            var clientKpis = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(aFakeCookie);
            mockHttpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            mockServiceLocator.Setup(sl => sl.GetService(typeof(IKpiManager))).Returns(mockKpiManager.Object);
            mockHttpContextHelper.Setup(hch => hch.GetCookieValue(It.IsAny<string>())).Returns(aFakeCookie);
            mockKpiManager.Setup(km => km.Get(It.IsAny<Guid>())).Returns(aClientKpi);
            mockWebRepo.Setup(wr => wr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(aFakeTest);
            mockHttpContextHelper.Setup(hch => hch.HasItem(It.IsAny<string>())).Returns(true);
            mockTestingHelper.Setup(mth => mth.IsHtmlContentType()).Returns(true);

            aClientKpiInjector.AppendClientKpiScript(clientKpis);

            mockTestingHelper.Verify(hch => hch.IsHtmlContentType(), Times.Once);
        }

        [Fact]
        public void AppendClientKpiScript_does_not_set_the_filter_when_the_cookie_is_missing()
        {
            var aClientKpiInjector = GetUnitUnderTest();
            mockHttpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(false);

            aClientKpiInjector.AppendClientKpiScript(new Dictionary<Guid, TestDataCookie>());

            mockHttpContextHelper.Verify(hch => hch.SetResponseFilter(It.IsAny<ABResponseFilter>()), 
                Times.Never(), "setup a filter with no cookie in the request");
        }

        [Fact]
        public void AppendClientKpiScript_does_not_set_the_filter_when_not_html_content()
        {
            var aClientKpiInjector = GetUnitUnderTest();
            var aClientKpi = new FakeClientKpi();
            var aKpiList = new List<IKpi> { aClientKpi };
            var aFakeTest = FakeABTest();
            var aFakeCookie = FakeClientCookie();
            var clientKpis = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(aFakeCookie);
            mockHttpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            mockServiceLocator.Setup(sl => sl.GetService(typeof(IKpiManager))).Returns(mockKpiManager.Object);
            mockHttpContextHelper.Setup(hch => hch.GetCookieValue(It.IsAny<string>())).Returns(aFakeCookie);
            mockKpiManager.Setup(km => km.Get(It.IsAny<Guid>())).Returns(aClientKpi);
            mockWebRepo.Setup(wr => wr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(aFakeTest);
            mockTestingHelper.Setup(mth => mth.IsHtmlContentType()).Returns(false);

            aClientKpiInjector.AppendClientKpiScript(clientKpis);

            mockTestingHelper.Verify(hch => hch.IsHtmlContentType(), Times.Once);
        }

        [Fact]
        public void AppendClientKpiScript_does_not_set_the_filter_when_there_are_no_client_kpis_to_inject()
        {
            var aClientKpi = new FakeClientKpi();
            var aKpiList = new List<IKpi> { aClientKpi };
            var aFakeTest = FakeABTest();
            var aFakeCookie = FakeClientCookie();
            var clientKpis = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(aFakeCookie);
            var aClientKpiInjector = GetUnitUnderTest();
            mockHttpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            
            mockHttpContextHelper.Setup(call => call.HasCookie(It.IsAny<string>())).Returns(true);
            mockHttpContextHelper.Setup(call => call.CanWriteToResponse()).Returns(true);
            //Force HasItem call to be false to test behavior
            mockHttpContextHelper.Setup(hch => hch.HasItem(It.IsAny<string>())).Returns(false);
            mockHttpContextHelper.Setup(call => call.GetCookieValue(It.IsAny<string>())).Returns(aFakeCookie);
            mockKpiManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns(aClientKpi);
            mockWebRepo.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(aFakeTest);


            aClientKpiInjector.AppendClientKpiScript(clientKpis);

            mockHttpContextHelper.Verify(hch => hch.SetResponseFilter(It.IsAny<ABResponseFilter>()),
                Times.Never(), "setup a filter when there was no client kpi");
        }

        [Fact]
        public void AppendClientKpiScript_does_not_set_the_filter_when_the_stream_is_not_writable()
        {
            var aClientKpi = new FakeClientKpi();
            var aKpiList = new List<IKpi> { aClientKpi };
            var aFakeTest = FakeABTest();
            var aFakeCookie = FakeClientCookie();
            var clientKpis = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(aFakeCookie);
            var aClientKpiInjector = GetUnitUnderTest();
            mockHttpContextHelper.Setup(call => call.HasCookie(It.IsAny<string>())).Returns(true);
            mockHttpContextHelper.Setup(call => call.GetCookieValue(It.IsAny<string>())).Returns(aFakeCookie);
            mockHttpContextHelper.Setup(call => call.HasItem(It.IsAny<string>())).Returns(true);
            //Force CanWriteToResponse call to be false to test behavior
            mockHttpContextHelper.Setup(call => call.CanWriteToResponse()).Returns(false);
            mockKpiManager.Setup(call => call.Get(It.IsAny<Guid>())).Returns(aClientKpi);
            mockWebRepo.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(aFakeTest);
            
            aClientKpiInjector.AppendClientKpiScript(clientKpis);

            mockHttpContextHelper.Verify(hch => hch.SetResponseFilter(It.IsAny<ABResponseFilter>()),
                Times.Never(), "setup a filter when there was no client kpi");
        }

        [Fact]
        public void AppendClientKpiScript_Does_Not_Set_Filter_When_Test_Not_Found()
        {
            var fakeCookie = FakeClientCookie();
            var aClientKpiInjector = GetUnitUnderTest();
            var clientKpis = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(fakeCookie);
            mockHttpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            mockHttpContextHelper.Setup(call => call.GetCookieValue(It.IsAny<string>())).Returns(fakeCookie);
            mockWebRepo.Setup(repo => repo.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns((IMarketingTest)null);

            aClientKpiInjector.AppendClientKpiScript(clientKpis);

            mockHttpContextHelper.Verify(hch => hch.SetResponseFilter(It.IsAny<ABResponseFilter>()), Times.Never());
        }

        [Fact]
        public void AppendClientKpiScript_Does_Not_Set_Filter_When_Variant_Not_Found()
        {
            var fakeCookie = FakeClientCookie();
            var clientKpis = JsonConvert.DeserializeObject<Dictionary<Guid, TestDataCookie>>(fakeCookie);
            var testMissingTheExpectedVariant = new ABTest()
            {
                Id = new Guid("3f183552-4549-4d80-90e6-6fcbbb339909"),
                Variants = new List<Variant> { new Variant() { Id = Guid.NewGuid(), ItemVersion = 10 } }
            };

            var aClientKpiInjector = GetUnitUnderTest();

            mockHttpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            mockHttpContextHelper.Setup(call => call.GetCookieValue(It.IsAny<string>())).Returns(fakeCookie);
            mockWebRepo.Setup(repo => repo.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(testMissingTheExpectedVariant);

            aClientKpiInjector.AppendClientKpiScript(clientKpis);

            mockHttpContextHelper.Verify(hch => hch.SetResponseFilter(It.IsAny<ABResponseFilter>()), Times.Never());
        }
    }
}

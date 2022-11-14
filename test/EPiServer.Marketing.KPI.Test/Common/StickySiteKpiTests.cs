using System;
using System.Collections.Generic;
using System.Threading;
using System.Web;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.KPI.Common;
using EPiServer.Marketing.KPI.Common.Helpers;
using EPiServer.Marketing.KPI.Exceptions;
using EPiServer.Marketing.KPI.Test.Fakes;
using EPiServer.ServiceLocation;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace EPiServer.Marketing.KPI.Test.Common
{
    public class StickySiteKpiTests
    {
        private Mock<IServiceLocator> _serviceLocator = new Mock<IServiceLocator>();
        private Mock<IContentRepository> _contentRepo;
        private Mock<IContentVersionRepository> _contentVersionRepo;
        private Mock<IContentEvents> _contentEvents;
        private Mock<UrlResolver> _urlResolver;
        internal Mock<IKpiHelper> _stickyHelperMock;
        private IContent _content100;
        private IContent _content200;
        private IContent _nullContent100;
        protected readonly Injected<IHttpContextAccessor> _httpContextAccessor;
        private Mock<IHttpContextAccessor> _httpContextAccessorMock;
        public IServiceCollection Services { get; } = new ServiceCollection();

        private StickySiteKpi GetUnitUnderTest()
        {
            _content100 = new BasicContent { ContentLink = new ContentReference(1, 2), ContentGuid = Guid.NewGuid() };
            _content200 = new BasicContent { ContentLink = new ContentReference(11, 12) };
            _nullContent100 = new BasicContent { ContentLink = new ContentReference(111, 112) };
            _stickyHelperMock = new Mock<IKpiHelper>();
            _stickyHelperMock.Setup(call => call.IsInSystemFolder()).Returns(false);
            Services.AddSingleton(_stickyHelperMock.Object);

            var pageRef2 = new PageReference() { ID = 2, WorkID = 5 };
            var contentData = new PageData(pageRef2);
            ContentVersion ver = null;

            _contentRepo = new Mock<IContentRepository>();
            _contentRepo.Setup(call => call.Get<IContent>(It.IsAny<Guid>())).Returns(contentData);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == _content100.ContentLink))).Returns(_content100);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == _content200.ContentLink))).Returns(_content200);
            _contentRepo.Setup(call => call.Get<IContent>(It.Is<ContentReference>(cf => cf == _nullContent100.ContentLink))).Returns(_nullContent100);
            Services.AddSingleton(_contentRepo.Object);

            _contentVersionRepo = new Mock<IContentVersionRepository>();
            _contentVersionRepo.Setup(call => call.LoadPublished(It.Is<ContentReference>(cf => cf != _content100))).Returns(new ContentVersion(ContentReference.EmptyReference, "", VersionStatus.Published, DateTime.Now, "", "", 1, "", true, false));
            Services.AddSingleton(_contentVersionRepo.Object);

            Services.AddSingleton(new FakeLocalizationService("whatever"));

            _contentEvents = new Mock<IContentEvents>();
            Services.AddSingleton(_contentEvents.Object);

            _urlResolver = new Mock<UrlResolver>();
            _urlResolver.Setup(call => call.GetUrl(It.IsAny<ContentReference>())).Returns("/alloy-plan/");
            Services.AddSingleton(_urlResolver.Object);

            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            Services.AddSingleton(_httpContextAccessorMock.Object);
            ServiceLocator.SetScopedServiceProvider(Services.BuildServiceProvider());

            return new StickySiteKpi();
        }

        [Fact]
        public void StickySiteKpi_Throws_Exception_For_Empty_String()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>
            {
                {"Timeout", ""},
                {"CurrentContent", _content200.ContentLink.ToString()}
            };

            Assert.Throws<KpiValidationException>(() => kpi.Validate(data));
        }

        [Fact]
        public void StickySiteKpi_Throws_Exception_For_Invalid_Timeout()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>
            {
                {"Timeout", null},
                {"CurrentContent", _content200.ContentLink.ToString()}
            };

            Assert.Throws<KpiValidationException>(() => kpi.Validate(data));
        }

        [Fact]
        public void StickySiteKpi_Valid_Data()
        {
            var kpi = GetUnitUnderTest();
            var data = new Dictionary<string, string>
            {
                {"Timeout", "5"},
                {"CurrentContent", _content200.ContentLink.ToString()}
            };

            kpi.Validate(data);

            Assert.Equal(5, kpi.Timeout);
        }

        [Fact]
        public void StickySiteKpi_GetUIMarkups()
        {
            var kpi = GetUnitUnderTest();

            Assert.NotNull(kpi.UiMarkup);
        }


        [Fact]
        public void StickySiteKpi_AddSessionOnLoadedContent()
        {
            var kpi = GetUnitUnderTest();

            Thread.Sleep(1000);
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new FakeHttpContext("http://localhost:48594/alloy-plan/").Current);

            kpi.AddSessionOnLoadedContent(new object(), new ContentEventArgs(new BasicContent()));
        }

        [Fact]
        public void StickySiteKpi_Evaluate()
        {
            var kpi = GetUnitUnderTest();

            var fakeContextMock = new FakeHttpContext("http://localhost:48594/alloy-plan/");
            fakeContextMock.AddCookie("SSK_" + Guid.Empty, "path=");
            var t = fakeContextMock.Current;
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(t);

            var content3 = new Mock<IContent>();
            var arg = new ContentEventArgs(new ContentReference()) { Content = content3.Object };

            var retVal = kpi.Evaluate(new object(), arg);

            Assert.False(retVal.HasConverted, "Evaluate should have returned false");
        }

        [Fact]
        public void MAR_914_KPI_VerifyKpiUsesUniqueContextSkipAttribute()
        {
            var kpi = GetUnitUnderTest();

            var t = new FakeHttpContext("http://localhost:48594/alloy-plan/").Current;
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(t);

            kpi.TestContentGuid = _content100.ContentGuid;
            var arg = new ContentEventArgs(new ContentReference()) { Content = _content100 };

            kpi.AddSessionOnLoadedContent(new object(), arg);

            Assert.True(t.Items.ContainsKey("SSK_" + kpi.TestContentGuid.ToString()), "Skip Attribute not unique");
        }
    }
}

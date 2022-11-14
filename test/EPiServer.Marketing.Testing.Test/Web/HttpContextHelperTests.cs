using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class HttpContextHelperTests
    {
        private const string GoodData = "testing";
        public IServiceCollection Services { get; } = new ServiceCollection();
        Mock<IHttpContextAccessor> _mockIHttpContextAccessor = new Mock<IHttpContextAccessor>();

        public static IEnumerable<object[]> GetBadData()
        {
            yield return new object[] { "\rbadstuffs" };
            yield return new object[] { "%0dbadstuffs" };
            yield return new object[] { "\nbadstuffs" };
            yield return new object[] { "%0abadstuffs" };
            yield return new object[] { "\r\nbadstuffs" };
        }

        [Theory]
        [MemberData(nameof(GetBadData))]
        public void GetCookieValueSplitsOff_BadData(string badData)
        {
            var httpContextMock = new FakeHttpContext("http://localhost:48594/alloy-plan/");
            httpContextMock.AddCookie("key", GoodData + badData);
            _mockIHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContextMock.Current);
            Services.AddSingleton<IHttpContextAccessor>(_mockIHttpContextAccessor.Object);
            ServiceLocator.SetScopedServiceProvider(Services.BuildServiceProvider());

            var helper = new HttpContextHelper();
            var value = helper.GetCookieValue("key");

            Assert.Equal(GoodData, value);
        }

        [Fact]
        public void GetCookieValue_WithNoBadData()
        {
            var httpContextMock = new FakeHttpContext("http://localhost:48594/alloy-plan/");
            httpContextMock.AddCookie("key", GoodData);
            _mockIHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContextMock.Current);
            Services.AddSingleton<IHttpContextAccessor>(_mockIHttpContextAccessor.Object);
            ServiceLocator.SetScopedServiceProvider(Services.BuildServiceProvider());

            var helper = new HttpContextHelper();
            var value = helper.GetCookieValue("key");

            Assert.Equal(GoodData, value);
        }

    }
}

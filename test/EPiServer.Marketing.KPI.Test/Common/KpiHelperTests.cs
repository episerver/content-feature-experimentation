using EPiServer.Marketing.KPI.Common.Helpers;
using EPiServer.Marketing.KPI.Test.Fakes;
using EPiServer.ServiceLocation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.KPI.Test.Common
{
    public class KpiHelperTests
    {
        private Mock<IHttpContextAccessor> _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        public IServiceCollection Services { get; } = new ServiceCollection();

        public KpiHelperTests()
        {
            Services.AddTransient(s => _httpContextAccessorMock.Object);
            ServiceLocator.SetScopedServiceProvider(Services.BuildServiceProvider());
        }

        private IKpiHelper GetUnitUnderTest()
        {            
            return new KpiHelper();
        }        

        [Fact]
        public void GetRequestPath_ReturnsCurrentRequestedUrlPath()
        {
            var kpiHelper = GetUnitUnderTest();
            _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(new FakeHttpContext("http://localhost:48594/alloy-plan/").Current);
            Assert.True(kpiHelper.GetRequestPath() == "/alloy-plan/");
        }

        [Fact]
        public void GetRequestPath_ReturnsEmptyStringIfContextIsNull()
        {
            var kpiHelper = GetUnitUnderTest();
            Assert.True(kpiHelper.GetRequestPath() == string.Empty);            
        }
    }
}
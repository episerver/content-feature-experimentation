using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.Marketing.Testing.Web.Controllers;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using Xunit;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Manager.DataClass.Enums;
using EPiServer.Marketing.KPI.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Mvc;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestingControllerTests
    {
        private Mock<IServiceProvider> _mockServiceLocator;
        private Mock<IMarketingTestingWebRepository> _marketingTestingRepoMock;
        private Mock<IMessagingManager> _messagingManagerMock;
        private Mock<ITestDataCookieHelper> _testDataCookieHelperMock;
        private Mock<ITestManager> _testManagerMock;
        private Mock<IKpiWebRepository> _kpiWebRepoMock;
        Mock<IHttpContextHelper> contextHelper;

        private TestingController GetUnitUnderTest()
        {
            _mockServiceLocator = new Mock<IServiceProvider>();
            _marketingTestingRepoMock = new Mock<IMarketingTestingWebRepository>();
            _testManagerMock = new Mock<ITestManager>();
            _messagingManagerMock = new Mock<IMessagingManager>();
            contextHelper = new Mock<IHttpContextHelper>();
            _testDataCookieHelperMock = new Mock<ITestDataCookieHelper>();
            _kpiWebRepoMock = new Mock<IKpiWebRepository>();
            contextHelper = new Mock<IHttpContextHelper>();
            
            _mockServiceLocator.Setup(s1 => s1.GetService(typeof(ITestDataCookieHelper))).Returns(_testDataCookieHelperMock.Object);
            _mockServiceLocator.Setup(s1 => s1.GetService(typeof(IMarketingTestingWebRepository))).Returns(_marketingTestingRepoMock.Object);
            _mockServiceLocator.Setup(s1 => s1.GetService(typeof(IMessagingManager))).Returns(_messagingManagerMock.Object);
            _mockServiceLocator.Setup(s1 => s1.GetService(typeof(ITestDataCookieHelper))).Returns(_testDataCookieHelperMock.Object);
            _mockServiceLocator.Setup(s1 => s1.GetService(typeof(ITestManager))).Returns(_testManagerMock.Object);
            _mockServiceLocator.Setup(sl => sl.GetService(typeof(IKpiWebRepository))).Returns(_kpiWebRepoMock.Object);
            
            return new TestingController(contextHelper.Object, _mockServiceLocator.Object);            
        }

        [Fact]
        public void UpdateConversion_Uses_ConfigurationSpecified_SessionID_Name()
        {
            var pairs = new Dictionary<string, StringValues>();
            pairs.Add("testId", Guid.NewGuid().ToString());
            pairs.Add("itemVersion", "1");
            pairs.Add("kpiId", Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f").ToString());
            
            var data = new FormCollection(pairs);

            var controller = GetUnitUnderTest();
            var result = controller.UpdateConversion(data);

            contextHelper.Verify(m => m.GetSessionCookieName(), "Failed to use the method that gets the session cookie name from the config");
        }

        [Fact]
        public void UpdateConversion_Returns_BadRequest_If_TestID_Is_Null()
        {
            var pairs = new Dictionary<string, StringValues>();
            pairs.Add("itemVersion", "1");
            pairs.Add("kpiId", Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f").ToString());
            
            var data = new FormCollection(pairs);

            var controller = GetUnitUnderTest();
            var result = controller.UpdateConversion(data) as BadRequestObjectResult;
            Assert.True(result.StatusCode == (int)HttpStatusCode.BadRequest);
        }

        [Fact]
        public void UpdateConversion_Returns_OK_And_Calls_EmitUpdateConversion_With_Form_Data()
        {
            var TestGuid = Guid.NewGuid();
            var KpiGuid = Guid.NewGuid();
            
            var pairs = new Dictionary<string, StringValues>();
            pairs.Add("testId", TestGuid.ToString());
            pairs.Add("itemVersion", "1");
            pairs.Add("kpiId", KpiGuid.ToString());

            var data = new FormCollection(pairs);

            var controller = GetUnitUnderTest();
            var result = controller.UpdateConversion(data) as OkObjectResult;

            Assert.True(result.StatusCode == (int)HttpStatusCode.OK);
            _messagingManagerMock.Verify(m => m.EmitUpdateConversion(It.Is<Guid>(g => g.Equals(TestGuid)), It.Is<int>(v => v == 1), It.Is<Guid>(g => g.Equals(KpiGuid)), It.IsAny<string>()), "Did not emit message with proper arguments");
        }


        [Fact]
        public void GetAllTests_Returns_OK_Status_Result()
        {
            var controller = GetUnitUnderTest();

            var result = controller.GetAllTests() as OkObjectResult;

            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void GetTest_Returns_Test()
        {
            var controller = GetUnitUnderTest();

            var result = controller.GetTest(Guid.NewGuid().ToString()) as OkObjectResult;

            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void GetTest_Returns_Not_Found()
        {
            var id = Guid.NewGuid();
            ABTest test = null;

            var controller = GetUnitUnderTest();
            _marketingTestingRepoMock.Setup(call => call.GetTestById(It.Is<Guid>(g => g == id), It.IsAny<bool>())).Returns(test);

            var result = controller.GetTest(id.ToString()) as OkObjectResult;
            var response = result.Value as string;

            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
            Assert.Contains(id.ToString(), response);
        }

        [Fact]
        public void SaveKpiResult_Financial_Returns_OK_Request()
        {
            var pairs = new Dictionary<string, StringValues>();
            pairs.Add("testId", Guid.NewGuid().ToString());
            pairs.Add("itemVersion", "1");
            pairs.Add("kpiId", Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f").ToString());
            pairs.Add("total", "3");

            var data = new FormCollection(pairs);

            var cookie = new TestDataCookie();
            cookie.KpiConversionDictionary.Add(Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f"), false);

            var controller = GetUnitUnderTest();
            _testDataCookieHelperMock.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(cookie);
            _marketingTestingRepoMock.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ABTest());

            var result = controller.SaveKpiResult(data) as OkObjectResult;

            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void SaveKpiResult_Returns_OK_Request()
        {
            var pairs = new Dictionary<string, StringValues>();
            pairs.Add("testId", Guid.NewGuid().ToString());
            pairs.Add("itemVersion", "1");
            pairs.Add("kpiId", Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f").ToString());
            pairs.Add("total", "3");

            var data = new FormCollection(pairs);

            var cookie = new TestDataCookie();
            cookie.KpiConversionDictionary.Add(Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f"), false);

            var controller = GetUnitUnderTest();
            _testDataCookieHelperMock.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(cookie);
            _marketingTestingRepoMock.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ABTest());

            var result = controller.SaveKpiResult(data) as OkObjectResult;

            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void SaveKpiResult_Returns_Bad_Request()
        {
            var pairs = new Dictionary<string, StringValues>();
            pairs.Add("testId", "");
            pairs.Add("itemVersion", "1");
            pairs.Add("keyResultType", "1");
            pairs.Add("kpiId", Guid.NewGuid().ToString());
            pairs.Add("total", "3");

            var data = new FormCollection(pairs);

            var controller = GetUnitUnderTest();
            _kpiWebRepoMock.Setup(call => call.GetKpiInstance(It.IsAny<Guid>())).Returns(new Kpi());

            var result = controller.SaveKpiResult(data) as BadRequestObjectResult;

            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public void SaveKpiResult_handles_full_range_of_itemversions()
        {
            // item versions can go up to int 32 ranges
            var pairs = new Dictionary<string, StringValues>();
            pairs.Add("testId", Guid.NewGuid().ToString());
            pairs.Add("itemVersion", "1695874");
            pairs.Add("kpiId", Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f").ToString());
            pairs.Add("total", "3");

            var data = new FormCollection(pairs);

            var cookie = new TestDataCookie();
            cookie.KpiConversionDictionary.Add(Guid.Parse("bb53bed8-978a-456d-9fd7-6a2bea1bf66f"), false);

            var controller = GetUnitUnderTest();
            _testDataCookieHelperMock.Setup(call => call.GetTestDataFromCookie(It.IsAny<string>(), It.IsAny<string>())).Returns(cookie);
            _marketingTestingRepoMock.Setup(call => call.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(new ABTest());

            var result = controller.SaveKpiResult(data) as OkObjectResult;

            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void UpdateView_Returns_OK_Request()
        {
            var pairs = new Dictionary<string, StringValues>();
            pairs.Add("testId", Guid.NewGuid().ToString());
            pairs.Add("itemVersion", "1");

            var data = new FormCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.UpdateView(data) as OkResult;

            Assert.Equal((int)HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public void UpdateView_Returns_Bad_Request()
        {
            var pairs = new Dictionary<string, StringValues>();
            pairs.Add("testId", "");
            pairs.Add("itemVersion", "1");

            var data = new FormCollection(pairs);

            var controller = GetUnitUnderTest();

            var result = controller.UpdateView(data) as BadRequestObjectResult;

            Assert.Equal((int)HttpStatusCode.BadRequest, result.StatusCode);
        }
    }

    class testFinancialKpi : IKpi
    {
        public DateTime CreatedDate { get; set; }

        public string Description { get; }

        public string FriendlyName { get; set; }

        public Guid Id { get; set; }

        public virtual string KpiResultType
        {
            get
            {
                return typeof(KpiFinancialResult).Name.ToString();
            }
        }

        public DateTime ModifiedDate { get; set; }

        public ResultComparison ResultComparison { get; set; }

        public string UiMarkup { get; set; }

        public string UiReadOnlyMarkup { get; set; }

        ResultComparison IKpi.ResultComparison
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler EvaluateProxyEvent;

        public IKpiResult Evaluate(object sender, EventArgs e) { return null; }

        public void Initialize() { }

        public void Uninitialize() { }

        public void Validate(Dictionary<string, string> kpiData) { }

        IKpiResult IKpi.Evaluate(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}

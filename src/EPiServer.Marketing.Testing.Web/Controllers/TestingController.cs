using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Newtonsoft.Json;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace EPiServer.Marketing.Testing.Web.Controllers
{
    /// <summary>
    /// Provides a web interface for retrieving a single test, retrieving all tests, and 
    /// updating views and conversions. Note this is provided as a rest end point
    /// for customers to use via jscript on thier site.
    /// </summary>
    [Route("api/episerver/[controller]/[action]/")]
    public class TestingController : Controller
    {
        private IServiceProvider _serviceLocator;
        private IHttpContextHelper _httpContextHelper;
        private IMarketingTestingWebRepository _webRepo;
        private IKpiWebRepository _kpiWebRepo;

        [ExcludeFromCodeCoverage]
        public TestingController(IHttpContextHelper contexthelper, IServiceProvider locator)
        {
            _serviceLocator = locator;
            _httpContextHelper = contexthelper;
        }

        /// <summary>
        /// Retreives all A/B tests.
        /// Get api/episerver/testing/GetAllTests
        /// </summary>
        /// <returns>List of tests.</returns>
        [HttpGet]
        [AppSettingsAuthorize(Roles="CmsAdmins, CmsEditors")]
        public IActionResult GetAllTests()
        {
            var tm = _serviceLocator.GetInstance<IMarketingTestingWebRepository>();

            return Ok(JsonConvert.SerializeObject(tm.GetTestList(new TestCriteria()), Formatting.Indented,
                new JsonSerializerSettings
                {
                    // Apparently there is some loop referenceing problem with the 
                    // KeyPerformace indicators, this gets rid of that issue so we can actually convert the 
                    // data to json
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
        }

        /// <summary>
        /// Retrieves a test based given an ID.
        /// Get api/episerver/testing/GetTest?id=2a74262e-ec1c-4aaf-bef9-0654721239d6
        /// </summary>
        /// <param name="id">ID of a test.</param>
        /// <returns>A test.</returns>
        [HttpGet]
        [AppSettingsAuthorize(Roles="CmsAdmins, CmsEditors")]
        public IActionResult GetTest(string id)
        {
            var tm = _serviceLocator.GetInstance<IMarketingTestingWebRepository>();

            var testId = Guid.Parse(id);
            var test = tm.GetTestById(testId);
            if (test != null)
            {
                return Ok(JsonConvert.SerializeObject(test, Formatting.Indented,
                new JsonSerializerSettings
                {
                    // Apparently there is some loop referenceing problem with the 
                    // KeyPerformace indicators, this gets rid of that issue so we can actually convert the 
                    // data to json
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));
            }
            else
            {
                return Ok("Test " + id + " not found");
            }
        }

        /// <summary>
        /// Updates the view count for a given variant.
        /// Post url: api/episerver/testing/updateview, 
        /// data: { testId: testId, itemVersion: itemVersion },  
        /// contentType: 'application/x-www-form-urlencoded'
        /// </summary>
        /// <param name="data">{ testId: testId, itemVersion: itemVersion }</param>
        /// <returns>HttpStatusCode.OK or HttpStatusCode.BadRequest</returns>
        [HttpPost]
        public IActionResult UpdateView(IFormCollection data)
        {
            var testId = data["testId"];
            var itemVersion = data["itemVersion"];
            if (!string.IsNullOrWhiteSpace(testId))
            {
                var mm = _serviceLocator.GetInstance<IMessagingManager>();
                mm.EmitUpdateViews(Guid.Parse(testId), int.Parse(itemVersion));

                return Ok();
            }
            else
                return BadRequest(new Exception("TestId and VariantId are not available in the collection of parameters"));
        }

        /// <summary>
        /// Updates the conversion count for a given variant and KPI.
        /// Post url: api/episerver/testing/updateconversion, data: { testId: testId, itemVersion: itemVersion, kpiId: kpiId },  contentType: 'application/x-www-form-urlencoded'
        /// </summary>
        /// <param name="data">{ testId: testId, itemVersion: itemVersion, kpiId: kpiId }</param>
        /// <returns>HttpStatusCode.OK or HttpStatusCode.BadRequest</returns>
        [HttpPost]
        public IActionResult UpdateConversion(IFormCollection data)
        {
            var testId = data["testId"];
            var itemVersion = data["itemVersion"];
            var kpiId = data["kpiId"];

            if (!string.IsNullOrWhiteSpace(testId))
            {
                var mm = _serviceLocator.GetInstance<IMessagingManager>();
                var sessionid = _httpContextHelper.GetRequestParam(_httpContextHelper.GetSessionCookieName());

                mm.EmitUpdateConversion(Guid.Parse(testId), int.Parse(itemVersion), Guid.Parse(kpiId), sessionid);

                return Ok("Conversion Successful");
            }
            else
                return BadRequest(new Exception("TestId and VariantId are not available in the collection of parameters"));
        }

        /// <summary>
        /// Saves a KPI result for a given KPI and variant.
        /// Post url: api/episerver/testing/savekpiresult, data: { testId: testId, itemVersion: itemVersion, kpiId: kpiId, keyResultType: keyResultType, total: total },  contentType: 'application/x-www-form-urlencoded'
        /// </summary>
        /// <param name="data">{ testId: testId, itemVersion: itemVersion, kpiId: kpiId, keyResultType: keyResultType, total: total }</param>
        /// <returns>HttpStatusCode.OK or HttpStatusCode.BadRequest</returns>
        [HttpPost]
        public IActionResult SaveKpiResult(IFormCollection data)
        {
            _kpiWebRepo = _serviceLocator.GetInstance<IKpiWebRepository>();
            _webRepo = _serviceLocator.GetInstance<IMarketingTestingWebRepository>();
            IActionResult responseMessage = new OkResult();

            var testId = data["testId"];
            var itemVersion = data["itemVersion"];
            var kpiId = data["kpiId"];
            var value = data["resultValue"];
            try
            {
                var activeTest = _webRepo.GetTestById(Guid.Parse(data["testId"]), true);
                var kpi = _kpiWebRepo.GetKpiInstance(Guid.Parse(kpiId));   
                var cookieHelper = _serviceLocator.GetInstance<ITestDataCookieHelper>();             
                var testCookie = cookieHelper.GetTestDataFromCookie(activeTest.OriginalItemId.ToString(), activeTest.ContentLanguage);

                if (testCookie.KpiConversionDictionary[Guid.Parse(kpiId)] == false || testCookie.AlwaysEval) // MAR-903 - if we already converted dont convert again.
                {
                    IKeyResult keyResult;
                    KeyResultType resultType;
                    if (data["resultValue"].Count > 0)
                    {
                        if (kpi.KpiResultType == "KpiFinancialResult")
                        {
                            resultType = KeyResultType.Financial;
                            decimal decimalValue;
                            bool isDecimal = decimal.TryParse(value, out decimalValue);
                            if (isDecimal)
                            {
                                keyResult = new KeyFinancialResult()
                                {
                                    KpiId = Guid.Parse(kpiId),
                                    Total = Convert.ToDecimal(decimalValue)
                                };
                            }
                            else
                            {
                                throw new FormatException("Conversion Failed: Kpi Type requires a value of type 'Decimal'");
                            }
                        }
                        else
                        {
                            resultType = KeyResultType.Value;
                            double doubleValue;
                            bool isDouble = double.TryParse(value, out doubleValue);
                            if (isDouble)
                            {
                                keyResult = new KeyValueResult()
                                {
                                    KpiId = Guid.Parse(kpiId),
                                    Value = Convert.ToDouble(doubleValue)
                                };
                            }
                            else
                            {
                                throw new FormatException("Conversion Failed: Kpi Type requires a value of type 'Double'");
                            }
                        }
                        _webRepo.SaveKpiResultData(Guid.Parse(testId), int.Parse(itemVersion), keyResult, resultType);
                    }

                    if (!string.IsNullOrWhiteSpace(testId))
                    {
                        // update cookie dectioary so we can handle mulitple kpi conversions
                        testCookie.KpiConversionDictionary.Remove(Guid.Parse(kpiId));
                        testCookie.KpiConversionDictionary.Add(Guid.Parse(kpiId), true);

                        // update conversion for specific kpi
                        UpdateConversion(data);

                        // only update cookie if all kpi's have converted
                        testCookie.Converted = testCookie.KpiConversionDictionary.All(x => x.Value);
                        cookieHelper.UpdateTestDataCookie(testCookie);
                        responseMessage = Ok("Conversion Successful");
                    }
                    else
                    {
                        responseMessage = BadRequest(new Exception("TestId and item version are not available in the collection of parameters"));
                    }
                }

            }
            catch (Exception ex)
            {
                responseMessage = BadRequest(ex.Message);
            }
            return responseMessage;
        }
    }
}

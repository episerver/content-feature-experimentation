using EPiServer.Marketing.KPI.Common.Attributes;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web;
using EPiServer.Marketing.KPI;
using Microsoft.AspNetCore.Http;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    [ServiceConfiguration(ServiceType = typeof(ITestDataCookieHelper), Lifecycle = ServiceInstanceScope.Singleton)]
    public class TestDataCookieHelper : ITestDataCookieHelper
    {
        private IMarketingTestingWebRepository _testRepo;
        private IHttpContextHelper _httpContextHelper;
        private IEpiserverHelper _episerverHelper;
        private IAdminConfigTestSettingsHelper _adminConfigTestSettingsHelper;

        public readonly string COOKIE_PREFIX = "EPI-MAR-";

        [ExcludeFromCodeCoverage]
        public TestDataCookieHelper()
        {
            _adminConfigTestSettingsHelper = new AdminConfigTestSettingsHelper();
            _testRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
            _httpContextHelper = new HttpContextHelper();
        }

        /// <summary>
        /// unit tests should use this contructor and add needed services to the service locator as needed
        /// </summary>
        public TestDataCookieHelper(IAdminConfigTestSettingsHelper adminConfigTestSettingsHelper, IMarketingTestingWebRepository testRepo, IHttpContextHelper contextHelper, IEpiserverHelper epiHelper)
        {
            _adminConfigTestSettingsHelper = adminConfigTestSettingsHelper;
            _testRepo = testRepo;
            _httpContextHelper = contextHelper;
            _episerverHelper = epiHelper;
        }

        /// <summary>
        /// Evaluates the supplied testdata cookie to determine if it is populated with valid test information
        /// </summary>
        /// <param name="testDataCookie"></param>
        /// <returns></returns>
        public bool HasTestData(TestDataCookie testDataCookie)
        {
            return testDataCookie.TestId != Guid.Empty;
        }

        /// <summary>
        /// Evaluates the supplied testdata cookie to determine if the user has been set as a participant.
        /// </summary>
        /// <param name="testDataCookie"></param>
        /// <returns></returns>
        public bool IsTestParticipant(TestDataCookie testDataCookie)
        {
            return testDataCookie.TestVariantId != Guid.Empty;
        }

        /// <summary>
        /// Saves the supplied test data as a cookie
        /// </summary>
        /// <param name="testData"></param>
        public void SaveTestDataToCookie(TestDataCookie testData)
        {
            var aTest = _testRepo.GetTestById(testData.TestId, true);
            int varIndex = -1;

            if (testData.TestVariantId != Guid.Empty)
            {
                varIndex = aTest.Variants.FindIndex(i => i.Id == testData.TestVariantId);
            }

            var cookieData = new Dictionary<string, string>()
            {
                {"start", aTest.StartDate.ToString(CultureInfo.InvariantCulture)},
                {"vId", varIndex.ToString()},
                {"viewed", testData.Viewed.ToString()},
                {"converted", testData.Converted.ToString()}
            };
            var option = new CookieOptions()
            {
                Expires = aTest.EndDate,
                HttpOnly = true
            };
            var cookieName = COOKIE_PREFIX + testData.TestContentId.ToString() + _adminConfigTestSettingsHelper.GetCookieDelimeter() + aTest.ContentLanguage;
            
            testData.KpiConversionDictionary.OrderBy(x => x.Key);
            for (var x = 0; x < testData.KpiConversionDictionary.Count; x++)
            {
                cookieData.Add("k" + x, testData.KpiConversionDictionary.ToList()[x].Value.ToString());
            }

            _httpContextHelper.AddCookie(cookieName, cookieData.ToLegacyCookieString(), option);

            // Cookie added to Response.Cookies is not immediately available in the Request.Cookies
            // Add it to HttpContext.Items so it's available for the first visit.
            _httpContextHelper.SetItemValue(cookieName, cookieData.ToLegacyCookieString());
        }

        /// <summary>
        /// Updates the current cookie
        /// </summary>
        /// <param name="testData"></param>
        public void UpdateTestDataCookie(TestDataCookie testData)
        {
            _httpContextHelper.RemoveCookie(COOKIE_PREFIX + testData.TestContentId.ToString() + _adminConfigTestSettingsHelper.GetCookieDelimeter() + _episerverHelper.GetContentCultureinfo().Name);
            SaveTestDataToCookie(testData);
        }

        /// <summary>
        /// Gets the current cookie data
        /// </summary>
        /// <param name="testContentId"></param>
        /// <returns></returns>
        public TestDataCookie GetTestDataFromCookie(string testContentId, string cultureName = null)
        {
            var cookieDelimeter = _adminConfigTestSettingsHelper.GetCookieDelimeter();
            var retCookie = new TestDataCookie();
            var currentCulture = cultureName != null ? CultureInfo.GetCultureInfo(cultureName) : _episerverHelper.GetContentCultureinfo();
            var currentCulturename = cultureName != null ? cultureName : _episerverHelper.GetContentCultureinfo().Name;
            var cookieKey = COOKIE_PREFIX + testContentId + cookieDelimeter + currentCulturename;
            var cookie = _httpContextHelper.HasCookie(cookieKey)
                ? _httpContextHelper.GetResponseCookie(cookieKey)
                : _httpContextHelper.GetRequestCookie(cookieKey);

            if (!string.IsNullOrEmpty(cookie))
            {
                var cookieData = cookie.FromLegacyCookieString();
                if (cookieData.ContainsKey("start"))
                {
                    Guid outguid;
                    int outint = 0;
                    retCookie.TestContentId = Guid.TryParse(cookieKey.Substring(COOKIE_PREFIX.Length).Split(cookieDelimeter[0])[0], out outguid) ? outguid : Guid.Empty;

                    bool outval;

                    retCookie.TestStart = DateTime.Parse(cookieData["start"]);
                    retCookie.Viewed = bool.TryParse(cookieData["viewed"], out outval) ? outval : false;
                    retCookie.Converted = bool.TryParse(cookieData["converted"], out outval) ? outval : false;

                    var test = _testRepo.GetActiveTestsByOriginalItemId(retCookie.TestContentId, currentCulture).FirstOrDefault();

                    if (test != null && retCookie.TestStart.ToString(CultureInfo.InvariantCulture) == test.StartDate.ToString(CultureInfo.InvariantCulture))
                    {
                        var index = int.TryParse(cookieData["vId"], out outint) ? outint : -1;
                        retCookie.TestVariantId = index != -1 ? test.Variants[outint].Id : Guid.Empty;
                        retCookie.ShowVariant = index != -1 ? !test.Variants[outint].IsPublished : false;
                        retCookie.TestId = test.Id;

                        var orderedKpiInstances = test.KpiInstances.OrderBy(x => x.Id).ToList();
                        test.KpiInstances = orderedKpiInstances;

                        for (var x = 0; x < test.KpiInstances.Count; x++)
                        {
                            bool converted = false;
                            bool.TryParse(cookieData["k" + x], out converted);
                            retCookie.KpiConversionDictionary.Add(test.KpiInstances[x].Id, converted);
                            retCookie.AlwaysEval = Attribute.IsDefined(test.KpiInstances[x].GetType(), typeof(AlwaysEvaluateAttribute));
                        }
                    }
                    else
                    {
                        ExpireTestDataCookie(retCookie);
                        retCookie = new TestDataCookie();
                    }
                }
            }

            return retCookie;
        }

        /// <summary>
        /// Sets the cookie associated with the supplied testData to expire
        /// </summary>
        /// <param name="testData"></param>
        public void ExpireTestDataCookie(TestDataCookie testData)
        {
            var cookieDelimeter = _adminConfigTestSettingsHelper.GetCookieDelimeter();
            var cultureName = _episerverHelper.GetContentCultureinfo().Name;
            var cookieKey = COOKIE_PREFIX + testData.TestContentId.ToString() + cookieDelimeter + cultureName;
            _httpContextHelper.RemoveCookie(cookieKey);
            var expiredCookie = COOKIE_PREFIX + testData.TestContentId + cookieDelimeter + cultureName;
            var option = new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(-1d),
                HttpOnly = true
            };
            _httpContextHelper.AddCookie(expiredCookie, "", option);
        }

        /// <summary>
        /// Resets the cookie associated with the supplied testData to an empty Test Data cookie.
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public TestDataCookie ResetTestDataCookie(TestDataCookie testData)
        {
            var cookieDelimeter = _adminConfigTestSettingsHelper.GetCookieDelimeter();
            var cultureName = _episerverHelper.GetContentCultureinfo().Name;
            var cookieKey = COOKIE_PREFIX + testData.TestContentId.ToString() + cookieDelimeter + cultureName;
            _httpContextHelper.RemoveCookie(cookieKey);
            var resetCookie = COOKIE_PREFIX + testData.TestContentId + cookieDelimeter + cultureName;
            var option = new CookieOptions()
            {
                HttpOnly = true
            };
            _httpContextHelper.AddCookie(resetCookie, "", option);
            return new TestDataCookie();
        }

        /// <summary>
        /// Gets test cookie data from both Response and Request.
        /// Fetching response cookies gets current cookie data for cookies actively being processed
        /// while fetching request cookies gets cookie data for cookies which have not been touched.
        /// This ensure a complete set of current cookie data and prevents missed views or duplicated conversions.
        /// </summary>
        /// <returns></returns>
        public IList<TestDataCookie> GetTestDataFromCookies()
        {
            var delimeter = _adminConfigTestSettingsHelper.GetCookieDelimeter()[0];

            //Get up to date cookies data for cookies which are actively being processed
            var aResponseCookieKeys = _httpContextHelper.GetRequestCookieKeys();
            List<TestDataCookie> tdcList = (from name in aResponseCookieKeys
                                            where name.Contains(COOKIE_PREFIX)
                                            select GetTestDataFromCookie(name.Split(delimeter)[0].Substring(COOKIE_PREFIX.Length))).ToList();

            //Get cookie data from cookies not recently updated.
            tdcList.AddRange(from name in _httpContextHelper.GetRequestCookieKeys()
                             where name.Contains(COOKIE_PREFIX) &&
                             !aResponseCookieKeys.Contains(name)
                             select GetTestDataFromCookie(name.Split(delimeter)[0].Substring(COOKIE_PREFIX.Length)));

            return tdcList;
        }
    }
}

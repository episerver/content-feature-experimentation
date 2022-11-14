﻿using EPiServer.Marketing.KPI;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Repositories;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class TestDataCookieHelperTest
    {
        private Mock<IMarketingTestingWebRepository> _testRepo;
        private Mock<IHttpContextHelper> _httpContextHelper;
        private Mock<IEpiserverHelper> _epiHelper;
        private Mock<IAdminConfigTestSettingsHelper> _settingsHelper;

        private Guid _activeTestId = Guid.Parse("a194bde9-af3c-40fa-9635-338d02f5dea4");
        private Guid _inactiveTestId = Guid.Parse("5e2f21e3-30f7-4dcf-89cd-b9d7ff8c7cd6");
        private Guid _testContentGuid = Guid.Parse("6d532659-006d-453b-b7f4-62a94b2f3a3c");
        private Guid _testVariantGuid = Guid.Parse("9226a971-605c-4fd7-8a5c-aa7873e1e818");
        private DateTime _testEndDateTime = DateTime.Parse("5/5/2050");
        private string _cookieDelimeter = "_";
        private IMarketingTest _activeTest;
        private IMarketingTest _inactiveTest;

        private TestDataCookieHelper GetUnitUnderTest()
        {
            _testRepo = new Mock<IMarketingTestingWebRepository>();

            _activeTest = new ABTest()
            {
                Id = _activeTestId,
                EndDate = _testEndDateTime,
                State = TestState.Active,
                Variants = new List<Variant>(),
                KpiInstances = new List<IKpi>()
            };

            _inactiveTest = new ABTest()
            {
                Id = _inactiveTestId,
                EndDate = _testEndDateTime,
                State = TestState.Archived,
                Variants = new List<Variant>(),
                KpiInstances = new List<IKpi>()
            };
            _settingsHelper = new Mock<IAdminConfigTestSettingsHelper>();
            _settingsHelper.Setup(call => call.GetCookieDelimeter()).Returns(_cookieDelimeter);

            _httpContextHelper = new Mock<IHttpContextHelper>();
            _epiHelper = new Mock<IEpiserverHelper>();

            return new TestDataCookieHelper(_settingsHelper.Object, _testRepo.Object, _httpContextHelper.Object, _epiHelper.Object);
        }

        [Fact]
        public void HasTestData_Returns_False_If_TestContentId_Is_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            var testData = new TestDataCookie { TestContentId = Guid.Empty };
            var hasTestData = testDataCookieHelper.HasTestData(testData);
            Assert.False(hasTestData);
        }

        [Fact]
        public void HasTestData_Returns_True_If_TestContentId_Is_Not_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            var testData = new TestDataCookie { TestId = Guid.NewGuid() };
            var hasTestData = testDataCookieHelper.HasTestData(testData);
            Assert.True(hasTestData);
        }

        [Fact]
        public void HasTestData_Returns_False_If_TestVariantId_Is_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            var testData = new TestDataCookie { TestVariantId = Guid.Empty };
            var isTestParticipant = testDataCookieHelper.IsTestParticipant(testData);
            Assert.False(isTestParticipant);
        }

        [Fact]
        public void HasTestData_Returns_True_If_TestVariantId_Is_Not_Empty()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            var testData = new TestDataCookie { TestVariantId = Guid.NewGuid() };
            var isTestParticipant = testDataCookieHelper.IsTestParticipant(testData);
            Assert.True(isTestParticipant);
        }

        [Fact]
        public void GetTestDataFromCookie_Returns_Correct_Values_For_Active_Test_From_Populated_Response_Cookie()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var startDate = DateTime.Now.AddDays(-2);
            var expireDate = DateTime.Now.AddDays(2);
            var variant1 = new Variant()
            {
                Id = Guid.Parse("dee37c30-973b-4e48-9b59-148a6a730ed9"),
                IsPublished = true
            };

            var variant2 = new Variant()
            {
                Id = Guid.Parse("a19221d7-b977-4f90-b256-2e6b3cfd8216"),
            };

            var testCookie = new Dictionary<string, string>()
            {
                {"start", startDate.ToString()},
                {"vId", "0"},
                {"viewed", "false"},
                {"converted", "false"},
                {"k0", true.ToString()}
            }.ToLegacyCookieString();

            var test = new ABTest()
            {
                Id = _activeTestId,
                StartDate = startDate,
                OriginalItemId = testContentId,
                Variants = new List<Variant>() { variant1, variant2 },
                KpiInstances = new List<IKpi>(),
                ContentLanguage = "en-US"

            };

            _httpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            _httpContextHelper.Setup(hch => hch.GetResponseCookie(It.IsAny<string>())).Returns(testCookie);
            _testRepo.Setup(tr => tr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(_activeTest);
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>());
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-US"));
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>() { test });

            var returnCookieData = mockTestDataCookiehelper.GetTestDataFromCookie(testContentId.ToString());
            Assert.True(returnCookieData.TestId == _activeTestId);
            Assert.True(returnCookieData.TestContentId == testContentId);
            Assert.False(returnCookieData.ShowVariant);
            Assert.True(returnCookieData.TestVariantId == variant1.Id);
            Assert.False(returnCookieData.Viewed);
            Assert.False(returnCookieData.Converted);
        }


        [Fact]
        public void GetTestDataFromCookie_Returns_Correct_Values_For_Active_Test_From_Populated_Response_Cookie_With_Culture()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var startDate = DateTime.Now.AddDays(-2);
            var expireDate = DateTime.Now.AddDays(2);
            var variant1 = new Variant()
            {
                Id = Guid.Parse("dee37c30-973b-4e48-9b59-148a6a730ed9"),
                IsPublished = true
            };

            var variant2 = new Variant()
            {
                Id = Guid.Parse("a19221d7-b977-4f90-b256-2e6b3cfd8216"),
            };

            var testCookie = new Dictionary<string, string>()
            {
                {"start", DateTime.Parse("28-09-2018 07:04:41", CultureInfo.GetCultureInfo("da-DK")).ToString(CultureInfo.InvariantCulture)},
                {"vId", "0"},
                {"viewed", "false"},
                {"converted", "false"},
                {"k0", true.ToString()}
            }.ToLegacyCookieString();

            var test = new ABTest()
            {
                Id = _activeTestId,
                StartDate = DateTime.Parse("28-09-2018 07:04:41", CultureInfo.GetCultureInfo("da-DK")),
                OriginalItemId = testContentId,
                Variants = new List<Variant>() { variant1, variant2 },
                KpiInstances = new List<IKpi>(),
                ContentLanguage = "da-DK"

            };

            _httpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            _httpContextHelper.Setup(hch => hch.GetResponseCookie(It.IsAny<string>())).Returns(testCookie);
            _testRepo.Setup(tr => tr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(_activeTest);
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>());
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("da-DK"));
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>() { test });

            var returnCookieData = mockTestDataCookiehelper.GetTestDataFromCookie(testContentId.ToString(), "da-DK");
            Assert.True(returnCookieData.TestId == _activeTestId);
            Assert.True(returnCookieData.TestContentId == testContentId);
            Assert.False(returnCookieData.ShowVariant);
            Assert.True(returnCookieData.TestVariantId == variant1.Id);
            Assert.False(returnCookieData.Viewed);
            Assert.False(returnCookieData.Converted);
        }

        [Fact]
        public void GetTestDataFromCookie_Returns_Correct_Values_For_Active_Test_From_Populated_Request_Cookies()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var startDate = DateTime.Now.AddDays(-2);
            var expireDate = DateTime.Now.AddDays(2);
            var variant1 = new Variant()
            {
                Id = Guid.Parse("dee37c30-973b-4e48-9b59-148a6a730ed9"),
                IsPublished = true
            };

            var variant2 = new Variant()
            {
                Id = Guid.Parse("a19221d7-b977-4f90-b256-2e6b3cfd8216"),
            };

            var testCookie = new Dictionary<string, string>()
            {
                {"start", startDate.ToString()},
                {"vId", "1"},
                {"viewed", "false"},
                {"converted", "false"},
                {"k0", true.ToString()}
            }.ToLegacyCookieString();

            var test = new ABTest()
            {
                Id = _activeTestId,
                StartDate = startDate,
                OriginalItemId = testContentId,
                Variants = new List<Variant>() { variant1, variant2 },
                KpiInstances = new List<IKpi>(),
                ContentLanguage = "en-US"

            };

            _httpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(false);
            _httpContextHelper.Setup(hch => hch.GetRequestCookie(It.IsAny<string>())).Returns(testCookie);
            _testRepo.Setup(tr => tr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(_activeTest);
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>());
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-US"));
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>() { test });

            var returnCookieData = mockTestDataCookiehelper.GetTestDataFromCookie(testContentId.ToString());
            Assert.True(returnCookieData.TestId == _activeTestId);
            Assert.True(returnCookieData.TestContentId == testContentId);
            Assert.True(returnCookieData.ShowVariant);
            Assert.True(returnCookieData.TestVariantId == variant2.Id);
            Assert.False(returnCookieData.Viewed);
            Assert.False(returnCookieData.Converted);
        }

        [Fact]
        public void GetTestDataFromCookie_adds_kpi_conversions_return_cookie_data()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var startDate = DateTime.Now.AddDays(-2);
            var expireDate = DateTime.Now.AddDays(2);

            var testVariantId = Guid.NewGuid();
            var kpiInstance = new Kpi()
            {
                Id = Guid.NewGuid()
            };
            var testCookie = new Dictionary<string, string>()
            {
                {"start", startDate.ToString()},
                {"vId", "-1"},
                {"viewed", "false"},
                {"converted", "false"},
                {"k0", true.ToString()}
            }.ToLegacyCookieString();

            var aTest = new ABTest()
            {
                OriginalItemId = testContentId,
                StartDate = startDate,
                KpiInstances = new List<IKpi>() { kpiInstance },
                ContentLanguage = "en-us",
                Variants = new List<Variant>() { new Variant() { Id = Guid.Parse("3352e74c-11c9-402f-86f9-7a7f1bfab756") } }
            };


            _httpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            _httpContextHelper.Setup(hch => hch.GetResponseCookie(It.IsAny<string>())).Returns(testCookie);
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-us"));
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>() { aTest });
            _testRepo.Setup(tr => tr.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>() { aTest });
            _testRepo.Setup(call => call.GetTestById(It.IsAny<Guid>(), true)).Returns(aTest);

            var returnCookieData = mockTestDataCookiehelper.GetTestDataFromCookie(testContentId.ToString());
            Assert.True(returnCookieData.KpiConversionDictionary.ContainsKey(kpiInstance.Id), "expected the kpi instance info to be added to the conversion dictionary");
            Assert.True(returnCookieData.KpiConversionDictionary[kpiInstance.Id], "kpi instance was not added with the expected conversion data");
        }

        [Fact]
        public void GetTestDataFromCookies_Returns_Correct_Values_For_Active_Test_From_Populated_Response_and_Request_Cookies()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var responseContentCookie1 = Guid.Parse("997636c1-fec2-43ff-8dd8-fa7f6b3ffa91");
            var requestContentCookie1 = Guid.Parse("997636c1-fec2-43ff-8dd8-fa7f6b3ffa91");
            var requestContentCookie2 = Guid.Parse("f222c926-999b-43f9-80dc-667f05e08260");
            var testContentId = Guid.NewGuid();
            var startDate = DateTime.Now.AddDays(-2);
            var expireDate = DateTime.Now.AddDays(2);
            var testVariantId = Guid.NewGuid();
            var kpiInstance = new Kpi()
            {
                Id = Guid.NewGuid()
            };

            var responseCookie1Name = mockTestDataCookiehelper.COOKIE_PREFIX + responseContentCookie1.ToString()
                + _cookieDelimeter + "en-US";
            var responseCookie1 = new Dictionary<string, string>()
            {
                {"start", startDate.ToString()},
                {"vId", "0"},
                {"viewed", "True"},
                {"converted", "True"},
                {"k0", true.ToString()}
            }.ToLegacyCookieString();

            var requestCookie1Name = mockTestDataCookiehelper.COOKIE_PREFIX + requestContentCookie1.ToString()
                + _cookieDelimeter + "en-US";
            var requestCookie1 = new Dictionary<string, string>()
            {
                {"start", startDate.ToString()},
                {"vId", "1"},
                {"viewed", "True"},
                {"converted", "False"},
                {"k0", true.ToString()}
            }.ToLegacyCookieString();

            var requestCookie2Name = mockTestDataCookiehelper.COOKIE_PREFIX + requestContentCookie2.ToString()
                + _cookieDelimeter + "en-US";
            var requestCookie2 = new Dictionary<string, string>()
            {
                {"start", startDate.ToString()},
                {"vId", "1"},
                {"viewed", "True"},
                {"converted", "False"},
                {"k0", true.ToString()}
            }.ToLegacyCookieString();

            var aTest = new ABTest()
            {
                OriginalItemId = testContentId,
                StartDate = startDate,
                KpiInstances = new List<IKpi>() { kpiInstance },
                ContentLanguage = "en-US",
                Variants = new List<Variant>()
                {
                    new Variant() {Id=Guid.NewGuid() },
                    new Variant() {Id=Guid.NewGuid() }
                }
            };

            _httpContextHelper.Setup(hch => hch.GetRequestCookieKeys()).Returns(new string[2] { requestCookie1Name, requestCookie2Name });

            _httpContextHelper.Setup(hch => hch.HasCookie(It.Is<string>(c => c == responseCookie1Name))).Returns(true);
            _httpContextHelper.Setup(hch => hch.HasCookie(It.Is<string>(c => c == requestCookie2Name))).Returns(false);
            _httpContextHelper.Setup(hch => hch.GetResponseCookie(It.Is<string>(c => c == responseCookie1Name))).Returns(responseCookie1);
            _httpContextHelper.Setup(hch => hch.GetRequestCookie(It.Is<string>(c => c == responseCookie1Name))).Returns(responseCookie1);
            _httpContextHelper.Setup(hch => hch.GetRequestCookie(It.Is<string>(c => c == requestCookie2Name))).Returns(requestCookie2);
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>());
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-US"));
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>() { aTest });
            _testRepo.Setup(call => call.GetTestById(It.IsAny<Guid>(), true)).Returns(aTest);

            var returnCookieData = mockTestDataCookiehelper.GetTestDataFromCookies();
            Assert.True(returnCookieData.Count == 2);
            Assert.True(returnCookieData[0].TestContentId == responseContentCookie1);
            Assert.True(returnCookieData[0].Converted);
            Assert.True(returnCookieData[1].TestContentId == requestContentCookie2);
            Assert.False(returnCookieData[1].Converted);
        }

        [Fact]
        public void GetTestDataFromCookie_Returns_EmptyData_When_CookieIsNotAvailable()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var invalidTestContentId = Guid.NewGuid();

            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));

            var returnCookieData = mockTestDataCookiehelper.GetTestDataFromCookie(mockTestDataCookiehelper.COOKIE_PREFIX + invalidTestContentId.ToString()
                + _cookieDelimeter + "en-GB");
            Assert.True(returnCookieData.TestId == Guid.Empty);
            Assert.True(returnCookieData.TestContentId == Guid.Empty);
            Assert.True(returnCookieData.ShowVariant == false);
            Assert.True(returnCookieData.TestVariantId == Guid.Empty);
            Assert.True(returnCookieData.Viewed == false);
            Assert.True(returnCookieData.Converted == false);
        }

        [Fact]
        public void SaveTestDataToCookie_creates_a_cookie_from_the_data_to_add()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var variant1Guid = Guid.Parse("dee37c30-973b-4e48-9b59-148a6a730ed9");
            var variant2Guid = Guid.Parse("a19221d7-b977-4f90-b256-2e6b3cfd8216");
            var startDate = DateTime.Now.AddDays(-2);
            var tdCookie = new TestDataCookie()
            {
                TestId = _activeTest.Id,
                TestContentId = _testContentGuid,
                TestVariantId = variant1Guid,
                ShowVariant = true,
                Viewed = true,
                Converted = false
            };

            _activeTest.Variants = new List<Variant>()
            {
                new Variant() {Id = variant1Guid },
                new Variant() {Id = variant2Guid }
            };
            _activeTest.StartDate = startDate;
            _activeTest.ContentLanguage = "en-GB";

            _testRepo.Setup(tr => tr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(_activeTest);
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));

            mockTestDataCookiehelper.SaveTestDataToCookie(tdCookie);

            _httpContextHelper.Verify(hch => hch.AddCookie(It.Is<string>(c => c == mockTestDataCookiehelper.COOKIE_PREFIX + tdCookie.TestContentId + _cookieDelimeter + "en-GB"),It.IsAny<string>(),It.IsAny<CookieOptions>()));
            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.Is<string>(c => c.FromLegacyCookieString()["start"] == startDate.ToString(CultureInfo.InvariantCulture)), It.IsAny<CookieOptions>()));
            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.Is<string>(c => int.Parse(c.FromLegacyCookieString()["vId"]) == 0), It.IsAny<CookieOptions>()));
            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.Is<string>(c => bool.Parse(c.FromLegacyCookieString()["viewed"]) == tdCookie.Viewed), It.IsAny<CookieOptions>()));
            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.Is<string>(c => bool.Parse(c.FromLegacyCookieString()["converted"]) == tdCookie.Converted), It.IsAny<CookieOptions>()));
        }

        [Fact]
        public void SaveTestDataToCookie_adds_kpi_conversion_info_to_the_cookie()
        {
            var cookieHelper = GetUnitUnderTest();
            var kpiId = Guid.NewGuid();
            var cookieKey = "k0";

            _testRepo.Setup(tr => tr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(_activeTest);
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));

            var tdCookie = new TestDataCookie()
            {
                TestId = _activeTest.Id,
                TestContentId = _testContentGuid,
                TestVariantId = _testVariantGuid,
                ShowVariant = true,
                Viewed = true,
                Converted = false
            };

            tdCookie.KpiConversionDictionary.Add(kpiId, true);
            cookieHelper.SaveTestDataToCookie(tdCookie);

            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.Is<string>(c => !string.IsNullOrEmpty(c.FromLegacyCookieString()[cookieKey])), It.IsAny<CookieOptions>()),
                Times.Once(), "expected kpi conversion info to be added to the cookie");
            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.Is<string>(c => bool.Parse(c.FromLegacyCookieString()[cookieKey])), It.IsAny<CookieOptions>()),
                "not the expected kpi conversion value");
        }

        [Fact]
        public void UpdateTestDataCookie_ProperlyUpdatesCookie()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var variant1Guid = Guid.Parse("dee37c30-973b-4e48-9b59-148a6a730ed9");
            var variant2Guid = Guid.Parse("a19221d7-b977-4f90-b256-2e6b3cfd8216");
            var startDate = DateTime.Now.AddDays(-2);

            var originalCookie = new TestDataCookie()
            {
                TestId = _activeTest.Id,
                TestContentId = _testContentGuid,
                TestVariantId = variant1Guid,
                ShowVariant = false,
                Viewed = true,
                Converted = false
            };

            var updatedCookie = new TestDataCookie()
            {
                TestId = _activeTest.Id,
                TestContentId = _testContentGuid,
                TestVariantId = variant2Guid,
                ShowVariant = true,
                Viewed = false,
                Converted = true
            };

            _activeTest.Variants = new List<Variant>() {
                new Variant() { Id = variant1Guid },
                new Variant() { Id = variant2Guid }
            };
            _activeTest.StartDate = startDate;
            _activeTest.ContentLanguage = "en-GB";
            _testRepo.Setup(tr => tr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(_activeTest);
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));

            mockTestDataCookiehelper.UpdateTestDataCookie(updatedCookie);
            var cookieKey = mockTestDataCookiehelper.COOKIE_PREFIX + originalCookie.TestContentId.ToString() + _cookieDelimeter + "en-GB";
            //Removed the old cookie
            _httpContextHelper.Verify(hch => hch.RemoveCookie(It.Is<string>(cid => cid == cookieKey)));
            _httpContextHelper.Verify(hch => hch.AddCookie(It.Is<string>(c => c == cookieKey), It.IsAny<string>(), It.IsAny<CookieOptions>()));
            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.Is<string>(c => c.FromLegacyCookieString()["start"] == startDate.ToString(CultureInfo.InvariantCulture)), It.IsAny<CookieOptions>()));
            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.Is<string>(c => int.Parse(c.FromLegacyCookieString()["vId"]) == 1), It.IsAny<CookieOptions>()));
            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.Is<string>(c => bool.Parse(c.FromLegacyCookieString()["viewed"]) == updatedCookie.Viewed), It.IsAny<CookieOptions>()));
            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.Is<string>(c => bool.Parse(c.FromLegacyCookieString()["converted"]) == updatedCookie.Converted), It.IsAny<CookieOptions>()));
        }

        [Fact]
        public void ExpireTestCookieData_Properly_Expires_A_Cookie()
        {
            var testDataCookieHelper = GetUnitUnderTest();
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));

            var tdCookie = new TestDataCookie()
            {
                TestId = _activeTest.Id,
                TestContentId = Guid.NewGuid()
            };

            testDataCookieHelper.ExpireTestDataCookie(tdCookie);
            var cookieKey = testDataCookieHelper.COOKIE_PREFIX + tdCookie.TestContentId.ToString() + _cookieDelimeter + "en-GB";
            _httpContextHelper.Verify(hch => hch.RemoveCookie(It.Is<string>(ck => ck == cookieKey)), Times.Once, "passed in cookie was not removed");
            _httpContextHelper.Verify(hch => hch.AddCookie(It.IsAny<string>(), It.IsAny<string>(), It.Is<CookieOptions>(c => c.Expires <= DateTime.Now)), Times.Once, "cookie was not added with a expiry less than now");
        }

        [Fact]
        public void GetTestDataFromCookie_MAR_896_NoExceptionForMissingEntries()
        {
            var cookieHelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var startDate = DateTime.Now.AddDays(-2);
            var expireDate = DateTime.Now.AddDays(2);

            var testVariantId = Guid.NewGuid();

            var testCookie = new Dictionary<string, string>()
            {// purposely empty properties...
                {"start", startDate.ToString()},
                {"vId", "-1"},
                {"viewed", "false"},
                {"converted", "false"},
                {"k0", true.ToString()}
            }.ToLegacyCookieString();

            _httpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            _httpContextHelper.Setup(hch => hch.GetResponseCookie(It.IsAny<string>())).Returns(testCookie);
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>());
            _testRepo.Setup(tr => tr.GetTestList(It.IsAny<TestCriteria>())).Returns(new List<IMarketingTest>());
            var returnCookieData = cookieHelper.GetTestDataFromCookie(testContentId.ToString());

            Assert.True(returnCookieData.TestId == Guid.Empty);
            Assert.True(returnCookieData.TestContentId == Guid.Empty);
            Assert.True(returnCookieData.TestVariantId == Guid.Empty);
            Assert.False(returnCookieData.ShowVariant);
            Assert.False(returnCookieData.Viewed);
            Assert.False(returnCookieData.Converted);
            Assert.False(returnCookieData.AlwaysEval);
        }

        [Fact]
        public void ResetTestDataCookie_replaces_the_cookie_associated_with_the_passed_in_test()
        {
            var cookieHelper = GetUnitUnderTest();
            var aCookieData = new TestDataCookie()
            {
                TestContentId = Guid.NewGuid()
            };

            var aCookieName = cookieHelper.COOKIE_PREFIX + aCookieData.TestContentId.ToString() + _cookieDelimeter + "en-GB";
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));
            var result = cookieHelper.ResetTestDataCookie(aCookieData);

            _httpContextHelper.Verify(hch => hch.RemoveCookie(It.Is<string>(c => c == aCookieName)), Times.Once(), "did not remove the old cookie");
            _httpContextHelper.Verify(hch => hch.AddCookie(It.Is<string>(c => c == aCookieName),It.IsAny<string>(), It.IsAny<CookieOptions>()), Times.Once(), "did not add the reset cookie");
            Assert.True(result.TestId == Guid.Empty);
        }

        [Fact]
        public void TestDataCookieHelper_expires_cookie_if_no_active_test_exists()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var startDate = DateTime.Now.AddDays(-2);
            var expireDate = DateTime.Now.AddDays(2);
            var variant1 = new Variant()
            {
                Id = Guid.Parse("dee37c30-973b-4e48-9b59-148a6a730ed9"),
                IsPublished = true
            };

            var variant2 = new Variant()
            {
                Id = Guid.Parse("a19221d7-b977-4f90-b256-2e6b3cfd8216"),
            };

            var testCookie = new Dictionary<string, string>()
            {
                {"start", startDate.ToString()},
                {"vId", "0"},
                {"viewed", "false"},
                {"converted", "false"},
                {"k0", true.ToString()}
            }.ToLegacyCookieString();

            _httpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            _httpContextHelper.Setup(hch => hch.GetResponseCookie(It.IsAny<string>())).Returns(testCookie);
            _testRepo.Setup(tr => tr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns((IMarketingTest)null);
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>());
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));

            var returnCookieData = mockTestDataCookiehelper.GetTestDataFromCookie(testContentId.ToString());

            Assert.True(returnCookieData.TestId == Guid.Empty);
            Assert.True(returnCookieData.TestContentId == Guid.Empty);
            Assert.False(returnCookieData.ShowVariant);
            Assert.True(returnCookieData.TestVariantId == Guid.Empty);
            Assert.False(returnCookieData.Viewed);
            Assert.False(returnCookieData.Converted);
        }

        [Fact]
        public void TestDataCookieHelper_Delivers_Empty_Guid_When_Variant_Index_Not_Set()
        {
            var mockTestDataCookiehelper = GetUnitUnderTest();
            var testContentId = Guid.NewGuid();
            var startDate = DateTime.Now.AddDays(-2);
            var expireDate = DateTime.Now.AddDays(2);
            var variant1 = new Variant()
            {
                Id = Guid.Parse("dee37c30-973b-4e48-9b59-148a6a730ed9"),
                IsPublished = true
            };

            var variant2 = new Variant()
            {
                Id = Guid.Parse("a19221d7-b977-4f90-b256-2e6b3cfd8216"),
            };

            var testCookie = new Dictionary<string, string>()
            {
                {"start", startDate.ToString()},
                {"vId", "-1"},
                {"viewed", "false"},
                {"converted", "false"},
                {"k0", true.ToString()}
            }.ToLegacyCookieString();
            
            var test = new ABTest()
            {
                Id = _activeTestId,
                StartDate = startDate,
                OriginalItemId = testContentId,
                Variants = new List<Variant>() { variant1, variant2 },
                KpiInstances = new List<IKpi>(),
                ContentLanguage = "en-US"
            };

            _httpContextHelper.Setup(hch => hch.HasCookie(It.IsAny<string>())).Returns(true);
            _httpContextHelper.Setup(hch => hch.GetResponseCookie(It.IsAny<string>())).Returns(testCookie);
            _testRepo.Setup(tr => tr.GetTestById(It.IsAny<Guid>(), It.IsAny<bool>())).Returns(_activeTest);
            _epiHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-US"));
            _testRepo.Setup(tr => tr.GetActiveTestsByOriginalItemId(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(new List<IMarketingTest>() { test });

            var returnCookieData = mockTestDataCookiehelper.GetTestDataFromCookie(testContentId.ToString());

            Assert.False(returnCookieData.ShowVariant);
            Assert.Equal(Guid.Empty, returnCookieData.TestVariantId);

        }
    }
}

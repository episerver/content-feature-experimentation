﻿using System;
using System.Collections.Generic;
using EPiServer.ServiceLocation;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Security;
using EPiServer.Core;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.KPI.Results;
using Newtonsoft.Json;
using EPiServer.Framework.Cache;
using EPiServer.Marketing.Testing.Web.Config;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using EPiServer.Marketing.Testing.Core;
using Microsoft.Extensions.Options;
using EPiServer.Marketing.Testing.Web.FullStackSDK;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.RestAPI;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    [ServiceConfiguration(ServiceType = typeof(IMarketingTestingWebRepository), Lifecycle = ServiceInstanceScope.Singleton) ]
    public class MarketingTestingWebRepository : IMarketingTestingWebRepository
    {
        private IServiceProvider _serviceLocator;
        private ITestResultHelper _testResultHelper;
        private ITestManager _testManager;
        private ILogger _logger;
        private IKpiManager _kpiManager;
        private IHttpContextHelper _httpContextHelper;
        private ICacheSignal _cacheSignal;
        private Injected<IExperimentationClient> _experimentationClient;
        private Injected<IFullstackSDKClient> _fsSDKClient;
        /// <summary>
        /// Default constructor
        /// </summary>
        [ExcludeFromCodeCoverage]
        public MarketingTestingWebRepository()
        {
            int.TryParse(ServiceLocator.Current.GetInstance<IOptions<TestingOption>>().Value.TestMonitorSeconds, out int testMonitorValue);

            _serviceLocator = ServiceLocator.Current;
            _testResultHelper = _serviceLocator.GetInstance<ITestResultHelper>();
            _testManager = _serviceLocator.GetInstance<ITestManager>();
            _kpiManager = _serviceLocator.GetInstance<IKpiManager>();
            _httpContextHelper = new HttpContextHelper();
            //_experimentationClient = _serviceLocator.GetInstance<IExperimentationClient>();
            _logger = LogManager.GetLogger();
            _cacheSignal = new RemoteCacheSignal(
                            ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>(),
                            LogManager.GetLogger(),
                            "epi/marketing/testing/webrepocache",
                            TimeSpan.FromSeconds(testMonitorValue > 15 ? testMonitorValue  : 15)
                        );

            _cacheSignal.Monitor(Refresh);
        }

        /// <summary>
        /// For unit testing
        /// </summary>
        /// <param name="locator"></param>
        public MarketingTestingWebRepository(IServiceProvider locator, ILogger logger)
        {
            _serviceLocator = locator;
            _testResultHelper = locator.GetInstance<ITestResultHelper>();
            _testManager = locator.GetInstance<ITestManager>();
            _kpiManager = locator.GetInstance<IKpiManager>();
            _httpContextHelper = locator.GetInstance<IHttpContextHelper>();
            _cacheSignal = locator.GetInstance<ICacheSignal>();

            _cacheSignal.Monitor(Refresh);

            _logger = logger;
        }

        /// <summary>
        /// Refreshes the cache and sets the cache signal for this machine.
        /// </summary>
        /// <remarks>
        /// On content editing machines this method gets called when a config is saved or the cache is empty.
        /// On content delivery machines this method gets called when the content editing machine
        ///     modifies the state of a test Or the state of the config.
        /// </remarks>
        public void Refresh()
        {
            var _testHandler = _serviceLocator.GetInstance<ITestHandler>();
            var testCriteria = new TestCriteria();
            testCriteria.AddFilter(
                new ABTestFilter
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And,
                    Value = TestState.Active
                }
            );

            AdminConfigTestSettings.Reset();

            if (AdminConfigTestSettings.Current.IsEnabled)
            {
                var dbTests = _testManager.GetTestList(testCriteria);
                _logger.Debug("Refresh - count = " + dbTests.Count);

                if (dbTests.Count == 0)
                {
                    _logger.Debug("Refresh - AB Testing disabled, there are no active tests.");
                    _testHandler.DisableABTesting();
                }
                else
                {
                    _logger.Debug("Refresh - AB Testing enabled with active tests.");
                    _testHandler.EnableABTesting();
                    ((CachingTestManager)_testManager).RefreshCache();
                }
            }
            else
            {
                _logger.Debug("Refresh - AB Testing disabled through configuration.");
                _testHandler.DisableABTesting();
            }

            _cacheSignal.Set();
        }

        /// <summary>
        /// Gets the test associated with the content guid specified. If no tests are found an empty test is returned
        /// </summary>
        /// <param name="aContentGuid">the content guid to search against</param>
        /// <returns>the first marketing test found that is not archived or an empty test in the case of no results</returns>
        public IMarketingTest GetActiveTestForContent(Guid aContentGuid)
        {
            var aTest = _testManager.GetTestByItemId(aContentGuid).Find(abTest => abTest.State != TestState.Archived);

            if (aTest == null)
            {
                aTest = new ABTest();
            }
            else
            {
                var sortedVariants = aTest.Variants.OrderByDescending(p => p.IsPublished).ThenBy(v => v.Id).ToList();
                aTest.Variants = sortedVariants;
            }  
            return aTest;
        }

        public IMarketingTest GetActiveTestForContent(Guid aContentGuid, CultureInfo contentCulture)
        {
            var aTest = _testManager.GetTestByItemId(aContentGuid).Find(abTest => abTest.State != TestState.Archived && (abTest.ContentLanguage == contentCulture.Name || abTest.ContentLanguage == string.Empty));

            if (aTest == null)
            {
                aTest = new ABTest();
            }
            else if (aTest.ContentLanguage == string.Empty)
            {
                aTest.ContentLanguage = contentCulture.Name;
                _testManager.Save(aTest);
            }

            return aTest;
        }

        public List<IMarketingTest> GetActiveTests()
        {
            return _testManager.GetActiveTests();
        }

        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            var tests = _testManager.GetActiveTestsByOriginalItemId(originalItemId);
            for(var x = 0; x < tests.Count; x++){
                var sortedVariants = tests[x].Variants.OrderByDescending(p => p.IsPublished).ThenBy(v => v.Id).ToList();
                tests[x].Variants = sortedVariants;
            };            
            return tests;
        }

        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId, CultureInfo contentCulture)
        {
            return _testManager.GetActiveTestsByOriginalItemId(originalItemId, contentCulture);
        }

        public IMarketingTest GetTestById(Guid testGuid, bool fromCache = false)
        {
            var aTest = _testManager.Get(testGuid,fromCache);
            if(aTest != null)
            {
                var sortedVariants = aTest.Variants.OrderByDescending(p => p.IsPublished).ThenBy(v => v.Id).ToList();
                aTest.Variants = sortedVariants;
            }
            return aTest;
        }

        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            var tests = _testManager.GetTestList(criteria);
            for (var x = 0; x < tests.Count; x++)
            {
                var sortedVariants = tests[x].Variants.OrderBy(v => v.Id).ToList();
                var sortedVariants2 = sortedVariants.OrderByDescending(p => p.IsPublished.ToString()).ToList();
                tests[x].Variants = sortedVariants2;
            };
            return tests;
        }

        public List<IMarketingTest> GetTestList(TestCriteria criteria, CultureInfo contentCulture)
        {
            var testList = _testManager.GetTestList(criteria);
            return testList.Where(x => x.ContentLanguage == contentCulture.Name).ToList();
        }

        public void DeleteTestForContent(Guid aContentGuid)
        {
            var testList = _testManager.GetTestByItemId(aContentGuid).FindAll(abtest => abtest.State != TestState.Archived);

            foreach (var test in testList)
            {
                _testManager.Delete(test.Id);
            }

            ConfigureABTestingUsingActiveTestsCount();
        }

        internal void ConfigureABTestingUsingActiveTestsCount()
        {
            var _testHandler = _serviceLocator.GetInstance<ITestHandler>();
            if (_testManager.GetActiveTests().Count == 0)
            {
                _logger.Debug("ConfigureABTestingUsingActiveTestsCount - AB Testing disabled, there are no active tests.");
                _testHandler.DisableABTesting();
            }
            else if (_testManager.GetActiveTests().Count == 1)
            {
                _logger.Debug("ConfigureABTestingUsingActiveTestsCount - AB Testing enabled with active test.");
                _testHandler.EnableABTesting();
            }
            _cacheSignal.Reset();
        }

        public void DeleteTestForContent(Guid aContentGuid, CultureInfo cultureInfo)
        {
            var testList = _testManager.GetTestByItemId(aContentGuid).FindAll(abtest => abtest.State != TestState.Archived && abtest.ContentLanguage == cultureInfo.Name);

            foreach (var test in testList)
            {
                _testManager.Delete(test.Id, cultureInfo);
                //_experimentationClient = new ExperimentationClient();
                //Full Stack Experiment Disable Running Experiment
                _experimentationClient.Service.DisableExperiment(test.FS_FlagKey);
            }

            ConfigureABTestingUsingActiveTestsCount();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testData"></param>
        /// <returns></returns>
        public Guid CreateMarketingTest(TestingStoreModel testData)
        {
            IMarketingTest test = ConvertToMarketingTest(testData);

            var testId = _testManager.Save(test);
            ConfigureABTestingUsingActiveTestsCount();
            return testId;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void DeleteMarketingTest(Guid testGuid)
        {
            _testManager.Delete(testGuid);
            ConfigureABTestingUsingActiveTestsCount();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void StartMarketingTest(Guid testGuid)
        {
            _testManager.Start(testGuid);
            ConfigureABTestingUsingActiveTestsCount();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        public void StopMarketingTest(Guid testGuid)
        {
            _testManager.Stop(testGuid);
            ConfigureABTestingUsingActiveTestsCount();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testGuid"></param>
        /// /// <param name="cultureInfo"></param>
        public void StopMarketingTest(Guid testGuid, CultureInfo cultureInfo)
        {
            _testManager.Stop(testGuid, cultureInfo);
            ConfigureABTestingUsingActiveTestsCount();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId)
        {
            _testManager.Archive(testObjectId, winningVariantId);
            ConfigureABTestingUsingActiveTestsCount();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId, CultureInfo cultureInfo)
        {
            _testManager.Archive(testObjectId, winningVariantId, cultureInfo);
            ConfigureABTestingUsingActiveTestsCount();
        }

        public Guid SaveMarketingTest(IMarketingTest testData)
        {
            var testId = _testManager.Save(testData);
            ConfigureABTestingUsingActiveTestsCount();
            return testId;
        }

        public IMarketingTest ConvertToMarketingTest(TestingStoreModel testData)
        {
            if (testData.StartDate == null)
            {
                testData.StartDate = DateTime.UtcNow.ToString(CultureInfo.CurrentCulture);
            }

            // get the name of the culture for the current loaded content. If none exists or not available we set it to en empty string.
            var contentCultureName = testData.ContentCulture != null ? testData.ContentCulture.Name : string.Empty;

            var kpiData = JsonConvert.DeserializeObject<Dictionary<Guid, string>>(testData.KpiId);
            var kpis = kpiData.Select(kpi => _kpiManager.Get(kpi.Key)).ToList();

            var variant1ConversionResults = new List<KeyConversionResult>();
            var variant2ConversionResults = new List<KeyConversionResult>();

            // if more than 1 kpi then we need to take weights into effect
            if (kpis.Count > 1)
            {
                CalculateKpiWeights(kpiData, kpis, ref variant1ConversionResults, ref variant2ConversionResults);
            }

            // convert startDate to DateTime in UTC
            var startDate = DateTime.Parse(testData.StartDate).ToUniversalTime();

            var test = new ABTest
            {
                OriginalItemId = testData.TestContentId,
                ContentLanguage = contentCultureName,
                Owner = GetCurrentUser(),
                Description = testData.TestDescription,
                Title = testData.TestTitle,
                StartDate = startDate,
                EndDate = startDate.AddDays(testData.TestDuration),
                ParticipationPercentage = testData.ParticipationPercent,
                State = testData.Start ? TestState.Active : TestState.Inactive,
                Variants = new List<Variant>
                {
                    new Variant()
                    {
                        ItemId = testData.TestContentId,
                        ItemVersion = testData.PublishedVersion,
                        IsPublished = true,
                        Views = 0,
                        Conversions = 0,
                        KeyConversionResults = variant1ConversionResults
                    },
                    new Variant()
                    {
                        ItemId = testData.TestContentId,
                        ItemVersion = testData.VariantVersion,
                        Views = 0,
                        Conversions = 0,
                        KeyConversionResults = variant2ConversionResults
                    }
                },
                KpiInstances = kpis,
                ConfidenceLevel = testData.ConfidenceLevel
            };

            if (DateTime.Now >= DateTime.Parse(testData.StartDate))
            {
                test.State = TestState.Active;
            }
            return test;
        }

        /// <summary>
        /// Performs functions necessary for publishing the content provided in the test result
        /// Winning variants will be published and replace current published content.
        /// Winning published content will have their variants published then republish the original content
        /// to maintain proper content history 
        /// </summary>
        /// <param name="testResult"></param>
        /// <returns></returns>
        public string PublishWinningVariant(TestResultStoreModel testResult)
        {
            if (!string.IsNullOrEmpty(testResult.WinningContentLink))
            {

                //setup versions as ints for repository
                int winningVersion;
                int.TryParse(testResult.WinningContentLink.Split('_')[1], out winningVersion);

                IMarketingTest currentTest = GetTestById(Guid.Parse(testResult.TestId));
                var initialTestState = currentTest;
                try
                {
                    //get the appropriate variant and set IsWinner to True. Archive test to remove the lock on the content
                    var workingVariantId = currentTest.Variants.FirstOrDefault(x => x.ItemVersion == winningVersion).Id;

                    var draftContent = _testResultHelper.GetClonedContentFromReference(ContentReference.Parse(testResult.DraftContentLink));

                    //Pre Archive the test to unlock content and attempt to publish the winning version
                    //This only sets the state to archived.
                    currentTest.State = TestState.Archived;
                    SaveMarketingTest(currentTest);

                    //publish draft content for history tracking.
                    //Even if winner is the current published version we want to show the draft
                    //had been on the site as published.
                    _testResultHelper.PublishContent(draftContent);

                    if (testResult.WinningContentLink == testResult.PublishedContentLink)
                    {
                        //republish original published version as winner.
                        var publishedContent = _testResultHelper.GetClonedContentFromReference(ContentReference.Parse(testResult.PublishedContentLink));
                        _testResultHelper.PublishContent(publishedContent);
                    }

                    // only want to archive the test if publishing the winning variant succeeds.
                    ArchiveMarketingTest(currentTest.Id, workingVariantId, testResult.ContentCulture);


                    //Full Stack Experiment Disable Running Experiment
                    _experimentationClient.Service.DisableExperiment(currentTest.FS_FlagKey);
                }
                catch (Exception ex)
                {
                    _logger.Error("PickWinner Failed: Unable to process and/or publish winning test results", ex);
                    // restore the previous test data in the event of an error in case we archive the test but fail to publish the winning variant
                    try
                    {
                        SaveMarketingTest(initialTestState);
                    }
                    catch { }
                }
            }
            return testResult.TestId;
        }

        public Variant ReturnLandingPage(Guid testId)
        {
            var currentTest = _testManager.Get(testId);//gets the information of test
            string variationKey = string.Empty;

            var decisionMade = _fsSDKClient.Service.LogUserDecideEvent(currentTest.FS_FlagKey, out variationKey);
            if (decisionMade == true && !string.IsNullOrEmpty(variationKey)) // No errors happened and variation is decided
            {
                var variantLandingPage = _testManager.ReturnLandingPage(testId, variationKey);
                return variantLandingPage;
            }
            else { return new Variant(); }
        }

        public IContent GetVariantContent(Guid contentGuid)
        {
            return _testManager.GetVariantContent(contentGuid);
        }

        public IContent GetVariantContent(Guid contentGuid, CultureInfo cultureInfo)
        {
            return _testManager.GetVariantContent(contentGuid, cultureInfo);
        }

        public void IncrementCount(Guid testId, int itemVersion, CountType resultType, Guid kpiId = default(Guid), bool async = true)
        {
            var currentTest = GetTestById(testId, false);
            var sessionid = _httpContextHelper.GetRequestParam(_httpContextHelper.GetSessionCookieName());
            var c = new IncrementCountCriteria()
            {
                testId = testId,
                itemVersion = itemVersion,
                resultType = resultType,
                kpiId = kpiId,
                asynch = async,
                clientId = sessionid
            };
            /// track version in user.decide
            if (resultType.ToString() == "View") { 
                /// page is loaded
                
            }
            else // resultType = conversion
            {
                /// user ended up on the landing page
                /// call page track event
                /// 
                var pageViewEventName = string.Empty;
                if (currentTest != null)
                {
                    pageViewEventName = FullStackConstants.GetEventName(currentTest.FS_FlagKey);
                }

                _fsSDKClient.Service.TrackPageViewEvent(pageViewEventName, itemVersion);
            }
            _testManager.IncrementCount(c);
        }

        public void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool async = true)
        {
            _testManager.SaveKpiResultData(testId, itemVersion, keyResult, type, async);
        }

        public IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e)
        {
            return _testManager.EvaluateKPIs(kpis, sender, e);
        }

        private
            string GetCurrentUser()
        {
            return PrincipalInfo.CurrentPrincipal.Identity.Name;
        }

        /// <summary>
        /// If more than 1 kpi, we need to calculate the weights for each one.
        /// </summary>
        /// <param name="kpiData"></param>
        /// <param name="kpis"></param>
        /// <param name="variant1ConversionResults"></param>
        /// <param name="variant2ConversionResults"></param>
        private void CalculateKpiWeights(Dictionary<Guid, string> kpiData, List<IKpi> kpis, ref List<KeyConversionResult> variant1ConversionResults, ref List<KeyConversionResult> variant2ConversionResults)
        {
            // check if all weights are the same
            var firstKpiWeight = kpiData.First().Value;
            if (kpiData.All(entries => entries.Value == firstKpiWeight))
            {
                variant1ConversionResults.AddRange(
                    kpis.Select(kpi => new KeyConversionResult() { KpiId = kpi.Id, Weight = 1.0 / kpis.Count, SelectedWeight = firstKpiWeight }));
                variant2ConversionResults.AddRange(
                    kpis.Select(kpi => new KeyConversionResult() { KpiId = kpi.Id, Weight = 1.0 / kpis.Count, SelectedWeight = firstKpiWeight }));
            }
            else  // otherwise we need to do some maths to calculate the weights
            {
                double totalWeight = 0;
                var kpiWeights = new Dictionary<Guid, double>();

                // calculate total weight and create dictionary of ids and individual weights as selected by the user
                foreach (var kpi in kpiData)
                {
                    switch (kpi.Value.ToLower())
                    {
                        case "low":
                            kpiWeights.Add(kpi.Key, 1);
                            totalWeight += 1;
                            break;
                        case "high":
                            kpiWeights.Add(kpi.Key, 3);
                            totalWeight += 3;
                            break;
                        case "medium":
                        default:
                            kpiWeights.Add(kpi.Key, 2);
                            totalWeight += 2;
                            break;
                    }
                }

                // create conversion results for each kpi based on their weight and the total weight for all kpis for each variant
                variant1ConversionResults.AddRange(
                    kpiWeights.Select(
                        kpiEntry =>
                            new KeyConversionResult()
                            {
                                KpiId = kpiEntry.Key,
                                Weight = kpiEntry.Value / totalWeight,
                                SelectedWeight = kpiData.First(d => d.Key == kpiEntry.Key).Value
                            }));
                variant2ConversionResults.AddRange(
                    kpiWeights.Select(
                        kpiEntry =>
                            new KeyConversionResult()
                            {
                                KpiId = kpiEntry.Key,
                                Weight = kpiEntry.Value / totalWeight,
                                SelectedWeight = kpiData.First(d => d.Key == kpiEntry.Key).Value
                            }));
            }
        }

        /// <summary>
        /// Called by the ui when the config is changed. Forces a reset and tells content deliver machines 
        /// to refresh.
        /// </summary>
        public void ConfigurationChanged()
        {
            Refresh();
            _cacheSignal.Reset();
        }
    }
}

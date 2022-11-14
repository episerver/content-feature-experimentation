﻿using EPiServer.Core;
using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// The CachingTestManager class delivers marketing tests from a cache ,
    /// if possible, prior to deferring to another test manager.
    /// </summary>
    public class CachingTestManager : ITestManager
    {
        public const string MasterCacheKey = "epi/marketing/testing/masterkey";
        public const string AllTestsKey = "epi/marketing/testing/all";
        private readonly object listLock = new object();

        private readonly ILogger _logger;
        private readonly ITestManager _inner;
        private readonly ISynchronizedObjectInstanceCache _cache;
        private readonly DefaultMarketingTestingEvents _events;
        private readonly int _cacheTimeout;

        public CachingTestManager():this(
            ServiceLocator.Current.GetInstance<ISynchronizedObjectInstanceCache>(),
            ServiceLocator.Current.GetInstance<DefaultMarketingTestingEvents>(),
            ServiceLocator.Current.GetInstance<ITestManager>()
            )
        { 
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cache">Cache in which to store tests and related data</param>
        /// <param name="events">Marketing event publisher</param>
        /// <param name="inner">Test manager to defer to when tests are not in the cache</param>
        /// <param name="logger">A logger.</param>
        /// <param name="cacheTimeout">Cache timeout in seconds.</param>
        public CachingTestManager(ISynchronizedObjectInstanceCache cache, DefaultMarketingTestingEvents events, ITestManager inner, int? time=null)
        {
            var configuredTimeout = ServiceLocator.Current.GetInstance<IOptions<TestingOption>>().Value.CacheTimeoutInMinutes;
            int.TryParse(configuredTimeout, out int timeout);

            _inner = inner;
            _events = events;
            _cache = cache;
            _logger = LogManager.GetLogger();
            _cacheTimeout = time ?? timeout;
        }

        /// <inheritdoc/>
        public void Archive(Guid testObjectId, Guid winningVariantId, CultureInfo cultureInfo = null)
        {
            _inner.Archive(testObjectId, winningVariantId, cultureInfo);
            RemoveFromCache(testObjectId);
        }

        /// <inheritdoc/>
        public void Delete(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            _inner.Delete(testObjectId, cultureInfo);
            RemoveFromCache(testObjectId);
        }

        /// <inheritdoc/>
        public IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e)
        {
            return _inner.EvaluateKPIs(kpis, sender, e);
        }

        /// <inheritdoc/>
        public IMarketingTest Get(Guid testObjectId, bool fromCache = false)
        {
            IMarketingTest test = null;

            if (fromCache)
            {
                test = GetActiveTests().FirstOrDefault(t => t.Id == testObjectId);
            }

            return test ?? _inner.Get(testObjectId, false);
        }

        /// <inheritdoc/>
        public List<IMarketingTest> GetActiveTests()
        {
            var returnList = new List<IMarketingTest>();
            var all = _cache.Get(AllTestsKey) as List<IMarketingTest>;

            if (all == null || !all.Any())
            {
                RefreshCache();
                all = _cache.Get(AllTestsKey) as List<IMarketingTest>;
            }
            if (all != null)
            {
                returnList.AddRange(all);
            }

            return returnList;
        }

        /// <inheritdoc/>
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            return GetActiveTests().Where(test => test.OriginalItemId == originalItemId).ToList();
        }

        /// <inheritdoc/>
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId, CultureInfo contentCulture)
        {
            return GetActiveTests().Where(test => test.OriginalItemId == originalItemId && test.ContentLanguage == contentCulture.Name).ToList();                
        }

        /// <inheritdoc/>
        public long GetDatabaseVersion(string schema, string contextKey, bool populateCache = false)
        {
            return _inner.GetDatabaseVersion(schema, contextKey, populateCache);
        }

        /// <inheritdoc/>
        public List<IMarketingTest> GetTestByItemId(Guid originalItemId)
        {
            return _inner.GetTestByItemId(originalItemId);
        }

        /// <inheritdoc/>
        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            return _inner.GetTestList(criteria); ;
        }

        /// <inheritdoc/>
        public IContent GetVariantContent(Guid contentGuid)
        {
            return GetVariantContent(contentGuid, CultureInfo.GetCultureInfo("en-GB"));
        }

        /// <inheritdoc/>
        public IContent GetVariantContent(Guid contentGuid, CultureInfo cultureInfo)
        {
            IContent variant = null;

            variant = _cache.Get(GetCacheKeyForVariant(contentGuid, cultureInfo.Name)) as IContent;

            if (variant == null)
            {
                variant = _inner.GetVariantContent(contentGuid, cultureInfo);

                if (variant != null)
                {
                    AddVariantToCache(contentGuid, cultureInfo, variant);
                }
            }

            return variant;
        }

        /// <inheritdoc/>
        public void IncrementCount(IncrementCountCriteria criteria)
        {
            _inner.IncrementCount(criteria);
        }

        /// <inheritdoc/>
        public void IncrementCount(Guid testId, int itemVersion, CountType resultType, Guid kpiId = default(Guid), bool asynch = true)
        {
            _inner.IncrementCount(testId, itemVersion, resultType, kpiId, asynch);
        }

        /// <inheritdoc/>
        public Variant ReturnLandingPage(Guid testId, string variationKey)
        {
            return _inner.ReturnLandingPage(testId, variationKey);
        }

        /// <inheritdoc/>
        public Guid Save(IMarketingTest test)
        {
            var testId = _inner.Save(test);

            if (test.State == TestState.Active)
            {
                AddTestToCache(test);
            }
            else
            {
                RemoveFromCache(test.Id);
            }

            return testId;
        }

        /// <inheritdoc/>
        public void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool isAsync = true)
        {
            _inner.SaveKpiResultData(testId, itemVersion, keyResult, type, isAsync);
        }

        /// <inheritdoc/>
        public IMarketingTest Start(Guid testId)
        {
            var startedTest = _inner.Start(testId);

            if (startedTest?.State == TestState.Active)
            {
                AddTestToCache(startedTest);
            }

            return startedTest;
        }

        /// <inheritdoc/>
        public void Stop(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            _inner.Stop(testObjectId, cultureInfo);
            RemoveFromCache(testObjectId);
        }

        /// <summary>
        /// Removes all tests from the cache and repopulates it from the test manager
        /// that this class decorates.
        /// </summary>
        public void RefreshCache()
        {
            var testCriteria = new TestCriteria();
            testCriteria.AddFilter(
                new ABTestFilter
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And,
                    Value = TestState.Active
                }
            );

            List<IMarketingTest> allTests;

            lock (listLock)
            {
                _cache.RemoveLocal(MasterCacheKey);

                allTests = _inner.GetTestList(testCriteria) ?? new List<IMarketingTest>();

                _logger.Debug("RefreshCache - count = " + allTests.Count);

                _cache.Insert(AllTestsKey, allTests, GetCacheEvictionPolicy());
            }

            foreach (var test in allTests)
            {
                _logger.Debug("RefreshCache - inserting variants.");
                _cache.Insert(GetCacheKeyForVariant(test.OriginalItemId, test.ContentLanguage),
                    _inner.GetVariantContent(test.OriginalItemId, CultureInfo.GetCultureInfo(test.ContentLanguage)),
                    GetCacheEvictionPolicy());
            }

            //Notify interested consumers that a test was added to the cache.
            foreach (var test in allTests)
            {
                _events.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent, new TestEventArgs(test));
            }
        }

        /// <summary>
        /// Adds the specified test to the cache.
        /// </summary>
        /// <param name="test">Test to cache</param>
        private void AddTestToCache(IMarketingTest test)
        {
            var testAdded = false;

            lock (listLock)
            {
                var allTests = GetActiveTests();

                if (allTests.FirstOrDefault(t => t.Id == test.Id) == null)
                {
                    testAdded = true;
                    allTests.Add(test);

                    _logger.Debug("AddTestToCache - count = " + allTests.Count);

                    _cache.Insert(AllTestsKey, allTests, GetCacheEvictionPolicy());
                }
            }

            if (testAdded)
            {
                _logger.Debug("AddTestToCache - inserting variant.");
                _cache.Insert(GetCacheKeyForVariant(test.OriginalItemId, test.ContentLanguage),
                        _inner.GetVariantContent(test.OriginalItemId, CultureInfo.GetCultureInfo(test.ContentLanguage)),
                        GetCacheEvictionPolicy());

                //Notify interested consumers that a test was added to the cache.
                _events.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestAddedToCacheEvent, new TestEventArgs(test));
            }
        }

        /// <summary>
        /// Adds a variant to the cache.
        /// </summary>
        /// <param name="originalItemId">ID of the original content item</param>
        /// <param name="culture">Culture of the original content item</param>
        /// <param name="variant">Variant content to cache</param>
        private void AddVariantToCache(Guid originalItemId, CultureInfo culture, IContent variant)
        {
            _cache.Insert(GetCacheKeyForVariant(originalItemId, culture.Name), variant,
                    GetCacheEvictionPolicy());
        }

        /// <summary>
        /// Removes the specified test from the cache. Remote nodes maintaining
        /// a cache will also be signaled.
        /// </summary>
        /// <param name="testId">ID of test to remove</param>
        private void RemoveFromCache(Guid testId)            
        {
            IMarketingTest test = null;
            lock (listLock)
            {
                var tests = _cache.Get(AllTestsKey) as List<IMarketingTest>;
                test = tests.FirstOrDefault(t => t.Id == testId);

                if (test != null)
                {
                    tests.Remove(test);

                    _logger.Debug("RemoveFromCache - count = " + tests.Count);

                    _cache.Insert(AllTestsKey, tests, GetCacheEvictionPolicy());
                }
            }

            if (test != null)
            {
                _cache.RemoveLocal(GetCacheKeyForVariant(test.OriginalItemId, test.ContentLanguage));
                _events.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestRemovedFromCacheEvent, new TestEventArgs(test));
            }
        }

        /// <summary>
        /// Gets the cache key for a variant.
        /// </summary>
        /// <param name="contentGuid">ID of original content item</param>
        /// <param name="contentLanguage">Content language of original content item</param>
        /// <returns>Cache key</returns>
        public static string GetCacheKeyForVariant(Guid contentGuid, string contentLanguage)
        {
            return $"epi/marketing/testing/variants?originalItem={contentGuid}&culture={contentLanguage}";
        }

        private CacheEvictionPolicy GetCacheEvictionPolicy()
        {
            return new CacheEvictionPolicy( new TimeSpan(0, _cacheTimeout, 0), 
                                            CacheTimeoutType.Absolute, 
                                            null, 
                                            new string[] { MasterCacheKey });
        }
    }
}

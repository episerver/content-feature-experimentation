using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Exceptions;
using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.RestAPI;
using EPiServer.Marketing.Testing.Dal.Exceptions;
using EPiServer.Marketing.Testing.Messaging;
using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace EPiServer.Marketing.Testing.Core.Manager
{
    /// <summary>
    /// Central point of access for test data and test manipulation.
    /// </summary>
    public class TestManager : ITestManager
    {
        private Injected<IExperimentationClient> _experimentationClient;
        private readonly Injected<ITestingDataAccess> _dataAccess;
        private Random _randomParticiaption = new Random();
        private readonly Injected<IKpiManager> _kpiManager;
        private readonly Injected<DefaultMarketingTestingEvents> _marketingTestingEvents;
        private bool _databaseNeedsConfiguring;
        private readonly Injected<IMessagingManager> _messagingManager;
        private readonly Injected<IContentLoader> _contentLoader;

        /// <inheritdoc />
        public IMarketingTest Get(Guid testObjectId, bool fromCachedTests = false)
        {
            return TestManagerHelper.ConvertToManagerTest(_kpiManager.Service, _dataAccess.Service.Get(testObjectId)) 
                ?? throw new TestNotFoundException();
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetActiveTests()
        {
            var allActiveTests = new TestCriteria();
            allActiveTests.AddFilter(
                new ABTestFilter
                {
                    Property = ABTestProperty.State,
                    Operator = FilterOperator.And, 
                    Value = TestState.Active
                }
            );

            return GetTestList(allActiveTests);
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId)
        {
            return GetTestByItemId(originalItemId).Where(t => t.State == TestState.Active).ToList();
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId,CultureInfo contentCulture)
        {
            return GetTestByItemId(originalItemId).Where(t => t.State == TestState.Active && t.ContentLanguage == contentCulture.Name).ToList();
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetTestByItemId(Guid originalItemId)
        {
            var testList = new List<IMarketingTest>();

            foreach (var dalTest in _dataAccess.Service.GetTestByItemId(originalItemId))
            {
                testList.Add(TestManagerHelper.ConvertToManagerTest(_kpiManager.Service, dalTest));
            }

            return testList;
        }

        /// <inheritdoc />
        public List<IMarketingTest> GetTestList(TestCriteria criteria)
        {
            var testList = new List<IMarketingTest>();

            foreach (var dalTest in _dataAccess.Service.GetTestList(TestManagerHelper.ConvertToDalCriteria(criteria)))
            {
                testList.Add(TestManagerHelper.ConvertToManagerTest(_kpiManager.Service, dalTest));
            }

            return testList;
        }

        /// <summary>
        /// Saves a test to the database.
        /// </summary>
        /// <param name="multivariateTest">A test.</param>
        /// <returns>ID of the test.</returns>
        public Guid Save(IMarketingTest multivariateTest)
        {
            if (multivariateTest.KpiInstances == null)
            {
                throw new SaveTestException("Unable to save test due to null list of KPI's.  One or more KPI's are required.");
            }

            if (multivariateTest.KpiInstances.Count == 0)
            {
                throw new SaveTestException("Unable to save test due to empty list of KPI's.  One or more KPI's are required.");
            }

            var testId = _dataAccess.Service.Save(TestManagerHelper.ConvertToDalTest(multivariateTest));

            _marketingTestingEvents.Service.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestSavedEvent, new TestEventArgs(multivariateTest));

            if (multivariateTest.State == TestState.Active)
            {
                _marketingTestingEvents.Service.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStartedEvent, new TestEventArgs(multivariateTest));
            }

            return testId;
        }

        /// <inheritdoc />
        public void Delete(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            var testToDelete = Get(testObjectId);            

            foreach (var kpi in testToDelete.KpiInstances)
            {
                _kpiManager.Service.Delete(kpi.Id);
            }

            _dataAccess.Service.Delete(testObjectId);

            _marketingTestingEvents.Service.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestDeletedEvent, new TestEventArgs(testToDelete));
        }
       
        /// <inheritdoc />
        public IMarketingTest Start(Guid testObjectId)
        {
            var dalTest = _dataAccess.Service.Start(testObjectId);
            var managerTest = TestManagerHelper.ConvertToManagerTest(_kpiManager.Service, dalTest);

            if (dalTest != null)
            {
                _marketingTestingEvents.Service.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStartedEvent, new TestEventArgs(managerTest));
            }

            return managerTest;
        }

        /// <inheritdoc />
        public void Stop(Guid testObjectId, CultureInfo cultureInfo = null)
        {
            _dataAccess.Service.Stop(testObjectId);

            var stoppedTest = Get(testObjectId);            
            
            if (stoppedTest != null)
            {                
                _marketingTestingEvents.Service.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestStoppedEvent, new TestEventArgs(stoppedTest));
            }
        }

        /// <inheritdoc />
        public void Archive(Guid testObjectId, Guid winningVariantId, CultureInfo cultureInfo = null)
        {
            _dataAccess.Service.Archive(testObjectId, winningVariantId);

            var archivedTest = Get(testObjectId);
            if (archivedTest != null)
            {
                _marketingTestingEvents.Service.RaiseMarketingTestingEvent(DefaultMarketingTestingEvents.TestArchivedEvent, new TestEventArgs(archivedTest));
            }
        }

        /// <inheritdoc />
        public void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool aSynch = true)
        {
            if (aSynch)
            {                
                _messagingManager.Service.EmitKpiResultData(testId, itemVersion, keyResult, type);
            }
            else
            {
                switch (type)
                {
                    case KeyResultType.Financial:
                        _dataAccess.Service.AddKpiResultData(testId, itemVersion, TestManagerHelper.ConvertToDalKeyFinancialResult((KeyFinancialResult)keyResult), (int)type);
                        break;
                    case KeyResultType.Value:
                        _dataAccess.Service.AddKpiResultData(testId, itemVersion,
                            TestManagerHelper.ConvertToDalKeyValueResult((KeyValueResult) keyResult), (int) type);
                        break;
                    case KeyResultType.Conversion:
                    default:
                        _dataAccess.Service.AddKpiResultData(testId, itemVersion,
                            TestManagerHelper.ConvertToDalKeyConversionResult((KeyConversionResult)keyResult), (int)type);
                        break;
                }
            }
        }

        /// <inheritdoc />
        public Variant ReturnLandingPage(Guid testId, string variationKey)
        {
            var currentTest = _dataAccess.Service.Get(testId);
            var managerTest = TestManagerHelper.ConvertToManagerTest(_kpiManager.Service, currentTest);
            var activePage = new Variant();
            if (managerTest != null)
            {
                if (variationKey.ToLower() == "off") // Load current published version
                {
                    //sometimes published version comes in variant[0] 
                    //other times published version comes in variant[1], not sure why
                    //handle it by checking if variant[0] is published or variant[1] is published
                    if (currentTest.Variants[0].IsPublished == true) //[0] has is published version
                    {
                        activePage = TestManagerHelper.ConvertToManagerVariant(currentTest.Variants[0]);
                    }
                    else //Variant[1] has published version, return variant[1]
                    {
                        activePage = TestManagerHelper.ConvertToManagerVariant(currentTest.Variants[1]);
                    }
                }
                else if (variationKey.ToLower() == "on")
                {
                    if (currentTest.Variants[0].IsPublished == true) //[0] has is published version
                    {
                        //if [0] has published version, return [1] becase flag is on and on means draft version
                        activePage = TestManagerHelper.ConvertToManagerVariant(currentTest.Variants[1]);
                    }
                    else //Variant[0] has draft version, return variant[0]
                    {
                        activePage = TestManagerHelper.ConvertToManagerVariant(currentTest.Variants[0]);
                    }

                }
                _marketingTestingEvents.Service.RaiseMarketingTestingEvent(
                    DefaultMarketingTestingEvents.UserIncludedInTestEvent, 
                    new TestEventArgs(managerTest)
                );
            }
            return activePage;
        }

        /// <inheritdoc />
        public IContent GetVariantContent(Guid contentGuid)
        {
            return GetVariantContent(contentGuid, CultureInfo.GetCultureInfo("en-GB"));
        }

        /// <inheritdoc />
        public IContent GetVariantContent(Guid contentGuid, CultureInfo cultureInfo)
        {
            IVersionable variantContent = null;

            var test = GetActiveTestsByOriginalItemId(contentGuid, cultureInfo).FirstOrDefault(x => x.State.Equals(TestState.Active));

            if (test != null)
            {
                var testContent = _contentLoader.Service.Get<IContent>(contentGuid);

                if (testContent != null)
                {
                    var contentVersion = testContent.ContentLink.WorkID == 0
                        ? test.Variants.First(v => v.IsPublished).ItemVersion
                        : testContent.ContentLink.WorkID;

                    var variant = test.Variants.Where(v => v.ItemVersion != contentVersion).FirstOrDefault();

                    if (variant != null)
                    {
                        variantContent = (IVersionable)TestManagerHelper.CreateVariantContent(_contentLoader.Service, testContent, variant);
                        variantContent.Status = VersionStatus.Published;
                        variantContent.StartPublish = DateTime.Now.AddDays(-1);
                    }
                }
            }

            return (IContent)variantContent;
        }

        private Object _incrementLock = new Object();
        /// <inheritdoc />
        public void IncrementCount(IncrementCountCriteria criteria)
        {
            if (criteria.asynch)
            {
                if (criteria.resultType == CountType.Conversion)
                    _messagingManager.Service.EmitUpdateConversion(criteria.testId, criteria.itemVersion, criteria.kpiId, criteria.clientId);
                else if (criteria.resultType == CountType.View)
                    _messagingManager.Service.EmitUpdateViews(criteria.testId, criteria.itemVersion);
            }
            else
            {
                lock (_incrementLock)
                {
                    _dataAccess.Service.IncrementCount(criteria.testId, criteria.itemVersion, TestManagerHelper.AdaptToDalCount(criteria.resultType), criteria.kpiId);
                }
            }
        }

        /// <inheritdoc />
        public void IncrementCount(Guid testId, int itemVersion, CountType resultType, Guid kpiId = default(Guid), bool asynch = true)
        {
            var c = new IncrementCountCriteria()
            {
                testId = testId,
                itemVersion = itemVersion,
                resultType = resultType,
                kpiId = kpiId,
                asynch = asynch
            };
            IncrementCount(c);
        }

        /// <inheritdoc />
        public IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e)
        {
            return kpis.Select(kpi => kpi.Evaluate(sender, e)).ToList();
        }

        /// <inheritdoc />
        public long GetDatabaseVersion(string schema, string contextKey, bool populateCache = false)
        {
            if (_databaseNeedsConfiguring)
            {
                _databaseNeedsConfiguring = false;
                return 0;
            }

            return _dataAccess.Service.GetDatabaseVersion(schema, contextKey);
        }
    }
}

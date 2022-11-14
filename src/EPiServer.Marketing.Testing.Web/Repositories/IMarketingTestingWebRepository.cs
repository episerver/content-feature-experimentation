﻿using EPiServer.Core;
using EPiServer.Marketing.KPI.Manager.DataClass;
using EPiServer.Marketing.KPI.Results;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Web.Models;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace EPiServer.Marketing.Testing.Web.Repositories
{
    public interface IMarketingTestingWebRepository
    {
        IMarketingTest GetTestById(Guid testGuid, bool fromCache = false);
        List<IMarketingTest> GetActiveTests();
        List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId);
        List<IMarketingTest> GetActiveTestsByOriginalItemId(Guid originalItemId, CultureInfo currentCulture);
        List<IMarketingTest> GetTestList(TestCriteria criteria);
        List<IMarketingTest> GetTestList(TestCriteria criteria, CultureInfo currentCulture);
        Guid CreateMarketingTest(TestingStoreModel testData);
        void DeleteMarketingTest(Guid testGuid);
        void StartMarketingTest(Guid testGuid);
        void StopMarketingTest(Guid testGuid);
        void StopMarketingTest(Guid testGuid, CultureInfo cultureInfo);
        void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId);
        void ArchiveMarketingTest(Guid testObjectId, Guid winningVariantId, CultureInfo cultureInfo);
        Guid SaveMarketingTest(IMarketingTest testData);
        IMarketingTest GetActiveTestForContent(Guid contentGuid);
        IMarketingTest GetActiveTestForContent(Guid contentGuid, CultureInfo currentCulture);
        void DeleteTestForContent(Guid contentGuid);
        void DeleteTestForContent(Guid contentGuid, CultureInfo currentCulture);
        string PublishWinningVariant(TestResultStoreModel testResult);
        Variant ReturnLandingPage(Guid testId);
        IContent GetVariantContent(Guid contentGuid);
        IContent GetVariantContent(Guid contentGuid, CultureInfo cultureInfo);
        void IncrementCount(Guid testId, int itemVersion, CountType resultType, Guid kpiId = default(Guid), bool async = true);
        void SaveKpiResultData(Guid testId, int itemVersion, IKeyResult keyResult, KeyResultType type, bool async = true);
        IList<IKpiResult> EvaluateKPIs(IList<IKpi> kpis, object sender, EventArgs e);
        void ConfigurationChanged();
    }
}

﻿using EPiServer.DataAbstraction;
using EPiServer.Framework.Localization;
using EPiServer.PlugIn;
using EPiServer.Scheduler;
using EPiServer.ServiceLocation;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Web.Statistics;
using EPiServer.Marketing.Testing.Web.Config;
using EPiServer.Marketing.Testing.Web.Helpers;
using EPiServer.Marketing.Testing.Web.Models;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.Security;
using EPiServer.Logging;
using System.Security.Principal;

namespace EPiServer.Marketing.Testing.Web.Jobs
{

    /// <summary>
    /// Scheduled job class that automatically starts and stops tests
    /// </summary>
    [ScheduledPlugIn(
        DisplayName = "displayname",
        Description = "description",
        SortIndex = 0,              // Brings it to top of job list.
        DefaultEnabled = true,      // By default the task is enabled.
        InitialTime = "00:02:00",   // First time only, start after 2 min
        IntervalLength = 30,        // Default configured interval is 30 minutes
        IntervalType = ScheduledIntervalType.Minutes,
        LanguagePath = "/abtesting/scheduler_plugin")
    ]
    public class TestSchedulingJob : ScheduledJobBase
    {
        private IServiceProvider _locator;
        private AdminConfigTestSettings _config;

        [ExcludeFromCodeCoverage]
        public TestSchedulingJob()
        {
            _locator = ServiceLocator.Current;
            _config = AdminConfigTestSettings.Current;
        }

        public TestSchedulingJob(IServiceProvider locator)
        {
            _locator = locator;
            _config = _locator.GetInstance<AdminConfigTestSettings>();
        }

        public override string Execute()
        {
            int started = 0, stopped = 0, active = 0, inactive = 0, done = 0;
            var ls = _locator.GetInstance<LocalizationService>();
            var msg = ls.GetString("/abtesting/scheduler_plugin/message");
            var testingContextHelper = _locator.GetInstance<ITestingContextHelper>();
            var webRepo = _locator.GetInstance<IMarketingTestingWebRepository>();
            var jobRepo = _locator.GetInstance<IScheduledJobRepository>();
            var principalAccessor = _locator.GetInstance<IPrincipalAccessor>();
            var userImpersonation = _locator.GetInstance<IUserImpersonation>();
            var job = jobRepo.Get(this.ScheduledJobId);
            var nextExecutionUTC = job.NextExecutionUTC;

            var autoPublishTestResults = _config.AutoPublishWinner;
            
            // Start / stop any tests that need to be.
            // If any tests are scheduled to start or stop prior to the next scheduled
            // exection date of this job, change the next execution date approprately. 
            foreach (var test in webRepo.GetTestList(new TestCriteria()))
            {
                switch (test.State)
                {
                    case TestState.Active:
                        var utcEndDate = test.EndDate.ToUniversalTime();
                        if (DateTime.UtcNow > utcEndDate) // stop it now
                        {
                            LogManager.GetLogger().Information("Stopping test " + test.Description);

                            CalculateResultsAndSaveTest(test, webRepo, testingContextHelper, principalAccessor, userImpersonation, autoPublishTestResults);

                            stopped++;
                        }
                        else if (nextExecutionUTC > utcEndDate)
                        {
                            // set a newer date to run the job again
                            nextExecutionUTC = utcEndDate;
                        }
                        break;

                    case TestState.Inactive:
                        var utcStartDate = test.StartDate.ToUniversalTime();
                        if (DateTime.UtcNow > utcStartDate) // start it now
                        {
                            LogManager.GetLogger().Information("starting test " + test.Description);
                            webRepo.StartMarketingTest(test.Id);
                            started++;
                        }
                        else if (nextExecutionUTC > utcStartDate)
                        {
                            // set a newer date to run the job again
                            nextExecutionUTC = utcStartDate;
                        }
                        break;
                }
            }

            // update the next run time if we need to
            if( job.NextExecutionUTC != nextExecutionUTC )
            {
                if (job.IsEnabled)
                {
                    // NextExecution requires local time
                    job.NextExecution = nextExecutionUTC.ToLocalTime();
                    jobRepo.Save(job);
                }
            }

            // Calculate active, inactive and done for log message
            foreach ( var test in webRepo.GetTestList(new TestCriteria()) )
            {
                if( test.State == TestState.Active )
                { active++; }
                else if (test.State == TestState.Inactive)
                { inactive++; }
                else if (test.State == TestState.Done)
                { done++; }
            }

            return string.Format(msg, started, stopped, active, inactive, done);
        }

        /// <summary>
        /// Calculate test results significance, auto publish if needed, update the test results, and stop the test
        /// </summary>
        /// <param name="test"></param>
        /// <param name="webRepo"></param>
        /// <param name="testingContextHelper"></param>
        /// <param name="autoPublishTestResults"></param>
        private void CalculateResultsAndSaveTest(IMarketingTest test, IMarketingTestingWebRepository webRepo, ITestingContextHelper testingContextHelper, IPrincipalAccessor principalAccessor, IUserImpersonation userImpersonation, bool autoPublishTestResults)
        {
            try
            {
                //calculate significance results
                var sigResults = Significance.CalculateIsSignificant(test);
                test.IsSignificant = sigResults.IsSignificant;
                test.ZScore = sigResults.ZScore;

                webRepo.StopMarketingTest(test.Id);

                if (autoPublishTestResults && sigResults.IsSignificant)
                {
                    if (Guid.Empty != sigResults.WinningVariantId)
                    {
                        var winningVariant = test.Variants.First(v => v.Id == sigResults.WinningVariantId);
                        winningVariant.IsWinner = true;

                        webRepo.SaveMarketingTest(test);

                        var contextData = testingContextHelper.GenerateContextData(test);
                        var winningLink = winningVariant.IsPublished ? contextData.PublishedVersionContentLink : contextData.DraftVersionContentLink;

                        var storeModel = new TestResultStoreModel()
                        {
                            DraftContentLink = contextData.DraftVersionContentLink,
                            PublishedContentLink = contextData.PublishedVersionContentLink,
                            TestId = test.Id.ToString(),
                            WinningContentLink = winningLink
                        };

                        //The job may not have sufficient priviledges so we impersonate the test creator identity for it
                        if (String.IsNullOrEmpty(principalAccessor?.Principal?.Identity?.Name))
                        {
                            principalAccessor.Principal = userImpersonation.CreatePrincipalAsync(test.Owner).Result;
                        }

                        webRepo.PublishWinningVariant(storeModel);
                    }
                }
                else
                {
                    webRepo.SaveMarketingTest(test);
                }
            }
            catch(Exception e)
            {
                LogManager.GetLogger().Error("Internal error publishing variant.", e);
            }
        }
    }
}

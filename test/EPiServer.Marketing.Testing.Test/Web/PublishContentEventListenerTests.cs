﻿using EPiServer.Core;
using EPiServer.Framework.Localization;
using EPiServer.Marketing.Testing.Test.Fakes;
using EPiServer.Marketing.Testing.Web.Initializers;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Web.Helpers;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Web
{
    public class PublishContentEventListenerTests
    {
        Mock<IServiceProvider> _locator = new Mock<IServiceProvider>();
        Mock<IMarketingTestingWebRepository> _webRepo = new Mock<IMarketingTestingWebRepository>();
        Mock<IList<IContent>> _list = new Mock<IList<IContent>>();
        Mock<IEpiserverHelper> _mockEpiserverHelper;

        private PublishContentEventListener GetUnitUnderTest()
        {
            _locator.Setup(sl => sl.GetService(typeof(IMarketingTestingWebRepository))).Returns(_webRepo.Object);
            _locator.Setup(sl => sl.GetService(typeof(LocalizationService))).Returns(new FakeLocalizationService("whatever"));
            _mockEpiserverHelper = new Mock<IEpiserverHelper>();
            _mockEpiserverHelper.Setup(call => call.GetContentCultureinfo()).Returns(CultureInfo.GetCultureInfo("en-GB"));
            _locator.Setup(s1 => s1.GetService(typeof(IEpiserverHelper))).Returns(_mockEpiserverHelper.Object);

            return new PublishContentEventListener(_locator.Object, _list.Object);
        }

        [Fact]
        public void EventListener_DoesNothing_IfTestIDIsEmpty()
        {
            var testUnit = GetUnitUnderTest();
            IMarketingTest abTest = new ABTest() { Id = Guid.Empty, ContentLanguage = "en-GB"};
            _webRepo.Setup(call => call.GetActiveTestForContent(It.IsAny<Guid>(), CultureInfo.GetCultureInfo("en-GB"))).Returns(abTest);
            var eventArg = new ContentEventArgs(new ContentReference(111))
            {
                CancelAction = false,
                CancelReason = "nada",
                Content = new BasicContent() { ContentGuid = Guid.Empty }
            };

            testUnit._publishingContentEventHandler(this, eventArg);

            _webRepo.Verify(tm => tm.GetActiveTestForContent(It.IsAny<Guid>(), It.IsAny<CultureInfo>()), Times.Once, "Event Listener did not call GetActiveTestForContent");
            Assert.True(eventArg.CancelAction == false, "Event listener is attempting to cancel the publish event when it should not.");
        }

        [Fact]
        public void EventLister_CancelsTest_If_Content_Not_In_List()
        {
            var testUnit = GetUnitUnderTest();
            IMarketingTest abTest = new ABTest() { Id = Guid.NewGuid() };
            _webRepo.Setup(call => call.GetActiveTestForContent(It.IsAny<Guid>(), CultureInfo.GetCultureInfo("en-GB"))).Returns(abTest);

            // make sure its not in the list, since its not in the listener the publish will be cancled
            // I.e. publish triggered for content that is part of a test but winner not selected.
            _list.Setup(call => call.Contains(It.IsAny<IContent>())).Returns(false);

            var eventArg = new ContentEventArgs(new ContentReference(111))
            {
                CancelAction = false,
                CancelReason = "nada",
                Content = new BasicContent() { ContentGuid = Guid.Empty }
            };

            testUnit._publishingContentEventHandler(this, eventArg);
            Assert.True(eventArg.CancelAction == true, "Event listener did not cancel publish when it should have.");
        }

        [Fact]
        public void EventLister_DoesNot_CancelsTest_If_Content_In_List()
        {
            var testUnit = GetUnitUnderTest();
            IMarketingTest abTest = new ABTest() { Id = Guid.NewGuid() };
            _webRepo.Setup(call => call.GetActiveTestForContent(It.IsAny<Guid>(), CultureInfo.GetCultureInfo("en-GB"))).Returns(abTest);

            // make sure its not in the list, since its not in the listener the publish will be cancled
            // I.e. publish triggered for content that is part of a test but winner not selected.
            _list.Setup(call => call.Contains(It.IsAny<IContent>())).Returns(true);

            var eventArg = new ContentEventArgs(new ContentReference(111))
            {
                CancelAction = false,
                CancelReason = "nada",
                Content = new BasicContent() { ContentGuid = Guid.Empty }
            };

            testUnit._publishingContentEventHandler(this, eventArg);
            Assert.True(eventArg.CancelAction == false, "Event listener canceled publish event when it should have.");
        }

        [Fact]
        public void EventListener_DoesNothing_IfTestIDIsEmpty_Checkin()
        {
            var testUnit = GetUnitUnderTest();
            IMarketingTest abTest = new ABTest() { Id = Guid.Empty };
            _webRepo.Setup(call => call.GetActiveTestForContent(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(abTest);
            var eventArg = new ContentEventArgs(new ContentReference(111))
            {
                CancelAction = false,
                CancelReason = "nada",
                Content = new BasicContent() { ContentGuid = Guid.Empty }
            };

            testUnit._checkingInContentEventHandler(this, eventArg);

            _webRepo.Verify(tm => tm.GetActiveTestForContent(It.IsAny<Guid>(), It.IsAny<CultureInfo>()), Times.Once, "Event Listener did not call GetActiveTestForContent");
            Assert.True(eventArg.CancelAction == false, "Event listener is attempting to cancel the checkin event when it should not.");
        }

        [Fact]
        public void _checkingInContentEventHandler_cancels_scheduled_publishes_when_there_is_a_test_running()
        {
            var testUnit = GetUnitUnderTest();
            var cancelReason = "test reason";
            IMarketingTest abTest = new ABTest() { Id = Guid.NewGuid() };
            _webRepo.Setup(call => call.GetActiveTestForContent(It.IsAny<Guid>(), It.IsAny<CultureInfo>())).Returns(abTest);

            var aEventContent = new MediaData();
            aEventContent.StartPublish = DateTime.Now.AddDays(1);

            var eventArg = new ContentEventArgs(new ContentReference(111))
            {
                CancelAction = false,
                CancelReason = cancelReason,
                Content = aEventContent
            };

            testUnit._checkingInContentEventHandler(this, eventArg);

            Assert.True(eventArg.CancelAction, "Should be canceling the publish action when we find a test for scheduled content");
            Assert.True(eventArg.CancelReason != cancelReason, "Cancel reason should be set when we cancel the publish");
        }

        [Fact]
        public void EventLister_CancelsTest_If_PublishDate_In_Future_Checkin()
        {
            var testUnit = GetUnitUnderTest();
            IMarketingTest abTest = new ABTest() { Id = Guid.NewGuid() };
            _webRepo.Setup(call => call.GetActiveTestForContent(It.IsAny<Guid>(), CultureInfo.GetCultureInfo("en-GB"))).Returns(abTest);

            var c = new FakeContentData() { ContentGuid = Guid.Empty };
            (c as IVersionable).StartPublish = DateTime.MaxValue;
            var eventArg = new ContentEventArgs(new ContentReference(111))
            {
                CancelAction = false,
                CancelReason = "nada",
                Content = c
            };

            testUnit._publishingContentEventHandler(this, eventArg);
            Assert.True(eventArg.CancelAction == true, "Event listener did not cancel checkin with publish date in future.");
        }

        private class FakeContentData : BasicContent, IVersionable
        {
            //
            // Summary:
            //     /// Gets or sets a value indicating whether this item is in pending publish state.
            //     ///
            public bool IsPendingPublish { get; set; }
            //
            // Summary:
            //     /// Gets or sets the start publish date for this item. ///
            public DateTime? StartPublish { get; set; }
            //
            // Summary:
            //     /// Gets or sets the version status of this item. ///
            public VersionStatus Status { get; set; }
            //
            // Summary:
            //     /// Gets or sets the stop publish date for this item. ///
            public DateTime? StopPublish { get; set; }

        }
    }
}

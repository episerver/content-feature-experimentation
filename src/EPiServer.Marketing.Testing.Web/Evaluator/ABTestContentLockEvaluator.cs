﻿using System;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Cms.Shell;
using EPiServer.Core;
using EPiServer.Marketing.Testing.Web.Repositories;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Helpers;

namespace EPiServer.Marketing.Testing.Web.Evaluator
{
    public class ABTestLockEvaluator : IContentLockEvaluator
    {
        private IMarketingTestingWebRepository _webRepo;
        private IContentRepository _contentRepo;
        private IEpiserverHelper _episerverHelper;
        public string _abLockId = "ActiveABTestLock";

        [ExcludeFromCodeCoverage]
        public ABTestLockEvaluator()
        {
            _webRepo = ServiceLocator.Current.GetInstance<IMarketingTestingWebRepository>();
            _contentRepo = ServiceLocator.Current.GetInstance<IContentRepository>();
            _episerverHelper = ServiceLocator.Current.GetInstance<IEpiserverHelper>();
        }

        public ABTestLockEvaluator(IMarketingTestingWebRepository theWebRepo, IContentRepository theContentRepo, IEpiserverHelper theEpiHelper)
        {
            _webRepo = theWebRepo;
            _contentRepo = theContentRepo;
            _episerverHelper = theEpiHelper;
        }

        public ContentLock IsLocked(ContentReference contentLink)
        {
            var contentCulture = _episerverHelper.GetContentCultureinfo();
            var content = _contentRepo.Get<IContent>(contentLink, contentCulture);

            if (content == null)
                return null;
            
            // MAR-1080 - need to check if the content in the current language before checking to see if a test is running against it
            if (content.LanguageBranch() != contentCulture.Name)
                return null;

            var test = _webRepo.GetActiveTestForContent(content.ContentGuid, contentCulture);

            if (test == null || test.Id == null || test.Id == Guid.Empty)
                return null;
            
            return new ContentLock(contentLink, test.Owner, _abLockId, test.CreatedDate);
        }
    }
}

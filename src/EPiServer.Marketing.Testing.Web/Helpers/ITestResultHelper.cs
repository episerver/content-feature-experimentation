﻿using EPiServer.Core;
using EPiServer.DataAccess;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public interface ITestResultHelper
    {
        IContent GetClonedContentFromReference(ContentReference reference);
        ContentReference PublishContent(IContent contentToPublish);

    }
}

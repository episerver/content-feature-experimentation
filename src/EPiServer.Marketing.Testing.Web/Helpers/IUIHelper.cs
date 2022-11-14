﻿using EPiServer.Core;
using System;

namespace EPiServer.Marketing.Testing.Web.Helpers
{
    public interface IUIHelper
    {
        /// <summary>
        /// Given a specific IContent Guid, get the IContent object associated with it.
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        IContent getContent(Guid guid);

        /// <summary>
        /// Gets the link to the test administration url.
        /// </summary>
        /// <returns></returns>
        String getConfigurationURL();

        string getEpiUrlFromLink(ContentReference urltext);

    }
}

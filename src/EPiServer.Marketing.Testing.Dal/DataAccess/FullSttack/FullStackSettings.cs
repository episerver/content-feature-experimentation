using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack;

namespace EPiServer.Marketing.Testing.Dal
{
    [Options]
    public class FullStackSettings
    {

        public int CacheInMinutes { get; set; } = 10;

        public string RestAuthToken { get; set; } = "";

        public string ProjectId { get; set; } = FullStackConstants.ProjectId;

        public string EnviromentKey { get; set; } = FullStackConstants.EnviromentKey;

        public string SDKKey { get; set; } = "";

        public int APIVersion { get; set; } = FullStackConstants.APIVersion;

        public string EventName { get; set; } = FullStackConstants.EventName;

        public string EventDescription { get; set; } = FullStackConstants.EventDescription;
    }
}

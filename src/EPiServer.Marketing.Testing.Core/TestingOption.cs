using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Core
{
    [Options]
    public class TestingOption
    {
        public const string Section = "EPiServer:Marketing:Testing";

        public string CacheTimeoutInMinutes { get; set; }
        public string PreviewStyleOverride { get; set; }
        public string Roles { get; set; }
        public string TestMonitorSeconds { get; set; }
    }
}

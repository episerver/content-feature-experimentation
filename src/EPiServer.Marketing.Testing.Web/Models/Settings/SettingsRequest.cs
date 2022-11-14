using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Web.Models.Settings
{
    public class SettingsRequest
    {
        public int TestDuration { get; set; }
        public int ParticipationPercent { get; set; }
        public int ConfidenceLevel { get; set; }
        public bool AutoPublishWinner { get; set; }
        public bool IsEnabled { get; set; }
    }
}

using EPiServer.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess
{
    [Options]
    public class ExperimentationOptions
    {
        public string SdkKey { get; set; }

        public int DataFilePollingIntervallInSeconds { get; set; } = 300;
        public int BlockingTimeoutPeriodInSeconds { get; set; } = 10;

        public bool SyncDefaultAudienceAttributesToOptimizely { get; set; } = true;
        public bool SyncDefaultEventsToOptimizely { get; set; } = true;
        public bool TrackEventsInOptimizely { get; set; } = true;
        public bool RegisterAndTrackPageTypesInOptimizely { get; set; } = true;
        public bool RegisterAndTrackCommerceCategoriesInOptimizely { get; set; } = true;
    }
}

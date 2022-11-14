using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack
{
    public static class FullStackConstants
    {
        public static string APIURL = "https://api.optimizely.com/";
        public static string ProjectId = "21972070188";

        public static string EnviromentKey  = "production";

        public static string EventName = "page_view";

        public static string EventDescription = "Event to calculate page view metrics";

        public static int APIVersion = 1;

        public static string ReplaceABTestExperiment = "AB_Test_Experiment";

        public static string ReplaceExperimentFlag = "_Experiment_Flag";

        public static string FullStackUserGUID = "FullStackUserGUID";

        public static string GetFlagKey(string FlagName)
        {
            return FlagName.Replace(" ", "_").Replace("/", "") + "_Flag";
        }

        public static string GetExperimentKey(string objTitle)
        {
            return objTitle.Replace(" ", "_").Replace("/", "") + "_Experiment";
        }

        public static string GetEventName(string flagName)
        {
            string EventNameReturn = string.Empty;
            EventNameReturn = EventName + "_" + flagName.Replace(ReplaceExperimentFlag, "");
            return EventNameReturn;
        }

        public static string ContentGUIDKey = "content_guid";

        public static string ContentGUIDDesc = "Guid of content";

        public static string ContentGUIDType = "string";

        public static string DraftVersionKey = "draft_version";

        public static string DraftVersionDesc = "Draft version of content";

        public static string DraftVersionType = "integer";

        public static string DraftVersionDefault = "0";

        public static string PublishedVersionKey = "published_version";

        public static string PublishedVersionDesc = "Published version of content";
    }
}

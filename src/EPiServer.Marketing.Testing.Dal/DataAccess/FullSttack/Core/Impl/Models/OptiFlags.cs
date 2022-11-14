using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models.OptiFeature;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OptiFlag
    {
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("outlier_filtering_enabled")]
        public bool OutlierFilteringEnabled { get; set; }
        [JsonProperty("variable_definitions")]
        public VariableDefinitions VariableDefinition { get; set; }




        public class VariableDefinitions
        {
            [JsonProperty("content_guid")]
            public Variable Content_Guid { get; set; }

            [JsonProperty("draft_version")]
            public Variable Draft_Version { get; set; }

            [JsonProperty("published_version")]
            public Variable Published_Version { get; set; }


        }

    }
}

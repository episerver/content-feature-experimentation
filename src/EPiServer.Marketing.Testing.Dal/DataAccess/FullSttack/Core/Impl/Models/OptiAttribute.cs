using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models
{
    /*
https://library.optimizely.com/docs/api/app/v2/index.html#operation/list_attributes
 */
    public class OptiAttribute
    {
        [JsonProperty("archived")]
        public bool Archived { get; set; }
        [JsonProperty("condition_type")]
        public string ConditionType { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("id")]
        public ulong Id { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("last_modified")]
        public DateTime LastModified { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("project_id")]
        public ulong ProjectId { get; set; }
    }
}

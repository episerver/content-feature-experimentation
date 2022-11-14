using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models
{
    public class OptiFeature
    {
        [JsonProperty("archived")]
        public bool Archived { get; set; }
        [JsonProperty("created")]
        public DateTime Created { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        //[JsonProperty("environments")]
        //public Environments EnvironmentList { get; set; }
        [JsonProperty("environments")]
        public Dictionary<string, Environment> Environments { get; set; } = new Dictionary<string, Environment>();
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
        [JsonProperty("variables")]
        public List<Variable> Variables { get; set; }

        public class RolloutRule
        {
            [JsonProperty("audience_conditions")]
            public string AudienceConditions { get; set; }
            [JsonProperty("enabled")]
            public bool Enabled { get; set; }
            [JsonProperty("percentage_included")]
            public int PercentageIncluded { get; set; }
        }

        public class Environment
        {
            [JsonProperty("id")]
            public ulong Id { get; set; }
            [JsonProperty("is_primary")]
            public bool IsPrimary { get; set; }
            [JsonProperty("rollout_rules")]
            public List<RolloutRule> RolloutRules { get; set; }
        }

        [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
        public class Variable
        {
            //[JsonProperty("archived")]
            //public bool Archived { get; set; }
            [JsonProperty("default_value")]
            public string DefaultValue { get; set; }
            //[JsonProperty("id")]
            //public ulong Id { get; set; }
            [JsonProperty("key")]
            public string Key { get; set; }
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("description")]
            public string Description { get; set; }
        }
    }
}

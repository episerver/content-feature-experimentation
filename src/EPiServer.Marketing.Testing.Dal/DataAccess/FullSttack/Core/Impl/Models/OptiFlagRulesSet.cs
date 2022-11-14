using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public partial class OptiFlagRulesSet
    {
        [JsonProperty("op")]
        public string Op { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        public string value { get; set; }

        [JsonProperty("ValueClass")]
        public ValueClass ValueClass { get; set; }
    }

    public partial class ValueClass
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("distribution_mode")]
        public string DistributionMode { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("percentage_included")]
        public long PercentageIncluded { get; set; }

        [JsonProperty("metrics")]
        public Metric[] Metrics { get; set; }

        [JsonProperty("variations")]
        public Variations Variations { get; set; }
    }

    public partial class Metric
    {
        [JsonProperty("event_id")]
        public long EventId { get; set; }

        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("aggregator")]
        public string Aggregator { get; set; }

        [JsonProperty("winning_direction")]
        public string WinningDirection { get; set; }

        [JsonProperty("display_title")]
        public string DisplayTitle { get; set; }
    }

    public partial class Variations
    {
        [JsonProperty("off")]
        public Off Off { get; set; }

        [JsonProperty("on")]
        public Off On { get; set; }
    }

    public partial class Off
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("percentage_included")]
        public long PercentageIncluded { get; set; }
    }

    public partial class OptiFlagRulesSet
    {
        public static OptiFlagRulesSet[] FromJson(string json) => JsonConvert.DeserializeObject<OptiFlagRulesSet[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this OptiFlagRulesSet[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}

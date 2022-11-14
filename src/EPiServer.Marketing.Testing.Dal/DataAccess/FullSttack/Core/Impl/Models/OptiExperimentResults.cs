
using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack.Core.Impl.Models
{
    public partial class OptiExperimentResults
    {
        [JsonProperty("confidence_threshold")]
        public double ConfidenceThreshold { get; set; }

        [JsonProperty("end_time")]
        public DateTimeOffset EndTime { get; set; }

        [JsonProperty("experiment_id")]
        public long ExperimentId { get; set; }

        [JsonProperty("metrics")]
        public Metric[] Metrics { get; set; }

        [JsonProperty("reach")]
        public Reach Reach { get; set; }

        [JsonProperty("start_time")]
        public DateTimeOffset StartTime { get; set; }

        [JsonProperty("stats_config")]
        public StatsConfig StatsConfig { get; set; }
    }

    public partial class Metric
    {
        [JsonProperty("aggregator")]
        public string Aggregator { get; set; }

        [JsonProperty("event_id")]
        public long EventId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("results")]
        public Dictionary<string, Result> Results { get; set; }

        [JsonProperty("scope")]
        public string Scope { get; set; }

        [JsonProperty("winning_direction")]
        public string WinningDirection { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("is_baseline")]
        public bool IsBaseline { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("rate")]
        public double Rate { get; set; }

        [JsonProperty("samples")]
        public long Samples { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("variance")]
        public double Variance { get; set; }

        [JsonProperty("variation_id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long VariationId { get; set; }

        [JsonProperty("lift", NullValueHandling = NullValueHandling.Ignore)]
        public Lift Lift { get; set; }
    }

    public partial class Lift
    {
        [JsonProperty("is_significant")]
        public bool IsSignificant { get; set; }

        [JsonProperty("lift_status")]
        public string LiftStatus { get; set; }

        [JsonProperty("significance")]
        public double Significance { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        [JsonProperty("visitors_remaining")]
        public long VisitorsRemaining { get; set; }
    }

    public partial class Reach
    {
        [JsonProperty("baseline_count")]
        public long BaselineCount { get; set; }

        [JsonProperty("baseline_reach")]
        public double BaselineReach { get; set; }

        [JsonProperty("total_count")]
        public long TotalCount { get; set; }

        [JsonProperty("treatment_count")]
        public long TreatmentCount { get; set; }

        [JsonProperty("treatment_reach")]
        public double TreatmentReach { get; set; }

        [JsonProperty("variations")]
        public Dictionary<string, Variation> Variations { get; set; }
    }

    public partial class Variation
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("variation_id")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long VariationId { get; set; }

        [JsonProperty("variation_reach")]
        public double VariationReach { get; set; }
    }

    public partial class StatsConfig
    {
        [JsonProperty("confidence_level")]
        public double ConfidenceLevel { get; set; }

        [JsonProperty("difference_type")]
        public string DifferenceType { get; set; }

        [JsonProperty("epoch_enabled")]
        public bool EpochEnabled { get; set; }
    }

    public partial class OptiExperimentResults
    {
        public static OptiExperimentResults FromJson(string json) => JsonConvert.DeserializeObject<OptiExperimentResults>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this OptiExperimentResults self) => JsonConvert.SerializeObject(self, Converter.Settings);
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

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }

}


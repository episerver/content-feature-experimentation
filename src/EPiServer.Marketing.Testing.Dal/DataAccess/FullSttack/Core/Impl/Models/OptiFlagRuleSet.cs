using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<OptiFlagRuleSet>>(myJsonResponse);



        public  class OptiFlagRuleSet
        {
            [JsonProperty("op")]
            public string Op { get; set; }

            [JsonProperty("path")]
            public string Path { get; set; }

            [JsonProperty("value")]
            public ValueUnion Value { get; set; }
        }

    
    public  class ValueClass
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

        public  class Metric
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

        public  class Variations
        {
            [JsonProperty("off")]
            public Off Off { get; set; }

            [JsonProperty("on")]
            public Off On { get; set; }
        }

        public  class Off
        {
            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("percentage_included")]
            public long PercentageIncluded { get; set; }
        }

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public  struct ValueUnion
        {
        
        public string String;
            public ValueClass ValueClass;

            public static implicit operator ValueUnion(string String) => new ValueUnion { String = String };
            public static implicit operator ValueUnion(ValueClass ValueClass) => new ValueUnion { ValueClass = ValueClass };
        }

        //internal static class Converter
        //{
        //    public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        //    {
        //        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        //        DateParseHandling = DateParseHandling.None,
        //        Converters =
        //    {
        //        ValueUnionConverter.Singleton,
        //        new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
        //    },
        //    };
        //}

        public class ValueUnionConverter : JsonConverter
        {
            public override bool CanConvert(Type t) => t == typeof(ValueUnion) || t == typeof(ValueUnion?);

            public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.String:
                    case JsonToken.Date:
                        var stringValue = serializer.Deserialize<string>(reader);
                        return new ValueUnion { String = stringValue };
                    case JsonToken.StartObject:
                        var objectValue = serializer.Deserialize<ValueClass>(reader);
                        return new ValueUnion { ValueClass = objectValue };
                }
                throw new Exception("Cannot unmarshal type ValueUnion");
            }

            public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
            {
                var value = (ValueUnion)untypedValue;
                if (value.String != null)
                {
                    serializer.Serialize(writer, value.String);
                    return;
                }
                if (value.ValueClass != null)
                {
                    serializer.Serialize(writer, value.ValueClass);
                    return;
                }
                throw new Exception("Cannot marshal type ValueUnion");
            }

            public static readonly ValueUnionConverter Singleton = new ValueUnionConverter();
        }
    }


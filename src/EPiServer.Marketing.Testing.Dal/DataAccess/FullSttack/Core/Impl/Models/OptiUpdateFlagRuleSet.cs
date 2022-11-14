using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack.Core.Impl.Models
{
    public partial class OptiUpdateFlagRuleSet
    {
        [JsonProperty("op")]
        public string Op { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}

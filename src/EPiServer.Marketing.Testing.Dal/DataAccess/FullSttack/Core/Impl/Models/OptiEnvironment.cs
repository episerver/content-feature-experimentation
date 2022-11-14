using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models
{
    public class OptiEnvironment
    {
        [JsonProperty("archived")]
        public bool Archived { get; set; }
        [JsonProperty("datafile")]
        public DatafileInformation Datafile { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("has_restricted_permissions")]
        public bool HasRestrictedPermissions { get; set; }
        [JsonProperty("id")]
        public ulong Id { get; set; }
        [JsonProperty("is_primary")]
        public bool IsPrimary { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("priority")]
        public int Priority { get; set; }
        [JsonProperty("project_id")]
        public ulong ProjectId { get; set; }
    }

    public class DatafileInformation
    {
        [JsonProperty("latest_file_size")]
        public int LatestFileSize { get; set; }
        [JsonProperty("ip_anonymization")]
        public bool IpAnonymization { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("other_urls")]
        public List<object> OtherUrls { get; set; }
        [JsonProperty("sdk_key")]
        public string SdkKey { get; set; }
        [JsonProperty("cache_ttl")]
        public int CacheTtl { get; set; }
        [JsonProperty("uid")]
        public long Uid { get; set; }
        [JsonProperty("is_private")]
        public bool IsPrivate { get; set; }
        [JsonProperty("revision")]
        public int Revision { get; set; }
    }
}

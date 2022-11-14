using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models
{

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class OptiEvent
    {
        //[JsonProperty("archived")]
        //public bool Archived { get; set; }
        //[JsonProperty("category")]
        //public string Category { get; set; }
        //[JsonProperty("created")]
        //public DateTime Created { get; set; }
        //[JsonProperty("event_type")]
        //public string EventType { get; set; }
        [JsonProperty("id")]
        public long Id { get; set; }
        //[JsonProperty("is_classic")]
        //public bool IsClassic { get; set; }
        [JsonProperty("key")]
        public string Key { get; set; }
        //[JsonProperty("last_modified")]
        //public DateTime LastModified { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        //[JsonProperty("project_id")]
        //public ulong ProjectId { get; set; }

        public enum Types
        {
            AddToCart = 0,
            Save = 1,
            Search = 2,
            Share = 3,
            Purchase = 4,
            Convert = 5,
            SignUp = 6,
            Subscribe = 7,
            Other = 8
        }
        public static string GetOptimizelyType(Types type)
        {
            switch (type)
            {
                case Types.AddToCart:
                    return "add_to_cart";
                case Types.Save:
                    return "save";
                case Types.Share:
                    return "share";
                case Types.Purchase:
                    return "purchase";
                case Types.Convert:
                    return "convert";
                case Types.SignUp:
                    return "sign_up";
                case Types.Subscribe:
                    return "subscribe";
            }

            return "other";
        }
    }
}

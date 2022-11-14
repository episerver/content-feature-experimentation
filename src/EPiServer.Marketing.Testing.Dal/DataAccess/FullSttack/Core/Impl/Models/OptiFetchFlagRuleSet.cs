using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack.Core.Impl.Models
{
    public class AllowList
    {
    }

    public class GroupRule
    {
        public int group_id { get; set; }
        public int traffic_allocation { get; set; }
    }

    //public class Metric
    //{
    //    public long event_id { get; set; }
    //    public string event_type { get; set; }
    //    public string scope { get; set; }
    //    public string aggregator { get; set; }
    //    public string winning_direction { get; set; }
    //    public string display_title { get; set; }
    //}

    public class Off
    {
        public string key { get; set; }
        public string name { get; set; }
        public int percentage_included { get; set; }
    }

    public class On
    {
        public string key { get; set; }
        public string name { get; set; }
        public int percentage_included { get; set; }
    }

    public class OptiFetchFlagRuleSet
    {
        public string url { get; set; }
        public string update_url { get; set; }
        public string disable_url { get; set; }
        public Rules rules { get; set; }
        public List<string> rule_priorities { get; set; }
        public int id { get; set; }
        public string urn { get; set; }
        public bool archived { get; set; }
        public bool enabled { get; set; }
        public DateTime updated_time { get; set; }
        public string flag_key { get; set; }
        public string environment_key { get; set; }
        public string environment_name { get; set; }
        public string default_variation_key { get; set; }
        public string default_variation_name { get; set; }
        public int revision { get; set; }
    }

    public class Rules
    {
        public ABTestExperiment AB_Test_Experiment { get; set; }
    }

    public class Variations
    {
        public Off off { get; set; }
        public On on { get; set; }
    }

    public class ABTestExperiment
    {
        public string key { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public Variations variations { get; set; }
        public string type { get; set; }
        public string distribution_mode { get; set; }
        public int id { get; set; }
        public string urn { get; set; }
        public bool archived { get; set; }
        public bool enabled { get; set; }
        public DateTime created_time { get; set; }
        public DateTime updated_time { get; set; }
        public List<object> audience_conditions { get; set; }
        public List<object> audience_ids { get; set; }
        public int percentage_included { get; set; }
        public List<Metric> metrics { get; set; }
        public string fetch_results_ui_url { get; set; }
        public AllowList allow_list { get; set; }
        public GroupRule group_rule { get; set; }
        public int group_remaining_traffic_allocation { get; set; }
    }


}

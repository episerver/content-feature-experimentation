using EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.Core.Impl.Models;
using EPiServer.Marketing.Testing.Dal.DataAccess.FullSttack.Core.Impl.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess.FullStack.RestAPI
{
    public interface IExperimentationClient
    {
        public bool GetExperimentID(out OptiFetchFlagRuleSet fetchedFlagRuleSet, string flagKey, string experimentKey);
        public bool GetExperimentResult(out OptiExperimentResults opResults, long ExperimentID);
        bool CreateOrUpdateAttribute(string key, string description = null);
        //bool CreateOrUpdateEvent(string key, OptiEvent.Types type = OptiEvent.Types.Other, string description = null);
        bool CreateEventIfNotExists(OptiEvent opEvent, out long EventID);

        Task<bool> CreateOrUpdateFlag(OptiFlag optiFlag);

        Task<bool> CreateFlagRuleSet(List<OptiFlagRulesSet> ruleSet);

        Task<bool> EnableExperiment();

        Task<bool> DisableExperiment(string FlagKey);
        List<OptiFeature> GetFeatureList();
        List<OptiAttribute> GetAttributeList();
        List<OptiEvent> GetEventList();
        List<OptiEnvironment> GetEnvironmentList();
        List<OptiExperiment> GetExperimentList();
        OptiExperiment GetExperiment(long experimentId);
        OptiExperiment GetExperiment(string experimentKey);
    }
}

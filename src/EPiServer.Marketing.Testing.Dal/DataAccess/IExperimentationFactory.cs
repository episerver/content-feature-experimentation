using OptimizelySDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess
{
    public interface IExperimentationFactory
    {
        Optimizely Instance { get; }
        bool IsConfigured { get; }
    }
}

using EPiServer.Logging;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Options;
using OptimizelySDK;
using OptimizelySDK.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPiServer.Marketing.Testing.Dal.DataAccess
{
    public class DefaultExperimentationFactory : IExperimentationFactory
    {
        private static readonly Lazy<Optimizely> OptimizelyInstance = new Lazy<Optimizely>(GetInstance);

        private static Optimizely GetInstance()
        {
            try
            {
                var errorLogger = ServiceLocator.Current.GetInstance<OptimizelySDK.Logger.ILogger>();
                var projectConfig = ServiceLocator.Current.GetInstance<ExperimentationProjectConfigManager>();

                var eventConfigManager = new BatchEventProcessor.Builder()
                    .WithMaxBatchSize(1)
                    .WithFlushInterval(TimeSpan.FromSeconds(5))
                    .WithLogger(errorLogger)
                    .Build();

                var instance = OptimizelyFactory.NewDefaultInstance(projectConfig, eventProcessor: eventConfigManager);
                return instance;
            }
            catch (Exception e)
            {
                ServiceLocator.Current.TryGetExistingInstance(out ILogger epiErrorLogger);
                epiErrorLogger?.Log(Level.Error, "Optimizely initialization error", e);
            }

            return null;
        }

        public Optimizely Instance => OptimizelyInstance.Value;

        public bool IsConfigured
        {
            get
            {
                //var options = ServiceLocator.Current.GetInstance<ExperimentationOptions>();
                var options = ServiceLocator.Current.GetInstance<IOptions<FullStackSettings>>();
                return !string.IsNullOrEmpty(options.Value.SDKKey);
            }
        }
    }          
}

using EPiServer.Marketing.Testing.Dal;
using EPiServer.Marketing.Testing.Dal.DataAccess;
using EPiServer.ServiceLocation;
using Microsoft.Extensions.Options;
using OptimizelySDK;
using OptimizelySDK.Config;
using System;
using System.Configuration;

namespace EPiServer.Marketing.Testing.Web.FullStackSDK
{
    internal static class FSExpClient
    {
        internal static Lazy<Optimizely> Get = new Lazy<Optimizely>(() => GenerateClient());

        internal static Optimizely GenerateClient()
        {
            //var options = ServiceLocator.Current.GetInstance<IOptions<ExperimentationOptions>>();
            var options = ServiceLocator.Current.GetInstance<IOptions<FullStackSettings>>();
            var pollingInterval = TimeSpan.Parse("0:0:30");

            var configManager = new HttpProjectConfigManager
              .Builder()
              .WithPollingInterval(pollingInterval)
              .WithSdkKey(options.Value.SDKKey)
              .Build(true); // sync mode

            return OptimizelyFactory.NewDefaultInstance(configManager);
        }
    }
}

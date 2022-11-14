using System;
using EPiServer.ServiceLocation;
using System.Diagnostics.CodeAnalysis;
using EPiServer.Marketing.Testing.Core.DataClass.Enums;
using EPiServer.Marketing.Testing.Core.Manager;
using EPiServer.Marketing.Testing.Core.Messaging.Messages;
using System.Runtime.Caching;

namespace EPiServer.Marketing.Testing.Messaging
{
    /// <inheritdoc />
    public class TestingMessageHandler : ITestingMessageHandler
    {
        private ObjectCache _sessionCache = MemoryCache.Default;

        [ExcludeFromCodeCoverage]
        public TestingMessageHandler()
        {
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Handle(UpdateViewsMessage message)
        {
            var tm = ServiceLocator.Current.GetInstance<ITestManager>();
            tm.IncrementCount(message.TestId, message.ItemVersion, CountType.View, default(Guid), false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Handle(UpdateConversionsMessage message)
        {
            if (ProcessMessage(message))
            {
                var tm = ServiceLocator.Current.GetInstance<ITestManager>();
                tm.IncrementCount(message.TestId, message.ItemVersion, CountType.Conversion, message.KpiId, false);
            }
        }

        private bool ProcessMessage(UpdateConversionsMessage message)
        {
            bool retval = false;
            if (message.ClientIdentifier != null)
            {
                string key = message.KpiId.ToString() + message.ClientIdentifier;
                lock (_sessionCache)
                {
                    if (!_sessionCache.Contains(key))
                    {
                        CacheItemPolicy policy = new CacheItemPolicy();
                        policy.SlidingExpiration = TimeSpan.FromSeconds(30);
                        _sessionCache.Add(key, "", policy);
                        retval = true; // next time we return false so we dont process the message
                    }
                }
            }
            else
            {
                // not using a unique client Id
                retval = true;
            }

            return retval;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void Handle(AddKeyResultMessage message)
        {
            var tm = ServiceLocator.Current.GetInstance<ITestManager>();
            tm.SaveKpiResultData(message.TestId, message.ItemVersion, message.Result, message.Type, false);
        }
    }
}

using System;
using Moq;
using EPiServer.ServiceLocation;
using EPiServer.Marketing.Testing.Web.Repositories;
using System.Threading;
using EPiServer.Marketing.Testing.Core.DataClass;
using EPiServer.Marketing.Testing.Core.Messaging.Messages;
using EPiServer.Marketing.Testing.Messaging;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace EPiServer.Marketing.Testing.Test.Core.Messaging
{
    public class MessagingManagerTests
    {
        private static Mock<IMarketingTestingWebRepository> _testRepository;
        private static Mock<ITestingMessageHandler> _messageHandler;
        public IServiceCollection Services { get; } = new ServiceCollection();

        private MessagingManager GetUnitUnderTest()
        {
            _testRepository = new Mock<IMarketingTestingWebRepository>();
            _messageHandler = new Mock<ITestingMessageHandler>();

            Services.AddSingleton(_testRepository.Object);
            Services.AddSingleton(_messageHandler.Object);

            ServiceLocator.SetScopedServiceProvider(Services.BuildServiceProvider());

            return new MessagingManager();
        }

        [Fact]
        public void EmitUpdateViewsEmitsMessageAndCallsMessageHandler()
        {            
            var messageManager = GetUnitUnderTest();
            messageManager.EmitUpdateViews(Guid.Empty, 1);
        }

        [Fact]
        public void EmitUpdateConversionEmitsMessageAndCallsMessageHandler()
        {            
            var messageManager = GetUnitUnderTest();
            messageManager.EmitUpdateConversion(Guid.Empty, 1);
        }

        [Fact]
        public void EmitKpiResultDataEmitsMessageAndCallsMessageHandler()
        {            
            var messageManager = GetUnitUnderTest();
            messageManager.EmitKpiResultData(Guid.Empty, 1, new KeyFinancialResult(), 0);
        }
    }
}

﻿using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.Marketing.Testing.Core.Manager;
using Moq;
using System;
using System.Threading;
using Xunit;

namespace EPiServer.Marketing.Testing.Test.Core
{
    public class RemoteCacheSignalTests
    {
        private Mock<ISynchronizedObjectInstanceCache> _mockCache;

        public RemoteCacheSignalTests()
        {
            _mockCache = new Mock<ISynchronizedObjectInstanceCache>();
        }

        [Fact]
        public void RemoteCacheSignal_Set_IndicatesCacheValidity()
        {
            var signal = new RemoteCacheSignal(_mockCache.Object, Mock.Of<ILogger>(), "validity-key", TimeSpan.FromMilliseconds(int.MaxValue));
            signal.Set();

            _mockCache.Verify(c => c.Insert("validity-key", true, CacheEvictionPolicy.Empty), Times.Once());
        }

        [Fact]
        public void RemoteCacheSignal_Reset_IndicatesCacheInvalidity()
        {
            var signal = new RemoteCacheSignal(_mockCache.Object, Mock.Of<ILogger>(), "validity-key", TimeSpan.FromMilliseconds(int.MaxValue));
            signal.Reset();

            _mockCache.Verify(c => c.RemoveRemote("validity-key"), Times.Once());
        }

        [Fact]
        public void RemoteCacheSignal_Monitor_PollsForCacheValidity()
        {
            _mockCache.SetupSequence(c => c.Get("validity-key"))
                .Returns(true)
                .Returns(true)
                .Returns(true)
                .Returns((object)null)
                .Returns(true);

            var invalidationActionInvocations = 0;

            var signal = new RemoteCacheSignal(_mockCache.Object, Mock.Of<ILogger>(), "validity-key", TimeSpan.FromMilliseconds(75));
            signal.Monitor(
                () =>
                {
                    invalidationActionInvocations++;
                }
            );

            Thread.Sleep(500); // Allow sufficient time for monitor to poll            

            // Assert that validy has been polled a sufficiently acceptable number of times

            Assert.True(invalidationActionInvocations > 0);
            Assert.True(invalidationActionInvocations < 5, $"{invalidationActionInvocations} > 5");

            _mockCache.VerifyAll();
        }

        [Fact]
        public void RemoteCacheSignal_Monitor_ACallbackErrorDoesNotDestabilizePolling()
        {
            _mockCache.SetupSequence(c => c.Get("validity-key"))
                .Returns((object)null);

            var signal = new RemoteCacheSignal(_mockCache.Object, Mock.Of<ILogger>(), "validity-key", TimeSpan.FromMilliseconds(50));            
            signal.Monitor(
                () =>
                {
                    throw new Exception("Callback error!");
                }
            );

            Thread.Sleep(500); // Allow sufficient time for monitor to poll            

            // Assert that validy has been polled a sufficiently acceptable number of times

            _mockCache.VerifyAll();
        }
    }
}

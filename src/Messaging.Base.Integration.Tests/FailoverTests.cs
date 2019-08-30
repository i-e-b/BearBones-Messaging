using System;
using System.Diagnostics;
using System.Threading;
using BearBonesMessaging;
using BearBonesMessaging.Routing;
using Example.Types;
using Messaging.Base.Integration.Tests.Helpers;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Integration.Tests
{
    [TestFixture]
    public class FailoverTests {
        private MessagingBaseConfiguration _config;

        [SetUp]
        public void setup()
        {
            _config = new MessagingBaseConfiguration()
                .WithDefaults()
                .SetDefaultAcknowledgeTimeout(TimeSpan.FromSeconds(1))
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettings());

            
            var msg = _config.GetMessagingBase();

            msg.CreateDestination<IMetadataFile>("ReleaseTestQueue", Expires.Never);
            msg.SendMessage(new SuperMetadata());
        }

        [TearDown]
        public void teardown()
        {
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().RemoveRouting(n=>true);
            _config.Shutdown();
        }
        
        [Test]
        public void messages_are_released_after_configured_lock_duration ()
        {
            var msg = _config.GetMessagingBase();

            var pending_1 = msg.TryStartMessageRaw("ReleaseTestQueue");
            Assert.That(pending_1, Is.Not.Null, "Failed to pick up test message");
            
            var empty = msg.TryStartMessageRaw("ReleaseTestQueue");
            Assert.That(empty, Is.Null, "Test queue has too many messages");

            NonBlockingDelay(TimeSpan.FromSeconds(5));
            
            var pending_2 = msg.TryStartMessageRaw("ReleaseTestQueue");
            Assert.That(pending_2, Is.Not.Null, "Test message was not released");
        }

        [Test]
        public void cancelling_twice_throws_an_exception()
        {
            var msg = _config.GetMessagingBase();

            var pending = msg.TryStartMessageRaw("ReleaseTestQueue");
            Assert.That(pending, Is.Not.Null, "Failed to pick up test message");

            pending.Cancel();

            Assert.Throws<InvalidOperationException>(() => { pending.Cancel(); });
        }

        [Test]
        public void attempting_to_complete_after_a_cancel_throws_an_exception()
        {
            var msg = _config.GetMessagingBase();

            var pending = msg.TryStartMessageRaw("ReleaseTestQueue");
            Assert.That(pending, Is.Not.Null, "Failed to pick up test message");

            pending.Cancel();

            Assert.Throws<InvalidOperationException>(() => { pending.Finish(); });
        }

        [Test]
        public void attempting_to_complete_after_a_timeout_throws_an_exception()
        {
            var msg = _config.GetMessagingBase();

            var pending = msg.TryStartMessageRaw("ReleaseTestQueue");
            Assert.That(pending, Is.Not.Null, "Failed to pick up test message");

            NonBlockingDelay(TimeSpan.FromSeconds(5));

            Assert.Throws<InvalidOperationException>(() => { 
                pending.Finish();
            });
        }

        [Test]
        public void attempting_to_cancel_after_completing_throws_an_exception()
        {
            var msg = _config.GetMessagingBase();

            var pending = msg.TryStartMessageRaw("ReleaseTestQueue");
            Assert.That(pending, Is.Not.Null, "Failed to pick up test message");

            pending.Finish();

            Assert.Throws<InvalidOperationException>(() => { pending.Cancel(); });
        }





        private static void NonBlockingDelay(TimeSpan delay)
        {
            // This odd code is to prevent us blocking the cancellation with Thread.Sleep
            var sw = new Stopwatch();
            sw.Start();
            while (sw.Elapsed < delay)
            {
                Thread.Sleep(10);
            }

            sw.Stop();
        }
    }
}
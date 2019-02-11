using System;
using System.Linq;
using System.Threading;
using BearBonesMessaging;
using BearBonesMessaging.RabbitMq.RabbitMqManagement;
using BearBonesMessaging.Routing;
using Example.Types;
using Messaging.Base.Integration.Tests.Helpers;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Integration.Tests
{
    [TestFixture]
    public class DeadLetterHandling
    {
        SuperMetadata testMessage;
        IMessagingBase messaging;
        IRabbitMqQuery query;

        [OneTimeSetUp]
        public void A_configured_messaging_base()
        {
            var config = new MessagingBaseConfiguration()
                .WithDefaults()
                .WithContractRoot<IMsg>()
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettings())
                .WithApplicationGroupName("app-group-name");
               
            messaging = config.GetMessagingBase();
            query = ConfigurationHelpers.RabbitMqQueryWithConfigSettings();

            testMessage = new SuperMetadata
            {
                CorrelationId = Guid.NewGuid(),
                Contents = "This is my message",
                FilePath = @"C:\temp\",
                HashValue = 893476,
                MetadataName = "KeyValuePair"
            };
        }
        
        [Test]
        public void a_queue_with_no_time_restriction_can_receive_messages_after_any_delay ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination_unbound", Expires.Never);
            messaging.SendMessage(testMessage);

            Thread.Sleep(1000); // 'huge' delay

            var message = messaging.TryStartMessage<IMsg>("Test_Destination_unbound");

            Assert.That(message, Is.Not.Null);
            message.Finish();
        }

        [Test]
        public void a_queue_with_a_time_restriction_set_can_receive_messages_within_its_TTL ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination_TTL_500", Expires.AfterMilliseconds(500)); // Very short TTL. Real-world is more likely to be hours or days.
            messaging.SendMessage(testMessage);

            // Hopefully, no delay here

            var message = messaging.TryStartMessage<IMsg>("Test_Destination_TTL_500");

            Assert.That(message, Is.Not.Null);
            message.Finish();
        }

        [Test]
        public void if_a_time_restriction_has_been_set__an_expired_message_will_be_written_to_a_dead_letter_queue ()
        {
            var queueName = "Test_Destination_TTL_500";
            var deadQueueName = MessagingBaseConfiguration.DeadLetterPrefix + queueName;

            messaging.CreateDestination<IMsg>(queueName, Expires.AfterMilliseconds(500)); // Very short TTL. Real-world is more likely to be hours or days.
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().Purge(queueName);
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().Purge(deadQueueName);
            messaging.SendMessage(testMessage);

            
            Thread.Sleep(1000); // 'huge' delay


            var message = messaging.TryStartMessage<IMsg>(queueName);
            Assert.That(message, Is.Null, "Message should NOT be in the original queue, but it was");
            
            message = messaging.TryStartMessage<IMsg>(deadQueueName);
            Assert.That(message, Is.Not.Null, "Message should be in the DLQ, but I didn't find it");

            message.Finish(); // Should be able to complete the message to take it out of the DLQ
            message = messaging.TryStartMessage<IMsg>(deadQueueName);
            Assert.That(message, Is.Null, "Message should have been removed from DLQ, but it stayed");
        }

        [Test, Description("This covers poison messages that can't be acknowledged")]
        public void unfinished_messages_will_be_dead_lettered_even_if_they_have_been_started_up_in_the_past ()
        {
            var queueName = "Test_Destination_TTL_500";
            var deadQueueName = MessagingBaseConfiguration.DeadLetterPrefix + queueName;

            messaging.CreateDestination<IMsg>(queueName, Expires.AfterMilliseconds(500)); // Very short TTL. Real-world is more likely to be hours or days.
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().Purge(queueName);
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().Purge(deadQueueName);
            messaging.SendMessage(testMessage);

            var message = messaging.TryStartMessage<IMsg>(queueName);
            message.Cancel(); // equivalent of the process crashing etc.
            

            Thread.Sleep(1000); // 'huge' delay


            message = messaging.TryStartMessage<IMsg>(queueName);
            Assert.That(message, Is.Null, "Message should NOT be in the original queue, but it was");
            
            message = messaging.TryStartMessage<IMsg>(deadQueueName);
            Assert.That(message, Is.Not.Null, "Message should be in the DLQ, but I didn't find it");
            message.Finish();
        }
        
        [Test, Explicit("Slow test")]
        public void a_list_of_dead_letter_queues_can_be_read_with_their_message_counts ()
        {
            var queueName = "Test_Destination_TTL_500";
            var deadQueueName = MessagingBaseConfiguration.DeadLetterPrefix + queueName;

            messaging.CreateDestination<IMsg>(queueName, Expires.AfterMilliseconds(500)); // Very short TTL. Real-world is more likely to be hours or days.
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().Purge(queueName);
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().Purge(deadQueueName);
            messaging.SendMessage(testMessage);
            
            Thread.Sleep(2500); // enough delay to expire the message

            var message = messaging.TryStartMessage<IMsg>(queueName);
            Assert.That(message, Is.Null, "Message should NOT be in the original queue, but it was");
            
            Thread.Sleep(2500); // enough delay to read the meta-data

            var DLQs = query.GetDeadLetterStatus();

            Assert.That(DLQs.Length, Is.GreaterThan(0), "Expected to find a dead-letter-queue, but there are none");
            Assert.That(DLQs.Single(q=>q.name.Contains(queueName)).messages, Is.GreaterThan(0), "Expected to find a queue with the message I sent, but didn't find it");
        }

        [OneTimeTearDown]
        public void cleanup()
        {
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().RemoveRouting(n=>true);
        }
    }
}
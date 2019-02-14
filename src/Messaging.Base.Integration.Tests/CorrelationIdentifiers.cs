using System;
using System.Threading;
using BearBonesMessaging;
using BearBonesMessaging.Routing;
using BearBonesMessaging.Serialisation;
using Example.Types;
using Messaging.Base.Integration.Tests.Helpers;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Integration.Tests
{
    [TestFixture]
    public class CorrelationIdentifiers
    {
        IMessagingBase messaging;
        SuperMetadata testMessage;

        [OneTimeSetUp]
        public void A_configured_messaging_base()
        {
            messaging = new MessagingBaseConfiguration()
                .WithDefaults()
                .WithContractRoot<IMsg>()
                .WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettings())
                .WithApplicationGroupName("app-group-name")
                .GetMessagingBase();

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
        public void sending_a_message_without_giving_a_correlation_id_sets_a_new_guid ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);
            messaging.SendMessage(testMessage);
            messaging.SendMessage(testMessage);

            var message1 = messaging.TryStartMessage<IMsg>("Test_Destination");

            Assert.That(message1, Is.Not.Null, "did not get message 1");
            Assert.That(message1.Properties.CorrelationId, Is.Not.Null, "message 1 had an empty correlation id");

            var message2 = messaging.TryStartMessage<IMsg>("Test_Destination");
            
            Assert.That(message2, Is.Not.Null, "did not get message 2");
            Assert.That(message2.Properties.CorrelationId, Is.Not.Null, "message 2 had an empty correlation id");

            
            Assert.That(message2.Properties.CorrelationId, Is.Not.EqualTo(message1.Properties.CorrelationId),
                "messages had the same correlation ID");

            message1.Finish();
            message2.Finish();
        }

        [Test]
        public void sending_a_message_with_a_fixed_correlation_id_always_uses_that_value ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);

            var staticId = "HelloWorld";

            messaging.SendMessage(testMessage, staticId);
            messaging.SendMessage(testMessage, staticId);

            var message1 = messaging.TryStartMessage<IMsg>("Test_Destination");

            Assert.That(message1, Is.Not.Null, "did not get message 1");
            Assert.That(message1.Properties.CorrelationId, Is.EqualTo(staticId), "message 1 had an unexpected correlation ID");

            var message2 = messaging.TryStartMessage<IMsg>("Test_Destination");
            
            Assert.That(message2, Is.Not.Null, "did not get message 2");
            Assert.That(message2.Properties.CorrelationId, Is.EqualTo(staticId), "message 2 had an unexpected correlation ID");


            message1.Finish();
            message2.Finish();
        }
        

        [Test]
        public void sending_a_prepared_message_with_a_fixed_correlation_id_always_uses_that_value ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination", Expires.Never);

            var staticId = "OhMyCorrelation!";

            messaging.SendPrepared(new PreparedMessage("Example.Types.IMsg", "{\"CorrelationId\":null}", "Example.Types.IMsg") { CorrelationId = staticId });
            messaging.SendPrepared(new PreparedMessage("Example.Types.IMsg", "{\"CorrelationId\":null}", "Example.Types.IMsg") { CorrelationId = staticId });

            var message1 = messaging.TryStartMessage<IMsg>("Test_Destination");

            Assert.That(message1, Is.Not.Null, "did not get message 1");
            Assert.That(message1.Properties.CorrelationId, Is.EqualTo(staticId), "message 1 had an unexpected correlation ID");

            var message2 = messaging.TryStartMessage<IMsg>("Test_Destination");
            
            Assert.That(message2, Is.Not.Null, "did not get message 2");
            Assert.That(message2.Properties.CorrelationId, Is.EqualTo(staticId), "message 2 had an unexpected correlation ID");


            message1.Finish();
            message2.Finish();
        }
        
        [OneTimeTearDown]
        public void cleanup()
        {
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().RemoveRouting(n=>true);
        }
    }
}
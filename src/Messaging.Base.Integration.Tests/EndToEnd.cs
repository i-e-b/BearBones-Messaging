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
	public class EndToEnd_WithInferredContracts
	{
		SuperMetadata testMessage;
		IMessagingBase messaging;

		[OneTimeSetUp]
		public void A_configured_messaging_base()
		{
			var config = new MessagingBaseConfiguration()
				.WithDefaults()
                //.WithContractRoot<...some type...>()  // by omitting this, we force the system to discover types itself.
				.WithConnection(ConfigurationHelpers.RabbitMqConnectionWithConfigSettings());

			messaging = config.Get<IMessagingBase>();

			testMessage = new SuperMetadata
			{
				CorrelationId = Guid.NewGuid(),
				Contents = "These are my ||\"\\' ' contents: ⰊⰄⰷἚ𐰕𐰑ꔘⶤعبػػ↴↳↲↰",
				FilePath = @"C:\temp\",
				HashValue = 893476,
				MetadataName = "KeyValuePair"
			};
		}

		[Test]
		public void Should_be_able_to_send_and_receive_messages_by_interface_type_and_destination_name()
		{
			messaging.CreateDestination<IMsg>("Test_Destination");
			messaging.SendMessage(testMessage);

			var finalObject = (IMetadataFile)messaging.GetMessage<IMsg>("Test_Destination");

			Assert.That(finalObject, Is.Not.Null);
			Assert.That(finalObject.CorrelationId, Is.EqualTo(testMessage.CorrelationId));
			Assert.That(finalObject.Contents, Is.EqualTo(testMessage.Contents));
			Assert.That(finalObject.FilePath, Is.EqualTo(testMessage.FilePath));
			Assert.That(finalObject.HashValue, Is.EqualTo(testMessage.HashValue));
			Assert.That(finalObject.MetadataName, Is.EqualTo(testMessage.MetadataName));
			Assert.That(finalObject.Equals(testMessage), Is.False);
		}

        [Test]
        public void type_headers_survive_a_send_and_receive_round_trip ()
        {
            messaging.CreateDestination<IMsg>("Test_Destination");
            messaging.SendMessage(testMessage);

            var message = messaging.TryStartMessage<IMsg>("Test_Destination");

            Assert.That(message, Is.Not.Null);
            Console.WriteLine(message.Properties.OriginalType);
            Assert.That(message.Properties.OriginalType, Is.EqualTo(
                "Example.Types.IMetadataFile;" +
                "Example.Types.IFile;" +
                "Example.Types.IHash;" +
                "Example.Types.IPath;" +
                "Example.Types.IMsg"));
        }
		

		[Test]
		public void Should_be_able_to_send_and_receive_messages_using_prepare_message_intermediates()
		{
			messaging.CreateDestination<IMsg>("Test_Destination");
			byte[] raw = messaging.PrepareForSend(testMessage).ToBytes();

			messaging.SendPrepared(PreparedMessage.FromBytes(raw));

			var finalObject = (IMetadataFile)messaging.GetMessage<IMsg>("Test_Destination");

			Assert.That(finalObject, Is.Not.Null);
			Assert.That(finalObject.CorrelationId, Is.EqualTo(testMessage.CorrelationId));
			Assert.That(finalObject.Contents, Is.EqualTo(testMessage.Contents));
			Assert.That(finalObject.FilePath, Is.EqualTo(testMessage.FilePath));
			Assert.That(finalObject.HashValue, Is.EqualTo(testMessage.HashValue));
			Assert.That(finalObject.MetadataName, Is.EqualTo(testMessage.MetadataName));
			Assert.That(finalObject.Equals(testMessage), Is.False);
		}

		[Test]
		public void Should_be_able_to_send_and_receive_messages_by_destination_name_and_get_correct_type()
		{
			messaging.CreateDestination<IMsg>("Test_Destination");
			messaging.SendMessage(testMessage);

			var finalObject = (IMetadataFile)messaging.GetMessage<IMsg>("Test_Destination");

			Assert.That(finalObject, Is.Not.Null);
			Assert.That(finalObject.CorrelationId, Is.EqualTo(testMessage.CorrelationId));
			Assert.That(finalObject.Contents, Is.EqualTo(testMessage.Contents));
			Assert.That(finalObject.FilePath, Is.EqualTo(testMessage.FilePath));
			Assert.That(finalObject.HashValue, Is.EqualTo(testMessage.HashValue));
			Assert.That(finalObject.MetadataName, Is.EqualTo(testMessage.MetadataName));
			Assert.That(finalObject.Equals(testMessage), Is.False);
		}

		[Test]
		public void should_be_able_to_get_cancel_get_again_and_finish_messages()
		{
			messaging.CreateDestination<IMsg>("Test_Destination");
			messaging.SendMessage(testMessage);

			var pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
			var pending_2 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_1, Is.Not.Null);
			Assert.That(pending_2, Is.Null);

			pending_1.Cancel();
			pending_2 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_2, Is.Not.Null);

			pending_2.Finish();
			var pending_3 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_3, Is.Null);

			var finalObject = (IMetadataFile)pending_2.Message;
			Assert.That(finalObject.CorrelationId, Is.EqualTo(testMessage.CorrelationId));
			Assert.That(finalObject.Contents, Is.EqualTo(testMessage.Contents));
			Assert.That(finalObject.FilePath, Is.EqualTo(testMessage.FilePath));
			Assert.That(finalObject.HashValue, Is.EqualTo(testMessage.HashValue));
			Assert.That(finalObject.MetadataName, Is.EqualTo(testMessage.MetadataName));
			Assert.That(finalObject.Equals(testMessage), Is.False);
		}

		[Test]
		public void should_protect_from_cancelling_the_same_message_twice ()
		{
			messaging.CreateDestination<IMsg>("Test_Destination");
			messaging.SendMessage(testMessage);

			var pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_1, Is.Not.Null);

			pending_1.Cancel();
			pending_1.Cancel();

			pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_1, Is.Not.Null);
			pending_1.Finish();

			Assert.Pass();
		}

		[Test]
		public void should_protect_from_finishing_the_same_message_twice ()
		{
			messaging.CreateDestination<IMsg>("Test_Destination");
			messaging.SendMessage(testMessage);

			var pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_1, Is.Not.Null);

			pending_1.Finish();
			pending_1.Finish();

			messaging.SendMessage(testMessage);

			pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_1, Is.Not.Null);
			pending_1.Finish();

			Assert.Pass();
		}

		[Test]
		public void should_protect_from_cancelling_then_finishing_a_message ()
		{
			messaging.CreateDestination<IMsg>("Test_Destination");
			messaging.SendMessage(testMessage);

			var pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_1, Is.Not.Null);

			pending_1.Cancel();
			pending_1.Finish();

			pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_1, Is.Not.Null);
			pending_1.Finish();

			Assert.Pass();
		}

		[Test]
		public void should_protect_from_finishing_then_cancelling_a_message ()
		{
			messaging.CreateDestination<IMsg>("Test_Destination");
			messaging.SendMessage(testMessage);

			var pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_1, Is.Not.Null);

			pending_1.Finish();
			pending_1.Cancel();

			messaging.SendMessage(testMessage);

			pending_1 = messaging.TryStartMessage<IMsg>("Test_Destination");
			Assert.That(pending_1, Is.Not.Null);
			pending_1.Finish();

			Assert.Pass();
		}

		[Test]
		public void Should_be_able_to_send_and_receive_1000_messages_in_a_minute()
		{
			messaging.CreateDestination<IMsg>("Test_Destination");

			int sent = 1000;
			int received = 0;
			var start = DateTime.Now;
			for (int i = 0; i < sent; i++)
			{
				messaging.SendMessage(testMessage);
			}

			Console.WriteLine("Sending took " + ((DateTime.Now) - start));
			var startGet = DateTime.Now;

			while (messaging.GetMessage<IMsg>("Test_Destination") != null)
			{
				Interlocked.Increment(ref received);
			}
			Console.WriteLine("Receiving took " + ((DateTime.Now) - startGet));

			var time = (DateTime.Now) - start;
			Assert.That(received, Is.EqualTo(sent));
			Assert.That(time.TotalSeconds, Is.LessThanOrEqualTo(60));
		}

		[OneTimeTearDown]
		public void cleanup()
		{
            MessagingBaseConfiguration.LastConfiguration.Get<IMessageRouter>().RemoveRouting(n=>true);
		}
	}
}

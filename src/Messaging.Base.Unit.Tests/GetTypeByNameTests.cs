using System;
using BearBonesMessaging;
using BearBonesMessaging.Routing;
using BearBonesMessaging.Serialisation;
using Example.Types;
using NSubstitute;
using NUnit.Framework;

namespace Messaging.Base.Unit.Tests
{
	[TestFixture]
	public class GetTypeByNameTests
	{
		ITypeRouter typeRouter;
		IMessageRouter messageRouter;
		IMessageSerialiser serialiser;
		IMessagingBase messaging;

		[SetUp]
		public void When_setting_up_a_named_destination ()
		{
			typeRouter = Substitute.For<ITypeRouter>();
			messageRouter = Substitute.For<IMessageRouter>();
			serialiser = Substitute.For<IMessageSerialiser>();

			messaging = new MessagingBase(typeRouter, messageRouter, serialiser);
			messaging.ResetCaches();
			messaging.GetMessage<IMetadataFile>("MyServiceDestination");
		}

		[Test]
		public void Should_get_message_string_from_endpoint ()
		{
			messageRouter.Received().GetAndFinish("MyServiceDestination");
		}

		[Test]
		public void When_there_is_no_message_should_return_null ()
		{
			messageRouter.GetAndFinish("MyServiceDestination").Returns((string)null);
			var result = messaging.GetMessage<IMetadataFile>("MyServiceDestination");

			Assert.That(result, Is.Null);
		}

		[Test]
		public void When_a_message_is_available_should_deserialise_and_return_requested_type ()
		{
			messageRouter.GetAndFinish("MyServiceDestination").Returns("");
			serialiser.DeserialiseByStack("").Returns(new SuperMetadata());
			var result = messaging.GetMessage<IMetadataFile>("MyServiceDestination");

			Assert.That(result, Is.InstanceOf<IMetadataFile>());
		}

		[Test]
		public void When_a_message_is_available_should_deserialise_and_return_requested_type_using_old_message_format ()
		{
			messageRouter.GetAndFinish("MyServiceDestination").Returns("");
			serialiser.DeserialiseByStack("").Returns(c => { throw new Exception(); });
			serialiser.Deserialise<IMetadataFile>("").Returns(new SuperMetadata());
			var result = messaging.GetMessage<IMetadataFile>("MyServiceDestination");

			Assert.That(result, Is.InstanceOf<IMetadataFile>());
		}
	}
}

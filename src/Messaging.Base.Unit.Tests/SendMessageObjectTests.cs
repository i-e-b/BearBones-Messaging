using System;
using BearBonesMessaging;
using BearBonesMessaging.Routing;
using BearBonesMessaging.Serialisation;
using Example.Types;
using NSubstitute;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Unit.Tests
{
	[TestFixture]
	public class SendMessageObjectTests
	{
		ITypeRouter typeRouter;
		IMessageRouter messageRouter;
		IMessageSerialiser serialiser;
		SuperMetadata metadataMessage;
		object badMessage;
		IMessagingBase messaging;
        string typeDescription;
        const string serialisedObject = "serialised object";

		[SetUp]
		public void When_setting_up_a_named_destination ()
		{
			metadataMessage = new SuperMetadata();

			badMessage = new {Who="What"};
			typeRouter = Substitute.For<ITypeRouter>();
			messageRouter = Substitute.For<IMessageRouter>();
			serialiser = Substitute.For<IMessageSerialiser>();
			serialiser.Serialise(metadataMessage, out typeDescription).Returns(serialisedObject);

			messaging = new MessagingBase(typeRouter, messageRouter, serialiser, "test");
			messaging.ResetCaches();
			messaging.SendMessage(metadataMessage);
		}

		[Test]
		public void Should_setup_type_message_type_if_not_already_in_place()
		{
			messaging.SendMessage(metadataMessage);
			typeRouter.Received().BuildRoutes(typeof(IMetadataFile));
		}

		[Test]
		public void Should_serialise_the_message()
		{
			serialiser.Received().Serialise(metadataMessage, out _);
		}

		[Test]
		public void Should_throw_exception_when_sending_message_without_exactly_one_parent_interface()
		{
			var ex = Assert.Throws<ArgumentException>(() => messaging.SendMessage(badMessage));
			Assert.That(ex.Message, Contains.Substring("Messages must directly implement exactly one interface"));
		}
	}

}

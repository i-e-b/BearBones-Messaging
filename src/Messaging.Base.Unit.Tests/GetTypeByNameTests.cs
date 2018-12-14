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

			messaging = new MessagingBase(typeRouter, messageRouter, serialiser, "test-group");
			messaging.ResetCaches();
			messaging.GetMessage<IMetadataFile>("MyServiceDestination");
		}

		[Test]
		public void Should_get_message_string_from_endpoint ()
		{
			messageRouter.Received().GetAndFinish("MyServiceDestination", out _);
		}

		[Test]
		public void When_there_is_no_message_should_return_null ()
		{
			messageRouter.GetAndFinish("MyServiceDestination", out _).Returns((string)null);
			var result = messaging.GetMessage<IMetadataFile>("MyServiceDestination");

			Assert.That(result, Is.Null);
		}
	}
}

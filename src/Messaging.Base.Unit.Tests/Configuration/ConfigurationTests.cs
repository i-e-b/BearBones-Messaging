using BearBonesMessaging;
using BearBonesMessaging.RabbitMq;
using BearBonesMessaging.Routing;
using BearBonesMessaging.Serialisation;
using NSubstitute;
using NUnit.Framework;
using StructureMap;

namespace Messaging.Base.Unit.Tests.Configuration
{
	[TestFixture]
	public class ConfigurationTests
	{
		[SetUp]
		public void A_configured_messaging_base ()
		{
			new MessagingBaseConfiguration().WithDefaults().WithConnection(Substitute.For<IRabbitMqConnection>());
		}

		[Test]
		public void Should_have_message_serialiser ()
		{
			Assert.That(
				ObjectFactory.GetInstance<IMessageSerialiser>(),
				Is.InstanceOf<MessageSerialiser>());
		}

		[Test]
		public void Should_have_rabbitmq_message_router ()
		{
			Assert.That(
				ObjectFactory.GetInstance<IMessageRouter>(),
				Is.InstanceOf<RabbitRouter>());
		}

		[Test]
		public void Should_have_type_structure_router ()
		{
			Assert.That(
				ObjectFactory.GetInstance<ITypeRouter>(),
				Is.InstanceOf<TypeRouter>());
		}

		[Test]
		public void Should_have_messaging_base ()
		{
			Assert.That(
				ObjectFactory.GetInstance<IMessagingBase>(),
				Is.InstanceOf<MessagingBase>());
		}

		[Test]
		public void Should_have_long_term_connection_as_singleton ()
		{
			var instance1 = ObjectFactory.GetInstance<IChannelAction>();
			var instance2 = ObjectFactory.GetInstance<IChannelAction>();

			Assert.That(instance1, Is.InstanceOf<LongTermRabbitConnection>());
			Assert.That(instance1, Is.SameAs(instance2));
		}
	}
}

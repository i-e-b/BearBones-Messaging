using BearBonesMessaging;
using BearBonesMessaging.RabbitMq;
using BearBonesMessaging.Routing;
using BearBonesMessaging.Serialisation;
using NSubstitute;
using NUnit.Framework;

namespace Messaging.Base.Unit.Tests.Configuration
{
	[TestFixture]
	public class ConfigurationTests
	{
        private MessagingBaseConfiguration _config;

        [SetUp]
		public void A_configured_messaging_base ()
		{
			_config = new MessagingBaseConfiguration().WithDefaults().WithConnection(Substitute.For<IRabbitMqConnection>());
		}

		[Test]
		public void Should_have_message_serialiser ()
		{
			Assert.That(
                _config.Get<IMessageSerialiser>(),
				Is.InstanceOf<MessageSerialiser>());
		}

		[Test]
		public void Should_have_rabbitmq_message_router ()
		{
			Assert.That(
                _config.Get<IMessageRouter>(),
				Is.InstanceOf<RabbitRouter>());
		}

		[Test]
		public void Should_have_type_structure_router ()
		{
			Assert.That(
                _config.Get<ITypeRouter>(),
				Is.InstanceOf<TypeRouter>());
		}

		[Test]
		public void Should_have_messaging_base ()
		{
			Assert.That(
                _config.Get<IMessagingBase>(),
				Is.InstanceOf<MessagingBase>());
		}

		[Test]
		public void Should_have_long_term_connection_as_singleton ()
		{
			var instance1 = _config.Get<IChannelAction>();
			var instance2 = _config.Get<IChannelAction>();

			Assert.That(instance1, Is.InstanceOf<LongTermRabbitConnection>());
			Assert.That(instance1, Is.SameAs(instance2));
		}
	}
}

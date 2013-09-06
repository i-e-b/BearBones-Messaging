using System;
using System.Configuration;
using NUnit.Framework;
using SevenDigital.Messaging.Base;
using SevenDigital.Messaging.Base.RabbitMq;
using StructureMap;

namespace Messaging.Base.Unit.Tests.Configuration
{
	[TestFixture]
	[Obsolete("To be removed with MessagingBaseConfiguration().WithConnectionFromAppConfig()")]
    public class RabbitConnectionConfigurationTests
    {
		IRabbitMqConnection connection;

		[SetUp]
		public void When_configuring_the_messaging_base_with_app_config_settings ()
		{
			new MessagingBaseConfiguration().WithConnectionFromAppConfig();
			connection = ObjectFactory.GetInstance<IRabbitMqConnection>();
		}

		[Test]
		public void Should_have_rabbit_mq_connection_in_configuration()
		{
			Assert.That(connection, Is.InstanceOf<RabbitMqConnection>());
		}

		[Test]
		public void Rabbit_mq_connection_should_have_host_uri_from_app_config ()
		{
			var host = ConfigurationManager.AppSettings["Messaging.Host"].SubstringBefore('/');
			Assert.That(connection.Host, Is.EqualTo(host));
		}
		
		[Test]
		public void Rabbit_mq_connection_should_have_virtual_host_from_app_config ()
		{
			var vhost = ConfigurationManager.AppSettings["Messaging.Host"].SubstringAfterLast('/');
			if (string.IsNullOrEmpty(vhost)) vhost = "/";

			Assert.That(connection.VirtualHost, Is.EqualTo(vhost));
		}
    }
}

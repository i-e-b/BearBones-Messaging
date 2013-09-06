using System;
using System.Configuration;
using NUnit.Framework;
using SevenDigital.Messaging.Base;
using SevenDigital.Messaging.Base.RabbitMq.RabbitMqManagement;
using StructureMap;

namespace Messaging.Base.Unit.Tests.Configuration
{
	[TestFixture]
	[Obsolete("To be removed along with MessagingBaseConfiguration().WithRabbitManagementFromAppConfig()")]
	public class RabbitQueryConfigurationTests
	{
		IRabbitMqQuery query;
		string host;
		string vhost;

		[SetUp]
		public void When_configuring_the_messaging_base_with_app_config_settings()
		{
			new MessagingBaseConfiguration().WithRabbitManagementFromAppConfig();
			query = ObjectFactory.GetInstance<IRabbitMqQuery>();
			host = ConfigurationManager.AppSettings["Messaging.Host"].SubstringBefore('/');
			vhost = ConfigurationManager.AppSettings["Messaging.Host"].SubstringAfterLast('/');
		}

		[Test]
		public void Should_have_rabbit_mq_query_in_configuration()
		{
			Assert.That(query, Is.InstanceOf<RabbitMqQuery>());
		}

		[Test]
		public void Should_have_host_uri_from_app_config()
		{
			Assert.That(query.HostUri.ToString(), Is.StringStarting("http://" + host + ":"));
		}

		[Test]
		public void Should_have_network_credentials_from_app_config()
		{
			Assert.That(query.Credentials.Password, Is.EqualTo(ConfigurationManager.AppSettings["ApiPassword"]));
			Assert.That(query.Credentials.UserName, Is.EqualTo(ConfigurationManager.AppSettings["ApiUsername"]));
		}

		[Test]
		public void Should_have_virtual_host_from_app_config()
		{
			Assert.That(query.VirtualHost, Is.EqualTo("/" + vhost));
		}
	}
}

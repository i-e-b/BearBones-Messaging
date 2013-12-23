﻿using System;
using SevenDigital.Messaging.Base.RabbitMq;
using SevenDigital.Messaging.Base.RabbitMq.RabbitMqManagement;
using SevenDigital.Messaging.Base.Routing;
using SevenDigital.Messaging.Base.Serialisation;
using StructureMap;

namespace SevenDigital.Messaging.Base
{
	/// <summary>
	/// Configuration options for messaging base
	/// </summary>
	public class MessagingBaseConfiguration
	{
		IRabbitMqConnection configuredConnection;

		/// <summary>
		/// Configure all default mappings in structure map.
		/// You must also call a `WithConnection...` method to get a
		/// working system.
		/// </summary>
		public MessagingBaseConfiguration WithDefaults()
		{
			ObjectFactory.Configure(map =>
			{
				map.For<IMessageSerialiser>().Use<MessageSerialiser>();
				map.For<ITypeRouter>().Use<TypeRouter>();
				map.For<IMessagingBase>().Use<MessagingBase>();

				map.For<IMessageRouter>().Singleton().Use<RabbitRouter>();
				map.For<IChannelAction>().Singleton().Use<LongTermRabbitConnection>();
			});

			return this;
		}

		/// <summary>
		/// Configure long and short term connections to use the specified connection details
		/// </summary>
		public MessagingBaseConfiguration WithConnection(IRabbitMqConnection connection)
		{
			configuredConnection = connection;
			ObjectFactory.Configure(map => map.For<IRabbitMqConnection>().Use(() => configuredConnection));
			return this;
		}

		/// <summary>
		/// </summary>
		{

		}

		const int port = 55672; // before RMQ 3; 3 redirects, so we use this for compatibility for now.
		//const int port = 15672; // RMQ 3+

		/// <summary>
		/// Use a specific rabbit management node
		/// </summary>
		public MessagingBaseConfiguration WithRabbitManagement(string host, string username, string password, string vhost)
		{
			ObjectFactory.Configure(map => map.For<IRabbitMqQuery>().Use(() =>
				new RabbitMqQuery("http://" + host + ":" + port, username, password, vhost)));
			return this;
		}
	}
}

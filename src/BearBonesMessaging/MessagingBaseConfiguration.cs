using System;
using BearBonesMessaging.RabbitMq;
using BearBonesMessaging.RabbitMq.RabbitMqManagement;
using BearBonesMessaging.Routing;
using BearBonesMessaging.Serialisation;
using StructureMap;

namespace BearBonesMessaging
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
            // ReSharper disable PossibleNullReferenceException
			ObjectFactory.Configure(map =>
			{
                if (map == null) throw new Exception("StructureMap configuration failure");

				map.For<IMessageSerialiser>().Use<MessageSerialiser>();
				map.For<ITypeRouter>().Use<TypeRouter>();
				map.For<IMessagingBase>().Use<MessagingBase>();

				map.For<IMessageRouter>().Singleton().Use<RabbitRouter>();
				map.For<IChannelAction>().Singleton().Use<LongTermRabbitConnection>();
			});
            // ReSharper restore PossibleNullReferenceException

			return this;
		}

		/// <summary>
		/// Configure long and short term connections to use the specified connection details
		/// </summary>
		public MessagingBaseConfiguration WithConnection(IRabbitMqConnection connection)
		{
			configuredConnection = connection;
            // ReSharper disable PossibleNullReferenceException
			ObjectFactory.Configure(map => map.For<IRabbitMqConnection>().Use(() => configuredConnection));
            // ReSharper restore PossibleNullReferenceException
			return this;
		}

		/// <summary>
		/// Use a specific rabbit management node
		/// </summary>
		[Obsolete("Please use WithRabbitManagement(string host, int port, string username, string password, string vhost) instead.")]
		public MessagingBaseConfiguration WithRabbitManagement(string host, string username, string password, string vhost)
		{
			const int port = 55672; // before RMQ 3; 3 redirects, so we use this for compatibility for now.
			//const int port = 15672; // RMQ 3+

			return WithRabbitManagement(host, port, username, password, vhost);
		}

		/// <summary>
		/// Use a specific rabbit management node
		/// </summary>
		public MessagingBaseConfiguration WithRabbitManagement(string host, int port, string username, string password, string vhost)
		{
            // ReSharper disable PossibleNullReferenceException
            ObjectFactory.Configure(map => map.For<IRabbitMqQuery>().Use(() =>
                new RabbitMqQuery("http://" + host + ":" + port, username, password, vhost)));
            // ReSharper restore PossibleNullReferenceException
            return this;
		}
	}
}

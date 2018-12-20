using System;
using RabbitMQ.Client;

namespace BearBonesMessaging.RabbitMq
{
	/// <summary>
	/// Default short-term connection.
	/// This class opens and closes a connection per request, and
	/// should not be used for polling.
	/// </summary>
	public class RabbitMqConnection : IRabbitMqConnection
	{
		/// <summary>
		/// Rabbit MQ Cluster host name uri fragment
		/// </summary>
		public string Host { get; }
        
        /// <summary>
        /// Rabbit MQ Cluster TCP/IP port
        /// </summary>
        public int Port { get; }

        /// <summary>
        /// User account used for connection
        /// </summary>
        public string UserName { get; }

        /// <summary>
        /// Account password used for connection
        /// </summary>
        public string Password { get; }

        /// <summary>
		/// Target virtual host
		/// </summary>
		public string VirtualHost { get; }

		/// <summary>
		/// Prepare a connection provider
		/// </summary>
		public RabbitMqConnection(string hostUri, int port, string userName, string password, string virtualHost)
		{
			Host = hostUri;
            Port = port;
            UserName = userName;
            Password = password;
            VirtualHost = virtualHost;
		}

		/// <summary>
		/// Return a connection factory.
		/// Use this to connect to the RMQ cluster.
		/// ALWAYS dispose your connections.
		/// </summary>
		public ConnectionFactory ConfigureConnectionFactory()
		{
			var fact = new ConnectionFactory
				{
					Protocol = Protocols.AMQP_0_9_1,
					HostName = Host,
					VirtualHost = VirtualHost,
					RequestedHeartbeat = 60,
                    UserName = UserName ?? "guest",
                    Password = Password ?? "guest"
				};
            if (Port > 0) fact.Port = Port;
            return fact;
		}

		/// <summary>
		/// Perform an action against the RMQ cluster, returning no data
		/// </summary>
		public void WithChannel(Action<IModel> actions)
		{
            if (actions == null) return;

			var factory = ConfigureConnectionFactory();
			using (var conn = factory?.CreateConnection())
			using (var channel = conn?.CreateModel())
			{
                if (channel == null) throw new Exception("Failed to open a channel");

				actions(channel);
				if (channel.IsOpen) channel.Close();
				if (conn.IsOpen) conn.Close();
			}
		}

		/// <summary>
		/// Perform an action against the RMQ cluster, returning data
		/// </summary>
		public T GetWithChannel<T>(Func<IModel, T> actions)
		{
            if (actions == null) throw new ArgumentNullException(nameof(actions));

			var factory = ConfigureConnectionFactory();
			using (var conn = factory?.CreateConnection())
			using (var channel = conn?.CreateModel())
			{
                if (channel == null) throw new Exception("Failed to open a channel");

				var result = actions(channel);
				if (channel.IsOpen) channel.Close();
				if (conn.IsOpen) conn.Close();
				return result;
			}
		}

		/// <summary>
		/// No action.
		/// </summary>
		public void Dispose()
		{
		}
	}
}

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
		public string Host { get; private set; }

		/// <summary>
		/// Target virtual host
		/// </summary>
		public string VirtualHost { get; private set; }

		/// <summary>
		/// Prepare a connection provider
		/// </summary>
		public RabbitMqConnection(string hostUri, string virtualHost = "/")
		{
			Host = hostUri;
			VirtualHost = virtualHost;
		}

		/// <summary>
		/// Return a connection factory.
		/// Use this to connect to the RMQ cluster.
		/// ALWAYS dispose your connections.
		/// </summary>
		public ConnectionFactory ConnectionFactory()
		{
			return new ConnectionFactory
				{
					Protocol = Protocols.AMQP_0_9_1,
					HostName = Host,
					VirtualHost = VirtualHost,
					RequestedHeartbeat = 60
				};
		}

		/// <summary>
		/// Perform an action against the RMQ cluster, returning no data
		/// </summary>
		public void WithChannel(Action<IModel> actions)
		{
            if (actions == null) return;

			var factory = ConnectionFactory();
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

			var factory = ConnectionFactory();
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

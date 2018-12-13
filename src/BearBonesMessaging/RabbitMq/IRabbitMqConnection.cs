using RabbitMQ.Client;

namespace BearBonesMessaging.RabbitMq
{
	/// <summary>
	/// Connection provider for RabbitMq
	/// </summary>
	public interface IRabbitMqConnection:IChannelAction
	{
		/// <summary>
		/// Rabbit MQ Cluster host name uri fragment
		/// </summary>
		string Host { get; }

		/// <summary>
		/// Target virtual host
		/// </summary>
		string VirtualHost { get; }

		/// <summary>
		/// Return a connection factory.
		/// Use this to connect to the RMQ cluster.
		/// ALWAYS dispose your connections.
		/// </summary>
		ConnectionFactory ConnectionFactory();
	}
}
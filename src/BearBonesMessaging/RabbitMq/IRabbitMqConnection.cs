using RabbitMQ.Client;

namespace BearBonesMessaging.RabbitMq
{
    /// <summary>
    /// Connection provider for RabbitMq
    /// </summary>
    public interface IRabbitMqConnection : IRabbitServerTarget, IChannelAction
    {
        /// <summary>
        /// Return a connection factory.
        /// Use this to connect to the RMQ cluster.
        /// ALWAYS dispose your connections.
        /// </summary>
        ConnectionFactory ConfigureConnectionFactory();
	}

    /// <summary>
    /// Server connection string details
    /// </summary>
    public interface IRabbitServerTarget
    {
        /// <summary>
        /// Rabbit MQ Cluster host name uri fragment
        /// </summary>
        string Host { get; }
        
        /// <summary>
        /// Rabbit MQ Cluster TCP/IP port
        /// </summary>
        int Port { get; }

        /// <summary>
        /// Target virtual host
        /// </summary>
        string VirtualHost { get; }
        
        /// <summary>
        /// User account used for connection
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// Account password used for connection
        /// </summary>
        string Password { get; }
    }
}
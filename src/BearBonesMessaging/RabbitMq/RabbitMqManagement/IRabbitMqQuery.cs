using System;
using System.Net;

namespace BearBonesMessaging.RabbitMq.RabbitMqManagement
{
	/// <summary>
	/// Interface to the RabbitMQ management API
	/// </summary>
	public interface IRabbitMqQuery
	{
		/// <summary>
		/// RabbitMQ cluster's management uri
		/// </summary>
		Uri HostUri { get; }

		/// <summary>
		/// Virtual host to use, where applicable
		/// </summary>
		string VirtualHost { get; }

		/// <summary>
		/// Log-in credentials for RabbitMQ management API
		/// </summary>
		NetworkCredential ManagementCredentials { get; }

		/// <summary>
		/// List all Destination queue in the given virtual host.
		/// Equivalent to /api/queues/vhost
		/// </summary>
		IRMQueue[] ListDestinations();

		/// <summary>
		/// List all nodes attached to the cluster.
		/// Equivalent to /api/nodes
		/// </summary>
		IRMNode[] ListNodes();

		/// <summary>
		/// List all Source exchanges in the given virtual host
		/// Equivalent to /api/exchanges/vhost
		/// </summary>
		IRMExchange[] ListSources();
	}
}
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

        /// <summary>
        /// List all users in the system
        /// </summary>
        IRMUser[] ListUsers();

        /// <summary>
        /// List all users in the system
        /// </summary>
        IRMUser TryGetUser(string userName);

        /// <summary>
        /// Ensure that a user exists for the given app group, and return the authentication details for it.
        /// </summary>
        NetworkCredential GetLimitedUser(string appGroup);

        /// <summary>
        /// Delete a user from the system. This will refuse to delete administrator accounts.
        /// </summary>
        bool DeleteUser(NetworkCredential credentials);

        /// <summary>
        /// List all dead-letter-queues on the system
        /// </summary>
        IRMQueue[] GetDeadLetterStatus();
    }
}
using System;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using ServiceStack.Text;

namespace BearBonesMessaging.RabbitMq.RabbitMqManagement
{
	/// <summary>
	/// Deafult RMQ query
	/// </summary>
	public class RabbitMqQuery : IRabbitMqQuery
	{
		/// <summary>
		/// RabbitMQ cluster's management uri
		/// </summary>
		public Uri HostUri { get; private set; }

		/// <summary>
		/// Virtual host to use, where applicable
		/// </summary>
		public string VirtualHost { get; private set; }

		/// <summary>
		/// Log-in credentials for RabbitMQ management API
		/// </summary>
		public NetworkCredential Credentials { get; private set; }

		/// <summary>
		/// Use `MessagingBaseConfiguration` and get an IRabbitMqQuery from ObjectFactory.
		/// </summary>
		public RabbitMqQuery(Uri managementApiHost, NetworkCredential credentials)
		{
			HostUri = managementApiHost;
			Credentials = credentials;
		}
		
		/// <summary>
		/// Use `MessagingBaseConfiguration` and get an IRabbitMqQuery from ObjectFactory.
		/// </summary>
		public RabbitMqQuery(string hostUri, string username, string password, string virtualHost = "/")
			: this(
                new Uri(hostUri ?? throw new ArgumentNullException(nameof(hostUri))),
                new NetworkCredential(username, password)
            )
        {
            if (virtualHost == null) virtualHost = "/";
			VirtualHost = virtualHost.StartsWith("/") ? virtualHost : ("/" + virtualHost);
		}

		/// <summary>
		/// List all Destination queue in the given virtual host.
		/// Equivalent to /api/queues/vhost
		/// </summary>
		public RMQueue[] ListDestinations()
		{
			return JsonSerializer.DeserializeFromString<RMQueue[]>(Get("/api/queues" + VirtualHost));
		}

		/// <summary>
		/// List all nodes attached to the cluster.
		/// Equivalent to /api/nodes
		/// </summary>
		public RMNode[] ListNodes()
		{
			return JsonSerializer.DeserializeFromString<RMNode[]>(Get("/api/nodes"));
		}

		/// <summary>
		/// List all Source exchanges in the given virtual host
		/// Equivalent to /api/exchanges/vhost
		/// </summary>
		public RMExchange[] ListSources()
		{
			return JsonSerializer.DeserializeFromString<RMExchange[]>(Get("/api/exchanges" + VirtualHost));
		}

		string Get(string endpoint)
		{
			Uri result;

			return Uri.TryCreate(HostUri, endpoint, out result) ? GetResponseString(result) : null;
		}

		static string GetResponseString([NotNull] Uri target)
		{
            if (target == null) throw new ArgumentNullException(nameof(target));

			var request = (HttpWebRequest) WebRequest.Create(target);
			request.AutomaticDecompression = DecompressionMethods.GZip;
			request.Credentials = new NetworkCredential("guest", "guest");

            using (var response = request.GetResponse())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream == null) throw new Exception("Failed to read response");
                    return new StreamReader(responseStream).ReadToEnd();
                }
            }
        }
    }
}
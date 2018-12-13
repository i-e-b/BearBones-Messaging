using System.Collections.Generic;

namespace BearBonesMessaging.RabbitMq.RabbitMqManagement
{
	/// <summary>
	/// Message as returned by RabbitMQ management API.
	/// See http://www.rabbitmq.com/management.html
	/// </summary>
	public class RMExchange : IRMExchange
	{
#pragma warning disable 1591
		public string name { get; set; }
		public string vhost { get; set; }
		public string type { get; set; }
		public bool durable { get; set; }
		public bool auto_delete { get; set; }
		public bool @internal { get; set; }
		public IDictionary<string, string> arguments { get; set; }
#pragma warning restore 1591
	}
}
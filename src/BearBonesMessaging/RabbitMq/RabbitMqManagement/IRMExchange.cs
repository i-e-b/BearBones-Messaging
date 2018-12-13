using System.Collections.Generic;

namespace BearBonesMessaging.RabbitMq.RabbitMqManagement
{
	/// <summary>
	/// Message as returned by RabbitMQ management API.
	/// See http://www.rabbitmq.com/management.html
	/// </summary>
	public interface IRMExchange
	{
#pragma warning disable 1591
		string name { get; set; }
		string vhost { get; set; }
		string type { get; set; }
		bool durable { get; set; }
		bool auto_delete { get; set; }
		bool @internal { get; set; }
		IDictionary<string, string> arguments { get; set; }
#pragma warning restore 1591
	}
}
using System;

namespace BearBonesMessaging.RabbitMq.RabbitMqManagement
{
	/// <summary>
	/// Message as returned by RabbitMQ management API.
	/// See http://www.rabbitmq.com/management.html
	/// </summary>
	public class RMQueue : IRMQueue
	{
#pragma warning disable 1591
		public long memory { get; set; }
		public DateTime idle_since { get; set; }
		public long messages_ready { get; set; }
		public long messages_unacknowledged { get; set; }
		public long messages { get; set; }
		public long consumers { get; set; }
		public long pending_acks { get; set; }
		public double avg_ingress_rate { get; set; }
		public double avg_egress_rate { get; set; }
		public string name { get; set; }
		public bool durable { get; set; }
		public string vhost { get; set; }
		public string node { get; set; }
#pragma warning restore 1591
	}
}
using System;
using System.Collections.Generic;

namespace BearBonesMessaging.RabbitMq.RabbitMqManagement
{
	/// <summary>
	/// Message as returned by RabbitMQ management API.
	/// See http://www.rabbitmq.com/management.html
	/// </summary>
	public interface IRMQueue
	{
#pragma warning disable 1591
		long memory { get; set; }
		DateTime idle_since { get; set; }
		long messages_ready { get; set; }
		long messages_unacknowledged { get; set; }
		long messages { get; set; }
		long consumers { get; set; }
		long pending_acks { get; set; }
		double avg_ingress_rate { get; set; }
		double avg_egress_rate { get; set; }
		string name { get; set; }
		bool durable { get; set; }
		string vhost { get; set; }
		string node { get; set; }
        Dictionary<string, object> arguments { get; set; }
#pragma warning restore 1591
    }
}
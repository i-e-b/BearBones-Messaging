using System.Collections.Generic;

namespace BearBonesMessaging.RabbitMq.RabbitMqManagement
{
    /// <summary>
    /// Message as returned by RabbitMQ management API.
    /// See http://www.rabbitmq.com/management.html
    /// </summary>
    public interface IRMBinding
    {
#pragma warning disable 1591
        string source { get; set; }
        string vhost { get; set; }
        string destination { get; set; }
        string destination_type { get; set; }
        string routing_key { get; set; }
        IDictionary<string, string> arguments { get; set; }
        string properties_key { get; set; }
#pragma warning restore 1591
    }
}
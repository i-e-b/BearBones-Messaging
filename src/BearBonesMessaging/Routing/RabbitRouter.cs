using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BearBonesMessaging.RabbitMq;
using JetBrains.Annotations;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace BearBonesMessaging.Routing
{
	/// <summary>
	/// Very simple synchronous message routing over RabbitMq
	/// </summary>
	public class RabbitRouter : IMessageRouter
	{
        [NotNull] readonly ISet<string> queues;
        [NotNull] readonly ISet<string> exchanges;
        [NotNull] readonly IDictionary<string, object> noOptions;
		[NotNull] readonly IChannelAction _longTermConnection;
		[NotNull] readonly IRabbitMqConnection _shortTermConnection;
		[NotNull] readonly object _lockObject;

		/// <summary>
		/// Create a new router from config settings
		/// </summary>
		public RabbitRouter(IChannelAction longTermConnection, IRabbitMqConnection shortTermConnection)
		{
			_lockObject = new object();

            _longTermConnection = longTermConnection ?? throw new ArgumentNullException(nameof(longTermConnection));
			_shortTermConnection = shortTermConnection ?? throw new ArgumentNullException(nameof(shortTermConnection));

			queues = new HashSet<string>();
			exchanges = new HashSet<string>();
			noOptions = new Dictionary<string, object>();
		}

		/// <summary>
		/// Deletes all queues and exchanges created or used by this Router.
		/// </summary>
		public void RemoveRouting(Func<string, bool> filter)
		{
            if (filter == null) throw new ArgumentNullException(nameof(filter));

			lock (_lockObject)
			{
				MessagingBase.InternalResetCaches();
				_shortTermConnection.WithChannel(channel => {
					foreach (var queue in queues.Where(filter))
					{
						channel.QueueDelete(queue);
					}

					foreach (var exchange in exchanges.Where(filter))
					{
						channel.ExchangeDelete(exchange);
					}
				});

				queues.Clear();
				exchanges.Clear();
			}
		}

		/// <summary>
		/// Add a new node to which messages can be sent.
		/// This node send messages over links that share a routing key.
		/// </summary>
		public void AddSource(string name)
		{
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("source name is not valid");

			lock (_lockObject)
			{
				_shortTermConnection.WithChannel(channel => channel?.ExchangeDeclare(name, "direct", true, false, noOptions));
				exchanges.Add(name);
			}
		}

		/// <summary>
		/// Add a new node to which messages can be sent.
		/// This node sends messages to all its links
		/// </summary>
		public void AddBroadcastSource(string className)
		{
            if (string.IsNullOrWhiteSpace(className)) throw new Exception("class name is not valid");

			lock (_lockObject)
			{
				_shortTermConnection.WithChannel(channel => channel?.ExchangeDeclare(className, "fanout", true, false, noOptions));
				exchanges.Add(className);
			}
		}

		/// <summary>
		/// Add a new node where messages can be picked up
		/// </summary>
		public void AddDestination(string name)
		{
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("source name is not valid");

			lock (_lockObject)
			{
				_shortTermConnection.WithChannel(channel => channel?.QueueDeclare(name, true, false, false, noOptions));
				queues.Add(name);
			}
		}

		/// <summary>
		/// Create a link between a source node and a destination node by a routing key
		/// </summary>
		public void Link(string sourceName, string destinationName)
		{
			lock (_lockObject)
			{
				_shortTermConnection.WithChannel(channel => channel.QueueBind(destinationName, sourceName, ""));
			}
		}

		/// <summary>
		/// Route a message between two sources.
		/// </summary>
		public void RouteSources(string child, string parent)
		{
			lock (_lockObject)
			{
				if (parent == child) throw new ArgumentException("Can't bind a source to itself");
				_shortTermConnection.WithChannel(channel => channel.ExchangeBind(parent, child, ""));
			}
		}

		/// <summary>
		/// Send a message to an established source (will be routed to destinations by key)
		/// </summary>
		public void Send(string sourceName, string typeDescription, string data)
		{
            if (data == null) data = "";

            _longTermConnection.WithChannel(channel => channel?.BasicPublish(
                exchange:        sourceName,
                routingKey:      "",
                mandatory:       false, // if true, there must be at least one routing output otherwise the send will error
                basicProperties: TaggedBasicProperties(typeDescription ?? sourceName),
                body:            Encoding.UTF8.GetBytes(data))
            );
        }

        /// <summary>
        /// Get a message from a destination. This claims the message without removing it from the destination.
        /// Ensure you use `Finish` on the result if you have processed the message
        /// </summary>
        public string Get(string destinationName, out MessageProperties properties)
		{
			var result = _longTermConnection.GetWithChannel(channel => channel?.BasicGet(destinationName, false));
			if (result == null)
			{
				properties = default(MessageProperties);
				return null;
			}

            properties.DeliveryTag = result.DeliveryTag;
            properties.OriginalType = result.BasicProperties?.Type;
            properties.Exchange = result.Exchange;
            properties.CorrelationId = result.BasicProperties?.CorrelationId;

            if (result.Body == null) return null;
			return Encoding.UTF8.GetString(result.Body);
		}

		/// <summary>
		/// Finish a message retrieved by 'Get'.
		/// This will remove the message from the queue
		/// </summary>
		/// <param name="deliveryTag">Delivery tag as provided by 'Get'</param>
		public void Finish(ulong deliveryTag)
		{
			_longTermConnection.WithChannel(channel => channel?.BasicAck(deliveryTag, false));
		}

		/// <summary>
		/// Get a message from a destination, removing it from the queue
		/// </summary>
		public string GetAndFinish(string destinationName, out MessageProperties properties)
		{
			var str = Get(destinationName, out properties);
			if (str != null) Finish(properties.DeliveryTag);
			return str;
		}

		/// <summary>
		/// Delete all waiting messages from a given destination
		/// </summary>
		public void Purge(string destinationName)
        {
            lock (_lockObject)
            {
                _shortTermConnection.WithChannel(channel => channel?.QueuePurge(destinationName));
            }
        }

		/// <summary>
		/// Cancel a 'Get' by it's tag. The message will remain on the queue and become available for another 'Get'
		/// </summary>
		/// <param name="deliveryTag">Delivery tag as provided by 'Get'</param>
		public void Cancel(ulong deliveryTag)
		{
			_longTermConnection.WithChannel(channel => channel?.BasicReject(deliveryTag, true));
		}

		/// <summary>
		/// Basic properties object with default settings
		/// </summary>
		public IBasicProperties TaggedBasicProperties(string contractTypeDescription)
		{
			return new BasicProperties{
                Type = contractTypeDescription
            };
		}
	}
}
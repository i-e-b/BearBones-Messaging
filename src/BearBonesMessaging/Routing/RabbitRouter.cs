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
        /// Header key for the complete list of types for a message
        /// </summary>
        const string RoutingHeaderKey = "all-types";

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

        /// <inheritdoc />
        public IRabbitServerTarget ConnectionDetails()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            return _shortTermConnection;
        }

        /// <summary>
		/// Add a new node to which messages can be sent.
		/// This node send messages over links that share a routing key.
		/// </summary>
		public void AddSource(string name, string metadata)
		{
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("source name is not valid");

			lock (_lockObject)
			{
				_shortTermConnection.WithChannel(channel => channel?.ExchangeDeclare(name, "direct", true, false, ArgumentsToStore(metadata)));
				exchanges.Add(name);
			}
		}

        /// <summary>
		/// Add a new node to which messages can be sent.
		/// This node sends messages to all its links
		/// </summary>
		public void AddBroadcastSource(string className, string metadata)
		{
            if (string.IsNullOrWhiteSpace(className)) throw new Exception("class name is not valid");

			lock (_lockObject)
			{
				_shortTermConnection.WithChannel(channel => channel?.ExchangeDeclare(className, "fanout", true, false, ArgumentsToStore(metadata)));
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
        /// Add a new node where messages can be picked up, with a limited wait time.
        /// Also creates a dead-letter exchange and dead-letter queue.
        /// </summary>
		public void AddLimitedDestination(string name, Expires expiryTime)
		{
            if (string.IsNullOrWhiteSpace(name)) throw new Exception("source name is not valid");

            if (expiryTime == null || expiryTime.Milliseconds <= 0) { // not actually limited
                AddDestination(name);
                return;
            }

			lock (_lockObject)
			{
                var dlqName = MessagingBaseConfiguration.DeadLetterPrefix + name;
                var ttlOptions = new Dictionary<string, object> {
                    { "x-dead-letter-exchange", dlqName },
                    { "x-message-ttl", expiryTime.Milliseconds }
                };

				_shortTermConnection.WithChannel(channel => {
                    if (channel == null) throw new Exception("Short-term channel failed");

                    // Dead-letter parts:
                    channel.ExchangeDeclare(dlqName, "fanout", autoDelete: true); // an exchange specifically for the DLQ. Deleted if that queue is removed
                    channel.QueueDeclare(MessagingBaseConfiguration.DeadLetterPrefix + name, true, false, false, noOptions); // no ttl on the dead-letter queue
                    channel.QueueBind(dlqName, dlqName, "");

                    // the actual queue:
                    channel.QueueDeclare(name, true, false, false, ttlOptions);
                });

				queues.Add(name);
				queues.Add(dlqName);
                exchanges.Add(dlqName);
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
        public void Send(string sourceName, string typeDescription, string senderName, string correlationId, string data)
        {
            Send(sourceName, typeDescription, senderName ?? "AnonymousSender", correlationId, Encoding.UTF8.GetBytes(data ?? ""));
        }

        /// <summary>
        /// Send a message to an established source (will be routed to destinations by key)
        /// </summary>
        public void Send(string sourceName, string typeDescription, string senderName, string correlationId, byte[] data)
        {
            if (data == null) data = new byte[0];

            var properties = TaggedBasicProperties(typeDescription ?? sourceName, senderName, correlationId);

            _longTermConnection.WithChannel(channel => channel?.BasicPublish(
                exchange: sourceName,
                routingKey: "",
                mandatory: false, // if true, there must be at least one routing output otherwise the send will error
                basicProperties: properties,
                body: data)
            );
        }


        /// <inheritdoc />
        public string Get(string destinationName, out MessageProperties properties)
        {
            var result = GetBytes(destinationName, out properties);
            if (result == null) return null;
            return Encoding.UTF8.GetString(result);
        }

        /// <summary>
        /// Get a message from a destination. This claims the message without removing it from the destination.
        /// Ensure you use `Finish` on the result if you have processed the message
        /// </summary>
        public byte[] GetBytes(string destinationName, out MessageProperties properties)
		{
			var result = _longTermConnection.GetWithChannel(channel => channel?.BasicGet(destinationName, false));
			if (result == null)
			{
				properties = default(MessageProperties);
				return null;
			}

            properties.DeliveryTag = result.DeliveryTag;
            properties.Exchange = result.Exchange;
            properties.CorrelationId = result.BasicProperties?.CorrelationId;
            properties.SenderName = result.BasicProperties?.ReplyTo;

            // 'OriginalType' tries to hold all contract types that match the message
            // BasicProperties.Type is limited to 255 chars, but is easier for clients to send.
            // Headers don't have this restriction, so we use them preferentially if they exist.
            properties.OriginalType = Coalesce(result.BasicProperties?.Headers?[RoutingHeaderKey], result.BasicProperties?.Type);

            return result.Body;
		}

        private string Coalesce(object a, string b) {
            if (a == null) return b;
            if (a is byte[] bytes) return Encoding.UTF8.GetString(bytes);
            if (a is string str) return str;
            return b;
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
		public IBasicProperties TaggedBasicProperties(string contractTypeDescription, string senderName, string correlationId)
		{
			return new BasicProperties{
                Type = LimitShortString(contractTypeDescription ?? "<unknown>"),
                ReplyTo = senderName,
                CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
                Headers = LongStringHeaders(contractTypeDescription)
            };
		}

        /// <summary>
        /// RabbitMQ short strings can only be 255 characters or less.
        /// If the contract type is too long, we shorten it here.
        /// A full-length version is always held in the header dictionary
        /// </summary>
        private string LimitShortString([NotNull]string contractTypeDescription)
        {
            if (contractTypeDescription.Length < 255) return contractTypeDescription; // no problem

            // ReSharper disable once ConstantNullCoalescingCondition
            var parts = contractTypeDescription.Split(';') ?? new string[0];
            var sb = new StringBuilder();
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == null) continue;
                if (sb.Length + parts[i].Length > 253) return sb.ToString();
                if (i != 0) sb.Append(';');
                sb.Append(parts[i]);
            }
            return sb.ToString();
        }
        
        /// <summary>
        /// RabbitMQ short strings can only be 255 characters or less.
        /// A full-length version of the contract type is always held in the header dictionary
        /// </summary>
        private IDictionary<string, object> LongStringHeaders(string contractTypeDescription)
        {
            return new Dictionary<string, object> {{RoutingHeaderKey, contractTypeDescription}};
        }


        private IDictionary<string, object> ArgumentsToStore(string metadata)
        {
            var outp = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(metadata)) {
                outp.Add(MessagingBaseConfiguration.MetaDataArgument, metadata);
            }
            return outp;
        }
	}
}
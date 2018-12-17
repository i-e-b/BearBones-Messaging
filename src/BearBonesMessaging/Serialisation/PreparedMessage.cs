using System;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// A pre-serialised message. This is useful to support store-and-forward on a client.
	/// </summary>
	public class PreparedMessage : IPreparedMessage
	{
        /// <summary>
        /// Required: Routing type description for AMQP message basic properties 'type'.
        /// This is used during deserialisation to get the best available runtime type.
        /// </summary>
        [NotNull] public string ContractType { get; }

        /// <summary>
        /// Required: Routable type name. This is the entry point to the Exchange graph
        /// </summary>
        [NotNull] public string TypeName { get; }

        /// <summary>
        /// Required: Serialised message data
        /// </summary>
        [NotNull] public byte[] SerialisedMessage { get; }

        /// <summary>
        /// Optional: Message correlation ID. If null, a random ID will be generated when the message is sent.
        /// </summary>
        [CanBeNull] public string CorrelationId { get; set; }

        /// <summary>
        /// Create a new prepared message from a type name and message string
        /// </summary>
        public PreparedMessage(string typeName, string message, string contractType)
		{
            ContractType = contractType ?? throw new ArgumentNullException(nameof(contractType));
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            SerialisedMessage = Encoding.UTF8.GetBytes(message ?? "");
        }

        /// <summary>
        /// Restore a prepared message from bytes
        /// </summary>
        public static PreparedMessage FromBytes(byte[] bytes)
		{
			var concatMsg = (bytes == null) ? "" : Encoding.UTF8.GetString(bytes);
			var parts = concatMsg.Split(new []{"|"}, 3, StringSplitOptions.None);
			if (parts.Length < 3) throw new Exception("Invalid prepared message");
			return new PreparedMessage(parts[0], parts[2], parts[1]);
		}

		/// <summary>
		/// Return a storable list of bytes representing the message
		/// </summary>
		public byte[] ToBytes()
		{
            return Encoding.UTF8.GetBytes(TypeName + "|" + ContractType + "|").Concat(SerialisedMessage).ToArray();
        }
	}
}
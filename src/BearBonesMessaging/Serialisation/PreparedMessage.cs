using System;
using System.Linq;
using System.Text;

namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// A pre-serialised message. This is useful to support store-and-forward on a client.
	/// </summary>
	public class PreparedMessage : IPreparedMessage
	{
        /// <summary>
        /// Routing type description for AMQP message basic properties 'type'.
        /// This is used during deserialisation to get the best available runtime type.
        /// </summary>
        public string ContractType { get; }

        /// <summary>
        /// Routable type name. This is the entry point to the Exchange graph
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Serialised message data
        /// </summary>
        public byte[] SerialisedMessage { get; }

        /// <summary>
        /// Create a new prepared message from a type name and message string
        /// </summary>
        public PreparedMessage(string typeName, string message, string contractType)
		{
            ContractType = contractType;
            TypeName = typeName;
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
            if (SerialisedMessage == null) return Encoding.UTF8.GetBytes(TypeName + "|" + ContractType + "|");
            return Encoding.UTF8.GetBytes(TypeName + "|" + ContractType + "|").Concat(SerialisedMessage).ToArray();
        }
	}
}
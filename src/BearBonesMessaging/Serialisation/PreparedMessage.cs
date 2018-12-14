using System;
using System.Text;

namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// A pre-serialised message
	/// </summary>
	public class PreparedMessage : IPreparedMessage
	{
        /// <summary>
        /// Routing type description for AMQP message basic properties 'type'
        /// </summary>
        public string ContractType { get; }

        /// <summary>
        /// Return routable type name
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Return serialised message string
        /// </summary>
        public string SerialisedMessage { get; }

        /// <summary>
        /// Create a new prepared message from a type name and message string
        /// </summary>
        public PreparedMessage(string typeName, string message, string contractType)
		{
            ContractType = contractType;
            TypeName = typeName;
            SerialisedMessage = message;
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
            return Encoding.UTF8.GetBytes(TypeName + "|" + ContractType + "|" + SerialisedMessage);
        }
	}
}
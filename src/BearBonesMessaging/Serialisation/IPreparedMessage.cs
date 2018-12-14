namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// Contract for serialised messages
	/// </summary>
	public interface IPreparedMessage
	{
		/// <summary>
		/// Return a storable list of bytes representing the message with metadata
		/// </summary>
		byte[] ToBytes();
        
        /// <summary>
        /// Routing type description for AMQP message basic properties 'type'.
        /// This is used during deserialisation to get the best available runtime type.
        /// </summary>
        string ContractType { get; }

        /// <summary>
        /// Routable type name. This is the entry point to the Exchange graph
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Serialised message data
        /// </summary>
        byte[] SerialisedMessage { get; }

	}
}
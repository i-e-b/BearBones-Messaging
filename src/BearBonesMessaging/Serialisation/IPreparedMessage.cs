namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// Contract for serialised messages
	/// </summary>
	public interface IPreparedMessage
	{
		/// <summary>
		/// Return a storable list of bytes representing the message
		/// </summary>
		byte[] ToBytes();
        
        /// <summary>
        /// Routing type description for AMQP message basic properties 'type'
        /// </summary>
        string ContractType { get; }

        /// <summary>
        /// Return routable type name
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Return serialised message string
        /// </summary>
        string SerialisedMessage { get; }

	}
}
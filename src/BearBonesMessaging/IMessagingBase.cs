using System;
using BearBonesMessaging.Serialisation;
using JetBrains.Annotations;

namespace BearBonesMessaging
{
	/// <summary>
	/// Core messaging functions
	/// </summary>
	public interface IMessagingBase
	{
		/// <summary>
		/// Ensure a destination exists, and bind it to the exchanges for the given type
		/// </summary>
		void CreateDestination<T>([NotNull]string destinationName, [NotNull] Expires messageExpiry);
		
		/// <summary>
		/// Ensure a destination exists, and bind it to the exchanges for the given type
		/// </summary>
		void CreateDestination([NotNull]Type sourceMessage, [NotNull]string destinationName, [NotNull] Expires messageExpiry);

		/// <summary>
		/// Send a message to all bound destinations.
		/// Message is serialised to a JSON string from the message object by the internal serialiser.
        /// The message metadata will have a randomly generated correlation ID
		/// </summary>
		void SendMessage(object messageObject);
        
        /// <summary>
        /// Send a message to all bound destinations.
        /// Message is serialised to a JSON string from the message object by the internal serialiser.
        /// The message metadata will have the exact correlation ID as given 
        /// </summary>
        void SendMessage(object messageObject, string correlationId);

		/// <summary>
		/// Poll for a waiting message. Returns default(T) if no message.
        /// <para></para>
        /// IMPORTANT: this will immediately remove the message from the broker queue.
        /// Use this only for non-critical transient applications.
        /// <para></para>
        /// For important messages, use `TryStartMessage`
		/// </summary>
        [CanBeNull] T GetMessage<T>(string destinationName);

		/// <summary>
		/// Try to start handling a waiting message. Returns default(T) if no message.
		/// The message may be acknowledged or cancelled to finish reception.
		/// </summary>
		[CanBeNull] IPendingMessage<T> TryStartMessage<T>(string destinationName);
        
        /// <summary>
        /// Try to start handling a waiting message. Returns null if no message.
        /// This version does no deserialisation.
        /// The message may be acknowledged or cancelled to finish reception.
        /// </summary>
        [CanBeNull] IPendingMessage<byte[]> TryStartMessageRaw(string destinationName);

		/// <summary>
		/// Ensure that routes and connections are rebuild on next SendMessage or CreateDestination.
		/// </summary>
		void ResetCaches();

		/// <summary>
		/// Convert a message object to a simplified serialisable format.
		/// This is intended for later sending with SendPrepared().
		/// If you want to send immediately, use SendMessage();
		/// </summary>
		IPreparedMessage PrepareForSend(object messageObject);

		/// <summary>
		/// Immediately send a prepared message.
		/// </summary>
		/// <param name="message">A message created by PrepareForSend()</param>
		void SendPrepared(IPreparedMessage message);
	}
}
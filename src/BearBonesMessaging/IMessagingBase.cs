using System;
using BearBonesMessaging.Serialisation;

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
		void CreateDestination<T>(string destinationName);
		
		/// <summary>
		/// Ensure a destination exists, and bind it to the exchanges for the given type
		/// </summary>
		void CreateDestination(Type sourceMessage, string destinationName);

		/// <summary>
		/// Send a message to all bound destinations.
		/// Returns serialised form of the message object.
		/// </summary>
		void SendMessage(object messageObject);

		/// <summary>
		/// Poll for a waiting message. Returns default(T) if no message.
		/// </summary>
		T GetMessage<T>(string destinationName);

		/// <summary>
		/// Try to start handling a waiting message.
		/// The message may be acknowledged or cancelled to finish reception.
		/// </summary>
		IPendingMessage<T> TryStartMessage<T>(string destinationName);

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
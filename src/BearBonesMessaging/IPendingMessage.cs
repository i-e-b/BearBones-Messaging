using System;

namespace BearBonesMessaging
{
	/// <summary>
	/// Represents a message received from RabbitMq.
	/// May be acknowledged or cancelled.
	/// </summary>
	public interface IPendingMessage<out T>
	{
		/// <summary>Message on queue</summary>
		T Message { get; }

		/// <summary>Action to cancel and return message to queue</summary>
		Action Cancel { get; }

		/// <summary>Action to complete message and remove from queue</summary>
		Action Finish { get; }
	}
}
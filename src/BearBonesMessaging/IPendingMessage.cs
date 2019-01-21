using System;
using BearBonesMessaging.Routing;

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
        
        /// <summary>
        /// Message properties
        /// </summary>
        MessageProperties Properties { get; }
	}

    /// <summary>
    /// A helper for unit testing of pending messages
    /// </summary>
    public class TestPendingMessage
    {
        /// <summary>
        /// Return a test helper for IPendingMessage, with the given message object
        /// </summary>
        public static PendingMessageWrapper<T> Wrapper<T>(T messageObject) {
            return new PendingMessageWrapper<T>(messageObject);
        }

        
        /// <summary>
        /// Return a test helper for IPendingMessage, with the given message object.
        /// Uses an explicit boxed type.
        /// </summary>
        public static PendingMessageWrapper<object> WrapperAnon(object messageObject) {
            return new PendingMessageWrapper<object>(messageObject);
        }

        /// <summary>
        /// A test hook for pending messages
        /// </summary>
        public class PendingMessageWrapper<T>:IPendingMessage<T>
        {
            /// <summary>
            /// True if the `Finish` action was invoked
            /// </summary>
            public bool WasFinished { get; private set; }

            /// <summary>
            /// True if the `Cancel` action was invoked
            /// </summary>
            public bool WasCancelled { get; private set;}

            internal PendingMessageWrapper(T msg)
            {
                Message = msg;

                WasCancelled = false;
                WasFinished = true;

                Properties = new MessageProperties{
                    CorrelationId = "UNIT-TEST",
                    DeliveryTag = 1234,
                    Exchange = "UNIT-TEST",
                    OriginalType = typeof(T).Name,
                    SenderName = "UNIT-TEST"
                };

                Cancel = ()=>{WasCancelled = true; };
                Finish = ()=>{WasFinished = true; };
            }

            /// <inheritdoc />
            public T Message { get; set; }

            /// <inheritdoc />
            public Action Cancel { get; }

            /// <inheritdoc />
            public Action Finish { get; }

            /// <inheritdoc />
            public MessageProperties Properties { get; }
        }
    }
}
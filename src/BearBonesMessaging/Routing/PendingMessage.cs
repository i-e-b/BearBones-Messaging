using System;
using System.Threading;

namespace BearBonesMessaging.Routing
{
	/// <summary>
	/// A received message instance
	/// </summary>
	public class PendingMessage<T> : IPendingMessage<T>
	{
		volatile IMessageRouter _router;

        /// <summary>
        /// Message properties
        /// </summary>
		public MessageProperties Properties { get; }

        /// <summary>
        /// Wrap a message object and delivery tag as a PendingMessage
        /// </summary>
        public PendingMessage(IMessageRouter router, T message, MessageProperties properties)
		{
            Message = message;
			_router = router ?? throw new ArgumentException("Must supply a valid router.", "router");
            Properties = properties;
			Cancel = DoCancel;
			Finish = DoFinish;
		}

		void DoCancel()
		{
			var router = Interlocked.Exchange(ref _router, null);
			if (router == null) return;
			router.Cancel(Properties.DeliveryTag);
		}

		void DoFinish()
		{
			var router = Interlocked.Exchange(ref _router, null);
			if (router == null) return;
			router.Finish(Properties.DeliveryTag);
		}

		/// <summary>Message on queue</summary>
		public T Message { get; set; }

		/// <summary>Action to cancel and return message to queue</summary>
		public Action Cancel { get; set; }

		/// <summary>Action to complete message and remove from queue</summary>
		public Action Finish { get; set; }
	}
}
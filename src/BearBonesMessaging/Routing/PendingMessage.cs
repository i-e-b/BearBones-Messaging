using System;
using System.Threading;

namespace BearBonesMessaging.Routing
{
	/// <summary>
	/// A received message instance
	/// </summary>
	public class PendingMessage<T> : IPendingMessage<T>
	{
		IMessageRouter _router;
		readonly ulong _deliveryTag;

		/// <summary>
		/// Wrap a message object and delivery tag as a PendingMessage
		/// </summary>
		public PendingMessage(IMessageRouter router, T message, ulong deliveryTag)
		{
			if (router == null) throw new ArgumentException("Must supply a valid router.", "router");

			Message = message;
			_router = router;
			_deliveryTag = deliveryTag;
			Cancel = DoCancel;
			Finish = DoFinish;
		}

		void DoCancel()
		{
			var router = Interlocked.Exchange(ref _router, null);
			if (router == null) return;
			router.Cancel(_deliveryTag);
		}

		void DoFinish()
		{
			var router = Interlocked.Exchange(ref _router, null);
			if (router == null) return;
			router.Finish(_deliveryTag);
		}

		/// <summary>Message on queue</summary>
		public T Message { get; set; }

		/// <summary>Action to cancel and return message to queue</summary>
		public Action Cancel { get; set; }

		/// <summary>Action to complete message and remove from queue</summary>
		public Action Finish { get; set; }
	}
}
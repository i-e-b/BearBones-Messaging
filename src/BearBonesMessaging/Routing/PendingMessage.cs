using System;
using System.Threading;
using JetBrains.Annotations;

namespace BearBonesMessaging.Routing
{
	/// <summary>
	/// A received message instance
	/// </summary>
	public class PendingMessage<T> : IPendingMessage<T>
	{
		[CanBeNull] private volatile IMessageRouter _router;
        [NotNull]   private readonly CancellationTokenSource _source;
        private CancellationToken _token;
        private volatile bool _cancelled = false;

        /// <summary>
        /// Message properties
        /// </summary>
		public MessageProperties Properties { get; }

        /// <summary>
        /// Wrap a message object and delivery tag as a PendingMessage
        /// </summary>
        public PendingMessage(IMessageRouter router, T message, MessageProperties properties, TimeSpan ackTimeout)
		{
            Message = message;
			_router = router ?? throw new ArgumentException("Must supply a valid router.", "router");
            Properties = properties;
			Cancel = DoCancel;
			Finish = DoFinish;

            _source = new CancellationTokenSource();
            _token = _source.Token;
            if (ackTimeout > TimeSpan.Zero && ackTimeout < TimeSpan.MaxValue){
                _source.CancelAfter(ackTimeout);
                _token.Register(OnTimeout, false);
            }
        }

        private void OnTimeout()
        {
            if (!_cancelled) DoCancel();
        }

        void DoCancel()
        {
            _cancelled = true;
            if (_source.IsCancellationRequested) Finish = Cancel = TimeoutCancelled;
            else Finish = Cancel = DoubleCancel;

            var router = Interlocked.Exchange(ref _router, null);
			if (router == null) return;
			router.Cancel(Properties.DeliveryTag);
		}

        void DoubleCancel() => throw new InvalidOperationException("This message has already been cancelled.");
        void DoubleFinish() => throw new InvalidOperationException("This message has already been finished.");
        void TimeoutCancelled() => throw new InvalidOperationException("This message has been cancelled by timeout. See MessagingBase.SetAcknowledgeTimeout()");

        void DoFinish()
		{
            Finish = Cancel = DoubleFinish;

            _token.ThrowIfCancellationRequested();
            _source.Dispose();

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
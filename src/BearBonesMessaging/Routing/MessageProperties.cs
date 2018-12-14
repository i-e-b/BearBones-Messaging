using JetBrains.Annotations;

namespace BearBonesMessaging.Routing
{
    /// <summary>
    /// Properties used for reading and acknowledging messages
    /// </summary>
    public struct MessageProperties
    {
        /// <summary>
        /// AMQP delivery tag for closing messages
        /// </summary>
        public ulong DeliveryTag;

        /// <summary>
        /// The original contract name that the sender gave this message
        /// </summary>
        [CanBeNull] public string OriginalType;

        /// <summary>
        /// Original exchange the message was published to
        /// </summary>
        public string Exchange;

        /// <summary>
        /// Correlation ID of the message, if given
        /// </summary>
        [CanBeNull]public string CorrelationId;

        /// <summary>
        /// Sender name or reply-to address of the message, if given
        /// </summary>
        [CanBeNull]public string SenderName;
    }
}
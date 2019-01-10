using System;
using BearBonesMessaging.RabbitMq;

namespace BearBonesMessaging.Routing
{
    /// <summary>
    /// A router that can be used with reduced permission users.
    /// Does not require any "Configure" permissions on RabbitMQ.
    /// This ignores all requests to change routing.
    /// </summary>
    public class ReducedPermissionRouter:IMessageRouter, ITypeRouter
    {
        private readonly RabbitRouter _rabbit;

        /// <inheritdoc />
        public ReducedPermissionRouter(IChannelAction longTermConnection, IRabbitMqConnection shortTermConnection)
        {
            _rabbit = new RabbitRouter(longTermConnection, shortTermConnection);
        }

        /// <inheritdoc />
        public void AddSource(string name, string metadata) { }

        /// <inheritdoc />
        public void AddBroadcastSource(string className, string metadata) { }

        /// <inheritdoc />
        public void AddDestination(string name) { }

        /// <inheritdoc />
        public void AddLimitedDestination(string name, Expires expiryTime) { }

        /// <inheritdoc />
        public void Link(string sourceName, string destinationName) { }

        /// <inheritdoc />
        public void RouteSources(string child, string parent) { }

        /// <inheritdoc />
        public void Send(string sourceName, string typeDescription, string senderName, string correlationId, byte[] data)
        {
            if (_rabbit == null) throw new InvalidProgramException();
            _rabbit.Send(sourceName, typeDescription, senderName, correlationId, data);
        }

        /// <inheritdoc />
        public void Send(string sourceName, string typeDescription, string senderName, string correlationId, string data)
        {
            if (_rabbit == null) throw new InvalidProgramException();
            _rabbit.Send(sourceName, typeDescription, senderName, correlationId, data);
        }

        /// <inheritdoc />
        public string Get(string destinationName, out MessageProperties properties)
        {
            if (_rabbit == null) throw new InvalidProgramException();
            return _rabbit.Get(destinationName, out properties);
        }

        /// <inheritdoc />
        public byte[] GetBytes(string destinationName, out MessageProperties properties)
        {
            if (_rabbit == null) throw new InvalidProgramException();
            return _rabbit.GetBytes(destinationName, out properties);
        }

        /// <inheritdoc />
        public void Finish(ulong deliveryTag)
        {
            if (_rabbit == null) throw new InvalidProgramException();
            _rabbit.Finish(deliveryTag);
        }

        /// <inheritdoc />
        public string GetAndFinish(string destinationName, out MessageProperties properties)
        {
            if (_rabbit == null) throw new InvalidProgramException();
            return _rabbit.GetAndFinish(destinationName, out properties);
        }

        /// <inheritdoc />
        public void Purge(string destinationName) { }

        /// <inheritdoc />
        public void Cancel(ulong deliveryTag)
        {
            if (_rabbit == null) throw new InvalidProgramException();
            _rabbit.Cancel(deliveryTag);
        }

        /// <inheritdoc />
        public void RemoveRouting(Func<string, bool> filter) { }

        /// <inheritdoc />
        public IRabbitServerTarget ConnectionDetails()
        {
            if (_rabbit == null) throw new InvalidProgramException();
            return _rabbit.ConnectionDetails();
        }

        /// <inheritdoc />
        public void BuildRoutes(Type type) { }

    }
}
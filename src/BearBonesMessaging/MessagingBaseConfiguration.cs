using System;
using System.Collections.Generic;
using BearBonesMessaging.RabbitMq;
using BearBonesMessaging.RabbitMq.RabbitMqManagement;
using BearBonesMessaging.Routing;
using BearBonesMessaging.Serialisation;
using JetBrains.Annotations;

namespace BearBonesMessaging
{
	/// <summary>
	/// Configuration options for messaging base
	/// </summary>
	public class MessagingBaseConfiguration
	{
		IRabbitMqConnection configuredConnection;
        [NotNull] readonly Dictionary<Type, Func<object>> _typeMap;
        private IMessageRouter _rabbitRouterSingleton;
        private IChannelAction _longConnectionSingleton;
        private string _appGroupName;

        /// <summary>
        /// A string added at the start of a queue name, to create a dead-letter queue
        /// </summary>
        public const string DeadLetterPrefix = "!-dead-";

        /// <summary>
        /// The most recently created messaging configuration
        /// </summary>
        [CanBeNull] public static MessagingBaseConfiguration LastConfiguration;

        /// <summary>
        /// Create a new configuration object
        /// </summary>
        public MessagingBaseConfiguration()
        {
            _typeMap = new Dictionary<Type, Func<object>>();
            LastConfiguration = this;
        }

        /// <summary>
        /// Configure all default mappings in this config object
        /// You must also call a `WithConnection...` method to get a
        /// working system.
        /// </summary>
        [NotNull] public MessagingBaseConfiguration WithDefaults()
		{
            Set<IMessageSerialiser>(() => new MessageSerialiser());
            Set<ITypeRouter>(() => new TypeRouter(Get<IMessageRouter>()));
            Set<IMessagingBase>(() => new MessagingBase(Get<ITypeRouter>(), Get<IMessageRouter>(), Get<IMessageSerialiser>(), _appGroupName));

            Set<IMessageRouter>(GetRabbitRouterSingleton);
            Set<IChannelAction>(GetChannelActionSingleton);

			return this;
		}
        
        /// <summary>
        /// Set a root contract type, preventing incorrect deserialisation from naming conflicts.
        /// All messages in the system should derive from this type if you set this.
        /// </summary>
        [NotNull] public MessagingBaseConfiguration WithContractRoot<TRoot>()
        {
            Set<IMessageSerialiser>(() => new MessageSerialiser(typeof(TRoot)));
            return this;
        }

        /// <summary>
		/// Configure long and short term connections to use the specified connection details
		/// </summary>
		[NotNull] public MessagingBaseConfiguration WithConnection(IRabbitMqConnection connection)
		{
			configuredConnection = connection;
            Set<IRabbitMqConnection>(() => configuredConnection);
			return this;
		}
        
        /// <summary>
        /// Configure long and short term connections to use the specified connection details
        /// </summary>
        /// <param name="host">Host DNS name or IP address</param>
        /// <param name="port">IP port the RabbitMQ server will be listening on</param>
        /// <param name="username">User name of an account with permission to connect</param>
        /// <param name="password">Password for the account</param>
        /// <param name="vhost">RabbitMQ virtual host this connection will be targeting. Use "/" if in doubt.</param>
        [NotNull] public MessagingBaseConfiguration WithConnection(string host, int port, string username, string password, string vhost)
        {
            Set<IRabbitMqConnection>(() => new RabbitMqConnection(host, username, password, vhost));
            return this;
        }

        /// <summary>
        /// Use a specific rabbit management node.
        /// This is required to do a range of Virtual Host and User management.
        /// <para></para>
        /// If you only need to produce and consume messages, you don't need to configure this.
        /// </summary>
        /// <param name="host">Host DNS name or IP address</param>
        /// <param name="port">IP port the management API will be listening on</param>
        /// <param name="username">User name of an Administrator account</param>
        /// <param name="password">Password for the Administrator account</param>
        /// <param name="vhost">RabbitMQ virtual host this connection will be managing</param>
        /// <param name="credentialSecret">A private secret used to generate names and passwords of 'Limited' user accounts</param>
        [NotNull] public MessagingBaseConfiguration WithRabbitManagement(string host, int port, string username, string password, string vhost, string credentialSecret)
		{
            Set<IRabbitMqQuery>(() =>
                new RabbitMqQuery("http://" + host + ":" + port, username, password, credentialSecret, vhost)
            );
            return this;
		}

        /// <summary>
        /// Add an application group name. This will be used as a reply-to address in any messages sent
        /// </summary>
        [NotNull] public MessagingBaseConfiguration WithApplicationGroupName(string appGroupName)
        {
            _appGroupName = appGroupName;
            return this;
        }

        /// <summary>
        /// Return a configured messaging instance.
        /// This can be used to send an receive messages
        /// </summary>
        public IMessagingBase GetMessagingBase()
        {
            return Get<IMessagingBase>();
        }
        
        /// <summary>
        /// Return a configured messaging instance.
        /// This can be used to manage and monitor the host server
        /// </summary>
        public IRabbitMqQuery GetManagement()
        {
            return Get<IRabbitMqQuery>();
        }

        /// <summary>
        /// Get the configured concrete type for an interface
        /// </summary>
        public T Get<T>()
        {
            if (!_typeMap.ContainsKey(typeof(T))) throw new Exception("No constructor for " + typeof(T).Name);

            return (T)_typeMap[typeof(T)]?.Invoke();
        }
        
        /// <summary>
        /// Replace a configured type with a new constructor.
        /// <para>NOTE: This should only be used for testing</para>
        /// </summary>
        public void Set<T>([NotNull]Func<object> constructor)
        {
            if (_typeMap.ContainsKey(typeof(T)))
            { // overwrite
                _typeMap[typeof(T)] = constructor;
                return;
            }

            _typeMap.Add(typeof(T), constructor);
        }

        private IChannelAction GetChannelActionSingleton()
        {
            if (_longConnectionSingleton == null) _longConnectionSingleton = new LongTermRabbitConnection(Get<IRabbitMqConnection>());
            return _longConnectionSingleton;
        }

        private IMessageRouter GetRabbitRouterSingleton()
        {
            if (_rabbitRouterSingleton == null) _rabbitRouterSingleton = new RabbitRouter(Get<IChannelAction>(), Get<IRabbitMqConnection>());
            return _rabbitRouterSingleton;
        }

    }
}

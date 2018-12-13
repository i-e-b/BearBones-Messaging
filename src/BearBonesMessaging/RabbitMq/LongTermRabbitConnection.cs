using System;
using System.Threading;
using JetBrains.Annotations;
using RabbitMQ.Client;

namespace BearBonesMessaging.RabbitMq
{
	/// <summary>
	/// Long-term connection to an RMQ cluster.
	/// This provider should be used when polling.
	/// </summary>
	public interface ILongTermConnection : IChannelAction
	{
		/// <summary>
		/// Close any existing connections.
		/// Connections will be re-opened if an action is requested.
		/// </summary>
		void Reset();
	}

	/// <summary>
	/// Default long-term connection to an RMQ cluster.
	/// This provider should be used when polling.
	/// </summary>
	public class LongTermRabbitConnection : ILongTermConnection
	{
		[NotNull] readonly IRabbitMqConnection rabbitMqConnection;
		[NotNull] readonly object lockObject = new object();

		ConnectionFactory _factory;
		IConnection _conn;
		IModel _channel;

		/// <summary>
		/// Prepare a long term connection with a connection provider.
		/// Call `MessagingBaseConfiguration` and request IChannelAction
		/// </summary>
		/// <param name="rabbitMqConnection"></param>
		public LongTermRabbitConnection(IRabbitMqConnection rabbitMqConnection)
		{
            this.rabbitMqConnection = rabbitMqConnection ?? throw new ArgumentNullException(nameof(rabbitMqConnection));
		}
		
		/// <summary>
		/// Close any existing connections and dispose of unmanaged resources
		/// </summary>
		~LongTermRabbitConnection()
		{
			ShutdownConnection();
		}

		/// <summary>
		/// Close any existing connections and dispose of unmanaged resources
		/// </summary>
		public void Dispose()
		{
			ShutdownConnection();
		}

		/// <summary>
		/// Close any existing connections.
		/// Connections will be re-opened if an action is requested.
		/// </summary>
		public void Reset()
		{
			ShutdownConnection();
		}

		/// <summary>
		/// Perform an action against the RMQ cluster, returning no data
		/// </summary>
		public void WithChannel(Action<IModel> actions)
		{
            if (actions == null) return;
			lock (lockObject)
			{
				actions(EnsureChannel());
			}
		}

		/// <summary>
		/// Perform an action against the RMQ cluster, returning data
		/// </summary>
		public T GetWithChannel<T>([NotNull]Func<IModel, T> actions)
		{
            if (actions == null) throw new ArgumentNullException(nameof(actions));
			lock (lockObject)
			{
				return actions(EnsureChannel());
			}
		}

		void ShutdownConnection()
		{
            lock (lockObject)
            {
                _factory = null;
            }
            DisposeChannel();
			DisposeConnection();
		}

		IModel EnsureChannel()
		{
			var lchan = _channel;
			if (lchan != null && lchan.IsOpen) return lchan;

			if (_factory == null)
			{
				_factory = rabbitMqConnection.ConnectionFactory();
			}
			if (_conn != null && _conn.IsOpen)
			{
				DisposeChannel();
				_channel = _conn?.CreateModel();
				return _channel;
			}

			DisposeConnection();

			var lfac = _factory;
			if (lfac == null) throw new Exception("RabbitMq Connection failed to generate a connection factory");
			lfac.RequestedHeartbeat = 60;
			_conn = lfac.CreateConnection();

			_channel = _conn?.CreateModel();
			return _channel;
		}

		void DisposeConnection()
		{
			lock (lockObject)
            {
                if (_conn == null) return;
                var conn = Interlocked.Exchange(ref _conn, null);
                if (conn == null) return;
                try
                {
                    if (conn.IsOpen) conn.Close();
                    conn.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Double-dispose. Not a massive problem, RabbitMQ SDK versions do this differently
                }
            }
        }

		void DisposeChannel()
		{
			lock (lockObject)
			{
                if (_channel == null) return;
                var chan = Interlocked.Exchange(ref _channel, null);
                if (chan == null) return;
                try
                {
                    if (chan.IsOpen) chan.Close();
                    chan.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Double-dispose. Not a massive problem, RabbitMQ SDK versions do this differently
                }
            }
        }
	}
}

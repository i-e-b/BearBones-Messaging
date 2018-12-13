using System;
using RabbitMQ.Client;

namespace BearBonesMessaging.RabbitMq
{
	/// <summary>
	/// Wrapper for actions that communicate with a RabbitMQ cluster.
	/// </summary>
	public interface IChannelAction : IDisposable
	{
		/// <summary>
		/// Perform an action against the RMQ cluster, returning no data
		/// </summary>
		void WithChannel(Action<IModel> actions);

		/// <summary>
		/// Perform an action against the RMQ cluster, returning data
		/// </summary>
		T GetWithChannel<T>(Func<IModel, T> actions);
	}
}
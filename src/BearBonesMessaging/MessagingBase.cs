﻿using System;
using System.Collections.Generic;
using System.Linq;
using BearBonesMessaging.RabbitMq;
using BearBonesMessaging.Routing;
using BearBonesMessaging.Serialisation;
using JetBrains.Annotations;
using SevenDigital.Messaging.Base;
using StructureMap;

namespace BearBonesMessaging
{
	/// <summary>
	/// Default messaging base.
	/// </summary>
	public class MessagingBase : IMessagingBase
	{
		[NotNull] readonly ITypeRouter typeRouter;
		[NotNull] readonly IMessageRouter messageRouter;
		[NotNull] readonly IMessageSerialiser serialiser;

        [NotNull] static readonly IDictionary<Type, RateLimitedAction> RouteCache = new Dictionary<Type, RateLimitedAction>();

		/// <summary>
		/// Create with `ObjectFactory.GetInstance&lt;IMessagingBase&gt;()`
		/// </summary>
		public MessagingBase(ITypeRouter typeRouter, IMessageRouter messageRouter, IMessageSerialiser serialiser)
		{
			this.typeRouter = typeRouter ?? throw new ArgumentNullException(nameof(typeRouter));
			this.messageRouter = messageRouter ?? throw new ArgumentNullException(nameof(messageRouter));
			this.serialiser = serialiser ?? throw new ArgumentNullException(nameof(serialiser));
		}

		/// <summary>
		/// Get the contract name of an object instance
		/// </summary>
		public static string ContractTypeName([NotNull] object instance)
		{
            if (instance == null) throw new ArgumentNullException(nameof(instance));
			return ContractTypeName(instance.GetType());
		}
		
		/// <summary>
		/// Get the contract name of a type
		/// </summary>
		[CanBeNull] public static string ContractTypeName([NotNull] Type type)
		{
            if (type == null) throw new ArgumentNullException(nameof(type));

			if (type.IsInterface) return type.FullName;

			var interfaceTypes = type.DirectlyImplementedInterfaces()?.ToList();

			if (interfaceTypes == null || !interfaceTypes.HasSingle())
				throw new ArgumentException("Messages must directly implement exactly one interface", "type");

			return interfaceTypes.Single()?.FullName;
		}

		/// <summary>
		/// Ensure a destination exists, and bind it to the exchanges for the given type
		/// </summary>
		public void CreateDestination<T>([NotNull] string destinationName)
		{
			CreateDestination(typeof(T), destinationName);
		}

        /// <summary>
        /// Ensure a destination exists, and bind it to the exchanges for the given type
        /// </summary>
        public void CreateDestination([NotNull]Type sourceType, [NotNull] string destinationName)
        {
            RouteSource(sourceType);
			messageRouter.AddDestination(destinationName);
			messageRouter.Link(sourceType.FullName, destinationName);
		}

		/// <summary>
		/// Send a message to all bound destinations.
		/// Returns serialised form of the message object.
		/// </summary>
		public void SendMessage([NotNull] object messageObject)
		{
			SendPrepared(PrepareForSend(messageObject));
		}

		/// <summary>
		/// Poll for a waiting message. Returns default(T) if no message.
		/// <para></para>
		/// IMPORTANT: this will immediately remove the message from the broker queue.
		/// Use this only for non-critical transient messages.
		/// <para></para>
		/// For important messages, use `TryStartMessage`
		/// </summary>
		public T GetMessage<T>(string destinationName)
		{
			var messageString = messageRouter.GetAndFinish(destinationName);

			if (messageString == null) return default(T);

			try
			{
				return (T)serialiser.DeserialiseByStack(messageString);
			}
			catch
			{
				return serialiser.Deserialise<T>(messageString);
			}
		}

		/// <summary>
		/// Try to start handling a waiting message.
		/// The message may be acknowledged or cancelled to finish reception.
		/// </summary>
		public IPendingMessage<T> TryStartMessage<T>(string destinationName)
		{
			var messageString = messageRouter.Get(destinationName, out var properties);

			if (messageString == null) return null;

			T message;
			try
			{
				message = (T)serialiser.DeserialiseByStack(messageString);
			}
			catch
			{
				message = serialiser.Deserialise<T>(messageString);
			}

			return new PendingMessage<T>(messageRouter, message, properties);
		}

		void RouteSource([NotNull] Type routeType)
		{
			lock (RouteCache)
			{
				if (!RouteCache.ContainsKey(routeType))
				{
					RouteCache.Add(routeType, RateLimitedAction.Of(() => typeRouter.BuildRoutes(routeType)));
				}
                RouteCache[routeType]?.YoungerThan(TimeSpan.FromMinutes(1));
            }
        }

		/// <summary>
		/// Ensure that routes and connections are rebuild on next SendMessage or CreateDestination.
		/// </summary>
		public void ResetCaches()
		{
			InternalResetCaches();
		}

		/// <summary>
		/// Convert a message object to a simplified serialisable format.
		/// This is intended for later sending with SendPrepared().
		/// If you want to send immediately, use SendMessage();
		/// </summary>
		[NotNull] public IPreparedMessage PrepareForSend([NotNull] object messageObject)
		{
            if (messageObject == null) throw new ArgumentNullException(nameof(messageObject));

			var interfaceTypes = messageObject.GetType().DirectlyImplementedInterfaces()?.ToList();

			if (interfaceTypes == null || !interfaceTypes.HasSingle())
				throw new ArgumentException("Messages must directly implement exactly one interface", "messageObject");
			
			var sourceType = interfaceTypes.Single() ?? throw new ArgumentException("Messages must directly implement exactly one interface", "messageObject");
			var serialised = serialiser.Serialise(messageObject);

			RouteSource(sourceType);
			return new PreparedMessage(sourceType.FullName, serialised);
		}

		/// <summary>
		/// Immediately send a prepared message.
		/// </summary>
		/// <param name="message">A message created by PrepareForSend()</param>
		public void SendPrepared([NotNull] IPreparedMessage message)
		{
            if (message == null) throw new ArgumentNullException(nameof(message));
			messageRouter.Send(message.TypeName(), message.SerialisedMessage());
		}

		internal static void InternalResetCaches()
		{
			lock (RouteCache)
			{
				RouteCache.Clear();
			}
			var channelAction = ObjectFactory.TryGetInstance<IChannelAction>();
			if (channelAction is LongTermRabbitConnection connection)
			{
				connection.Reset();
			}
		}
	}
}

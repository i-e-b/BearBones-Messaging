using System;
using BearBonesMessaging.Serialisation;
using JetBrains.Annotations;
using SevenDigital.Messaging.Base;

namespace BearBonesMessaging.Routing
{
	/// <summary>
	/// Default type router
	/// </summary>
	public class TypeRouter : ITypeRouter
	{
		[NotNull] readonly IMessageRouter router;

		/// <summary>
		/// Create a type router to drive the given message router.
		/// You don't need to do this yourself -- Use `MessagingBaseConfiguration`
		/// </summary>
		public TypeRouter(IMessageRouter router)
		{
			this.router = router ?? throw new ArgumentNullException(nameof(router));
		}

		/// <summary>
		/// Build all dependant types into the messaging server
		/// </summary>
		/// <param name="type"></param>
		public void BuildRoutes([NotNull] Type type)
		{
            if (type == null) throw new ArgumentNullException(nameof(type));
			if (type.IsInterface) router.AddSource(type.FullName, InterfaceStack.OfInterface(type));
			AddSourcesAndRoute(type);
		}

		void AddSourcesAndRoute([NotNull] Type type)
		{
            var interfaces = type.DirectlyImplementedInterfaces();
            if (interfaces == null) return;

			foreach (var interfaceType in interfaces)
			{
				router.AddSource(interfaceType.FullName, InterfaceStack.OfInterface(interfaceType));
				router.RouteSources(type.FullName, interfaceType.FullName);
				AddSourcesAndRoute(interfaceType);
			}
		}
	}
}
using System;

namespace BearBonesMessaging.Routing
{
	/// <summary>
	/// Building contract-type routing tree
	/// </summary>
	public interface ITypeRouter
	{
		/// <summary>
		/// Build all dependant types into the messaging server
		/// </summary>
		/// <param name="type"></param>
		void BuildRoutes(Type type);
	}
}
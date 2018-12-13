// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using System.Linq;

namespace SevenDigital.Messaging.Base
{
	/// <summary>
	/// Extension methods for reading the interfaces defined in types
	/// </summary>
	public static class TypeExtensions
	{
		/// <summary>
		/// Return the list of interfaces explicitly defined by the type of the object;
		/// Does not return subinterfaces.
		/// </summary>
		public static IEnumerable<Type> DirectlyImplementedInterfaces(this Type type)
		{
			return type?.GetInterfaces().Where(i => !type.GetInterfaces().Any(i2 => i2.GetInterfaces().Contains(i)));
		}
		
		/// <summary>
		/// Return the list of interfaces explicitly defined by the type of the object;
		/// Does not return subinterfaces.
		/// </summary>
		public static IEnumerable<Type> DirectlyImplementedInterfaces<T>(this T src)
		{
			var type = src?.GetType();
			return DirectlyImplementedInterfaces(type);
		}
	}
}

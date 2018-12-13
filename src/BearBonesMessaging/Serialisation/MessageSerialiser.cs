using System;
using System.Linq;
using JetBrains.Annotations;
using ServiceStack.Text;
using SevenDigital.Messaging.Base;

namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// Default serialiser for message objects.
	/// Uses ServiceStack.Text
	/// </summary>
	public class MessageSerialiser : IMessageSerialiser
	{
		///<summary>Return a JSON string representing a source object</summary>
		public string Serialise(object messageObject)
		{
            if (messageObject == null) throw new ArgumentNullException(nameof(messageObject));
			var type = messageObject.GetType();
			var interfaces = type.DirectlyImplementedInterfaces()?.ToList();
			if ( interfaces == null || ! interfaces.HasSingle())
				throw new ArgumentException("Messages must directly implement exactly one interface", "messageObject");

			JsConfig.PreferInterfaces = true;
			var str = JsonSerializer.SerializeToString(messageObject, interfaces.Single());
			return str?.Insert(str.Length - 1, ",\"__contracts\":\""+InterfaceStack.Of(messageObject)+"\"");
		}

		///<summary>Return an object of a known type based on it's JSON representation</summary>
		public T Deserialise<T>(string source)
		{
			JsConfig.PreferInterfaces = true;
			return JsonSerializer.DeserializeFromString<T>(source);
		}

		///<summary>Return an object of an unknown type based on it's claimed hierarchy</summary>
		public object DeserialiseByStack(string source)
		{
			var bestKnownType = ContractStack.FirstKnownType(source);
			if (bestKnownType == null) 
				throw new Exception("Can't deserialise message, as no matching types are available. Are you missing an assembly reference?");

			return JsonSerializer.DeserializeFromString(source, WrapperTypeFor(bestKnownType));
		}

		/// <summary>
		/// Returns an instatiable class that implements the given interface class
		/// </summary>
		[CanBeNull] public Type WrapperTypeFor<T>()
		{
			return (typeof(T).IsInterface) ? (DynamicProxy.GetInstanceFor<T>()?.GetType()) : (typeof(T));
		}
		/// <summary>
		/// Returns an instatiable class that implements the given interface class
		/// </summary>
		[CanBeNull] public Type WrapperTypeFor(Type t)
		{
            if (t == null) return null;
			return (t.IsInterface) ? (DynamicProxy.GetInstanceFor(t)?.GetType()) : (t);
		}
	}
}
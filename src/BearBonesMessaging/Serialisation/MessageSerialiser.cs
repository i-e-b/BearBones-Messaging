using System;
using System.Linq;
using JetBrains.Annotations;
using SevenDigital.Messaging.Base;
using SkinnyJson;

namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// Default serialiser for message objects.
	/// Uses ServiceStack.Text
	/// </summary>
	public class MessageSerialiser : IMessageSerialiser
	{
        [CanBeNull] private readonly string _rootAssemblyName;

        /// <summary>
        /// Create a message serialiser with no fixed root type
        /// </summary>
        public MessageSerialiser()
        {
            
            _rootAssemblyName = null;
        }

        /// <summary>
        /// Create a message serialiser with  a root contract type.
        /// All messages in the system should derive from this type.
        /// </summary>
        public MessageSerialiser(Type ContractRootType)
        {
            _rootAssemblyName = ContractRootType?.Assembly.GetName().Name;
        }

		///<summary>Return a JSON string representing a source object</summary>
		public string Serialise(object messageObject, out string typeDescription)
		{
            if (messageObject == null) throw new ArgumentNullException(nameof(messageObject));
			var type = messageObject.GetType();
			var interfaces = type.DirectlyImplementedInterfaces()?.ToList();
			if ( interfaces == null || ! interfaces.HasSingle())
				throw new ArgumentException("Messages must directly implement exactly one interface", "messageObject");

            typeDescription = InterfaceStack.Of(messageObject);
			return Json.Freeze(messageObject);
		}

		///<summary>Return an object of a known type based on it's JSON representation</summary>
		public T Deserialise<T>(string source)
		{
			return Json.Defrost<T>(source);
		}

		///<summary>Return an object of an unknown type based on it's claimed hierarchy</summary>
		public object DeserialiseByStack(string source, string typeDescription)
		{
			var bestKnownType = ContractStack.FirstKnownType(typeDescription, _rootAssemblyName);
			if (bestKnownType == null) 
				throw new Exception("Can't deserialise message, as no matching types are available. Are you missing an assembly reference?");

			return Json.Defrost(source, bestKnownType); 
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
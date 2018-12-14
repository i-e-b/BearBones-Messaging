using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// Helper class to read the contract stack from incoming JSON messages
	/// </summary>
	public class ContractStack
	{
        /// <summary>
        /// Marker for JSON documents if types are being stored inline
        /// </summary>
		public const string Marker = "\"__contracts\":\"";

		/// <summary>
		/// Return the type object for the first contract available in the calling assembly,
		/// as read from a supplied JSON message or type description
		/// </summary>
		public static Type FirstKnownType(string message, string contractAssemblyName)
		{
			if (string.IsNullOrEmpty(message)) return null;
			const StringComparison ord = StringComparison.Ordinal;


			int left = message.IndexOf(Marker, ord);
			if (left < 0) left = 0;
			else left += Marker.Length;
            
            if (left >= message.Length) return null;

			while (left < message.Length)
			{
				var right = message.IndexOfAny(new[] { ';', '"' }, left);
				if (right <= left) return null;

                var name = message.Substring(left, right - left);
                var t = TryGetTypeByName(name, contractAssemblyName);
                if (t != null) return t;

				left = right + 1;
				while (char.IsWhiteSpace(message[left]))
				{
					left++;
				}
			}
			return null;
		}


        [CanBeNull] private static Type TryGetTypeByName([NotNull] string name, [CanBeNull] string contractAssemblyName)
        {
            if (name.Contains(", "))
            { // contract includes expected assembly. We will respect that, even if it means failing to find a result.
                return Type.GetType(name, false);
            }

            if (!string.IsNullOrWhiteSpace(contractAssemblyName))
            { // we have been supplied with a target assembly, so use that *exclusively*
                return Type.GetType(name + ", " + contractAssemblyName, false);
            }

            // Fallback to a cached full search
            if (_typeNameCache == null) _typeNameCache = BuildTypeNameCache();
            if (_typeNameCache.ContainsKey(name)) return _typeNameCache[name];

            // Found nothing
            return null;
        }

        [NotNull] private static Dictionary<string, Type>  BuildTypeNameCache()
        {
            var outp = new Dictionary<string, Type>();

            var everything = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in everything)
            {
                var name = assembly.GetName().Name;
                if (name == null || name.StartsWith("System.")) continue; // skip anything we know is not going to contain contract types

                var types = assembly.GetTypes().Where(t => t.IsPublic && t.IsInterface);
                foreach (var interf in types)
                {
                    var shortName = InterfaceStack.Shorten(interf);
                    if (outp.ContainsKey(shortName)) outp[shortName] = null; // null out on conflict, as a feedback mechanism
                    else outp.Add(shortName, interf);
                }
            }

            return outp;
        }

        private static Dictionary<string, Type> _typeNameCache;
    }
}
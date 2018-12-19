using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using SevenDigital.Messaging.Base;

namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// Helper class for reading the definition stack of interfaces.
	/// </summary>
	public class InterfaceStack
	{
		/// <summary>
		/// Return a string list of all interfaces implemented by the given object INSTANCE
		/// </summary>
		public static string Of(object source)
		{
			var set = new List<Type>();
			Interfaces(source.DirectlyImplementedInterfaces(), set);
			
			var sb = new StringBuilder();
			for (int i = 0; i < set.Count; i++)
			{
				var type = set[i];
                if (type == null) continue;
				if (i>0) sb.Append(";");
				sb.Append(Shorten(type));
			}
			return sb.ToString();
		}

        /// <summary>
        /// Return a string list of all interfaces implemented by the given type
        /// </summary>
        public static string OfInterface(Type source)
        {
            var set = new List<Type>();
            Interfaces(new[] { source }, set);

            var sb = new StringBuilder();
            for (int i = 0; i < set.Count; i++)
            {
                var type = set[i];
                if (type == null) continue;
                if (i>0) sb.Append(";");
                sb.Append(Shorten(type));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get a shortened name (with namespace but excluding Assembly information) for the given type.
        /// </summary>
		[NotNull]public static string Shorten([NotNull]Type type)
		{
            var assemblyQualifiedName = type.AssemblyQualifiedName;
            if (string.IsNullOrWhiteSpace(assemblyQualifiedName)) throw new Exception("Invalid type definition for " + type);
            //var idx = assemblyQualifiedName?.IndexOf(", Version", StringComparison.Ordinal);
            var idx = assemblyQualifiedName.IndexOf(",", StringComparison.Ordinal);

            if (idx < 1) return assemblyQualifiedName;
			return idx < 0 ? assemblyQualifiedName : assemblyQualifiedName.Substring(0, idx);
		}

		static void Interfaces(IEnumerable<Type> interfaces, [NotNull] ICollection<Type> set)
		{
			var types = interfaces as Type[] ?? interfaces?.ToArray() ?? new Type[0];
			foreach (var type in types.Where(type => !set.Contains(type)))
			{
				set.Add(type);
			}
			foreach (var type in types)
			{
				Interfaces(type.DirectlyImplementedInterfaces(), set);
			}
		}
	}
}
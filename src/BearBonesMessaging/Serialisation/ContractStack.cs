using System;

namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// Helper class to read the contract stack from incoming JSON messages
	/// </summary>
	public class ContractStack
	{
		const string Marker = "\"__contracts\":\"";

		/// <summary>
		/// Return the type object for the first contract available in the calling assembly,
		/// as read from the supplied JSON message.
		/// </summary>
		public static Type FirstKnownType(string message)
		{
			if (string.IsNullOrEmpty(message)) return null;
			const StringComparison ord = StringComparison.Ordinal;


			int left = message.IndexOf(Marker, ord) + Marker.Length;
			if (left < 0 || (left >= message.Length)) return null;

			while (left < message.Length)
			{
				var right = message.IndexOfAny(new[] { ';', '"' }, left);
				if (right <= left) return null;

				var t = Type.GetType(message.Substring(left, right - left), false);
				if (t != null) return t;

				left = right + 1;
				while (char.IsWhiteSpace(message[left]))
				{
					left++;
				}
			}
			return null;
		}
	}
}
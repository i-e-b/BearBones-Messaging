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
		/// as read from a supplied JSON message or type description
		/// </summary>
		public static Type FirstKnownType(string message)
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
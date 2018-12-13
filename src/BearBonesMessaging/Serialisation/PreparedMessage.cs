using System;
using System.Text;

namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// A pre-serialised message
	/// </summary>
	public class PreparedMessage : IPreparedMessage
	{
		readonly string _typeName;
		readonly string _message;

		/// <summary>
		/// Create a new prepared message from a type name and message string
		/// </summary>
		public PreparedMessage(string typeName, string message)
		{
			_typeName = typeName;
			_message = message;
		}

		/// <summary>
		/// Restore a prepared message from bytes
		/// </summary>
		public static PreparedMessage FromBytes(byte[] bytes)
		{
			var concatMsg = (bytes == null) ? "" : Encoding.UTF8.GetString(bytes);
			var parts = concatMsg.Split(new []{"|"}, 2, StringSplitOptions.None);
			if (parts.Length < 2) throw new Exception("Invalid prepared message");
			return new PreparedMessage(parts[0], parts[1]);
		}

		/// <summary>
		/// Return a storable list of bytes representing the message
		/// </summary>
		public byte[] ToBytes()
		{
			return Encoding.UTF8.GetBytes(_typeName + "|" + _message);
		}

		/// <summary>
		/// Return routable type name
		/// </summary>
		public string TypeName()
		{
			return _typeName;
		}

		/// <summary>
		/// Return serialised message string
		/// </summary>
		public string SerialisedMessage()
		{
			return _message;
		}
	}
}
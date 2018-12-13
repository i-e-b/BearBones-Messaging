
namespace BearBonesMessaging.Serialisation
{
	/// <summary>
	/// Contract for message serialisation
	/// </summary>
	public interface IMessageSerialiser
	{
		///<summary>Return a JSON string representing a source object</summary>
		string Serialise(object source);

		///<summary>Return an object of a known type based on it's JSON representation</summary>
		T Deserialise<T>(string source);
		
		///<summary>Return an object of an unknown type based on it's claimed hierarchy</summary>
		object DeserialiseByStack(string source);
	}
}
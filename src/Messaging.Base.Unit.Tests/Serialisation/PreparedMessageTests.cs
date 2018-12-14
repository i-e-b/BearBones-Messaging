using BearBonesMessaging.Serialisation;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Unit.Tests.Serialisation
{
	[TestFixture]
	public class PreparedMessageTests
	{
		const string typeString = "this.is.my.type";
		const string typeDescription = "this.is.my.type;parent";
		const string messageString = "{'a' : '|b'}";

		[Test]
		public void should_be_able_to_round_trip_between_bytes_and_strings ()
		{
			var originalMessage = new PreparedMessage(typeString, messageString, typeDescription);

			var bytes = originalMessage.ToBytes();
			var result = PreparedMessage.FromBytes(bytes);

			Assert.That(result.TypeName, Is.EqualTo(typeString));
			Assert.That(result.SerialisedMessage, Is.EqualTo(messageString));
			Assert.That(result.ContractType, Is.EqualTo(typeDescription));
		}

	}
}
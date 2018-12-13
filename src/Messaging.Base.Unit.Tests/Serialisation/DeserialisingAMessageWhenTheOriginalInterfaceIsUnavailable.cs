using System;
using BearBonesMessaging.Serialisation;
using Example.Types;
using NUnit.Framework;

namespace Messaging.Base.Unit.Tests.Serialisation
{
	[TestFixture]
	public class DeserialisingAMessageWhenTheOriginalInterfaceIsUnavailable
	{
		IMessageSerialiser subject;
		string message;
		SuperMetadata originalObject;

		[SetUp]
		public void SetUp()
		{
			subject = new MessageSerialiser();
			originalObject = new SuperMetadata
				{
					CorrelationId = Guid.Parse("05C90FEB-5C10-4179-9FC0-D26DDA5FD1C6"),
					Contents = "My message contents",
					FilePath = "C:\\work\\message",
					HashValue = 123124512,
					MetadataName = "Mind the gap"
				};
			message = subject.Serialise(originalObject).Replace("Example.Types.IMetadataFile, Example.Types", "Pauls.IMum, Phils.Face");
			Console.WriteLine(message);
		}


		[Test]
		public void Should_return_null ()
		{
			var result = subject.Deserialise<IMetadataFile>(message);

			Assert.That(result, Is.Null);
		}
	}
}
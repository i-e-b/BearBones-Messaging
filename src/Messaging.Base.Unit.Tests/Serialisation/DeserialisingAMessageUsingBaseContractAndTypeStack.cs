using System;
using BearBonesMessaging.Serialisation;
using Example.Types;
using NUnit.Framework;

namespace Messaging.Base.Unit.Tests.Serialisation
{
	[TestFixture]
	public class DeserialisingAMessageUsingBaseContractAndTypeStack
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

			message = subject.Serialise(originalObject);
			Console.WriteLine(message);
		}

		[Test]
		public void if_first_type_is_unknown_should_get_second_type ()
		{
			var newmsg = message.Replace("IMetadataFile","rubbish");
			
			object result = subject.DeserialiseByStack(newmsg);

			Assert.That(result, Is.InstanceOf<IFile>());
		}

		[Test]
		public void Should_deserialise_to_a_concrete_implementation_of_the_requested_interface()
		{
			object result = subject.DeserialiseByStack(message);

			Assert.That(result, Is.InstanceOf<IMetadataFile>());
		}

		[Test]
		public void Should_deserialise_properties_as_initially_provided()
		{
			var result = subject.DeserialiseByStack(message) as IMetadataFile;
			if (result == null)
				Assert.Fail();

			Assert.That(result.CorrelationId, Is.EqualTo(Guid.Parse("05C90FEB-5C10-4179-9FC0-D26DDA5FD1C6")));
			Assert.That(result.Contents, Is.EqualTo("My message contents"));
			Assert.That(result.FilePath, Is.EqualTo("C:\\work\\message"));
			Assert.That(result.HashValue, Is.EqualTo(123124512));
		}

		[Test]
		public void if_no_types_are_known_should_throw_exception ()
		{
			Assert.Throws<Exception>(()=>subject.DeserialiseByStack(message.Replace("Example.Types", "Phils.Face")));
		}
	}
}
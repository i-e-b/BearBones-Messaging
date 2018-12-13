using System;
using BearBonesMessaging.Serialisation;
using Example.Types;
using NUnit.Framework;

namespace Messaging.Base.Unit.Tests.Serialisation
{
	[TestFixture]
	public class WhenSerialisingAnObjectWithNoTopLevelInterfaces
	{
		IMessageSerialiser subject;

		static MyPOCO source;

		[SetUp]
		public void setup()
		{
			source = new MyPOCO
				{
					CorrelationId = Guid.Parse("05C90FEB-5C10-4179-9FC0-D26DDA5FD1C6"),
					FilePath = "C:\\work\\message",
					HashValue = 123124512
				};

			subject = new MessageSerialiser();
		}

		[Test]
		public void Should_get_an_argument_exception()
		{
			var exception = Assert.Throws<ArgumentException>(
				() => subject.Serialise(source));

			Assert.That(exception.Message, Contains.Substring("Messages must directly implement exactly one interface"));
		}

	}
}
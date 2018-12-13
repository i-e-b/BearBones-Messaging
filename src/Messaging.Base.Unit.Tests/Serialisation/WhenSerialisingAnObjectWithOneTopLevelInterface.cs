using System;
using BearBonesMessaging.Serialisation;
using Example.Types;
using NUnit.Framework;

namespace Messaging.Base.Unit.Tests.Serialisation
{
	[TestFixture]
	public class WhenSerialisingAnObjectWithOneTopLevelInterface
	{
		IMessageSerialiser subject;

		static SuperMetadata source;
		string result;

		[SetUp]
		public void With_string_serialised_from_a_source_object()
		{
			source = new SuperMetadata
			{
				CorrelationId = Guid.Parse("05C90FEB-5C10-4179-9FC0-D26DDA5FD1C6"),
				Contents = "My message contents",
				FilePath = "C:\\work\\message",
				HashValue = 123124512,
				MetadataName = "Mind the gap"
			};

			subject = new MessageSerialiser();
			result = subject.Serialise(source);
			Console.WriteLine(result);
		}

		[Test]
		[TestCase("CorrelationId", "05c90feb5c1041799fc0d26dda5fd1c6")]
		[TestCase("Contents", "My message contents")]
		[TestCase("FilePath", @"C:\\work\\message")]
		[TestCase("MetadataName", "Mind the gap")]
		public void Should_serialise_its_properties(string expectedPropertyName, string expectedPropertyValue)
		{
			Assert.That(result, Contains.Substring("\"" + expectedPropertyName + "\":\"" + expectedPropertyValue + "\""));
		}

		[Test]
		public void Should_store_interface_stack_in_order_in_message ()
		{
			var contracts = InterfaceStack.Of(source);
			Assert.That(result, Contains.Substring("\"__contracts\":\"" + contracts + "\""));
		}

		[Test]
		public void Should_serialise_int_values_without_quotes()
		{
			Assert.That(result, Contains.Substring("\"HashValue\":123124512"));
		}

		[Test]
		public void Should_include_directly_implemented_interface_type ()
		{
			Assert.That(result, Contains.Substring("\"__type\":\"Example.Types.IMetadataFile"));
		}
	}
}

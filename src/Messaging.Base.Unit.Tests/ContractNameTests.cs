using BearBonesMessaging;
using Example.Types;
using NUnit.Framework;

namespace Messaging.Base.Unit.Tests
{
	[TestFixture]
	public class ContractNameTests
	{
		[Test]
		public void Should_get_full_name_of_single_interface_from_contract_name_method ()
		{
			var instance = new SuperMetadata();
			var result = MessagingBase.ContractTypeName(instance);

			Assert.That(result, Is.EqualTo("Example.Types.IMetadataFile"));
		}

		[Test]
		public void Should_get_full_name_of_single_interface_from_contract_name_using_type_of_interface ()
		{
			var result = MessagingBase.ContractTypeName(typeof(IMetadataFile));

			Assert.That(result, Is.EqualTo("Example.Types.IMetadataFile"));
		}

		[Test]
		public void Should_get_full_name_of_single_interface_from_contract_name_using_concrete_type ()
		{
			var result = MessagingBase.ContractTypeName(typeof(SuperMetadata));

			Assert.That(result, Is.EqualTo("Example.Types.IMetadataFile"));
		}
	}
}

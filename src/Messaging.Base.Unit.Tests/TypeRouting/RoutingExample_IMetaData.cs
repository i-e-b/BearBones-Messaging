using BearBonesMessaging.Routing;
using NSubstitute;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Unit.Tests.TypeRouting
{
	[TestFixture]
	public class RoutingExample_IMetaData
	{
		ITypeRouter subject;
		IMessageRouter router;

		[SetUp]
		public void A_routing_table_build_from_IMetadataFile()
		{
			router = Substitute.For<IMessageRouter>();

			subject = new TypeRouter(router);

			subject.BuildRoutes(typeof(Example.Types.IMetadataFile));
		}
        
        [Test]
        [TestCase("Example.Types.IMetadataFile", "Example.Types.IMetadataFile;Example.Types.IFile;Example.Types.IHash;Example.Types.IPath;Example.Types.IMsg")]
        [TestCase("Example.Types.IFile", "Example.Types.IFile;Example.Types.IHash;Example.Types.IPath;Example.Types.IMsg")]
        [TestCase("Example.Types.IHash", "Example.Types.IHash;Example.Types.IMsg")]
        [TestCase("Example.Types.IPath", "Example.Types.IPath;Example.Types.IMsg")]
        [TestCase("Example.Types.IMsg", "Example.Types.IMsg")]
        public void Should_create_source_for_each_interface_type(string interfaceFullType, string chainString)
        {
            router.Received().AddSource(interfaceFullType, chainString);
        }


		[Test]
		public void Should_link_IMetadataFile_to_IFile_with_IFile_as_parent()
		{
			router.Received().RouteSources("Example.Types.IMetadataFile", "Example.Types.IFile");
		}

		[Test]
		public void Should_link_IFile_to_IHash_with_IHash_as_parent()
		{
			router.Received().RouteSources("Example.Types.IFile", "Example.Types.IHash");
		}

		[Test]
		public void Should_link_IFile_to_IPath_with_IPath_as_parent()
		{
			router.Received().RouteSources("Example.Types.IFile", "Example.Types.IPath");
		}

		[Test]
		public void Should_link_IHash_to_IPath_with_IMsg_as_parent()
		{
			router.Received().RouteSources("Example.Types.IHash", "Example.Types.IMsg");
		}

		[Test]
		public void Should_link_IPath_to_IPath_with_IMsg_as_parent()
		{
			router.Received().RouteSources("Example.Types.IPath", "Example.Types.IMsg");
		}

		[Test]
		[TestCase("Example.Types.IMetadataFile")]
		[TestCase("Example.Types.IHash")]
		[TestCase("Example.Types.IPath")]
		[TestCase("Example.Types.IMsg")]
		public void Should_not_link_IMetadataFile_to_itself_or_indirect_antecendants(string antecendant)
		{
			router.DidNotReceive().RouteSources("Example.Types.IMetadataFile", antecendant);
		}

		[Test]
		[TestCase("Example.Types.IMetadataFile")]
		[TestCase("Example.Types.IFile")]
		[TestCase("Example.Types.IMsg")]
		public void Should_not_link_IFile_to_itself_or_parents_or_indirect_antecendants(string antecendant)
		{
			router.DidNotReceive().RouteSources("Example.Types.IFile", antecendant);
		}

		[Test]
		[TestCase("Example.Types.IMetadataFile")]
		[TestCase("Example.Types.IFile")]
		[TestCase("Example.Types.IHash")]
		[TestCase("Example.Types.IPath")]
		public void Should_not_link_IHash_to_itself_or_parents_or_indirect_antecendants(string antecendant)
		{
			router.DidNotReceive().RouteSources("Example.Types.IHash", antecendant);
		}

		[Test]
		[TestCase("Example.Types.IMetadataFile")]
		[TestCase("Example.Types.IFile")]
		[TestCase("Example.Types.IHash")]
		[TestCase("Example.Types.IPath")]
		public void Should_not_link_IPath_to_itself_or_parents_or_indirect_antecendants(string antecendant)
		{
			router.DidNotReceive().RouteSources("Example.Types.IPath", antecendant);
		}

		[Test]
		[TestCase("Example.Types.IMetadataFile")]
		[TestCase("Example.Types.IFile")]
		[TestCase("Example.Types.IHash")]
		[TestCase("Example.Types.IPath")]
		[TestCase("Example.Types.IMsg")]
		public void Should_not_link_IMsg_to_anything(string antecendant)
		{
			router.DidNotReceive().RouteSources("Example.Types.IMsg", antecendant);
		}
	}
}
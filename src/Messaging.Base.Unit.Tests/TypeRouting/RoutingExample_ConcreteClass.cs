using BearBonesMessaging.Routing;
using Example.Types;
using NSubstitute;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Unit.Tests.TypeRouting
{
	[TestFixture]
	public class RoutingExample_ConcreteClass
	{
		ITypeRouter subject;
		IMessageRouter router;

		[SetUp]
		public void A_routing_table_build_from_a_class_implementing_IMetadataFile()
		{
			router = Substitute.For<IMessageRouter>();
			subject = new TypeRouter(router);
			subject.BuildRoutes(typeof(SuperMetadata));
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
		public void Should_not_route_for_concrete_type ()
		{
			router.DidNotReceive().AddSource("Example.Types.SuperMetadata", Arg.Any<string>());
		}


	}
}

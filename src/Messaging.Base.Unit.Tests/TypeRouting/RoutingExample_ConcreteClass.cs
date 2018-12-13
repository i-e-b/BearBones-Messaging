using BearBonesMessaging.Routing;
using Example.Types;
using NSubstitute;
using NUnit.Framework;

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
		[TestCase("Example.Types.IMetadataFile")]
		[TestCase("Example.Types.IFile")]
		[TestCase("Example.Types.IHash")]
		[TestCase("Example.Types.IPath")]
		[TestCase("Example.Types.IMsg")]
		public void Should_create_source_for_each_interface_type(string interfaceFullType)
		{
			router.Received().AddSource(interfaceFullType);
		}

		[Test]
		public void Should_not_route_for_concrete_type ()
		{
			router.DidNotReceive().AddSource("Example.Types.SuperMetadata");
		}


	}
}

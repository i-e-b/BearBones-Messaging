﻿using NSubstitute;
using NUnit.Framework;
using SevenDigital.Messaging.Base;

namespace Messaging.Base.Unit.Tests
{
	[TestFixture]
	public class RoutingExample_ConcreteClass
	{
		ITypeStructureRouter subject;
		IMessageRouting router;

		[SetUp]
		public void A_routing_table_build_from_a_class_implementing_IMetadataFile()
		{
			router = Substitute.For<IMessageRouting>();
			subject = new TypeStructureRouter(router);
			subject.BuildRoutes<Example.Types.SuperMetadata>();
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

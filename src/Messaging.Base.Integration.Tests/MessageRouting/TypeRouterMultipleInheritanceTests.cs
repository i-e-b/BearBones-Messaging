using BearBonesMessaging.Routing;
using Example.Types;
using Messaging.Base.Integration.Tests.Helpers;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Integration.Tests.MessageRouting
{
	[TestFixture]
	public class TypeRouterMultipleInheritanceTests
	{
		ITypeRouter subject;
		IMessageRouter router;

		[SetUp]
		public void SetUp()
		{
			var longTermConnection = ConfigurationHelpers.ChannelWithAppConfigSettings();
			var shortTermConnection = ConfigurationHelpers.FreshConnectionFromAppConfig();
			router = new RabbitRouter(longTermConnection, shortTermConnection);
			subject = new TypeRouter(router);
		}

		[Test]
		public void When_sending_a_message_with_mulitple_inheritance_should_receive_one_copy_at_base_level()
		{
			subject.BuildRoutes(typeof(IFile));
			
			router.AddDestination("dst");
			router.Link("Example.Types.IMsg", "dst");

			router.Send("Example.Types.IFile", null, "Hello");

			Assert.That(router.GetAndFinish("dst", out _), Is.EqualTo("Hello"));
			Assert.That(router.GetAndFinish("dst", out _), Is.Null);
		}

		[TearDown]
		public void teardown()
		{
			((RabbitRouter)router).RemoveRouting(n=>true);
		}
	}
}
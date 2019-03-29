using System.Linq;
using BearBonesMessaging.RabbitMq;
using BearBonesMessaging.RabbitMq.RabbitMqManagement;
using BearBonesMessaging.Routing;
using Messaging.Base.Integration.Tests.Helpers;
using NUnit.Framework;

// ReSharper disable PossibleNullReferenceException
// ReSharper disable AssignNullToNotNullAttribute

namespace Messaging.Base.Integration.Tests.MessageRouting
{
	[TestFixture]
	public class DeclaringSourcesAndDestinationsThatAlreadyExist
	{
		private RabbitMqQuery query;
		private IMessageRouter router;
		IChannelAction connection;

		[SetUp]
		public void SetupApi()
		{
			query = ConfigurationHelpers.RabbitMqQueryWithConfigSettings();
			connection = ConfigurationHelpers.ChannelWithAppConfigSettings();
			var shortTermConnection = ConfigurationHelpers.FreshConnectionFromAppConfig();
			router = new RabbitRouter(connection, shortTermConnection);
		}

		[Test]
		public void If_I_add_a_destination_twice_I_get_one_destination_and_no_errors ()
		{
			router.AddDestination("B");
			router.AddDestination("B");

			Assert.That(query.ListDestinations().Count(e=>e.name == "B"), Is.EqualTo(1));
		}
		
		[Test]
		public void If_I_add_a_source_twice_I_get_one_source_and_no_errors ()
		{
			router.AddSource("S", null);
			router.AddSource("S", null);

			Assert.That(query.ListSources().Count(e=>e.name == "S"), Is.EqualTo(1));
		}

		[Test]
		public void If_I_make_a_link_twice_I_only_get_one_copy_of_each_message ()
		{
			router.AddSource("src", null);
			router.AddDestination("dst");

			router.Link("src", "dst");
			router.Link("src", "dst");

			router.Send("src", null, null, null, "Hello");

			Assert.That(router.GetAndFinish("dst", out _), Is.EqualTo("Hello"));
			Assert.That(router.GetAndFinish("dst", out _), Is.Null);
		}

		[Test]
		public void If_I_make_a_route_between_two_sources_twice_I_only_get_one_copy_of_each_message ()
		{
			router.AddSource("srcA", null);
			router.AddSource("srcB", null);
			router.AddDestination("dst");

			router.RouteSources("srcA", "srcB");
			router.RouteSources("srcA", "srcB");

			router.Link("srcB", "dst");
			router.Send("srcA", null, null, null, "Hello");

			Assert.That(router.GetAndFinish("dst", out _), Is.EqualTo("Hello"));
			Assert.That(router.GetAndFinish("dst", out _), Is.Null);
		}

        [Test]
        public void If_I_delete_a_route_between_a_source_and_destination_I_no_longer_get_new_messages () {
            
            router.AddSource("src_1", null);
            router.AddSource("src_2", null);
            router.AddDestination("dst");

            router.Link("src_1", "dst");
            router.Link("src_2", "dst");

            router.Unlink("src_1", "dst");

            router.Send("src_1", null, null, null, "Hello1");
            router.Send("src_2", null, null, null, "Hello2");
            
            Assert.That(router.GetAndFinish("dst", out _), Is.EqualTo("Hello2"));
            Assert.That(router.GetAndFinish("dst", out _), Is.Null);
        }

		[TearDown]
		public void CleanUp()
		{
			((RabbitRouter)router).RemoveRouting(n=>true);
		}
	}
}

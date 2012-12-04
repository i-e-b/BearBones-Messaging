﻿using System;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SevenDigital.Messaging.Base;
using SevenDigital.Messaging.Base.RabbitMq;
using SevenDigital.Messaging.Base.RabbitMq.RabbitMqManagement;

namespace Messaging.Base.Integration.Tests
{
	[TestFixture]
	public class RoutingMessagesOverRabbitMq
	{
		private RabbitMqQuery query;
		private IMessageRouting router;
		RabbitMqConnection connection;

		[SetUp]
		public void SetupApi()
		{
			query = ConfigurationHelpers.RabbitMqQueryWithConfigSettings();
			connection = ConfigurationHelpers.RabbitMqConnectionWithAppConfigSettings();
			router = new RabbitRouting(connection);
		}

		[Test]
		public void Router_can_remove_routing ()
		{
			router.AddSource("A");
			router.AddDestination("B");

			Assert.IsTrue(query.ListSources().Any(e=>e.name == "A"), "Exchange not created");
			Assert.IsTrue(query.ListDestinations().Any(e=>e.name == "B"), "Queue not created");

			((RabbitRouting)router).RemoveRouting();


			Assert.IsFalse(query.ListSources().Any(e=>e.name == "A"), "Exchange not cleared");
			Assert.IsFalse(query.ListDestinations().Any(e=>e.name == "B"), "Queue not cleared");
		}

		[Test]
		public void Should_not_be_able_to_route_a_source_to_itself()
		{
			router.AddSource("A");
			Assert.Throws<ArgumentException>(() => router.RouteSources("A", "A"));
		}

		[Test]
		public void Can_create_a_single_exchange_and_queue_and_send_a_simple_message()
		{
			router.AddSource("exchange_A");
			router.AddDestination("queue_A");
			router.Link("exchange_A", "queue_A");

			router.Send("exchange_A", "Hello, world");

			var message = router.Get("queue_A");
			Assert.That(message, Is.EqualTo("Hello, world"));
		}

		[Test]
		public void Can_pick_up_a_message_from_a_different_connection_after_its_acknowledged_but_not_received()
		{
			router.AddSource("src");
			router.AddDestination("dst");
			router.Link("src", "dst");
			router.Send("src", "Hello, World");

			connection.WithChannel(channel => channel.BasicAck(0, true));

			var conn2 = connection.ConnectionFactory().CreateConnection();
			var channel2 = conn2.CreateModel();
			var result = channel2.BasicGet("dst", false);
			var message = Encoding.UTF8.GetString(result.Body);
			Assert.That(message, Is.EqualTo("Hello, World"));
		}

		[Test]
		public void Can_route_a_message_to_two_queues()
		{
			router.AddSource("exchange");
			router.AddDestination("queue_A");
			router.AddDestination("queue_B");
			router.Link("exchange", "queue_A");
			router.Link("exchange", "queue_B");

			router.Send("exchange", "Hello, World");

			
			var message1 = router.Get("queue_A");
			var message2 = router.Get("queue_B");

			Assert.That(message1, Is.EqualTo("Hello, World"), "queue_A");
			Assert.That(message2, Is.EqualTo("Hello, World"), "queue_B");
		}

		[Test]
		public void Can_request_a_message_from_an_empty_destination ()
		{
			router.AddDestination("A");
			var result = router.Get("A");

			Assert.That(result, Is.Null);
		}

		[Test]
		public void When_sending_multiple_messages_they_are_picked_up_one_at_a_time_in_order ()
		{
			router.AddSource("exchange_A");
			router.AddDestination("queue_A");
			router.Link("exchange_A", "queue_A");

			router.Send("exchange_A", "One");
			router.Send("exchange_A", "Two");
			router.Send("exchange_A", "Three");
			router.Send("exchange_A", "Four");

			Assert.That(router.Get("queue_A"), Is.EqualTo("One"));
			Assert.That(router.Get("queue_A"), Is.EqualTo("Two"));
			Assert.That(router.Get("queue_A"), Is.EqualTo("Three"));
			Assert.That(router.Get("queue_A"), Is.EqualTo("Four"));
		}

		[Test]
		public void Can_route_a_hierarchy_of_keys ()
		{
			// Routing: (all sources)
			// H1╶┬ N1 \
			// H2╶┘     B
			// H3╶╴N2 /

			// Linking: (destination <- source)
			// D1 <- N1
			// D2 <- H2
			// D3 <- B

			router.AddSource("B");
			router.AddSource("N1");
			router.AddSource("N2");
			router.AddSource("H1");
			router.AddSource("H2");
			router.AddSource("H3");

			router.AddDestination("D1");
			router.AddDestination("D2");
			router.AddDestination("D3");

			router.RouteSources("N1", "B");
			router.RouteSources("N2", "B");
			router.RouteSources("H1", "N1");
			router.RouteSources("H2", "N1");
			router.RouteSources("H3", "N2");

			router.Link("N1", "D1");
			router.Link("H2", "D2");
			router.Link("B", "D3");

			router.Send("B", "D3 gets this");
			router.Send("H2", "D2, D1, D3 get this");
			router.Send("N1", "D1, D3 get this");

			Assert.That(router.Get("D3"), Is.EqualTo("D3 gets this"));
			Assert.That(router.Get("D3"), Is.EqualTo("D2, D1, D3 get this"));
			Assert.That(router.Get("D3"), Is.EqualTo("D1, D3 get this"));
			Assert.That(router.Get("D3"), Is.Null);

			Assert.That(router.Get("D2"), Is.EqualTo("D2, D1, D3 get this"));
			Assert.That(router.Get("D2"), Is.Null);

			Assert.That(router.Get("D1"), Is.EqualTo("D2, D1, D3 get this"));
			Assert.That(router.Get("D1"), Is.EqualTo("D1, D3 get this"));
			Assert.That(router.Get("D1"), Is.Null);
		}

		[Test, Explicit]
		public void Can_send_and_receive_1000_messages_synchronously_in_a_minute ()
		{
			router.AddSource("A");
			router.AddDestination("B");
			router.Link("A","B");

			var start = DateTime.Now;
			int received = 0;
			for (int i = 0; i < 1000; i++)
			{
				router.Send("A", "Woo");
			}
			while (router.Get("B") != null) received++;

			var time = (DateTime.Now) - start;
			Assert.That(received, Is.EqualTo(1000));
			Assert.That(time.TotalSeconds, Is.LessThanOrEqualTo(60));
		}
		
		[Test, Explicit]
		public void Multithreaded_get_does_not_provide_duplicate_messages ()
		{
			router.AddSource("A");
			router.AddDestination("B");
			router.Link("A","B");

			int[] received = {0};
			int count = 200;
			for (int i = 0; i < count; i++)
			{
				router.Send("A", "message");
			}

			var A = new Thread(()=> {
				while (router.Get("B") != null) Interlocked.Increment(ref received[0]);
			});
			var B = new Thread(()=> {
				while (router.Get("B") != null) Interlocked.Increment(ref received[0]);
			});
			var C = new Thread(()=> {
				while (router.Get("B") != null) Interlocked.Increment(ref received[0]);
			});

			A.Start();
			B.Start();
			C.Start();
			A.Join();
			B.Join();
			C.Join();

			Console.WriteLine(received[0]);
			Assert.That(received[0], Is.EqualTo(count));
		}

		[TearDown]
		public void CleanUp()
		{
			((RabbitRouting)router).RemoveRouting();
		}
	}
}

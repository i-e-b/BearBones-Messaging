using BearBonesMessaging.Routing;
using Example.Types;
using Messaging.Base.Integration.Tests.Helpers;
using NUnit.Framework;
// ReSharper disable PossibleNullReferenceException

namespace Messaging.Base.Integration.Tests.MessageRouting
{
	[TestFixture]
	public class RetryingMessagesTests
	{
		ITypeRouter typeRouter;
		IMessageRouter subject;

		[SetUp]
		public void SetUp()
		{
			var longTermConnection = ConfigurationHelpers.ChannelWithAppConfigSettings();
			var shortTermConnection = ConfigurationHelpers.FreshConnectionFromAppConfig();
			subject = new RabbitRouter(longTermConnection, shortTermConnection);

			typeRouter = new TypeRouter(subject);
			typeRouter.BuildRoutes(typeof(IFile));

			subject.AddDestination("dst");
			subject.Link("Example.Types.IMsg", "dst");
			subject.Send("Example.Types.IFile", null, "Hello");
		}

		[Test]
		public void cant_get_a_message_twice_even_if_its_not_finished()
		{
			Assert.That(subject.Get("dst", out var tag1), Is.EqualTo("Hello"));
			Assert.That(subject.Get("dst", out _), Is.Null);

			subject.Finish(tag1.DeliveryTag);
		}

		[Test]
		public void can_cancel_a_message_making_it_available_again()
		{
			Assert.That(subject.Get("dst", out var tag1), Is.EqualTo("Hello"));
			Assert.That(subject.Get("dst", out _), Is.Null);

			subject.Cancel(tag1.DeliveryTag);
			Assert.That(subject.Get("dst", out var tag2), Is.EqualTo("Hello"));

			subject.Finish(tag2.DeliveryTag);
		}

		[Test]
		public void cancelled_messages_return_to_the_head_of_the_queue()
		{

			Assert.That(subject.Get("dst", out var tag1), Is.EqualTo("Hello"));
			subject.Send("Example.Types.IFile", null, "SecondMessage");

			subject.Cancel(tag1.DeliveryTag);
			Assert.That(subject.Get("dst", out tag1), Is.EqualTo("Hello"));
			Assert.That(subject.Get("dst", out var tag2), Is.EqualTo("SecondMessage"));

			subject.Finish(tag1.DeliveryTag);
			subject.Finish(tag2.DeliveryTag);
		}


		[Test]
		public void with_two_messages_waiting_and_one_is_in_progress_the_other_can_be_picked_up()
		{
			subject.Send("Example.Types.IFile", null, "SecondMessage");

			Assert.That(subject.Get("dst", out var tag1), Is.EqualTo("Hello"));
			Assert.That(subject.Get("dst", out var tag2), Is.EqualTo("SecondMessage"));

			subject.Finish(tag1.DeliveryTag);
			subject.Finish(tag2.DeliveryTag);
		}
	}
}
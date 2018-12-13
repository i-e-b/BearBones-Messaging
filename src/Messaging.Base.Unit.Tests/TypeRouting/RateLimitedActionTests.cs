using System;
using System.Threading;
using BearBonesMessaging;
using NUnit.Framework;

// ReSharper disable RedundantAssignment

namespace Messaging.Base.Unit.Tests.TypeRouting
{
	[TestFixture]
	public class RateLimitedActionTests
	{
		[Test]
		public void A_timed_cache_calls_source_on_first_use ()
		{
			int calls = 0;
			var subject = RateLimitedAction.Of(() => {
				calls++;
			});

			subject.YoungerThan(TimeSpan.Zero);

			Assert.That(calls, Is.EqualTo(1));
		}

		[Test]
		public void A_timed_cache_doesnt_call_source_on_second_use_within_duration ()
		{
			int calls = 0;
			var subject = RateLimitedAction.Of(() => {
				calls++;
			});

			subject.YoungerThan(TimeSpan.FromMinutes(1));
			subject.YoungerThan(TimeSpan.FromMinutes(1));

			Assert.That(calls, Is.EqualTo(1));
		}

		[Test]
		public void A_timed_cache_calls_source_on_second_use_outside_of_duration ()
		{
			int calls = 0;
			var subject = RateLimitedAction.Of(() => {
				calls++;
			});

			subject.YoungerThan(TimeSpan.FromSeconds(0.5));
			Thread.Sleep(750);
			subject.YoungerThan(TimeSpan.FromSeconds(0.5));

			Assert.That(calls, Is.EqualTo(2));
		}
	}
}

using System;
using JetBrains.Annotations;

namespace SevenDigital.Messaging.Base
{
	/// <summary>
	/// Helper to perform actions that should not be repeated often
	/// </summary>
	public class RateLimitedAction
	{
		[NotNull] private readonly Action _action;
		private DateTime _lastRecall;

		private RateLimitedAction(Action actionToPerform)
		{
			_action = actionToPerform ?? throw new ArgumentNullException(nameof(actionToPerform));
			_lastRecall = DateTime.MinValue;
		}

		/// <summary>
		/// Set action to perform.
		/// </summary>
		public static RateLimitedAction Of(Action actionToPerform)
		{
			return new RateLimitedAction(actionToPerform);
		}

		/// <summary>
		/// Perform the action if not performed within the given age.
		/// </summary>
		public void YoungerThan(TimeSpan Age)
		{
			if ((DateTime.Now - _lastRecall) <= Age) return;
			_action();
			_lastRecall = DateTime.Now;
		}
	}
}

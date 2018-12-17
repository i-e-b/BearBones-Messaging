using System;
using JetBrains.Annotations;

namespace BearBonesMessaging
{
    /// <summary>
    /// Helpers for expiry times
    /// </summary>
    public class Expires {
        /// <summary>
        /// Expiry duration in milliseconds
        /// </summary>
        public readonly long Milliseconds;

        /// <summary>
        /// Expires after the given number of milliseconds
        /// </summary>
        /// <param name="milliseconds"></param>
        protected Expires(long milliseconds)
        {
            Milliseconds = milliseconds;
        }

        /// <summary>
        /// Messages never expire, and will be stored in a queue until they are picked up
        /// </summary>
        [NotNull] public static Expires Never {get{ return new Expires(-1); } }

        /// <summary>
        /// Messages expire after a given number of milliseconds
        /// </summary>
        [NotNull] public static Expires AfterMilliseconds(long ms) {
            return new Expires(ms);
        }

        /// <summary>
        /// Messages expire after a given duration
        /// </summary>
        [NotNull] public static Expires After(TimeSpan time) {
            return new Expires((long) time.TotalMilliseconds);
        }
    }
}
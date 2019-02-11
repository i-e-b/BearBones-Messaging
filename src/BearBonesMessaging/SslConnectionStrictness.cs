namespace BearBonesMessaging
{
    /// <summary>
    /// Indicate how strict the checks for SSL certificates should be
    /// </summary>
    public enum SslConnectionStrictness
    {
        /// <summary>
        /// Certificate must be correct and valid, from a machine-level trusted certificate chain
        /// </summary>
        Strict = 0,

        /// <summary>
        /// Certificate must be valid, but self-signed certificates will be accepted.
        /// This setting should only be used on test and development environments.
        /// </summary>
        Relaxed = 1
    }
}
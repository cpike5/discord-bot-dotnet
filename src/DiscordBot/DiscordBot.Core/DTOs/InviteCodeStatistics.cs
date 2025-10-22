namespace DiscordBot.Core.DTOs
{
    /// <summary>
    /// Data transfer object containing statistical counts for invite codes.
    /// Used for dashboard display in the admin invite code management interface.
    /// </summary>
    public class InviteCodeStatistics
    {
        /// <summary>
        /// Number of invite codes that are active (not used and not expired).
        /// </summary>
        public int ActiveCount { get; set; }

        /// <summary>
        /// Number of invite codes that have been used for registration.
        /// </summary>
        public int UsedCount { get; set; }

        /// <summary>
        /// Number of invite codes that have expired without being used.
        /// </summary>
        public int ExpiredCount { get; set; }

        /// <summary>
        /// Number of invite codes that were manually revoked.
        /// Counts as expired codes that were revoked before natural expiration.
        /// </summary>
        public int RevokedCount { get; set; }
    }
}

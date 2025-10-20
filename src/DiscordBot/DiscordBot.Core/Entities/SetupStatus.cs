namespace DiscordBot.Core.Entities
{
    /// <summary>
    /// Tracks the completion status of the first-time setup process.
    /// Only one record should exist in this table.
    /// </summary>
    public class SetupStatus
    {
        /// <summary>
        /// Primary key. Should always be 1.
        /// </summary>
        public int Id { get; set; } = 1;

        /// <summary>
        /// Indicates whether the initial setup has been completed.
        /// </summary>
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// Timestamp when setup was completed (UTC).
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// User ID of the admin account created during setup.
        /// </summary>
        public string? AdminUserId { get; set; }

        /// <summary>
        /// Version of setup that was completed (for future migrations).
        /// </summary>
        public string SetupVersion { get; set; } = "1.0";

        /// <summary>
        /// Navigation property to admin user.
        /// </summary>
        public virtual ApplicationUser? AdminUser { get; set; }
    }
}

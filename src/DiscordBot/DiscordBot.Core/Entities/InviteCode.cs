namespace DiscordBot.Core.Entities
{
    /// <summary>
    /// Represents a time-limited invite code for Discord user registration.
    /// Generated when a Discord user runs the /register command.
    /// </summary>
    public class InviteCode
    {
        /// <summary>
        /// Unique identifier for this invite code record.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The invite code string (format: XXXX-XXXX-XXXX).
        /// Cryptographically secure, unique across all codes.
        /// </summary>
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// Discord user ID (snowflake) of the user who requested this code.
        /// </summary>
        public ulong DiscordUserId { get; set; }

        /// <summary>
        /// Discord username at the time of code generation.
        /// Cached for audit trail purposes.
        /// </summary>
        public string DiscordUsername { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp when the code was generated (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp when the code expires (UTC).
        /// Typically 24 hours after creation.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Indicates whether this code has been used for registration.
        /// </summary>
        public bool IsUsed { get; set; } = false;

        /// <summary>
        /// Timestamp when the code was used for registration (UTC).
        /// Null if not yet used.
        /// </summary>
        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// Application user ID who used this code for registration.
        /// Foreign key to ApplicationUser.
        /// Null if not yet used.
        /// </summary>
        public string? UsedByApplicationUserId { get; set; }

        /// <summary>
        /// Navigation property to the ApplicationUser who used this code.
        /// </summary>
        public virtual ApplicationUser? UsedByApplicationUser { get; set; }

        #region Business Logic Helpers

        /// <summary>
        /// Indicates whether this code is currently valid (not used and not expired).
        /// </summary>
        public bool IsValid => !IsUsed && ExpiresAt > DateTime.UtcNow;

        /// <summary>
        /// Indicates whether this code has expired (regardless of usage).
        /// </summary>
        public bool IsExpired => ExpiresAt <= DateTime.UtcNow;

        #endregion
    }
}

using Microsoft.AspNetCore.Identity;

namespace DiscordBot.Core.Entities
{
    /// <summary>
    /// Application user entity with Discord integration support.
    /// Extends ASP.NET Identity user with Discord account linking and audit fields.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        #region Discord Integration

        /// <summary>
        /// Discord user ID (snowflake). Unique identifier for linked Discord account.
        /// Null if Discord account is not yet linked.
        /// </summary>
        public ulong? DiscordUserId { get; set; }

        /// <summary>
        /// Current Discord username (without discriminator).
        /// Updated periodically via sync jobs.
        /// </summary>
        public string? DiscordUsername { get; set; }

        /// <summary>
        /// Discord discriminator (legacy format: #0001-9999).
        /// Note: Discord is phasing out discriminators in favor of unique usernames.
        /// </summary>
        public string? DiscordDiscriminator { get; set; }

        /// <summary>
        /// Discord avatar hash for constructing avatar URLs.
        /// Format: https://cdn.discordapp.com/avatars/{userId}/{avatarHash}.png
        /// </summary>
        public string? DiscordAvatarHash { get; set; }

        /// <summary>
        /// Timestamp when the Discord account was linked to this application account.
        /// Null if never linked.
        /// </summary>
        public DateTime? DiscordLinkedAt { get; set; }

        #endregion

        #region Audit Fields

        /// <summary>
        /// Timestamp when the user account was created.
        /// Always in UTC.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Timestamp of the user's last successful login.
        /// Used for activity tracking and security auditing.
        /// </summary>
        public DateTime? LastLoginAt { get; set; }

        #endregion

        #region Relationships

        /// <summary>
        /// Collection of invite codes generated for this user.
        /// Used when Discord user requests registration codes.
        /// </summary>
        public virtual ICollection<InviteCode> GeneratedInviteCodes { get; set; } = new List<InviteCode>();

        #endregion

        #region Business Logic Helpers

        /// <summary>
        /// Indicates whether this user has a linked Discord account.
        /// </summary>
        public bool IsDiscordLinked => DiscordUserId.HasValue;

        /// <summary>
        /// Display name for UI purposes.
        /// Prefers Discord username, falls back to application username, email, or "Unknown".
        /// </summary>
        public string DisplayName => DiscordUsername ?? UserName ?? Email ?? "Unknown";

        /// <summary>
        /// Full Discord tag (username#discriminator) if available.
        /// Returns just username if discriminator is not set (new Discord format).
        /// </summary>
        public string? DiscordTag => DiscordUsername != null && DiscordDiscriminator != null
            ? $"{DiscordUsername}#{DiscordDiscriminator}"
            : DiscordUsername;

        /// <summary>
        /// Constructs the Discord avatar URL if avatar hash is available.
        /// Returns null if user has no custom avatar.
        /// </summary>
        public string? DiscordAvatarUrl => DiscordUserId.HasValue && !string.IsNullOrEmpty(DiscordAvatarHash)
            ? $"https://cdn.discordapp.com/avatars/{DiscordUserId}/{DiscordAvatarHash}.png"
            : null;

        #endregion
    }
}

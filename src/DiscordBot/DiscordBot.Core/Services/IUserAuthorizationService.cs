using DiscordBot.Core.Entities;

namespace DiscordBot.Core.Services
{
    /// <summary>
    /// Service interface for Discord user authorization and role management.
    /// Bridges Discord user IDs with ASP.NET Identity roles and permissions.
    /// </summary>
    public interface IUserAuthorizationService
    {
        /// <summary>
        /// Retrieves an application user by their Discord user ID.
        /// </summary>
        /// <param name="discordUserId">Discord user ID (snowflake)</param>
        /// <returns>ApplicationUser if linked, null otherwise</returns>
        Task<ApplicationUser?> GetUserByDiscordIdAsync(ulong discordUserId);

        /// <summary>
        /// Checks if a Discord user is in a specific role.
        /// </summary>
        /// <param name="discordUserId">Discord user ID</param>
        /// <param name="roleName">Name of the role to check</param>
        /// <returns>True if user is linked and in the role, false otherwise</returns>
        Task<bool> IsInRoleAsync(ulong discordUserId, string roleName);

        /// <summary>
        /// Retrieves all roles for a Discord user.
        /// </summary>
        /// <param name="discordUserId">Discord user ID</param>
        /// <returns>Collection of role names, empty if user not linked</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(ulong discordUserId);

        /// <summary>
        /// Checks if a Discord user has a linked application account.
        /// </summary>
        /// <param name="discordUserId">Discord user ID</param>
        /// <returns>True if account is linked, false otherwise</returns>
        Task<bool> IsLinkedAsync(ulong discordUserId);

        /// <summary>
        /// Links a Discord account to an existing application user.
        /// Updates the user's Discord information and sets the linked timestamp.
        /// </summary>
        /// <param name="applicationUserId">Application user ID to link</param>
        /// <param name="discordUserId">Discord user ID</param>
        /// <param name="discordUsername">Discord username</param>
        /// <param name="discordDiscriminator">Discord discriminator (optional, deprecated by Discord)</param>
        /// <param name="discordAvatarHash">Discord avatar hash (optional)</param>
        /// <returns>True if successfully linked, false otherwise</returns>
        Task<bool> LinkDiscordAccountAsync(
            string applicationUserId,
            ulong discordUserId,
            string discordUsername,
            string? discordDiscriminator = null,
            string? discordAvatarHash = null);

        /// <summary>
        /// Unlinks a Discord account from an application user.
        /// Removes Discord user ID and related information.
        /// </summary>
        /// <param name="applicationUserId">Application user ID to unlink</param>
        /// <returns>True if successfully unlinked, false otherwise</returns>
        Task<bool> UnlinkDiscordAccountAsync(string applicationUserId);

        /// <summary>
        /// Updates the last login timestamp for a user.
        /// </summary>
        /// <param name="applicationUserId">Application user ID</param>
        /// <returns>True if successfully updated, false otherwise</returns>
        Task<bool> UpdateLastLoginAsync(string applicationUserId);

        /// <summary>
        /// Invalidates the role cache for a specific Discord user.
        /// Should be called when roles are modified.
        /// </summary>
        /// <param name="discordUserId">Discord user ID</param>
        void InvalidateRoleCache(ulong discordUserId);

        /// <summary>
        /// Invalidates the entire role cache.
        /// Should be called for bulk role changes.
        /// </summary>
        void InvalidateAllRoleCache();
    }
}

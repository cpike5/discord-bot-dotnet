using DiscordBot.Core.DTOs;
using DiscordBot.Core.Entities;

namespace DiscordBot.Core.Services
{
    /// <summary>
    /// Service interface for managing invite code generation, validation, and lifecycle.
    /// Handles the business logic for Discord user registration codes.
    /// </summary>
    public interface IInviteCodeService
    {
        /// <summary>
        /// Generates a new invite code for a Discord user.
        /// If the user already has an active (unused, non-expired) code, returns that instead.
        /// </summary>
        /// <param name="discordUserId">Discord user ID (snowflake)</param>
        /// <param name="discordUsername">Discord username for audit trail</param>
        /// <param name="expirationHours">Hours until code expires (default: 24)</param>
        /// <returns>The generated or existing active invite code</returns>
        /// <exception cref="InvalidOperationException">Thrown if user is already registered</exception>
        Task<InviteCode> GenerateCodeAsync(ulong discordUserId, string discordUsername, int expirationHours = 24);

        /// <summary>
        /// Validates an invite code for registration.
        /// Checks if code exists, is not used, and is not expired.
        /// </summary>
        /// <param name="code">The invite code to validate (format: XXXX-XXXX-XXXX)</param>
        /// <returns>The valid InviteCode entity, or null if invalid</returns>
        Task<InviteCode?> ValidateCodeAsync(string code);

        /// <summary>
        /// Marks an invite code as used and associates it with an application user.
        /// </summary>
        /// <param name="code">The invite code to mark as used</param>
        /// <param name="applicationUserId">The ID of the ApplicationUser who used the code</param>
        /// <returns>True if successfully marked as used, false otherwise</returns>
        Task<bool> MarkCodeAsUsedAsync(string code, string applicationUserId);

        /// <summary>
        /// Retrieves an active (unused, non-expired) invite code for a Discord user.
        /// </summary>
        /// <param name="discordUserId">Discord user ID</param>
        /// <returns>Active invite code if exists, null otherwise</returns>
        Task<InviteCode?> GetActiveCodeForUserAsync(ulong discordUserId);

        /// <summary>
        /// Retrieves all invite codes for a Discord user (for admin purposes).
        /// </summary>
        /// <param name="discordUserId">Discord user ID</param>
        /// <returns>Collection of all invite codes for the user</returns>
        Task<IEnumerable<InviteCode>> GetAllCodesForUserAsync(ulong discordUserId);

        /// <summary>
        /// Retrieves all active invite codes (for admin purposes).
        /// </summary>
        /// <returns>Collection of all unused, non-expired invite codes</returns>
        Task<IEnumerable<InviteCode>> GetAllActiveCodesAsync();

        /// <summary>
        /// Revokes (marks as expired) an invite code.
        /// </summary>
        /// <param name="code">The invite code to revoke</param>
        /// <returns>True if successfully revoked, false if code not found or already used</returns>
        Task<bool> RevokeCodeAsync(string code);

        /// <summary>
        /// Checks if a Discord user already has a linked application account.
        /// </summary>
        /// <param name="discordUserId">Discord user ID</param>
        /// <returns>True if user is already registered, false otherwise</returns>
        Task<bool> IsUserAlreadyRegisteredAsync(ulong discordUserId);

        /// <summary>
        /// Cleans up expired invite codes older than the specified number of days.
        /// </summary>
        /// <param name="daysOld">Delete expired codes older than this many days (default: 7)</param>
        /// <returns>Number of codes deleted</returns>
        Task<int> CleanupExpiredCodesAsync(int daysOld = 7);

        /// <summary>
        /// Retrieves statistical counts for invite codes (active, used, expired, revoked).
        /// Used for dashboard display in the admin interface.
        /// </summary>
        /// <returns>Statistics object containing counts for different code states</returns>
        Task<InviteCodeStatistics> GetStatisticsAsync();

        /// <summary>
        /// Retrieves a paginated list of invite codes with optional filtering and search.
        /// </summary>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="statusFilter">Filter by status: "active", "used", "expired", or null for all</param>
        /// <param name="searchTerm">Search term to filter by code or Discord username</param>
        /// <returns>Tuple containing the list of codes and total count for pagination</returns>
        Task<(IEnumerable<InviteCode> Codes, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? statusFilter = null, string? searchTerm = null);
    }
}

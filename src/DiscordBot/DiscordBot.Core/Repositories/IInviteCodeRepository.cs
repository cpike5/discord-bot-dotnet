using DiscordBot.Core.Entities;

namespace DiscordBot.Core.Repositories
{
    /// <summary>
    /// Repository interface for invite code data access operations.
    /// Provides abstraction layer between business logic and data persistence.
    /// </summary>
    public interface IInviteCodeRepository
    {
        /// <summary>
        /// Retrieves an invite code by its code string.
        /// </summary>
        /// <param name="code">The invite code string</param>
        /// <returns>InviteCode entity if found, null otherwise</returns>
        Task<InviteCode?> GetByCodeAsync(string code);

        /// <summary>
        /// Retrieves all invite codes for a specific Discord user.
        /// </summary>
        /// <param name="discordUserId">Discord user ID</param>
        /// <returns>Collection of invite codes</returns>
        Task<IEnumerable<InviteCode>> GetByDiscordUserIdAsync(ulong discordUserId);

        /// <summary>
        /// Retrieves the active (unused, non-expired) invite code for a Discord user.
        /// </summary>
        /// <param name="discordUserId">Discord user ID</param>
        /// <returns>Active invite code if exists, null otherwise</returns>
        Task<InviteCode?> GetActiveCodeByDiscordUserIdAsync(ulong discordUserId);

        /// <summary>
        /// Retrieves all active invite codes across all users.
        /// </summary>
        /// <returns>Collection of active invite codes</returns>
        Task<IEnumerable<InviteCode>> GetAllActiveCodesAsync();

        /// <summary>
        /// Adds a new invite code to the database.
        /// </summary>
        /// <param name="inviteCode">The invite code entity to add</param>
        /// <returns>The added invite code with generated ID</returns>
        Task<InviteCode> AddAsync(InviteCode inviteCode);

        /// <summary>
        /// Updates an existing invite code in the database.
        /// </summary>
        /// <param name="inviteCode">The invite code entity to update</param>
        /// <returns>The updated invite code</returns>
        Task<InviteCode> UpdateAsync(InviteCode inviteCode);

        /// <summary>
        /// Deletes expired invite codes older than the specified date.
        /// </summary>
        /// <param name="olderThan">Delete codes with ExpiresAt before this date</param>
        /// <returns>Number of codes deleted</returns>
        Task<int> DeleteExpiredCodesAsync(DateTime olderThan);

        /// <summary>
        /// Checks if a specific invite code exists in the database.
        /// </summary>
        /// <param name="code">The invite code string to check</param>
        /// <returns>True if exists, false otherwise</returns>
        Task<bool> CodeExistsAsync(string code);
    }
}

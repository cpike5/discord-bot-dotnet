using DiscordBot.Core.Entities;

namespace DiscordBot.Core.Services
{
    /// <summary>
    /// Service interface for administrative maintenance tasks and app setup.
    /// Provides operations for bootstrapping the application and performing
    /// maintenance tasks that require elevated privileges.
    /// </summary>
    public interface IAdminMaintenanceService
    {
        #region Admin Bootstrap

        /// <summary>
        /// Promotes a user to administrator role by email address.
        /// This is typically used for initial setup or emergency access.
        /// </summary>
        /// <param name="email">Email address of the user to promote</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> PromoteUserToAdminByEmailAsync(string email);

        /// <summary>
        /// Promotes a user to administrator role by username.
        /// This is typically used for initial setup or emergency access.
        /// </summary>
        /// <param name="username">Username of the user to promote</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> PromoteUserToAdminByUsernameAsync(string username);

        /// <summary>
        /// Promotes a user to administrator role by Discord ID.
        /// This is typically used for initial setup when the user has already
        /// linked their Discord account.
        /// </summary>
        /// <param name="discordId">Discord user ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> PromoteUserToAdminByDiscordIdAsync(ulong discordId);

        /// <summary>
        /// Promotes a user to administrator role by their application user ID.
        /// </summary>
        /// <param name="userId">Application user ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> PromoteUserToAdminByUserIdAsync(string userId);

        #endregion

        #region Role Initialization

        /// <summary>
        /// Ensures all required application roles exist in the database.
        /// Creates missing roles if they don't exist.
        /// </summary>
        /// <returns>List of roles that were created (empty if all existed)</returns>
        Task<IEnumerable<string>> EnsureRolesExistAsync();

        /// <summary>
        /// Gets a list of all application roles.
        /// </summary>
        /// <returns>Collection of role names</returns>
        Task<IEnumerable<string>> GetAllRolesAsync();

        #endregion

        #region Discord Account Linking

        /// <summary>
        /// Creates a new admin user account with Discord account pre-linked.
        /// This is useful for initial setup when you know your Discord ID.
        /// </summary>
        /// <param name="email">Email address for the new account</param>
        /// <param name="username">Username for the new account</param>
        /// <param name="password">Password for the new account</param>
        /// <param name="discordId">Discord user ID to link</param>
        /// <param name="discordUsername">Optional Discord username for display</param>
        /// <returns>The created user if successful, null otherwise</returns>
        Task<ApplicationUser?> CreateAdminWithDiscordAsync(
            string email,
            string username,
            string password,
            ulong discordId,
            string? discordUsername = null);

        /// <summary>
        /// Links a Discord account to an existing user account.
        /// This manually creates the link without requiring the invite code flow.
        /// </summary>
        /// <param name="userId">Application user ID</param>
        /// <param name="discordId">Discord user ID to link</param>
        /// <param name="discordUsername">Optional Discord username for display</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> LinkDiscordAccountAsync(string userId, ulong discordId, string? discordUsername = null);

        /// <summary>
        /// Links a Discord account to an existing user by email.
        /// </summary>
        /// <param name="email">Email address of the user</param>
        /// <param name="discordId">Discord user ID to link</param>
        /// <param name="discordUsername">Optional Discord username for display</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> LinkDiscordAccountByEmailAsync(string email, ulong discordId, string? discordUsername = null);

        /// <summary>
        /// Unlinks a Discord account from a user.
        /// </summary>
        /// <param name="userId">Application user ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UnlinkDiscordAccountAsync(string userId);

        #endregion

        #region System Diagnostics

        /// <summary>
        /// Gets system health information including admin count, total users, etc.
        /// </summary>
        /// <returns>Dictionary of diagnostic information</returns>
        Task<Dictionary<string, object>> GetSystemDiagnosticsAsync();

        /// <summary>
        /// Checks if the system has at least one administrator account.
        /// </summary>
        /// <returns>True if at least one admin exists, false otherwise</returns>
        Task<bool> HasAdminAccountAsync();

        #endregion
    }
}

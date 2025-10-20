using DiscordBot.Core.Entities;

namespace DiscordBot.Core.Services
{
    /// <summary>
    /// Service interface for managing application users.
    /// Provides CRUD operations and user-related business logic.
    /// </summary>
    public interface IUserManagementService
    {
        #region User Retrieval

        /// <summary>
        /// Retrieves all users with optional pagination.
        /// </summary>
        /// <param name="skip">Number of users to skip</param>
        /// <param name="take">Number of users to take</param>
        /// <returns>Collection of application users</returns>
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(int skip = 0, int take = 100);

        /// <summary>
        /// Retrieves a user by their application user ID.
        /// </summary>
        /// <param name="userId">Application user ID</param>
        /// <returns>ApplicationUser if found, null otherwise</returns>
        Task<ApplicationUser?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Retrieves a user by their email address.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <returns>ApplicationUser if found, null otherwise</returns>
        Task<ApplicationUser?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        /// <param name="username">Username</param>
        /// <returns>ApplicationUser if found, null otherwise</returns>
        Task<ApplicationUser?> GetUserByUsernameAsync(string username);

        /// <summary>
        /// Searches for users matching a query string.
        /// Searches email, username, and Discord username.
        /// </summary>
        /// <param name="query">Search query</param>
        /// <param name="skip">Number of users to skip</param>
        /// <param name="take">Number of users to take</param>
        /// <returns>Collection of matching users</returns>
        Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string query, int skip = 0, int take = 100);

        /// <summary>
        /// Gets the total count of users.
        /// </summary>
        /// <returns>Total number of users</returns>
        Task<int> GetUserCountAsync();

        /// <summary>
        /// Gets users with linked Discord accounts.
        /// </summary>
        /// <param name="skip">Number of users to skip</param>
        /// <param name="take">Number of users to take</param>
        /// <returns>Collection of users with Discord links</returns>
        Task<IEnumerable<ApplicationUser>> GetDiscordLinkedUsersAsync(int skip = 0, int take = 100);

        #endregion

        #region User Management

        /// <summary>
        /// Creates a new user account.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>Created user if successful, null otherwise</returns>
        Task<ApplicationUser?> CreateUserAsync(string email, string username, string password);

        /// <summary>
        /// Updates user profile information.
        /// </summary>
        /// <param name="userId">User ID to update</param>
        /// <param name="email">New email (optional)</param>
        /// <param name="username">New username (optional)</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateUserAsync(string userId, string? email = null, string? username = null);

        /// <summary>
        /// Deletes a user account.
        /// </summary>
        /// <param name="userId">User ID to delete</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteUserAsync(string userId);

        /// <summary>
        /// Resets a user's password.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="newPassword">New password</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> ResetPasswordAsync(string userId, string newPassword);

        /// <summary>
        /// Locks a user account.
        /// </summary>
        /// <param name="userId">User ID to lock</param>
        /// <param name="lockoutEnd">When the lockout ends (null for permanent)</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> LockUserAsync(string userId, DateTimeOffset? lockoutEnd = null);

        /// <summary>
        /// Unlocks a user account.
        /// </summary>
        /// <param name="userId">User ID to unlock</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UnlockUserAsync(string userId);

        #endregion

        #region Role Management

        /// <summary>
        /// Gets all roles assigned to a user.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <returns>Collection of role names</returns>
        Task<IEnumerable<string>> GetUserRolesAsync(string userId);

        /// <summary>
        /// Adds a user to a role.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleName">Role name</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> AddUserToRoleAsync(string userId, string roleName);

        /// <summary>
        /// Removes a user from a role.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleName">Role name</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RemoveUserFromRoleAsync(string userId, string roleName);

        /// <summary>
        /// Checks if a user is in a specific role.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleName">Role name</param>
        /// <returns>True if user is in role, false otherwise</returns>
        Task<bool> IsInRoleAsync(string userId, string roleName);

        #endregion

        #region Statistics

        /// <summary>
        /// Gets user statistics.
        /// </summary>
        /// <returns>Dictionary of statistics</returns>
        Task<Dictionary<string, object>> GetUserStatisticsAsync();

        #endregion
    }
}

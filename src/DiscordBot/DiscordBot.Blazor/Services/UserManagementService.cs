using DiscordBot.Blazor.Data;
using DiscordBot.Core.Entities;
using DiscordBot.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Blazor.Services
{
    /// <summary>
    /// Service implementation for managing application users.
    /// </summary>
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserManagementService> _logger;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<UserManagementService> logger)
        {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        #region User Retrieval

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync(int skip = 0, int take = 100)
        {
            try
            {
                return await _userManager.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all users");
                return Enumerable.Empty<ApplicationUser>();
            }
        }

        public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
        {
            try
            {
                return await _userManager.FindByIdAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by ID: {UserId}", userId);
                return null;
            }
        }

        public async Task<ApplicationUser?> GetUserByEmailAsync(string email)
        {
            try
            {
                return await _userManager.FindByEmailAsync(email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by email: {Email}", email);
                return null;
            }
        }

        public async Task<ApplicationUser?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _userManager.FindByNameAsync(username);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username: {Username}", username);
                return null;
            }
        }

        public async Task<IEnumerable<ApplicationUser>> SearchUsersAsync(string query, int skip = 0, int take = 100)
        {
            try
            {
                var lowerQuery = query.ToLower();
                return await _userManager.Users
                    .Where(u =>
                        (u.Email != null && u.Email.ToLower().Contains(lowerQuery)) ||
                        (u.UserName != null && u.UserName.ToLower().Contains(lowerQuery)) ||
                        (u.DiscordUsername != null && u.DiscordUsername.ToLower().Contains(lowerQuery)))
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with query: {Query}", query);
                return Enumerable.Empty<ApplicationUser>();
            }
        }

        public async Task<int> GetUserCountAsync()
        {
            try
            {
                return await _userManager.Users.CountAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user count");
                return 0;
            }
        }

        public async Task<IEnumerable<ApplicationUser>> GetDiscordLinkedUsersAsync(int skip = 0, int take = 100)
        {
            try
            {
                return await _userManager.Users
                    .Where(u => u.DiscordUserId.HasValue)
                    .OrderByDescending(u => u.DiscordLinkedAt)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Discord linked users");
                return Enumerable.Empty<ApplicationUser>();
            }
        }

        #endregion

        #region User Management

        public async Task<ApplicationUser?> CreateUserAsync(string email, string username, string password)
        {
            try
            {
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = false,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Created new user: {Username} ({Email})", username, email);
                    return user;
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error creating user {Username}: {Error}", username, error.Description);
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user: {Email}", email);
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(string userId, string? email = null, string? username = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for update: {UserId}", userId);
                    return false;
                }

                bool changed = false;

                if (email != null && user.Email != email)
                {
                    user.Email = email;
                    user.EmailConfirmed = false; // Require re-confirmation
                    changed = true;
                }

                if (username != null && user.UserName != username)
                {
                    user.UserName = username;
                    changed = true;
                }

                if (!changed)
                {
                    return true; // Nothing to update
                }

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Updated user: {UserId}", userId);
                    return true;
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error updating user {UserId}: {Error}", userId, error.Description);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for deletion: {UserId}", userId);
                    return false;
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Deleted user: {UserId} ({Username})", userId, user.UserName);
                    return true;
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error deleting user {UserId}: {Error}", userId, error.Description);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(string userId, string newPassword)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for password reset: {UserId}", userId);
                    return false;
                }

                // Remove old password
                await _userManager.RemovePasswordAsync(user);

                // Add new password
                var result = await _userManager.AddPasswordAsync(user, newPassword);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Password reset for user: {UserId}", userId);
                    return true;
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error resetting password for user {UserId}: {Error}", userId, error.Description);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> LockUserAsync(string userId, DateTimeOffset? lockoutEnd = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for lockout: {UserId}", userId);
                    return false;
                }

                // Enable lockout if not already enabled
                if (!user.LockoutEnabled)
                {
                    user.LockoutEnabled = true;
                }

                // Set lockout end (null = permanent until manually unlocked)
                var result = await _userManager.SetLockoutEndDateAsync(
                    user,
                    lockoutEnd ?? DateTimeOffset.MaxValue);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Locked user: {UserId} until {LockoutEnd}", userId, lockoutEnd);
                    return true;
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error locking user {UserId}: {Error}", userId, error.Description);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user: {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> UnlockUserAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for unlock: {UserId}", userId);
                    return false;
                }

                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Unlocked user: {UserId}", userId);
                    return true;
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error unlocking user {UserId}: {Error}", userId, error.Description);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user: {UserId}", userId);
                return false;
            }
        }

        #endregion

        #region Role Management

        public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for role retrieval: {UserId}", userId);
                    return Enumerable.Empty<string>();
                }

                return await _userManager.GetRolesAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user: {UserId}", userId);
                return Enumerable.Empty<string>();
            }
        }

        public async Task<bool> AddUserToRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for role assignment: {UserId}", userId);
                    return false;
                }

                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Added user {UserId} to role {RoleName}", userId, roleName);
                    return true;
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error adding user {UserId} to role {RoleName}: {Error}",
                        userId, roleName, error.Description);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to role {RoleName}", userId, roleName);
                return false;
            }
        }

        public async Task<bool> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("User not found for role removal: {UserId}", userId);
                    return false;
                }

                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Removed user {UserId} from role {RoleName}", userId, roleName);
                    return true;
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error removing user {UserId} from role {RoleName}: {Error}",
                        userId, roleName, error.Description);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from role {RoleName}", userId, roleName);
                return false;
            }
        }

        public async Task<bool> IsInRoleAsync(string userId, string roleName)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                return await _userManager.IsInRoleAsync(user, roleName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} is in role {RoleName}", userId, roleName);
                return false;
            }
        }

        #endregion

        #region Statistics

        public async Task<Dictionary<string, object>> GetUserStatisticsAsync()
        {
            try
            {
                var stats = new Dictionary<string, object>();
                var now = DateTimeOffset.UtcNow;
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                // Total users
                stats["TotalUsers"] = await _userManager.Users.CountAsync();

                // Discord linked users
                stats["DiscordLinkedUsers"] = await _userManager.Users
                    .CountAsync(u => u.DiscordUserId.HasValue);

                // Email confirmed users
                stats["EmailConfirmedUsers"] = await _userManager.Users
                    .CountAsync(u => u.EmailConfirmed);

                // Locked users (client-side evaluation needed for DateTimeOffset comparison)
                var usersWithLockout = await _userManager.Users
                    .Where(u => u.LockoutEnd.HasValue)
                    .Select(u => u.LockoutEnd)
                    .ToListAsync();
                stats["LockedUsers"] = usersWithLockout.Count(lockoutEnd => lockoutEnd > now);

                // Users created in last 30 days
                stats["NewUsersLast30Days"] = await _userManager.Users
                    .CountAsync(u => u.CreatedAt >= thirtyDaysAgo);

                // Users with last login in last 30 days
                stats["ActiveUsersLast30Days"] = await _userManager.Users
                    .CountAsync(u => u.LastLoginAt.HasValue && u.LastLoginAt >= thirtyDaysAgo);

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user statistics");
                return new Dictionary<string, object>();
            }
        }

        #endregion
    }
}

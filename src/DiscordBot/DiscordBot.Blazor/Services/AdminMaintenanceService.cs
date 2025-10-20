using DiscordBot.Blazor.Data;
using DiscordBot.Core.Entities;
using DiscordBot.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Blazor.Services
{
    /// <summary>
    /// Service implementation for administrative maintenance tasks.
    /// </summary>
    public class AdminMaintenanceService : IAdminMaintenanceService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminMaintenanceService> _logger;

        // Define standard application roles
        private static readonly string[] StandardRoles = new[]
        {
            "Admin",
            "Moderator",
            "Premium",
            "User"
        };

        public AdminMaintenanceService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AdminMaintenanceService> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        #region Admin Bootstrap

        public async Task<bool> PromoteUserToAdminByEmailAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Cannot promote user to admin: User not found with email {Email}", email);
                    return false;
                }

                return await PromoteUserToAdminInternalAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user to admin by email: {Email}", email);
                return false;
            }
        }

        public async Task<bool> PromoteUserToAdminByUsernameAsync(string username)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("Cannot promote user to admin: User not found with username {Username}", username);
                    return false;
                }

                return await PromoteUserToAdminInternalAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user to admin by username: {Username}", username);
                return false;
            }
        }

        public async Task<bool> PromoteUserToAdminByDiscordIdAsync(ulong discordId)
        {
            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.DiscordUserId == discordId);

                if (user == null)
                {
                    _logger.LogWarning("Cannot promote user to admin: User not found with Discord ID {DiscordId}", discordId);
                    return false;
                }

                return await PromoteUserToAdminInternalAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user to admin by Discord ID: {DiscordId}", discordId);
                return false;
            }
        }

        public async Task<bool> PromoteUserToAdminByUserIdAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Cannot promote user to admin: User not found with ID {UserId}", userId);
                    return false;
                }

                return await PromoteUserToAdminInternalAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user to admin by user ID: {UserId}", userId);
                return false;
            }
        }

        /// <summary>
        /// Internal helper method to promote a user to admin role.
        /// </summary>
        private async Task<bool> PromoteUserToAdminInternalAsync(ApplicationUser user)
        {
            // Ensure Admin role exists
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole("Admin"));
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to create Admin role: {Errors}",
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    return false;
                }
                _logger.LogInformation("Created Admin role");
            }

            // Check if user already has admin role
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                _logger.LogInformation("User {Username} ({Email}) is already an Admin", user.UserName, user.Email);
                return true;
            }

            // Add user to admin role
            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "Successfully promoted user to Admin: {Username} ({Email}, UserId: {UserId})",
                    user.UserName,
                    user.Email,
                    user.Id);
                return true;
            }

            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Error promoting user {UserId} to Admin: {Error}",
                    user.Id, error.Description);
            }

            return false;
        }

        #endregion

        #region Role Initialization

        public async Task<IEnumerable<string>> EnsureRolesExistAsync()
        {
            var createdRoles = new List<string>();

            try
            {
                foreach (var roleName in StandardRoles)
                {
                    if (!await _roleManager.RoleExistsAsync(roleName))
                    {
                        var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                        if (result.Succeeded)
                        {
                            createdRoles.Add(roleName);
                            _logger.LogInformation("Created role: {RoleName}", roleName);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to create role {RoleName}: {Errors}",
                                roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                }

                if (createdRoles.Any())
                {
                    _logger.LogInformation("Created {Count} roles: {Roles}",
                        createdRoles.Count, string.Join(", ", createdRoles));
                }
                else
                {
                    _logger.LogInformation("All standard roles already exist");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring roles exist");
            }

            return createdRoles;
        }

        public async Task<IEnumerable<string>> GetAllRolesAsync()
        {
            try
            {
                return await _roleManager.Roles
                    .Select(r => r.Name!)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all roles");
                return Enumerable.Empty<string>();
            }
        }

        #endregion

        #region Discord Account Linking

        public async Task<ApplicationUser?> CreateAdminWithDiscordAsync(
            string email,
            string username,
            string password,
            ulong discordId,
            string? discordUsername = null)
        {
            try
            {
                // Check if Discord ID is already linked to another account
                var existingUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.DiscordUserId == discordId);

                if (existingUser != null)
                {
                    _logger.LogWarning(
                        "Cannot create admin: Discord ID {DiscordId} is already linked to user {UserId}",
                        discordId, existingUser.Id);
                    return null;
                }

                // Create the user with Discord account pre-linked
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true, // Auto-confirm for admin accounts
                    CreatedAt = DateTime.UtcNow,
                    DiscordUserId = discordId,
                    DiscordUsername = discordUsername,
                    DiscordLinkedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        _logger.LogWarning("Error creating admin user {Username}: {Error}",
                            username, error.Description);
                    }
                    return null;
                }

                // Promote to admin
                var promoted = await PromoteUserToAdminInternalAsync(user);
                if (!promoted)
                {
                    _logger.LogError("User created but failed to promote to admin: {UserId}", user.Id);
                    return null;
                }

                _logger.LogInformation(
                    "Successfully created admin account with Discord link: {Username} ({Email}, Discord ID: {DiscordId})",
                    username, email, discordId);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin with Discord account");
                return null;
            }
        }

        public async Task<bool> LinkDiscordAccountAsync(string userId, ulong discordId, string? discordUsername = null)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Cannot link Discord account: User not found with ID {UserId}", userId);
                    return false;
                }

                // Check if Discord ID is already linked to another account
                var existingUser = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.DiscordUserId == discordId);

                if (existingUser != null && existingUser.Id != userId)
                {
                    _logger.LogWarning(
                        "Cannot link Discord account: Discord ID {DiscordId} is already linked to user {ExistingUserId}",
                        discordId, existingUser.Id);
                    return false;
                }

                // Link the Discord account
                user.DiscordUserId = discordId;
                user.DiscordUsername = discordUsername;
                user.DiscordLinkedAt = DateTime.UtcNow;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "Successfully linked Discord account {DiscordId} to user {UserId} ({Username})",
                        discordId, userId, user.UserName);
                    return true;
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error linking Discord account to user {UserId}: {Error}",
                        userId, error.Description);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking Discord account to user {UserId}", userId);
                return false;
            }
        }

        public async Task<bool> LinkDiscordAccountByEmailAsync(string email, ulong discordId, string? discordUsername = null)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    _logger.LogWarning("Cannot link Discord account: User not found with email {Email}", email);
                    return false;
                }

                return await LinkDiscordAccountAsync(user.Id, discordId, discordUsername);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking Discord account by email: {Email}", email);
                return false;
            }
        }

        public async Task<bool> UnlinkDiscordAccountAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogWarning("Cannot unlink Discord account: User not found with ID {UserId}", userId);
                    return false;
                }

                if (!user.DiscordUserId.HasValue)
                {
                    _logger.LogInformation("User {UserId} has no Discord account linked", userId);
                    return true; // Already unlinked
                }

                var discordId = user.DiscordUserId.Value;

                // Unlink the Discord account
                user.DiscordUserId = null;
                user.DiscordUsername = null;
                user.DiscordDiscriminator = null;
                user.DiscordAvatarHash = null;
                user.DiscordLinkedAt = null;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation(
                        "Successfully unlinked Discord account {DiscordId} from user {UserId} ({Username})",
                        discordId, userId, user.UserName);
                    return true;
                }

                foreach (var error in result.Errors)
                {
                    _logger.LogWarning("Error unlinking Discord account from user {UserId}: {Error}",
                        userId, error.Description);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking Discord account from user {UserId}", userId);
                return false;
            }
        }

        #endregion

        #region System Diagnostics

        public async Task<Dictionary<string, object>> GetSystemDiagnosticsAsync()
        {
            var diagnostics = new Dictionary<string, object>();

            try
            {
                // Total users
                diagnostics["TotalUsers"] = await _userManager.Users.CountAsync();

                // Total admins
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                diagnostics["TotalAdmins"] = adminUsers.Count;

                // Admin users (list of usernames)
                diagnostics["AdminUsernames"] = adminUsers.Select(u => u.UserName ?? "Unknown").ToList();

                // Total roles
                diagnostics["TotalRoles"] = await _roleManager.Roles.CountAsync();

                // All roles
                diagnostics["AllRoles"] = await GetAllRolesAsync();

                // Discord linked users
                diagnostics["DiscordLinkedUsers"] = await _userManager.Users
                    .CountAsync(u => u.DiscordUserId.HasValue);

                // Email confirmed users
                diagnostics["EmailConfirmedUsers"] = await _userManager.Users
                    .CountAsync(u => u.EmailConfirmed);

                // System ready (has at least one admin)
                diagnostics["SystemReady"] = adminUsers.Count > 0;

                // Recent users (last 7 days)
                var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                diagnostics["RecentUsers"] = await _userManager.Users
                    .CountAsync(u => u.CreatedAt >= sevenDaysAgo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system diagnostics");
                diagnostics["Error"] = ex.Message;
            }

            return diagnostics;
        }

        public async Task<bool> HasAdminAccountAsync()
        {
            try
            {
                // Check if Admin role exists first
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    return false;
                }

                // Check if any users have the Admin role
                var adminUsers = await _userManager.GetUsersInRoleAsync("Admin");
                return adminUsers.Any();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for admin accounts");
                return false;
            }
        }

        #endregion
    }
}

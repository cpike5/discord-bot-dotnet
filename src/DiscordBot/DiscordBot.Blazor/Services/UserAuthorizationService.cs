using DiscordBot.Core.Entities;
using DiscordBot.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DiscordBot.Blazor.Services
{
    /// <summary>
    /// Service implementation for Discord user authorization.
    /// Provides role-based authorization with caching for performance.
    /// </summary>
    public class UserAuthorizationService : IUserAuthorizationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly ILogger<UserAuthorizationService> _logger;

        // Cache configuration
        private const string CacheKeyPrefix = "user_roles:";
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

        public UserAuthorizationService(
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache,
            ILogger<UserAuthorizationService> logger)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<ApplicationUser?> GetUserByDiscordIdAsync(ulong discordUserId)
        {
            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.DiscordUserId == discordUserId);
        }

        /// <inheritdoc />
        public async Task<bool> IsInRoleAsync(ulong discordUserId, string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return false;

            var user = await GetUserByDiscordIdAsync(discordUserId);
            if (user == null)
            {
                _logger.LogDebug("Discord user {DiscordUserId} is not linked to an application account", discordUserId);
                return false;
            }

            // Try to get roles from cache
            var roles = await GetUserRolesWithCacheAsync(discordUserId, user);

            var isInRole = roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);

            _logger.LogDebug(
                "Discord user {DiscordUserId} role check for '{RoleName}': {Result}",
                discordUserId,
                roleName,
                isInRole);

            return isInRole;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetUserRolesAsync(ulong discordUserId)
        {
            var user = await GetUserByDiscordIdAsync(discordUserId);
            if (user == null)
            {
                _logger.LogDebug("Discord user {DiscordUserId} is not linked, returning empty roles", discordUserId);
                return Enumerable.Empty<string>();
            }

            return await GetUserRolesWithCacheAsync(discordUserId, user);
        }

        /// <inheritdoc />
        public async Task<bool> IsLinkedAsync(ulong discordUserId)
        {
            var user = await GetUserByDiscordIdAsync(discordUserId);
            return user != null;
        }

        /// <inheritdoc />
        public async Task<bool> LinkDiscordAccountAsync(
            string applicationUserId,
            ulong discordUserId,
            string discordUsername,
            string? discordDiscriminator = null,
            string? discordAvatarHash = null)
        {
            if (string.IsNullOrWhiteSpace(applicationUserId))
                throw new ArgumentException("Application user ID cannot be empty", nameof(applicationUserId));

            if (string.IsNullOrWhiteSpace(discordUsername))
                throw new ArgumentException("Discord username cannot be empty", nameof(discordUsername));

            var user = await _userManager.FindByIdAsync(applicationUserId);
            if (user == null)
            {
                _logger.LogWarning("Cannot link Discord account: application user {ApplicationUserId} not found", applicationUserId);
                return false;
            }

            // Check if Discord user is already linked to another account
            var existingLink = await GetUserByDiscordIdAsync(discordUserId);
            if (existingLink != null && existingLink.Id != applicationUserId)
            {
                _logger.LogWarning(
                    "Discord user {DiscordUserId} is already linked to application user {ExistingUserId}",
                    discordUserId,
                    existingLink.Id);
                return false;
            }

            // Update Discord information
            user.DiscordUserId = discordUserId;
            user.DiscordUsername = discordUsername;
            user.DiscordDiscriminator = discordDiscriminator;
            user.DiscordAvatarHash = discordAvatarHash;
            user.DiscordLinkedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "Successfully linked Discord user {DiscordUserId} ({DiscordUsername}) to application user {ApplicationUserId}",
                    discordUserId,
                    discordUsername,
                    applicationUserId);

                // Invalidate cache
                InvalidateRoleCache(discordUserId);
            }
            else
            {
                _logger.LogError(
                    "Failed to link Discord account: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return result.Succeeded;
        }

        /// <inheritdoc />
        public async Task<bool> UnlinkDiscordAccountAsync(string applicationUserId)
        {
            if (string.IsNullOrWhiteSpace(applicationUserId))
                throw new ArgumentException("Application user ID cannot be empty", nameof(applicationUserId));

            var user = await _userManager.FindByIdAsync(applicationUserId);
            if (user == null)
            {
                _logger.LogWarning("Cannot unlink Discord account: application user {ApplicationUserId} not found", applicationUserId);
                return false;
            }

            var discordUserId = user.DiscordUserId;

            // Clear Discord information
            user.DiscordUserId = null;
            user.DiscordUsername = null;
            user.DiscordDiscriminator = null;
            user.DiscordAvatarHash = null;
            user.DiscordLinkedAt = null;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation(
                    "Successfully unlinked Discord user {DiscordUserId} from application user {ApplicationUserId}",
                    discordUserId,
                    applicationUserId);

                // Invalidate cache
                if (discordUserId.HasValue)
                {
                    InvalidateRoleCache(discordUserId.Value);
                }
            }
            else
            {
                _logger.LogError(
                    "Failed to unlink Discord account: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return result.Succeeded;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateLastLoginAsync(string applicationUserId)
        {
            if (string.IsNullOrWhiteSpace(applicationUserId))
                throw new ArgumentException("Application user ID cannot be empty", nameof(applicationUserId));

            var user = await _userManager.FindByIdAsync(applicationUserId);
            if (user == null)
            {
                _logger.LogWarning("Cannot update last login: application user {ApplicationUserId} not found", applicationUserId);
                return false;
            }

            user.LastLoginAt = DateTime.UtcNow;
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _logger.LogDebug("Updated last login for user {ApplicationUserId}", applicationUserId);
            }
            else
            {
                _logger.LogError(
                    "Failed to update last login: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return result.Succeeded;
        }

        /// <inheritdoc />
        public void InvalidateRoleCache(ulong discordUserId)
        {
            var cacheKey = GetCacheKey(discordUserId);
            _cache.Remove(cacheKey);
            _logger.LogDebug("Invalidated role cache for Discord user {DiscordUserId}", discordUserId);
        }

        /// <inheritdoc />
        public void InvalidateAllRoleCache()
        {
            // Note: MemoryCache doesn't support clearing by prefix, so this is a no-op
            // In a production system with Redis, you could use pattern-based deletion
            // For now, individual cache entries will expire naturally
            _logger.LogInformation("Full role cache invalidation requested (individual entries will expire)");
        }

        /// <summary>
        /// Gets user roles with caching support.
        /// </summary>
        private async Task<IList<string>> GetUserRolesWithCacheAsync(ulong discordUserId, ApplicationUser user)
        {
            var cacheKey = GetCacheKey(discordUserId);

            // Try to get from cache
            if (_cache.TryGetValue<IList<string>>(cacheKey, out var cachedRoles))
            {
                _logger.LogDebug("Cache hit for Discord user {DiscordUserId} roles", discordUserId);
                return cachedRoles!;
            }

            // Cache miss - fetch from database
            _logger.LogDebug("Cache miss for Discord user {DiscordUserId} roles, fetching from database", discordUserId);
            var roles = await _userManager.GetRolesAsync(user);

            // Store in cache with sliding expiration
            var cacheOptions = new MemoryCacheEntryOptions
            {
                SlidingExpiration = CacheDuration
            };
            _cache.Set(cacheKey, roles, cacheOptions);

            return roles;
        }

        /// <summary>
        /// Generates cache key for a Discord user.
        /// </summary>
        private static string GetCacheKey(ulong discordUserId)
        {
            return $"{CacheKeyPrefix}{discordUserId}";
        }
    }
}

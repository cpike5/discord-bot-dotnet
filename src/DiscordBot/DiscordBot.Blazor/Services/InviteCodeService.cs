using DiscordBot.Core.DTOs;
using DiscordBot.Core.Entities;
using DiscordBot.Core.Repositories;
using DiscordBot.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DiscordBot.Blazor.Services
{
    /// <summary>
    /// Service implementation for invite code management.
    /// Handles code generation, validation, and lifecycle management.
    /// </summary>
    public class InviteCodeService : IInviteCodeService
    {
        private readonly IInviteCodeRepository _inviteCodeRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<InviteCodeService> _logger;

        // Characters for invite code generation (excluding ambiguous characters)
        // Excludes: 0, O, 1, I, l (zero, uppercase O, one, uppercase I, lowercase L)
        private const string CodeCharacters = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";
        private const int CodeLength = 12; // XXXX-XXXX-XXXX format
        private const int CodeSegmentLength = 4;

        public InviteCodeService(
            IInviteCodeRepository inviteCodeRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<InviteCodeService> logger)
        {
            _inviteCodeRepository = inviteCodeRepository ?? throw new ArgumentNullException(nameof(inviteCodeRepository));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public async Task<InviteCode> GenerateCodeAsync(ulong discordUserId, string discordUsername, int expirationHours = 24)
        {
            if (string.IsNullOrWhiteSpace(discordUsername))
                throw new ArgumentException("Discord username cannot be empty", nameof(discordUsername));

            if (expirationHours <= 0)
                throw new ArgumentException("Expiration hours must be positive", nameof(expirationHours));

            // Check if user is already registered
            if (await IsUserAlreadyRegisteredAsync(discordUserId))
            {
                _logger.LogWarning("Discord user {DiscordUserId} attempted to generate code but is already registered", discordUserId);
                throw new InvalidOperationException("Discord user is already registered");
            }

            // Check for existing active code
            var existingCode = await GetActiveCodeForUserAsync(discordUserId);
            if (existingCode != null)
            {
                _logger.LogInformation("Returning existing active code for Discord user {DiscordUserId}", discordUserId);
                return existingCode;
            }

            // Generate new unique code
            var code = await GenerateUniqueCodeAsync();

            var inviteCode = new InviteCode
            {
                Code = code,
                DiscordUserId = discordUserId,
                DiscordUsername = discordUsername,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(expirationHours),
                IsUsed = false
            };

            var result = await _inviteCodeRepository.AddAsync(inviteCode);

            _logger.LogInformation(
                "Generated invite code for Discord user {DiscordUserId} ({DiscordUsername}). Code expires at {ExpiresAt}",
                discordUserId,
                discordUsername,
                result.ExpiresAt);

            return result;
        }

        /// <inheritdoc />
        public async Task<InviteCode?> ValidateCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            var inviteCode = await _inviteCodeRepository.GetByCodeAsync(code);

            if (inviteCode == null)
            {
                _logger.LogWarning("Invite code validation failed: code {Code} not found", code);
                return null;
            }

            if (inviteCode.IsUsed)
            {
                _logger.LogWarning("Invite code validation failed: code {Code} already used", code);
                return null;
            }

            if (inviteCode.IsExpired)
            {
                _logger.LogWarning("Invite code validation failed: code {Code} expired at {ExpiresAt}", code, inviteCode.ExpiresAt);
                return null;
            }

            _logger.LogInformation("Invite code {Code} validated successfully for Discord user {DiscordUserId}", code, inviteCode.DiscordUserId);
            return inviteCode;
        }

        /// <inheritdoc />
        public async Task<bool> MarkCodeAsUsedAsync(string code, string applicationUserId)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Code cannot be empty", nameof(code));

            if (string.IsNullOrWhiteSpace(applicationUserId))
                throw new ArgumentException("Application user ID cannot be empty", nameof(applicationUserId));

            var inviteCode = await _inviteCodeRepository.GetByCodeAsync(code);

            if (inviteCode == null || inviteCode.IsUsed || inviteCode.IsExpired)
            {
                _logger.LogWarning(
                    "Failed to mark code {Code} as used. Code found: {Found}, IsUsed: {IsUsed}, IsExpired: {IsExpired}",
                    code,
                    inviteCode != null,
                    inviteCode?.IsUsed,
                    inviteCode?.IsExpired);
                return false;
            }

            inviteCode.IsUsed = true;
            inviteCode.UsedAt = DateTime.UtcNow;
            inviteCode.UsedByApplicationUserId = applicationUserId;

            await _inviteCodeRepository.UpdateAsync(inviteCode);

            _logger.LogInformation(
                "Marked invite code {Code} as used by application user {ApplicationUserId}",
                code,
                applicationUserId);

            return true;
        }

        /// <inheritdoc />
        public async Task<InviteCode?> GetActiveCodeForUserAsync(ulong discordUserId)
        {
            return await _inviteCodeRepository.GetActiveCodeByDiscordUserIdAsync(discordUserId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<InviteCode>> GetAllCodesForUserAsync(ulong discordUserId)
        {
            return await _inviteCodeRepository.GetByDiscordUserIdAsync(discordUserId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<InviteCode>> GetAllActiveCodesAsync()
        {
            return await _inviteCodeRepository.GetAllActiveCodesAsync();
        }

        /// <inheritdoc />
        public async Task<bool> RevokeCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            var inviteCode = await _inviteCodeRepository.GetByCodeAsync(code);

            if (inviteCode == null)
            {
                _logger.LogWarning("Cannot revoke code {Code}: not found", code);
                return false;
            }

            if (inviteCode.IsUsed)
            {
                _logger.LogWarning("Cannot revoke code {Code}: already used", code);
                return false;
            }

            // Mark as expired (set ExpiresAt to now)
            inviteCode.ExpiresAt = DateTime.UtcNow;
            await _inviteCodeRepository.UpdateAsync(inviteCode);

            _logger.LogInformation("Revoked invite code {Code}", code);
            return true;
        }

        /// <inheritdoc />
        public async Task<bool> IsUserAlreadyRegisteredAsync(ulong discordUserId)
        {
            var users = await _userManager.Users
                .Where(u => u.DiscordUserId == discordUserId)
                .ToListAsync();

            return users.Any();
        }

        /// <inheritdoc />
        public async Task<int> CleanupExpiredCodesAsync(int daysOld = 7)
        {
            if (daysOld <= 0)
                throw new ArgumentException("Days must be positive", nameof(daysOld));

            var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
            var deletedCount = await _inviteCodeRepository.DeleteExpiredCodesAsync(cutoffDate);

            _logger.LogInformation("Cleaned up {Count} expired invite codes older than {CutoffDate}", deletedCount, cutoffDate);

            return deletedCount;
        }

        /// <summary>
        /// Generates a cryptographically secure unique invite code.
        /// Format: XXXX-XXXX-XXXX (12 characters, no ambiguous characters)
        /// </summary>
        private async Task<string> GenerateUniqueCodeAsync()
        {
            const int maxAttempts = 10;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                var code = GenerateRandomCode();

                if (!await _inviteCodeRepository.CodeExistsAsync(code))
                {
                    return code;
                }

                attempts++;
                _logger.LogWarning("Code collision detected on attempt {Attempt}. Generating new code.", attempts);
            }

            throw new InvalidOperationException("Failed to generate unique invite code after maximum attempts");
        }

        /// <summary>
        /// Generates a random code string using cryptographically secure random number generator.
        /// </summary>
        private static string GenerateRandomCode()
        {
            var codeBytes = new byte[CodeLength];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(codeBytes);
            }

            var sb = new StringBuilder();
            for (int i = 0; i < CodeLength; i++)
            {
                sb.Append(CodeCharacters[codeBytes[i] % CodeCharacters.Length]);

                // Add hyphen after every 4 characters (except at the end)
                if ((i + 1) % CodeSegmentLength == 0 && i != CodeLength - 1)
                {
                    sb.Append('-');
                }
            }

            return sb.ToString();
        }

        /// <inheritdoc />
        public async Task<InviteCodeStatistics> GetStatisticsAsync()
        {
            var allCodes = await _inviteCodeRepository.GetAllAsync();
            var now = DateTime.UtcNow;

            return new InviteCodeStatistics
            {
                ActiveCount = allCodes.Count(c => !c.IsUsed && c.ExpiresAt > now),
                UsedCount = allCodes.Count(c => c.IsUsed),
                ExpiredCount = allCodes.Count(c => !c.IsUsed && c.ExpiresAt <= now),
                RevokedCount = 0 // For MVP, we don't track revoked separately - they're included in ExpiredCount
            };
        }

        /// <inheritdoc />
        public async Task<(IEnumerable<InviteCode> Codes, int TotalCount)> GetPagedAsync(
            int page, int pageSize, string? statusFilter = null, string? searchTerm = null)
        {
            if (page < 1)
                throw new ArgumentException("Page must be greater than 0", nameof(page));

            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));

            var skip = (page - 1) * pageSize;
            return await _inviteCodeRepository.GetPagedAsync(skip, pageSize, statusFilter, searchTerm);
        }
    }
}

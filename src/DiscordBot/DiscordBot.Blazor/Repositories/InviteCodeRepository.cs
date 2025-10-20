using DiscordBot.Blazor.Data;
using DiscordBot.Core.Entities;
using DiscordBot.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Blazor.Repositories
{
    /// <summary>
    /// Repository implementation for invite code data access.
    /// Uses Entity Framework Core with ApplicationDbContext.
    /// </summary>
    public class InviteCodeRepository : IInviteCodeRepository
    {
        private readonly ApplicationDbContext _context;

        public InviteCodeRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <inheritdoc />
        public async Task<InviteCode?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            return await _context.InviteCodes
                .Include(ic => ic.UsedByApplicationUser)
                .FirstOrDefaultAsync(ic => ic.Code == code);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<InviteCode>> GetByDiscordUserIdAsync(ulong discordUserId)
        {
            return await _context.InviteCodes
                .Where(ic => ic.DiscordUserId == discordUserId)
                .OrderByDescending(ic => ic.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<InviteCode?> GetActiveCodeByDiscordUserIdAsync(ulong discordUserId)
        {
            var now = DateTime.UtcNow;

            return await _context.InviteCodes
                .Where(ic => ic.DiscordUserId == discordUserId
                          && !ic.IsUsed
                          && ic.ExpiresAt > now)
                .OrderByDescending(ic => ic.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<InviteCode>> GetAllActiveCodesAsync()
        {
            var now = DateTime.UtcNow;

            return await _context.InviteCodes
                .Where(ic => !ic.IsUsed && ic.ExpiresAt > now)
                .OrderByDescending(ic => ic.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<InviteCode> AddAsync(InviteCode inviteCode)
        {
            if (inviteCode == null)
                throw new ArgumentNullException(nameof(inviteCode));

            _context.InviteCodes.Add(inviteCode);
            await _context.SaveChangesAsync();

            return inviteCode;
        }

        /// <inheritdoc />
        public async Task<InviteCode> UpdateAsync(InviteCode inviteCode)
        {
            if (inviteCode == null)
                throw new ArgumentNullException(nameof(inviteCode));

            _context.InviteCodes.Update(inviteCode);
            await _context.SaveChangesAsync();

            return inviteCode;
        }

        /// <inheritdoc />
        public async Task<int> DeleteExpiredCodesAsync(DateTime olderThan)
        {
            var expiredCodes = await _context.InviteCodes
                .Where(ic => ic.ExpiresAt < olderThan)
                .ToListAsync();

            _context.InviteCodes.RemoveRange(expiredCodes);
            await _context.SaveChangesAsync();

            return expiredCodes.Count;
        }

        /// <inheritdoc />
        public async Task<bool> CodeExistsAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            return await _context.InviteCodes
                .AnyAsync(ic => ic.Code == code);
        }
    }
}

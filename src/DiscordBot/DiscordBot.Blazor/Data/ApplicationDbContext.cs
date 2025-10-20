using DiscordBot.Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DiscordBot.Blazor.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        /// <summary>
        /// Invite codes for Discord user registration.
        /// </summary>
        public DbSet<InviteCode> InviteCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Value converters for ulong (Discord IDs) - ensures cross-database compatibility
            // Discord snowflake IDs are ulong but most databases use long for BIGINT
            var ulongConverter = new ValueConverter<ulong, long>(
                v => unchecked((long)v),
                v => unchecked((ulong)v));

            var nullableUlongConverter = new ValueConverter<ulong?, long?>(
                v => v.HasValue ? unchecked((long)v.Value) : null,
                v => v.HasValue ? unchecked((ulong)v.Value) : null);

            #region ApplicationUser Configuration

            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Discord fields configuration
                entity.Property(e => e.DiscordUserId)
                    .HasConversion(nullableUlongConverter);

                entity.Property(e => e.DiscordUsername)
                    .HasMaxLength(100);  // Discord max username length

                entity.Property(e => e.DiscordDiscriminator)
                    .HasMaxLength(4);    // Legacy format: #0001-9999

                entity.Property(e => e.DiscordAvatarHash)
                    .HasMaxLength(100);  // Hash string length

                entity.Property(e => e.DiscordLinkedAt)
                    .IsRequired(false);

                // Audit fields configuration
                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.LastLoginAt)
                    .IsRequired(false);

                // Indexes for performance
                // Note: SQLite doesn't enforce unique constraint on nullable columns the same way
                // Multiple NULL values are allowed, but duplicate non-NULL values are not
                entity.HasIndex(e => e.DiscordUserId)
                    .IsUnique();

                entity.HasIndex(e => e.DiscordUsername);  // For search/lookup

                entity.HasIndex(e => e.CreatedAt);        // For chronological queries

                entity.HasIndex(e => e.LastLoginAt);      // For activity queries
            });

            #endregion

            #region InviteCode Configuration

            modelBuilder.Entity<InviteCode>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Code)
                    .HasMaxLength(14)      // XXXX-XXXX-XXXX format
                    .IsRequired();

                entity.Property(e => e.DiscordUserId)
                    .HasConversion(ulongConverter)
                    .IsRequired();

                entity.Property(e => e.DiscordUsername)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.CreatedAt)
                    .IsRequired();

                entity.Property(e => e.ExpiresAt)
                    .IsRequired();

                entity.Property(e => e.IsUsed)
                    .IsRequired()
                    .HasDefaultValue(false);

                entity.Property(e => e.UsedAt)
                    .IsRequired(false);

                entity.Property(e => e.UsedByApplicationUserId)
                    .HasMaxLength(450)     // ASP.NET Identity default key length
                    .IsRequired(false);

                // Indexes
                entity.HasIndex(e => e.Code)
                    .IsUnique();

                entity.HasIndex(e => e.DiscordUserId);

                entity.HasIndex(e => new { e.IsUsed, e.ExpiresAt });

                entity.HasIndex(e => e.CreatedAt);

                // Foreign key relationship
                entity.HasOne(e => e.UsedByApplicationUser)
                    .WithMany(u => u.GeneratedInviteCodes)
                    .HasForeignKey(e => e.UsedByApplicationUserId)
                    .OnDelete(DeleteBehavior.SetNull);  // Keep code history if user deleted
            });

            #endregion
        }
    }
}

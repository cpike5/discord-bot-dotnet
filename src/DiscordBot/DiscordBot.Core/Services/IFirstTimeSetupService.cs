namespace DiscordBot.Core.Services
{
    /// <summary>
    /// Service for managing the first-time setup workflow.
    /// </summary>
    public interface IFirstTimeSetupService
    {
        /// <summary>
        /// Checks if the initial setup has been completed.
        /// </summary>
        /// <returns>True if setup is complete, false otherwise.</returns>
        Task<bool> IsSetupCompleteAsync();

        /// <summary>
        /// Creates the initial admin account with SuperAdmin role.
        /// </summary>
        /// <param name="email">Admin email address</param>
        /// <param name="username">Admin username</param>
        /// <param name="password">Admin password</param>
        /// <returns>Result containing user ID or error messages</returns>
        Task<AdminCreationResult> CreateAdminAccountAsync(
            string email,
            string username,
            string password);

        /// <summary>
        /// Seeds the database with essential data (roles, settings).
        /// </summary>
        /// <returns>Result indicating success or failure</returns>
        Task<SeedResult> SeedDatabaseAsync();

        /// <summary>
        /// Marks the setup as complete in the database.
        /// </summary>
        /// <param name="adminUserId">ID of the admin user created</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> MarkSetupCompleteAsync(string adminUserId);

        /// <summary>
        /// Validates that the system is in a valid setup state.
        /// </summary>
        /// <returns>Validation result with any issues found</returns>
        Task<SetupValidationResult> ValidateSetupStateAsync();
    }

    /// <summary>
    /// Result of admin account creation.
    /// </summary>
    public class AdminCreationResult
    {
        public bool Success { get; set; }
        public string? UserId { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Result of database seeding operation.
    /// </summary>
    public class SeedResult
    {
        public bool Success { get; set; }
        public int RolesCreated { get; set; }
        public List<string> Errors { get; set; } = new();
    }

    /// <summary>
    /// Result of setup state validation.
    /// </summary>
    public class SetupValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Issues { get; set; } = new();
    }
}

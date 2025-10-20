using DiscordBot.Blazor.Data;
using DiscordBot.Core.Entities;
using DiscordBot.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DiscordBot.Blazor.Services
{
    /// <summary>
    /// Service for managing the first-time setup workflow.
    /// </summary>
    public class FirstTimeSetupService : IFirstTimeSetupService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<FirstTimeSetupService> _logger;

        // Role constants
        private static readonly string[] RequiredRoles = new[]
        {
            "SuperAdmin",
            "Admin",
            "Moderator",
            "User"
        };

        public FirstTimeSetupService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<FirstTimeSetupService> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<bool> IsSetupCompleteAsync()
        {
            try
            {
                // Check SetupStatus table
                var setupStatus = await _context.SetupStatus
                    .FirstOrDefaultAsync(s => s.Id == 1);

                if (setupStatus?.IsComplete == true)
                {
                    return true;
                }

                // Double-check: if SuperAdmin role exists and has users, setup is complete
                var superAdminRole = await _roleManager.FindByNameAsync("SuperAdmin");
                if (superAdminRole != null)
                {
                    var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                    if (superAdmins.Any())
                    {
                        // Update setup status if it's out of sync
                        if (setupStatus != null && !setupStatus.IsComplete)
                        {
                            _logger.LogWarning(
                                "Setup status was incomplete but SuperAdmin exists. Marking as complete.");
                            setupStatus.IsComplete = true;
                            setupStatus.CompletedAt = DateTime.UtcNow;
                            await _context.SaveChangesAsync();
                        }
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking setup completion status");
                return false;
            }
        }

        public async Task<AdminCreationResult> CreateAdminAccountAsync(
            string email,
            string username,
            string password)
        {
            var result = new AdminCreationResult();

            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(email) ||
                    string.IsNullOrWhiteSpace(username) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    result.Errors.Add("All fields are required.");
                    return result;
                }

                // Check if admin already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    result.Errors.Add("An admin account already exists.");
                    return result;
                }

                // Create user
                var user = new ApplicationUser
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true, // Auto-confirm for admin
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user, password);

                if (!createResult.Succeeded)
                {
                    result.Errors.AddRange(createResult.Errors.Select(e => e.Description));
                    return result;
                }

                // Ensure SuperAdmin role exists before assigning
                if (!await _roleManager.RoleExistsAsync("SuperAdmin"))
                {
                    var role = new IdentityRole("SuperAdmin");
                    var roleCreateResult = await _roleManager.CreateAsync(role);
                    if (!roleCreateResult.Succeeded)
                    {
                        result.Errors.Add("Failed to create SuperAdmin role.");
                        await _userManager.DeleteAsync(user);
                        return result;
                    }
                }

                // Assign SuperAdmin role
                var roleResult = await _userManager.AddToRoleAsync(user, "SuperAdmin");
                if (!roleResult.Succeeded)
                {
                    result.Errors.Add("Failed to assign SuperAdmin role.");
                    // Rollback user creation
                    await _userManager.DeleteAsync(user);
                    return result;
                }

                result.Success = true;
                result.UserId = user.Id;

                _logger.LogInformation("Admin account created: {Email}", email);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin account");
                result.Errors.Add("An unexpected error occurred.");
                return result;
            }
        }

        public async Task<SeedResult> SeedDatabaseAsync()
        {
            var result = new SeedResult();

            try
            {
                // Create roles if they don't exist
                foreach (var roleName in RequiredRoles)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (!roleExists)
                    {
                        var role = new IdentityRole(roleName);
                        var createResult = await _roleManager.CreateAsync(role);

                        if (createResult.Succeeded)
                        {
                            result.RolesCreated++;
                            _logger.LogInformation("Created role: {RoleName}", roleName);
                        }
                        else
                        {
                            result.Errors.Add($"Failed to create role: {roleName}");
                            _logger.LogError("Failed to create role: {RoleName}", roleName);
                        }
                    }
                }

                // Future: Add other seed data here (system settings, defaults, etc.)

                result.Success = result.Errors.Count == 0;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding database");
                result.Errors.Add("An unexpected error occurred during seeding.");
                return result;
            }
        }

        public async Task<bool> MarkSetupCompleteAsync(string adminUserId)
        {
            try
            {
                var setupStatus = await _context.SetupStatus
                    .FirstOrDefaultAsync(s => s.Id == 1);

                if (setupStatus == null)
                {
                    setupStatus = new SetupStatus { Id = 1 };
                    _context.SetupStatus.Add(setupStatus);
                }

                // If adminUserId is empty, try to find the SuperAdmin user
                if (string.IsNullOrWhiteSpace(adminUserId))
                {
                    var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                    adminUserId = superAdmins.FirstOrDefault()?.Id ?? string.Empty;
                }

                setupStatus.IsComplete = true;
                setupStatus.CompletedAt = DateTime.UtcNow;
                setupStatus.AdminUserId = adminUserId;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Setup marked as complete. Admin ID: {AdminUserId}", adminUserId);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking setup as complete");
                return false;
            }
        }

        public async Task<SetupValidationResult> ValidateSetupStateAsync()
        {
            var result = new SetupValidationResult { IsValid = true };

            try
            {
                // Check if roles exist
                foreach (var roleName in RequiredRoles)
                {
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (!roleExists)
                    {
                        result.IsValid = false;
                        result.Issues.Add($"Missing role: {roleName}");
                    }
                }

                // Check if SuperAdmin role has users
                var superAdmins = await _userManager.GetUsersInRoleAsync("SuperAdmin");
                if (!superAdmins.Any())
                {
                    result.IsValid = false;
                    result.Issues.Add("No SuperAdmin users exist");
                }

                // Check setup status consistency
                var setupStatus = await _context.SetupStatus.FirstOrDefaultAsync(s => s.Id == 1);
                if (setupStatus == null)
                {
                    result.IsValid = false;
                    result.Issues.Add("Setup status record missing");
                }
                else if (setupStatus.IsComplete && !superAdmins.Any())
                {
                    result.IsValid = false;
                    result.Issues.Add("Setup marked complete but no admin exists");
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating setup state");
                result.IsValid = false;
                result.Issues.Add("Validation error occurred");
                return result;
            }
        }
    }
}

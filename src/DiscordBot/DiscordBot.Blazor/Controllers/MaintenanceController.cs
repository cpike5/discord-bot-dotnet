using DiscordBot.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace DiscordBot.Blazor.Controllers
{
    /// <summary>
    /// API controller for administrative maintenance and setup tasks.
    /// WARNING: These endpoints should be protected in production or disabled after initial setup.
    /// Consider using IP whitelisting, API keys, or removing these endpoints once setup is complete.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IAdminMaintenanceService _maintenanceService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MaintenanceController> _logger;

        public MaintenanceController(
            IAdminMaintenanceService maintenanceService,
            IConfiguration configuration,
            ILogger<MaintenanceController> logger)
        {
            _maintenanceService = maintenanceService;
            _configuration = configuration;
            _logger = logger;
        }

        #region System Diagnostics

        /// <summary>
        /// Gets system diagnostics and health information.
        /// This endpoint is intentionally public to check system status.
        /// </summary>
        [HttpGet("diagnostics")]
        public async Task<IActionResult> GetDiagnostics()
        {
            try
            {
                var diagnostics = await _maintenanceService.GetSystemDiagnosticsAsync();
                return Ok(diagnostics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving system diagnostics");
                return StatusCode(500, new { Error = "An error occurred while retrieving diagnostics" });
            }
        }

        /// <summary>
        /// Checks if the system has at least one admin account.
        /// </summary>
        [HttpGet("has-admin")]
        public async Task<IActionResult> HasAdmin()
        {
            try
            {
                var hasAdmin = await _maintenanceService.HasAdminAccountAsync();
                return Ok(new { HasAdmin = hasAdmin });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking for admin account");
                return StatusCode(500, new { Error = "An error occurred while checking for admin account" });
            }
        }

        #endregion

        #region Role Setup

        /// <summary>
        /// Ensures all required application roles exist.
        /// Creates any missing standard roles (Admin, Moderator, Premium, User).
        /// </summary>
        [HttpPost("ensure-roles")]
        public async Task<IActionResult> EnsureRoles()
        {
            try
            {
                var createdRoles = await _maintenanceService.EnsureRolesExistAsync();

                if (createdRoles.Any())
                {
                    return Ok(new
                    {
                        Message = "Roles created successfully",
                        CreatedRoles = createdRoles
                    });
                }

                return Ok(new
                {
                    Message = "All roles already exist",
                    CreatedRoles = Array.Empty<string>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ensuring roles exist");
                return StatusCode(500, new { Error = "An error occurred while ensuring roles exist" });
            }
        }

        /// <summary>
        /// Gets all application roles.
        /// </summary>
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            try
            {
                var roles = await _maintenanceService.GetAllRolesAsync();
                return Ok(new { Roles = roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles");
                return StatusCode(500, new { Error = "An error occurred while retrieving roles" });
            }
        }

        #endregion

        #region Admin Bootstrap

        /// <summary>
        /// Promotes a user to administrator by email.
        /// WARNING: This endpoint should be secured or removed in production.
        /// In development, this can be used for initial admin setup.
        /// In production, consider requiring a setup token or disabling after first use.
        /// </summary>
        [HttpPost("promote-admin/by-email")]
        public async Task<IActionResult> PromoteAdminByEmail([FromBody] PromoteByEmailRequest request)
        {
            try
            {
                // Check if setup mode is enabled (for production safety)
                var setupMode = _configuration.GetValue<bool>("Maintenance:SetupModeEnabled", true);
                if (!setupMode)
                {
                    return Forbid("Setup mode is disabled. This endpoint cannot be used in production.");
                }

                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest(new { Error = "Email is required" });
                }

                var success = await _maintenanceService.PromoteUserToAdminByEmailAsync(request.Email);

                if (!success)
                {
                    return NotFound(new { Error = "User not found or could not be promoted to admin" });
                }

                _logger.LogWarning("User with email {Email} was promoted to Admin via maintenance endpoint", request.Email);

                return Ok(new { Message = "User promoted to Admin successfully", Email = request.Email });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user to admin by email");
                return StatusCode(500, new { Error = "An error occurred while promoting user to admin" });
            }
        }

        /// <summary>
        /// Promotes a user to administrator by username.
        /// WARNING: This endpoint should be secured or removed in production.
        /// </summary>
        [HttpPost("promote-admin/by-username")]
        public async Task<IActionResult> PromoteAdminByUsername([FromBody] PromoteByUsernameRequest request)
        {
            try
            {
                // Check if setup mode is enabled (for production safety)
                var setupMode = _configuration.GetValue<bool>("Maintenance:SetupModeEnabled", true);
                if (!setupMode)
                {
                    return Forbid("Setup mode is disabled. This endpoint cannot be used in production.");
                }

                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return BadRequest(new { Error = "Username is required" });
                }

                var success = await _maintenanceService.PromoteUserToAdminByUsernameAsync(request.Username);

                if (!success)
                {
                    return NotFound(new { Error = "User not found or could not be promoted to admin" });
                }

                _logger.LogWarning("User with username {Username} was promoted to Admin via maintenance endpoint", request.Username);

                return Ok(new { Message = "User promoted to Admin successfully", Username = request.Username });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user to admin by username");
                return StatusCode(500, new { Error = "An error occurred while promoting user to admin" });
            }
        }

        /// <summary>
        /// Promotes a user to administrator by Discord ID.
        /// WARNING: This endpoint should be secured or removed in production.
        /// </summary>
        [HttpPost("promote-admin/by-discord-id")]
        public async Task<IActionResult> PromoteAdminByDiscordId([FromBody] PromoteByDiscordIdRequest request)
        {
            try
            {
                // Check if setup mode is enabled (for production safety)
                var setupMode = _configuration.GetValue<bool>("Maintenance:SetupModeEnabled", true);
                if (!setupMode)
                {
                    return Forbid("Setup mode is disabled. This endpoint cannot be used in production.");
                }

                if (request.DiscordId == 0)
                {
                    return BadRequest(new { Error = "Discord ID is required" });
                }

                var success = await _maintenanceService.PromoteUserToAdminByDiscordIdAsync(request.DiscordId);

                if (!success)
                {
                    return NotFound(new { Error = "User not found or could not be promoted to admin" });
                }

                _logger.LogWarning("User with Discord ID {DiscordId} was promoted to Admin via maintenance endpoint", request.DiscordId);

                return Ok(new { Message = "User promoted to Admin successfully", DiscordId = request.DiscordId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user to admin by Discord ID");
                return StatusCode(500, new { Error = "An error occurred while promoting user to admin" });
            }
        }

        /// <summary>
        /// Promotes a user to administrator by User ID.
        /// WARNING: This endpoint should be secured or removed in production.
        /// </summary>
        [HttpPost("promote-admin/by-user-id")]
        public async Task<IActionResult> PromoteAdminByUserId([FromBody] PromoteByUserIdRequest request)
        {
            try
            {
                // Check if setup mode is enabled (for production safety)
                var setupMode = _configuration.GetValue<bool>("Maintenance:SetupModeEnabled", true);
                if (!setupMode)
                {
                    return Forbid("Setup mode is disabled. This endpoint cannot be used in production.");
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { Error = "User ID is required" });
                }

                var success = await _maintenanceService.PromoteUserToAdminByUserIdAsync(request.UserId);

                if (!success)
                {
                    return NotFound(new { Error = "User not found or could not be promoted to admin" });
                }

                _logger.LogWarning("User with ID {UserId} was promoted to Admin via maintenance endpoint", request.UserId);

                return Ok(new { Message = "User promoted to Admin successfully", UserId = request.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error promoting user to admin by user ID");
                return StatusCode(500, new { Error = "An error occurred while promoting user to admin" });
            }
        }

        #endregion

        #region Discord Account Linking

        /// <summary>
        /// Creates a new admin account with Discord account pre-linked.
        /// This is the easiest way to bootstrap your first admin account with Discord integration.
        /// WARNING: This endpoint should be secured or removed in production.
        /// </summary>
        [HttpPost("create-admin-with-discord")]
        public async Task<IActionResult> CreateAdminWithDiscord([FromBody] CreateAdminWithDiscordRequest request)
        {
            try
            {
                // Check if setup mode is enabled (for production safety)
                var setupMode = _configuration.GetValue<bool>("Maintenance:SetupModeEnabled", true);
                if (!setupMode)
                {
                    return Forbid("Setup mode is disabled. This endpoint cannot be used in production.");
                }

                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Username) ||
                    string.IsNullOrWhiteSpace(request.Password) ||
                    request.DiscordId == 0)
                {
                    return BadRequest(new { Error = "Email, username, password, and Discord ID are required" });
                }

                var user = await _maintenanceService.CreateAdminWithDiscordAsync(
                    request.Email,
                    request.Username,
                    request.Password,
                    request.DiscordId,
                    request.DiscordUsername);

                if (user == null)
                {
                    return BadRequest(new { Error = "Failed to create admin account. Check if Discord ID is already linked or validation requirements." });
                }

                _logger.LogWarning(
                    "Admin account created with Discord link via maintenance endpoint: {Email} (Discord ID: {DiscordId})",
                    request.Email, request.DiscordId);

                return Ok(new
                {
                    Message = "Admin account created successfully with Discord link",
                    UserId = user.Id,
                    Email = user.Email,
                    Username = user.UserName,
                    DiscordId = user.DiscordUserId,
                    DiscordUsername = user.DiscordUsername
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating admin with Discord account");
                return StatusCode(500, new { Error = "An error occurred while creating admin account" });
            }
        }

        /// <summary>
        /// Links a Discord account to an existing user by email.
        /// Useful if you created an account without Discord linking and want to add it later.
        /// WARNING: This endpoint should be secured or removed in production.
        /// </summary>
        [HttpPost("link-discord/by-email")]
        public async Task<IActionResult> LinkDiscordByEmail([FromBody] LinkDiscordByEmailRequest request)
        {
            try
            {
                // Check if setup mode is enabled (for production safety)
                var setupMode = _configuration.GetValue<bool>("Maintenance:SetupModeEnabled", true);
                if (!setupMode)
                {
                    return Forbid("Setup mode is disabled. This endpoint cannot be used in production.");
                }

                if (string.IsNullOrWhiteSpace(request.Email) || request.DiscordId == 0)
                {
                    return BadRequest(new { Error = "Email and Discord ID are required" });
                }

                var success = await _maintenanceService.LinkDiscordAccountByEmailAsync(
                    request.Email,
                    request.DiscordId,
                    request.DiscordUsername);

                if (!success)
                {
                    return NotFound(new { Error = "User not found or Discord ID is already linked to another account" });
                }

                _logger.LogWarning(
                    "Discord account linked to user via maintenance endpoint: {Email} (Discord ID: {DiscordId})",
                    request.Email, request.DiscordId);

                return Ok(new
                {
                    Message = "Discord account linked successfully",
                    Email = request.Email,
                    DiscordId = request.DiscordId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking Discord account by email");
                return StatusCode(500, new { Error = "An error occurred while linking Discord account" });
            }
        }

        /// <summary>
        /// Links a Discord account to an existing user by user ID.
        /// WARNING: This endpoint should be secured or removed in production.
        /// </summary>
        [HttpPost("link-discord/by-user-id")]
        public async Task<IActionResult> LinkDiscordByUserId([FromBody] LinkDiscordByUserIdRequest request)
        {
            try
            {
                // Check if setup mode is enabled (for production safety)
                var setupMode = _configuration.GetValue<bool>("Maintenance:SetupModeEnabled", true);
                if (!setupMode)
                {
                    return Forbid("Setup mode is disabled. This endpoint cannot be used in production.");
                }

                if (string.IsNullOrWhiteSpace(request.UserId) || request.DiscordId == 0)
                {
                    return BadRequest(new { Error = "User ID and Discord ID are required" });
                }

                var success = await _maintenanceService.LinkDiscordAccountAsync(
                    request.UserId,
                    request.DiscordId,
                    request.DiscordUsername);

                if (!success)
                {
                    return NotFound(new { Error = "User not found or Discord ID is already linked to another account" });
                }

                _logger.LogWarning(
                    "Discord account linked to user via maintenance endpoint: {UserId} (Discord ID: {DiscordId})",
                    request.UserId, request.DiscordId);

                return Ok(new
                {
                    Message = "Discord account linked successfully",
                    UserId = request.UserId,
                    DiscordId = request.DiscordId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking Discord account by user ID");
                return StatusCode(500, new { Error = "An error occurred while linking Discord account" });
            }
        }

        /// <summary>
        /// Unlinks a Discord account from a user.
        /// WARNING: This endpoint should be secured or removed in production.
        /// </summary>
        [HttpPost("unlink-discord")]
        public async Task<IActionResult> UnlinkDiscord([FromBody] UnlinkDiscordRequest request)
        {
            try
            {
                // Check if setup mode is enabled (for production safety)
                var setupMode = _configuration.GetValue<bool>("Maintenance:SetupModeEnabled", true);
                if (!setupMode)
                {
                    return Forbid("Setup mode is disabled. This endpoint cannot be used in production.");
                }

                if (string.IsNullOrWhiteSpace(request.UserId))
                {
                    return BadRequest(new { Error = "User ID is required" });
                }

                var success = await _maintenanceService.UnlinkDiscordAccountAsync(request.UserId);

                if (!success)
                {
                    return NotFound(new { Error = "User not found or could not be unlinked" });
                }

                _logger.LogWarning("Discord account unlinked from user via maintenance endpoint: {UserId}", request.UserId);

                return Ok(new { Message = "Discord account unlinked successfully", UserId = request.UserId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlinking Discord account");
                return StatusCode(500, new { Error = "An error occurred while unlinking Discord account" });
            }
        }

        #endregion

        #region Request Models

        public class PromoteByEmailRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        public class PromoteByUsernameRequest
        {
            public string Username { get; set; } = string.Empty;
        }

        public class PromoteByDiscordIdRequest
        {
            public ulong DiscordId { get; set; }
        }

        public class PromoteByUserIdRequest
        {
            public string UserId { get; set; } = string.Empty;
        }

        public class CreateAdminWithDiscordRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public ulong DiscordId { get; set; }
            public string? DiscordUsername { get; set; }
        }

        public class LinkDiscordByEmailRequest
        {
            public string Email { get; set; } = string.Empty;
            public ulong DiscordId { get; set; }
            public string? DiscordUsername { get; set; }
        }

        public class LinkDiscordByUserIdRequest
        {
            public string UserId { get; set; } = string.Empty;
            public ulong DiscordId { get; set; }
            public string? DiscordUsername { get; set; }
        }

        public class UnlinkDiscordRequest
        {
            public string UserId { get; set; } = string.Empty;
        }

        #endregion
    }
}

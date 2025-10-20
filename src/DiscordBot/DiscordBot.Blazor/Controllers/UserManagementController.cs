using DiscordBot.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DiscordBot.Blazor.Controllers
{
    /// <summary>
    /// API controller for user management operations.
    /// Provides endpoints for user CRUD, role management, and statistics.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserManagementService _userManagementService;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            IUserManagementService userManagementService,
            ILogger<UserManagementController> logger)
        {
            _userManagementService = userManagementService;
            _logger = logger;
        }

        #region User Retrieval

        /// <summary>
        /// Gets all users with pagination.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                var users = await _userManagementService.GetAllUsersAsync(skip, take);
                var total = await _userManagementService.GetUserCountAsync();

                return Ok(new
                {
                    Users = users.Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.EmailConfirmed,
                        u.DiscordUserId,
                        u.DiscordUsername,
                        u.DiscordTag,
                        u.DiscordAvatarUrl,
                        u.IsDiscordLinked,
                        u.CreatedAt,
                        u.LastLoginAt,
                        u.LockoutEnd,
                        IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow
                    }),
                    Total = total,
                    Skip = skip,
                    Take = take
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, new { Error = "An error occurred while retrieving users" });
            }
        }

        /// <summary>
        /// Gets a specific user by ID.
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUser(string userId)
        {
            try
            {
                var user = await _userManagementService.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Error = "User not found" });
                }

                var roles = await _userManagementService.GetUserRolesAsync(userId);

                return Ok(new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.EmailConfirmed,
                    user.DiscordUserId,
                    user.DiscordUsername,
                    user.DiscordTag,
                    user.DiscordAvatarUrl,
                    user.IsDiscordLinked,
                    user.DiscordLinkedAt,
                    user.CreatedAt,
                    user.LastLoginAt,
                    user.LockoutEnd,
                    IsLocked = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                    Roles = roles
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", userId);
                return StatusCode(500, new { Error = "An error occurred while retrieving the user" });
            }
        }

        /// <summary>
        /// Searches for users.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string query, [FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return BadRequest(new { Error = "Search query is required" });
                }

                var users = await _userManagementService.SearchUsersAsync(query, skip, take);

                return Ok(new
                {
                    Users = users.Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.EmailConfirmed,
                        u.DiscordUserId,
                        u.DiscordUsername,
                        u.DiscordTag,
                        u.DiscordAvatarUrl,
                        u.IsDiscordLinked,
                        u.CreatedAt,
                        u.LastLoginAt,
                        u.LockoutEnd,
                        IsLocked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow
                    }),
                    Query = query,
                    Skip = skip,
                    Take = take
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching users with query: {Query}", query);
                return StatusCode(500, new { Error = "An error occurred while searching users" });
            }
        }

        /// <summary>
        /// Gets users with linked Discord accounts.
        /// </summary>
        [HttpGet("discord-linked")]
        public async Task<IActionResult> GetDiscordLinkedUsers([FromQuery] int skip = 0, [FromQuery] int take = 100)
        {
            try
            {
                var users = await _userManagementService.GetDiscordLinkedUsersAsync(skip, take);

                return Ok(new
                {
                    Users = users.Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        u.DiscordUserId,
                        u.DiscordUsername,
                        u.DiscordTag,
                        u.DiscordAvatarUrl,
                        u.DiscordLinkedAt,
                        u.CreatedAt,
                        u.LastLoginAt
                    }),
                    Skip = skip,
                    Take = take
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Discord linked users");
                return StatusCode(500, new { Error = "An error occurred while retrieving Discord linked users" });
            }
        }

        /// <summary>
        /// Gets user statistics.
        /// </summary>
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            try
            {
                var stats = await _userManagementService.GetUserStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user statistics");
                return StatusCode(500, new { Error = "An error occurred while retrieving statistics" });
            }
        }

        #endregion

        #region User Management

        /// <summary>
        /// Creates a new user.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Username) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest(new { Error = "Email, username, and password are required" });
                }

                var user = await _userManagementService.CreateUserAsync(
                    request.Email,
                    request.Username,
                    request.Password);

                if (user == null)
                {
                    return BadRequest(new { Error = "Failed to create user. Check validation requirements." });
                }

                return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, new
                {
                    user.Id,
                    user.UserName,
                    user.Email,
                    user.CreatedAt
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, new { Error = "An error occurred while creating the user" });
            }
        }

        /// <summary>
        /// Updates a user.
        /// </summary>
        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
        {
            try
            {
                var success = await _userManagementService.UpdateUserAsync(
                    userId,
                    request.Email,
                    request.Username);

                if (!success)
                {
                    return BadRequest(new { Error = "Failed to update user" });
                }

                return Ok(new { Message = "User updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", userId);
                return StatusCode(500, new { Error = "An error occurred while updating the user" });
            }
        }

        /// <summary>
        /// Deletes a user.
        /// </summary>
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var success = await _userManagementService.DeleteUserAsync(userId);

                if (!success)
                {
                    return NotFound(new { Error = "User not found or could not be deleted" });
                }

                return Ok(new { Message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", userId);
                return StatusCode(500, new { Error = "An error occurred while deleting the user" });
            }
        }

        /// <summary>
        /// Resets a user's password.
        /// </summary>
        [HttpPost("{userId}/reset-password")]
        public async Task<IActionResult> ResetPassword(string userId, [FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    return BadRequest(new { Error = "New password is required" });
                }

                var success = await _userManagementService.ResetPasswordAsync(userId, request.NewPassword);

                if (!success)
                {
                    return BadRequest(new { Error = "Failed to reset password. Check password requirements." });
                }

                return Ok(new { Message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user {UserId}", userId);
                return StatusCode(500, new { Error = "An error occurred while resetting the password" });
            }
        }

        /// <summary>
        /// Locks a user account.
        /// </summary>
        [HttpPost("{userId}/lock")]
        public async Task<IActionResult> LockUser(string userId, [FromBody] LockUserRequest? request = null)
        {
            try
            {
                var lockoutEnd = request?.LockoutEnd;
                var success = await _userManagementService.LockUserAsync(userId, lockoutEnd);

                if (!success)
                {
                    return NotFound(new { Error = "User not found or could not be locked" });
                }

                return Ok(new { Message = "User locked successfully", LockoutEnd = lockoutEnd });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error locking user {UserId}", userId);
                return StatusCode(500, new { Error = "An error occurred while locking the user" });
            }
        }

        /// <summary>
        /// Unlocks a user account.
        /// </summary>
        [HttpPost("{userId}/unlock")]
        public async Task<IActionResult> UnlockUser(string userId)
        {
            try
            {
                var success = await _userManagementService.UnlockUserAsync(userId);

                if (!success)
                {
                    return NotFound(new { Error = "User not found or could not be unlocked" });
                }

                return Ok(new { Message = "User unlocked successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error unlocking user {UserId}", userId);
                return StatusCode(500, new { Error = "An error occurred while unlocking the user" });
            }
        }

        #endregion

        #region Role Management

        /// <summary>
        /// Gets roles for a user.
        /// </summary>
        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            try
            {
                var roles = await _userManagementService.GetUserRolesAsync(userId);
                return Ok(new { UserId = userId, Roles = roles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles for user {UserId}", userId);
                return StatusCode(500, new { Error = "An error occurred while retrieving user roles" });
            }
        }

        /// <summary>
        /// Adds a user to a role.
        /// </summary>
        [HttpPost("{userId}/roles/{roleName}")]
        public async Task<IActionResult> AddUserToRole(string userId, string roleName)
        {
            try
            {
                var success = await _userManagementService.AddUserToRoleAsync(userId, roleName);

                if (!success)
                {
                    return BadRequest(new { Error = "Failed to add user to role. User or role may not exist." });
                }

                return Ok(new { Message = $"User added to role '{roleName}' successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to role {RoleName}", userId, roleName);
                return StatusCode(500, new { Error = "An error occurred while adding user to role" });
            }
        }

        /// <summary>
        /// Removes a user from a role.
        /// </summary>
        [HttpDelete("{userId}/roles/{roleName}")]
        public async Task<IActionResult> RemoveUserFromRole(string userId, string roleName)
        {
            try
            {
                var success = await _userManagementService.RemoveUserFromRoleAsync(userId, roleName);

                if (!success)
                {
                    return BadRequest(new { Error = "Failed to remove user from role" });
                }

                return Ok(new { Message = $"User removed from role '{roleName}' successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from role {RoleName}", userId, roleName);
                return StatusCode(500, new { Error = "An error occurred while removing user from role" });
            }
        }

        #endregion

        #region Request Models

        public class CreateUserRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class UpdateUserRequest
        {
            public string? Email { get; set; }
            public string? Username { get; set; }
        }

        public class ResetPasswordRequest
        {
            public string NewPassword { get; set; } = string.Empty;
        }

        public class LockUserRequest
        {
            public DateTimeOffset? LockoutEnd { get; set; }
        }

        #endregion
    }
}

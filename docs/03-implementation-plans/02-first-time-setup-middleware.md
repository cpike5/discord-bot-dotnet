# First Time Setup Middleware Implementation Plan

**Version:** 1.0
**Last Updated:** 2025-10-20
**Status:** Active
**Estimated Duration:** 1-2 weeks
**Priority:** Medium

---

## Overview

This document outlines the implementation plan for a First Time Setup Middleware feature that detects when the application is run for the first time and guides administrators through the initial configuration process. This ensures the application is properly bootstrapped with essential data before regular operations begin.

**Key Features:**
- Automatic detection of first-time application launch
- Admin account creation workflow
- Database seeding with essential data (roles, system configurations)
- Setup completion tracking
- Middleware-based request interception
- Clean setup UI experience

---

## Table of Contents

- [1. Purpose & Goals](#1-purpose--goals)
- [2. System Requirements](#2-system-requirements)
- [3. Architecture Overview](#3-architecture-overview)
- [4. Implementation Phases](#4-implementation-phases)
- [5. Detailed Implementation Tasks](#5-detailed-implementation-tasks)
- [6. Database Changes](#6-database-changes)
- [7. File Structure](#7-file-structure)
- [8. Testing Strategy](#8-testing-strategy)
- [9. Security Considerations](#9-security-considerations)
- [10. References](#10-references)

---

## 1. Purpose & Goals

### 1.1 Problem Statement

When deploying the application to a new environment or running it for the first time:
- The database is empty (no users, no roles, no system data)
- There is no admin account to access administrative functions
- Manual database seeding is error-prone and inconsistent
- Users may encounter confusing errors when trying to access the application

### 1.2 Solution Goals

**Primary Goals:**
1. **Automated Detection**: Detect when the application needs initial setup
2. **Guided Setup**: Provide a user-friendly interface for configuration
3. **Data Seeding**: Populate database with essential data (roles, default settings)
4. **Admin Creation**: Create the first admin account securely
5. **One-Time Execution**: Ensure setup runs only once

**Success Criteria:**
- ✅ Application detects first-time launch automatically
- ✅ Setup workflow is intuitive and error-free
- ✅ Database is properly seeded with all required data
- ✅ Admin account is created securely
- ✅ Setup cannot be accidentally re-triggered
- ✅ Application starts normally after setup completion

---

## 2. System Requirements

### 2.1 Prerequisites

**Existing Infrastructure:**
- ASP.NET Identity configured ([Program.cs:43-47](../../src/DiscordBot/DiscordBot.Blazor/Program.cs#L43-L47))
- Entity Framework Core with SQLite ([ApplicationDbContext.cs](../../src/DiscordBot/DiscordBot.Blazor/Data/ApplicationDbContext.cs))
- Blazor Server components
- Role-based authorization system

### 2.2 Required Data Seeds

**Identity Roles:**
- `SuperAdmin` - Full system access, cannot be removed
- `Admin` - Administrative access
- `Moderator` - Moderation capabilities
- `User` - Standard user access (default)

**System Configuration (Future):**
- Default application settings
- Feature flags
- Discord bot configuration validation

### 2.3 Security Requirements

- ✅ Setup page must be accessible without authentication
- ✅ Setup page must be inaccessible after completion
- ✅ Admin password must meet security requirements
- ✅ Setup completion must be persisted securely
- ✅ Setup process must be atomic (all-or-nothing)

---

## 3. Architecture Overview

### 3.1 Flow Diagram

```
┌─────────────────────────────────────────────────────────┐
│              Application Startup (Program.cs)           │
└────────────────────┬────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│         First Time Setup Middleware                     │
│                                                          │
│  ┌─────────────────────────────────────────────────┐   │
│  │ 1. Check Setup Completion Status                │   │
│  │    - Query SetupStatus table                    │   │
│  │    - Check for existing admin users             │   │
│  └─────────────────┬───────────────────────────────┘   │
│                    │                                     │
│         ┌──────────┴──────────┐                         │
│         ▼                     ▼                          │
│   [Setup Complete]      [Setup Needed]                  │
│         │                     │                          │
│         │                     ▼                          │
│         │          ┌──────────────────────┐             │
│         │          │ Redirect to          │             │
│         │          │ /setup/welcome       │             │
│         │          └──────────────────────┘             │
│         │                                                │
│         ▼                                                │
│   [Continue to requested page]                          │
└─────────────────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────┐
│              Setup Workflow Pages                       │
│                                                          │
│  Step 1: Welcome & Overview                             │
│  ├─ /setup/welcome                                      │
│  └─ Explains what will be configured                    │
│                                                          │
│  Step 2: Create Admin Account                           │
│  ├─ /setup/admin                                        │
│  ├─ Email, username, password                           │
│  ├─ Real-time validation                                │
│  └─ Password strength indicator                         │
│                                                          │
│  Step 3: Database Seeding                               │
│  ├─ /setup/seed                                         │
│  ├─ Seed roles (SuperAdmin, Admin, Moderator, User)    │
│  ├─ Create admin user with SuperAdmin role             │
│  └─ Progress indicator                                  │
│                                                          │
│  Step 4: Completion                                     │
│  ├─ /setup/complete                                     │
│  ├─ Mark setup as complete in database                 │
│  ├─ Display success message                            │
│  └─ Redirect to login page                             │
└─────────────────────────────────────────────────────────┘
```

### 3.2 Component Architecture

**Middleware Layer:**
```
FirstTimeSetupMiddleware
├─ Checks: IsSetupComplete()
├─ Redirects: /setup/welcome (if incomplete)
└─ Allows: Setup pages + static resources
```

**Service Layer:**
```
IFirstTimeSetupService
├─ IsSetupCompleteAsync()
├─ CreateAdminAccountAsync(AdminSetupModel)
├─ SeedDatabaseAsync()
├─ MarkSetupCompleteAsync()
└─ ValidateSetupStateAsync()
```

**Database Layer:**
```
SetupStatus (Entity)
├─ IsComplete (bool)
├─ CompletedAt (DateTime?)
├─ AdminUserId (string?)
└─ SetupVersion (string)
```

---

## 4. Implementation Phases

### Phase 1: Database & Entity Setup (Days 1-2)
- Create `SetupStatus` entity
- Add migration
- Update `ApplicationDbContext`
- Test database operations

### Phase 2: Service Layer (Days 3-4)
- Create `IFirstTimeSetupService` interface
- Implement `FirstTimeSetupService`
- Add role seeding logic
- Add admin account creation
- Write unit tests

### Phase 3: Middleware (Days 5-6)
- Create `FirstTimeSetupMiddleware`
- Add setup detection logic
- Configure routing rules
- Test middleware flow

### Phase 4: UI Components (Days 7-9)
- Create setup page layouts
- Implement step-by-step wizard
- Add form validation
- Apply design system styles
- Add progress indicators

### Phase 5: Integration & Testing (Day 10)
- Integration testing
- End-to-end setup flow testing
- Security testing
- Documentation

---

## 5. Detailed Implementation Tasks

### 5.1 Phase 1: Database & Entity Setup

#### Task 1.1: Create SetupStatus Entity
**File:** `src/DiscordBot/DiscordBot.Core/Entities/SetupStatus.cs`

```csharp
namespace DiscordBot.Core.Entities
{
    /// <summary>
    /// Tracks the completion status of the first-time setup process.
    /// Only one record should exist in this table.
    /// </summary>
    public class SetupStatus
    {
        /// <summary>
        /// Primary key. Should always be 1.
        /// </summary>
        public int Id { get; set; } = 1;

        /// <summary>
        /// Indicates whether the initial setup has been completed.
        /// </summary>
        public bool IsComplete { get; set; } = false;

        /// <summary>
        /// Timestamp when setup was completed (UTC).
        /// </summary>
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// User ID of the admin account created during setup.
        /// </summary>
        public string? AdminUserId { get; set; }

        /// <summary>
        /// Version of setup that was completed (for future migrations).
        /// </summary>
        public string SetupVersion { get; set; } = "1.0";

        /// <summary>
        /// Navigation property to admin user.
        /// </summary>
        public virtual ApplicationUser? AdminUser { get; set; }
    }
}
```

**Acceptance Criteria:**
- [ ] Entity class created with all properties
- [ ] XML documentation complete
- [ ] Navigation properties configured

---

#### Task 1.2: Update ApplicationDbContext
**File:** `src/DiscordBot/DiscordBot.Blazor/Data/ApplicationDbContext.cs`

**Changes:**
1. Add DbSet for `SetupStatus`
2. Configure entity in `OnModelCreating`

```csharp
// Add to ApplicationDbContext
public DbSet<SetupStatus> SetupStatus { get; set; }

// Add to OnModelCreating
#region SetupStatus Configuration

modelBuilder.Entity<SetupStatus>(entity =>
{
    entity.HasKey(e => e.Id);

    entity.Property(e => e.IsComplete)
        .IsRequired()
        .HasDefaultValue(false);

    entity.Property(e => e.SetupVersion)
        .HasMaxLength(10)
        .IsRequired();

    entity.Property(e => e.CompletedAt)
        .IsRequired(false);

    entity.Property(e => e.AdminUserId)
        .HasMaxLength(450)  // ASP.NET Identity key length
        .IsRequired(false);

    // Foreign key to admin user
    entity.HasOne(e => e.AdminUser)
        .WithMany()
        .HasForeignKey(e => e.AdminUserId)
        .OnDelete(DeleteBehavior.SetNull);

    // Seed initial record (setup incomplete)
    entity.HasData(new SetupStatus
    {
        Id = 1,
        IsComplete = false,
        SetupVersion = "1.0"
    });
});

#endregion
```

**Acceptance Criteria:**
- [ ] DbSet added
- [ ] Entity configuration complete
- [ ] Initial seed data configured
- [ ] Foreign key relationship established

---

#### Task 1.3: Create and Apply Migration
**Commands:**
```bash
# Create migration
dotnet ef migrations add AddSetupStatus --project src/DiscordBot/DiscordBot.Blazor

# Review migration file
# Ensure seed data is included

# Apply migration (development)
dotnet ef database update --project src/DiscordBot/DiscordBot.Blazor
```

**Acceptance Criteria:**
- [ ] Migration created successfully
- [ ] Migration includes table creation
- [ ] Migration includes seed data
- [ ] Migration applied to database
- [ ] SetupStatus table exists with initial record

---

### 5.2 Phase 2: Service Layer

#### Task 2.1: Create Service Interface
**File:** `src/DiscordBot/DiscordBot.Core/Services/IFirstTimeSetupService.cs`

```csharp
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
```

**Acceptance Criteria:**
- [ ] Interface created with all methods
- [ ] Result classes defined
- [ ] XML documentation complete
- [ ] Located in Core project (interface)

---

#### Task 2.2: Implement Service
**File:** `src/DiscordBot/DiscordBot.Blazor/Services/FirstTimeSetupService.cs`

```csharp
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
```

**Acceptance Criteria:**
- [ ] Service implements all interface methods
- [ ] Role seeding logic complete
- [ ] Admin creation with validation
- [ ] Error handling implemented
- [ ] Logging added
- [ ] Atomic operations (rollback on failure)

---

#### Task 2.3: Register Service in DI Container
**File:** `src/DiscordBot/DiscordBot.Blazor/Program.cs`

**Changes:**
```csharp
// Add after line 57 (other service registrations)
builder.Services.AddScoped<IFirstTimeSetupService, FirstTimeSetupService>();
```

**Acceptance Criteria:**
- [ ] Service registered in DI container
- [ ] Service can be resolved

---

### 5.3 Phase 3: Middleware

#### Task 3.1: Create Middleware
**File:** `src/DiscordBot/DiscordBot.Blazor/Middleware/FirstTimeSetupMiddleware.cs`

```csharp
using DiscordBot.Core.Services;

namespace DiscordBot.Blazor.Middleware
{
    /// <summary>
    /// Middleware that intercepts requests and redirects to setup if not complete.
    /// </summary>
    public class FirstTimeSetupMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<FirstTimeSetupMiddleware> _logger;

        // Paths that are always allowed (even during setup)
        private static readonly HashSet<string> AllowedPaths = new(StringComparer.OrdinalIgnoreCase)
        {
            "/setup/welcome",
            "/setup/admin",
            "/setup/seed",
            "/setup/complete",
            "/setup/error",
            "/_framework",  // Blazor framework files
            "/_blazor",     // Blazor SignalR hub
            "/css",         // Static CSS
            "/js",          // Static JS
            "/lib",         // Library files
            "/images",      // Static images
            "/favicon.ico"
        };

        public FirstTimeSetupMiddleware(
            RequestDelegate next,
            ILogger<FirstTimeSetupMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IFirstTimeSetupService setupService)
        {
            // Check if path is allowed without setup
            var path = context.Request.Path.Value ?? string.Empty;
            if (IsPathAllowed(path))
            {
                await _next(context);
                return;
            }

            // Check if setup is complete
            var isSetupComplete = await setupService.IsSetupCompleteAsync();

            if (!isSetupComplete)
            {
                _logger.LogInformation("Setup incomplete. Redirecting to setup workflow.");
                context.Response.Redirect("/setup/welcome");
                return;
            }

            // Setup is complete, continue to next middleware
            await _next(context);
        }

        private static bool IsPathAllowed(string path)
        {
            return AllowedPaths.Any(allowed =>
                path.StartsWith(allowed, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Extension method for registering the middleware.
    /// </summary>
    public static class FirstTimeSetupMiddlewareExtensions
    {
        public static IApplicationBuilder UseFirstTimeSetup(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<FirstTimeSetupMiddleware>();
        }
    }
}
```

**Acceptance Criteria:**
- [ ] Middleware intercepts requests
- [ ] Allowed paths are correctly exempted
- [ ] Redirects to setup when incomplete
- [ ] Allows normal operation when complete
- [ ] Extension method created

---

#### Task 3.2: Register Middleware
**File:** `src/DiscordBot/DiscordBot.Blazor/Program.cs`

**Changes:**
```csharp
// Add after line 78 (after UseHttpsRedirection)
// IMPORTANT: Must be before UseStaticFiles and authentication
app.UseFirstTimeSetup();
```

**Acceptance Criteria:**
- [ ] Middleware registered in correct order
- [ ] Middleware executes before authentication
- [ ] Middleware executes before static files

---

### 5.4 Phase 4: UI Components

#### Task 4.1: Create Setup Layout
**File:** `src/DiscordBot/DiscordBot.Blazor/Components/Setup/SetupLayout.razor`

```razor
@inherits LayoutComponentBase

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>First Time Setup - Discord Bot</title>
    <base href="~/" />
    <link rel="stylesheet" href="css/bootstrap/bootstrap.min.css" />
    <link rel="stylesheet" href="css/app.css" />
    <link rel="stylesheet" href="css/setup.css" />
    <HeadOutlet />
</head>
<body class="setup-body">
    <div class="setup-container">
        <header class="setup-header">
            <h1>Discord Bot Setup</h1>
            <p>Let's get your application configured</p>
        </header>

        <main class="setup-content">
            @Body
        </main>

        <footer class="setup-footer">
            <p>&copy; 2025 Discord Bot. All rights reserved.</p>
        </footer>
    </div>

    <script src="_framework/blazor.server.js"></script>
</body>
</html>
```

**CSS File:** `src/DiscordBot/DiscordBot.Blazor/wwwroot/css/setup.css`

```css
/* Setup-specific styles */
.setup-body {
    background: linear-gradient(135deg, #555B6E 0%, #D3C4E3 100%);
    min-height: 100vh;
    display: flex;
    align-items: center;
    justify-content: center;
    font-family: 'Inter', sans-serif;
}

.setup-container {
    background: white;
    border-radius: 12px;
    box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15);
    max-width: 600px;
    width: 90%;
    padding: 2rem;
}

.setup-header {
    text-align: center;
    margin-bottom: 2rem;
    padding-bottom: 1rem;
    border-bottom: 2px solid #D3C4E3;
}

.setup-header h1 {
    color: #DA4167;
    font-size: 2rem;
    margin-bottom: 0.5rem;
}

.setup-header p {
    color: #555B6E;
    font-size: 1rem;
}

.setup-content {
    margin-bottom: 2rem;
}

.setup-footer {
    text-align: center;
    color: #888;
    font-size: 0.875rem;
    padding-top: 1rem;
    border-top: 1px solid #eee;
}

.setup-progress {
    display: flex;
    justify-content: space-between;
    margin-bottom: 2rem;
}

.setup-step {
    flex: 1;
    text-align: center;
    position: relative;
    padding-bottom: 1rem;
}

.setup-step::before {
    content: '';
    position: absolute;
    bottom: 0;
    left: 0;
    right: 0;
    height: 4px;
    background: #eee;
}

.setup-step.active::before {
    background: #DA4167;
}

.setup-step.complete::before {
    background: #87B38D;
}

.btn-primary-setup {
    background-color: #DA4167;
    border-color: #DA4167;
    color: white;
    padding: 0.75rem 2rem;
    border-radius: 8px;
    font-weight: 600;
    transition: all 0.3s ease;
}

.btn-primary-setup:hover {
    background-color: #c23556;
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(218, 65, 103, 0.3);
}

.setup-form-group {
    margin-bottom: 1.5rem;
}

.setup-form-label {
    display: block;
    margin-bottom: 0.5rem;
    color: #555B6E;
    font-weight: 600;
}

.setup-form-input {
    width: 100%;
    padding: 0.75rem;
    border: 2px solid #D3C4E3;
    border-radius: 8px;
    font-size: 1rem;
    transition: border-color 0.3s ease;
}

.setup-form-input:focus {
    outline: none;
    border-color: #DA4167;
}

.setup-error {
    background-color: #FEE;
    border-left: 4px solid #DA4167;
    padding: 1rem;
    margin-bottom: 1rem;
    border-radius: 4px;
}

.setup-success {
    background-color: #EFE;
    border-left: 4px solid #87B38D;
    padding: 1rem;
    margin-bottom: 1rem;
    border-radius: 4px;
}
```

**Acceptance Criteria:**
- [ ] Layout created with header/footer
- [ ] CSS styles applied matching design system
- [ ] Responsive design
- [ ] Progress indicator structure

---

#### Task 4.2: Create Setup Pages

**File:** `src/DiscordBot/DiscordBot.Blazor/Components/Pages/Setup/Welcome.razor`

```razor
@page "/setup/welcome"
@layout SetupLayout
@inject NavigationManager Navigation

<PageTitle>Welcome - Setup</PageTitle>

<div class="setup-welcome">
    <div class="welcome-icon">
        <svg width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="#DA4167" stroke-width="2">
            <path d="M12 2L2 7l10 5 10-5-10-5z"></path>
            <path d="M2 17l10 5 10-5"></path>
            <path d="M2 12l10 5 10-5"></path>
        </svg>
    </div>

    <h2>Welcome to Discord Bot!</h2>
    <p>This setup wizard will help you configure your application for first use.</p>

    <div class="setup-checklist">
        <h3>What we'll configure:</h3>
        <ul>
            <li>✅ Create your admin account</li>
            <li>✅ Set up user roles and permissions</li>
            <li>✅ Initialize the database</li>
        </ul>
    </div>

    <div class="setup-notes">
        <p><strong>⏱️ Estimated time:</strong> 2-3 minutes</p>
        <p><strong>📝 Note:</strong> You'll need to provide an email and password for the admin account.</p>
    </div>

    <div class="setup-actions">
        <button class="btn btn-primary-setup" @onclick="StartSetup">
            Get Started
        </button>
    </div>
</div>

@code {
    private void StartSetup()
    {
        Navigation.NavigateTo("/setup/admin");
    }
}
```

**Additional CSS for Welcome page:**
```css
.setup-welcome {
    text-align: center;
}

.welcome-icon {
    margin-bottom: 1.5rem;
}

.setup-welcome h2 {
    color: #555B6E;
    margin-bottom: 1rem;
}

.setup-checklist {
    text-align: left;
    margin: 2rem auto;
    max-width: 400px;
    padding: 1.5rem;
    background: #f9f9f9;
    border-radius: 8px;
}

.setup-checklist h3 {
    color: #555B6E;
    margin-bottom: 1rem;
}

.setup-checklist ul {
    list-style: none;
    padding: 0;
}

.setup-checklist li {
    padding: 0.5rem 0;
    color: #555B6E;
}

.setup-notes {
    margin: 1.5rem 0;
    padding: 1rem;
    background: #FFF9E6;
    border-radius: 8px;
}

.setup-actions {
    margin-top: 2rem;
}
```

---

**File:** `src/DiscordBot/DiscordBot.Blazor/Components/Pages/Setup/Admin.razor`

```razor
@page "/setup/admin"
@layout SetupLayout
@inject IFirstTimeSetupService SetupService
@inject NavigationManager Navigation
@inject ILogger<Admin> Logger

<PageTitle>Create Admin Account - Setup</PageTitle>

<div class="setup-admin">
    <h2>Create Admin Account</h2>
    <p>This account will have full access to the application.</p>

    @if (errors.Any())
    {
        <div class="setup-error">
            <strong>⚠️ Error:</strong>
            <ul>
                @foreach (var error in errors)
                {
                    <li>@error</li>
                }
            </ul>
        </div>
    }

    <EditForm Model="model" OnValidSubmit="HandleSubmit">
        <DataAnnotationsValidator />

        <div class="setup-form-group">
            <label class="setup-form-label" for="email">Email Address</label>
            <InputText id="email"
                       class="setup-form-input"
                       @bind-Value="model.Email"
                       placeholder="admin@example.com" />
            <ValidationMessage For="@(() => model.Email)" />
        </div>

        <div class="setup-form-group">
            <label class="setup-form-label" for="username">Username</label>
            <InputText id="username"
                       class="setup-form-input"
                       @bind-Value="model.Username"
                       placeholder="admin" />
            <ValidationMessage For="@(() => model.Username)" />
        </div>

        <div class="setup-form-group">
            <label class="setup-form-label" for="password">Password</label>
            <InputText id="password"
                       type="password"
                       class="setup-form-input"
                       @bind-Value="model.Password"
                       placeholder="••••••••" />
            <ValidationMessage For="@(() => model.Password)" />
            <small class="form-text">
                Password must be at least 6 characters and contain uppercase, lowercase, and numbers.
            </small>
        </div>

        <div class="setup-form-group">
            <label class="setup-form-label" for="confirmPassword">Confirm Password</label>
            <InputText id="confirmPassword"
                       type="password"
                       class="setup-form-input"
                       @bind-Value="model.ConfirmPassword"
                       placeholder="••••••••" />
            <ValidationMessage For="@(() => model.ConfirmPassword)" />
        </div>

        <div class="setup-actions">
            <button type="submit" class="btn btn-primary-setup" disabled="@isSubmitting">
                @if (isSubmitting)
                {
                    <span>Creating Account...</span>
                }
                else
                {
                    <span>Create Admin Account</span>
                }
            </button>
        </div>
    </EditForm>
</div>

@code {
    private AdminSetupModel model = new();
    private List<string> errors = new();
    private bool isSubmitting = false;

    private async Task HandleSubmit()
    {
        isSubmitting = true;
        errors.Clear();

        try
        {
            var result = await SetupService.CreateAdminAccountAsync(
                model.Email!,
                model.Username!,
                model.Password!);

            if (result.Success)
            {
                Logger.LogInformation("Admin account created successfully");
                Navigation.NavigateTo("/setup/seed");
            }
            else
            {
                errors = result.Errors;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error creating admin account");
            errors.Add("An unexpected error occurred. Please try again.");
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private class AdminSetupModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        public string? ConfirmPassword { get; set; }
    }
}
```

---

**File:** `src/DiscordBot/DiscordBot.Blazor/Components/Pages/Setup/Seed.razor`

```razor
@page "/setup/seed"
@layout SetupLayout
@inject IFirstTimeSetupService SetupService
@inject NavigationManager Navigation
@inject ILogger<Seed> Logger
@implements IDisposable

<PageTitle>Initializing Database - Setup</PageTitle>

<div class="setup-seed">
    <h2>Initializing Database</h2>
    <p>Setting up roles and system data...</p>

    @if (isSeeding)
    {
        <div class="setup-progress-indicator">
            <div class="spinner-border text-primary" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
            <p>Please wait...</p>
        </div>
    }
    else if (errors.Any())
    {
        <div class="setup-error">
            <strong>⚠️ Seeding Failed:</strong>
            <ul>
                @foreach (var error in errors)
                {
                    <li>@error</li>
                }
            </ul>
            <button class="btn btn-primary-setup" @onclick="RetrySeed">
                Retry
            </button>
        </div>
    }
    else if (seedComplete)
    {
        <div class="setup-success">
            <strong>✅ Database Initialized Successfully!</strong>
            <p>Created @rolesCreated roles.</p>
        </div>
        <div class="setup-actions">
            <button class="btn btn-primary-setup" @onclick="CompleteSetup">
                Continue
            </button>
        </div>
    }
</div>

@code {
    private bool isSeeding = true;
    private bool seedComplete = false;
    private int rolesCreated = 0;
    private List<string> errors = new();
    private CancellationTokenSource? cts;

    protected override async Task OnInitializedAsync()
    {
        await PerformSeed();
    }

    private async Task PerformSeed()
    {
        isSeeding = true;
        errors.Clear();
        cts = new CancellationTokenSource();

        try
        {
            var result = await SetupService.SeedDatabaseAsync();

            if (result.Success)
            {
                rolesCreated = result.RolesCreated;
                seedComplete = true;
                Logger.LogInformation("Database seeded successfully");
            }
            else
            {
                errors = result.Errors;
                Logger.LogError("Database seeding failed");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during database seeding");
            errors.Add("An unexpected error occurred during seeding.");
        }
        finally
        {
            isSeeding = false;
            StateHasChanged();
        }
    }

    private async Task RetrySeed()
    {
        await PerformSeed();
    }

    private void CompleteSetup()
    {
        Navigation.NavigateTo("/setup/complete");
    }

    public void Dispose()
    {
        cts?.Cancel();
        cts?.Dispose();
    }
}
```

---

**File:** `src/DiscordBot/DiscordBot.Blazor/Components/Pages/Setup/Complete.razor`

```razor
@page "/setup/complete"
@layout SetupLayout
@inject IFirstTimeSetupService SetupService
@inject NavigationManager Navigation
@inject ILogger<Complete> Logger

<PageTitle>Setup Complete - Setup</PageTitle>

<div class="setup-complete">
    @if (isCompleting)
    {
        <div class="setup-progress-indicator">
            <div class="spinner-border text-success" role="status">
                <span class="visually-hidden">Completing...</span>
            </div>
            <p>Finalizing setup...</p>
        </div>
    }
    else if (errors.Any())
    {
        <div class="setup-error">
            <strong>⚠️ Error Completing Setup:</strong>
            <ul>
                @foreach (var error in errors)
                {
                    <li>@error</li>
                }
            </ul>
        </div>
    }
    else if (setupComplete)
    {
        <div class="success-animation">
            <svg width="100" height="100" viewBox="0 0 100 100">
                <circle cx="50" cy="50" r="45" fill="#87B38D"/>
                <path d="M30 50 L45 65 L70 35" stroke="white" stroke-width="5" fill="none"/>
            </svg>
        </div>

        <h2>Setup Complete! 🎉</h2>
        <p>Your application is now ready to use.</p>

        <div class="setup-summary">
            <h3>What's been configured:</h3>
            <ul>
                <li>✅ Admin account created</li>
                <li>✅ User roles initialized</li>
                <li>✅ Database seeded</li>
            </ul>
        </div>

        <div class="setup-actions">
            <button class="btn btn-primary-setup" @onclick="GoToLogin">
                Go to Login
            </button>
        </div>
    }
</div>

@code {
    private bool isCompleting = true;
    private bool setupComplete = false;
    private List<string> errors = new();

    protected override async Task OnInitializedAsync()
    {
        await FinalizeSetup();
    }

    private async Task FinalizeSetup()
    {
        try
        {
            // Get admin user ID from session or database query
            // For simplicity, we'll query the SuperAdmin role
            var validation = await SetupService.ValidateSetupStateAsync();

            if (validation.IsValid)
            {
                // Mark setup complete (service will determine admin ID)
                var success = await SetupService.MarkSetupCompleteAsync(string.Empty);

                if (success)
                {
                    setupComplete = true;
                    Logger.LogInformation("Setup completed successfully");
                }
                else
                {
                    errors.Add("Failed to mark setup as complete.");
                }
            }
            else
            {
                errors.AddRange(validation.Issues);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error finalizing setup");
            errors.Add("An unexpected error occurred.");
        }
        finally
        {
            isCompleting = false;
            StateHasChanged();
        }
    }

    private void GoToLogin()
    {
        Navigation.NavigateTo("/Account/Login", forceLoad: true);
    }
}
```

---

**Additional CSS for pages:**
```css
.setup-progress-indicator {
    text-align: center;
    padding: 3rem;
}

.setup-progress-indicator p {
    margin-top: 1rem;
    color: #555B6E;
}

.success-animation {
    text-align: center;
    margin-bottom: 2rem;
}

.setup-summary {
    text-align: left;
    margin: 2rem auto;
    max-width: 400px;
    padding: 1.5rem;
    background: #f9f9f9;
    border-radius: 8px;
}

.setup-summary h3 {
    color: #555B6E;
    margin-bottom: 1rem;
}

.setup-summary ul {
    list-style: none;
    padding: 0;
}

.setup-summary li {
    padding: 0.5rem 0;
    color: #555B6E;
}
```

**Acceptance Criteria:**
- [ ] All setup pages created
- [ ] Navigation flow works correctly
- [ ] Form validation functioning
- [ ] Error handling implemented
- [ ] Loading states displayed
- [ ] Success states shown
- [ ] Design system styles applied

---

### 5.5 Phase 5: Integration & Testing

#### Task 5.1: Integration Testing
**Tests to perform:**

1. **Fresh Database Test:**
   - Delete database file
   - Start application
   - Verify redirect to /setup/welcome
   - Complete setup workflow
   - Verify database seeded correctly
   - Verify admin account created
   - Verify redirect to login works

2. **Middleware Path Tests:**
   - Test that setup pages are accessible before setup
   - Test that static resources load correctly
   - Test that non-setup pages redirect to setup
   - Test that setup pages are inaccessible after setup

3. **Error Scenarios:**
   - Test password validation errors
   - Test duplicate email handling
   - Test database connection errors
   - Test partial setup recovery

4. **Security Tests:**
   - Verify password requirements enforced
   - Verify setup cannot be re-run after completion
   - Verify setup pages require no authentication

**Acceptance Criteria:**
- [ ] All integration tests pass
- [ ] Error scenarios handled gracefully
- [ ] Security requirements met
- [ ] User experience is smooth

---

#### Task 5.2: Create Manual Test Checklist

**Test Checklist:**
- [ ] Fresh installation setup completes successfully
- [ ] Admin account can log in after setup
- [ ] All roles are created (SuperAdmin, Admin, Moderator, User)
- [ ] Setup status is marked complete in database
- [ ] Middleware allows normal operation after setup
- [ ] Setup pages are inaccessible after completion
- [ ] Form validation works correctly
- [ ] Error messages are clear and helpful
- [ ] Loading indicators display during operations
- [ ] UI matches design system
- [ ] Responsive design works on mobile

---

## 6. Database Changes

### 6.1 New Entity: SetupStatus

**Table:** `SetupStatus`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | int | PK, NOT NULL | Primary key (always 1) |
| IsComplete | bit | NOT NULL, Default: false | Setup completion flag |
| CompletedAt | datetime | NULL | UTC timestamp when completed |
| AdminUserId | nvarchar(450) | FK, NULL | Admin user created during setup |
| SetupVersion | nvarchar(10) | NOT NULL | Version of setup completed |

**Relationships:**
- `AdminUserId` → `AspNetUsers.Id` (FK, ON DELETE SET NULL)

**Indexes:**
- Primary key on `Id`

**Seed Data:**
```sql
INSERT INTO SetupStatus (Id, IsComplete, SetupVersion)
VALUES (1, 0, '1.0');
```

---

### 6.2 Migration Commands

```bash
# Create migration
dotnet ef migrations add AddSetupStatus --project src/DiscordBot/DiscordBot.Blazor

# Review migration
cat src/DiscordBot/DiscordBot.Blazor/Migrations/*_AddSetupStatus.cs

# Apply migration (dev)
dotnet ef database update --project src/DiscordBot/DiscordBot.Blazor

# Production migration (included in deployment)
dotnet ef database update --project src/DiscordBot/DiscordBot.Blazor --configuration Release
```

---

## 7. File Structure

```
src/DiscordBot/
├── DiscordBot.Core/
│   ├── Entities/
│   │   └── SetupStatus.cs                    [NEW]
│   └── Services/
│       └── IFirstTimeSetupService.cs         [NEW]
│
└── DiscordBot.Blazor/
    ├── Components/
    │   ├── Pages/
    │   │   └── Setup/                         [NEW]
    │   │       ├── Welcome.razor              [NEW]
    │   │       ├── Admin.razor                [NEW]
    │   │       ├── Seed.razor                 [NEW]
    │   │       └── Complete.razor             [NEW]
    │   └── Setup/
    │       └── SetupLayout.razor              [NEW]
    │
    ├── Data/
    │   └── ApplicationDbContext.cs            [MODIFIED]
    │
    ├── Middleware/
    │   └── FirstTimeSetupMiddleware.cs        [NEW]
    │
    ├── Migrations/
    │   └── *_AddSetupStatus.cs                [NEW]
    │
    ├── Services/
    │   └── FirstTimeSetupService.cs           [NEW]
    │
    ├── wwwroot/
    │   └── css/
    │       └── setup.css                      [NEW]
    │
    └── Program.cs                             [MODIFIED]
```

---

## 8. Testing Strategy

### 8.1 Unit Tests

**Test Class:** `FirstTimeSetupServiceTests`

**Tests:**
- `IsSetupComplete_ReturnsTrue_WhenSetupStatusIsComplete`
- `IsSetupComplete_ReturnsFalse_WhenSetupStatusIsIncomplete`
- `CreateAdminAccount_CreatesUser_WithSuperAdminRole`
- `CreateAdminAccount_ReturnsError_WhenEmailIsInvalid`
- `CreateAdminAccount_ReturnsError_WhenPasswordIsTooWeak`
- `SeedDatabase_CreatesAllRoles`
- `SeedDatabase_SkipsExistingRoles`
- `MarkSetupComplete_UpdatesSetupStatus`
- `ValidateSetupState_ReturnsValid_WhenSetupIsComplete`
- `ValidateSetupState_ReturnsInvalid_WhenRolesAreMissing`

---

### 8.2 Integration Tests

**Test Class:** `FirstTimeSetupMiddlewareTests`

**Tests:**
- `Middleware_RedirectsToSetup_WhenSetupIsIncomplete`
- `Middleware_AllowsSetupPages_WhenSetupIsIncomplete`
- `Middleware_AllowsStaticFiles_WhenSetupIsIncomplete`
- `Middleware_AllowsAllPages_WhenSetupIsComplete`
- `Middleware_DoesNotRedirect_WhenOnSetupPage`

---

### 8.3 Manual Testing Scenarios

1. **Scenario: Fresh Installation**
   - Steps:
     1. Delete database file
     2. Start application
     3. Navigate to /
   - Expected: Redirect to /setup/welcome

2. **Scenario: Complete Setup Workflow**
   - Steps:
     1. Click "Get Started" on welcome page
     2. Fill in admin form with valid data
     3. Submit form
     4. Wait for database seeding
     5. View completion page
     6. Click "Go to Login"
   - Expected: All steps succeed, redirect to login page

3. **Scenario: Invalid Admin Data**
   - Steps:
     1. Enter weak password
     2. Submit form
   - Expected: Validation error displayed

4. **Scenario: Setup Re-Run Prevention**
   - Steps:
     1. Complete setup
     2. Navigate to /setup/welcome
   - Expected: Redirect to home page

---

## 9. Security Considerations

### 9.1 Security Requirements

1. **Password Security**
   - Minimum length: 6 characters
   - Must contain: uppercase, lowercase, number
   - Enforced by ASP.NET Identity

2. **Setup Protection**
   - Setup pages must be inaccessible after completion
   - Setup status cannot be manually reset (requires database edit)
   - Admin account is auto-confirmed (no email confirmation needed)

3. **Middleware Security**
   - Middleware executes early in pipeline
   - Static files are allowed during setup
   - Authentication pages are allowed during setup
   - All other pages require setup completion

### 9.2 Attack Vectors & Mitigations

| Attack Vector | Mitigation |
|---------------|------------|
| Brute force admin creation | Rate limiting (future), account lockout |
| SQL injection | Entity Framework parameterized queries |
| XSS | Blazor automatic encoding |
| CSRF | Blazor anti-forgery tokens |
| Unauthorized setup re-run | Database-backed completion flag |

---

## 10. References

### 10.1 Related Documentation

- [Technical Overview](../02-architecture/01-technical-overview.md)
- [Database Design](../02-architecture/02-database-design.md)
- [Security Architecture](../02-architecture/04-security-architecture.md)
- [Authentication Implementation Plan](01-authentication-registration-system.md)

### 10.2 External Resources

- [ASP.NET Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Middleware in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/middleware)
- [Blazor Layouts](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/layouts)
- [Entity Framework Migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations)

---

## Change Log

### Version 1.0 (2025-10-20)
- Initial implementation plan created
- Complete phase breakdown
- Detailed task specifications
- Code samples provided

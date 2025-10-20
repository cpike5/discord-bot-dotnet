# Admin Setup and Maintenance Guide

**Version:** 1.0
**Last Updated:** 2025-10-20
**Status:** Active

---

## Overview

This guide explains how to use the Admin Maintenance Service to perform initial setup tasks and ongoing maintenance for the Discord Bot application. The service provides functionality for:

- Creating the first admin account
- Ensuring required roles exist
- System diagnostics and health checks
- User promotion to administrator

---

## Admin Maintenance Service

### Service Interface

The `IAdminMaintenanceService` provides the following capabilities:

#### Admin Bootstrap Methods
- `PromoteUserToAdminByEmailAsync(string email)` - Promote user by email
- `PromoteUserToAdminByUsernameAsync(string username)` - Promote user by username
- `PromoteUserToAdminByDiscordIdAsync(ulong discordId)` - Promote user by Discord ID
- `PromoteUserToAdminByUserIdAsync(string userId)` - Promote user by application user ID

#### Discord Account Linking (NEW)
- `CreateAdminWithDiscordAsync(...)` - Create new admin account with Discord pre-linked
- `LinkDiscordAccountAsync(userId, discordId, ...)` - Link Discord to existing user
- `LinkDiscordAccountByEmailAsync(email, discordId, ...)` - Link Discord by email
- `UnlinkDiscordAccountAsync(userId)` - Remove Discord link from user

#### Role Initialization
- `EnsureRolesExistAsync()` - Creates missing standard roles (Admin, Moderator, Premium, User)
- `GetAllRolesAsync()` - Lists all application roles

#### System Diagnostics
- `GetSystemDiagnosticsAsync()` - Returns system health information
- `HasAdminAccountAsync()` - Checks if at least one admin exists

---

## Initial Setup

There are **two main approaches** to setting up your initial admin account with Discord integration:

### Approach 1: Create Admin with Discord in One Step (RECOMMENDED)

This is the easiest method. It creates an admin account with Discord already linked.

**Step 1: Get Your Discord ID**

To find your Discord ID:
1. Enable Developer Mode in Discord (User Settings → Advanced → Developer Mode)
2. Right-click your username in Discord and select "Copy User ID"
3. Your Discord ID will be a long number like `123456789012345678`

**Step 2: Ensure Roles Exist**

```bash
POST https://localhost:5001/api/maintenance/ensure-roles
```

**Step 3: Create Admin Account with Discord**

```bash
POST https://localhost:5001/api/maintenance/create-admin-with-discord
Content-Type: application/json

{
  "email": "your-email@example.com",
  "username": "your-username",
  "password": "YourSecurePassword123!",
  "discordId": 123456789012345678,
  "discordUsername": "YourDiscordName"
}
```

**Done!** You now have an admin account that's already linked to your Discord account. You can skip to the "Verify Admin Access" section.

---

### Approach 2: Create Account Then Link Discord

If you already have an account or prefer the two-step process:

#### Step 1: Create Your User Account

First, you need to create a user account in the system. You have two options:

**Option A: Via Discord Bot Registration**
1. Run the `/register` command in Discord
2. Follow the invite code flow to create your account
3. Complete registration through the web interface

**Option B: Direct Database Creation (Development Only)**
Create a user account directly through EF migrations or seed data.

#### Step 2: Ensure Roles Exist

Before promoting a user to admin, make sure all required roles exist in the database.

**Using the API:**
```bash
POST https://localhost:5001/api/maintenance/ensure-roles
```

**Response:**
```json
{
  "message": "Roles created successfully",
  "createdRoles": ["Admin", "Moderator", "Premium", "User"]
}
```

#### Step 3: Promote Yourself to Admin

Once you have a user account, promote it to admin using one of the following methods:

#### Method 1: Promote by Email
```bash
POST https://localhost:5001/api/maintenance/promote-admin/by-email
Content-Type: application/json

{
  "email": "your-email@example.com"
}
```

#### Method 2: Promote by Username
```bash
POST https://localhost:5001/api/maintenance/promote-admin/by-username
Content-Type: application/json

{
  "username": "your-username"
}
```

#### Method 3: Promote by Discord ID
```bash
POST https://localhost:5001/api/maintenance/promote-admin/by-discord-id
Content-Type: application/json

{
  "discordId": 123456789012345678
}
```

#### Method 4: Promote by User ID
```bash
POST https://localhost:5001/api/maintenance/promote-admin/by-user-id
Content-Type: application/json

{
  "userId": "your-aspnet-identity-user-id"
}
```

#### Step 4: Link Discord Account (Optional)

If you promoted an existing account that doesn't have Discord linked yet, you can link it:

**Get Your Discord ID First:**
1. Enable Developer Mode in Discord (User Settings → Advanced → Developer Mode)
2. Right-click your username and select "Copy User ID"

**Link by Email:**
```bash
POST https://localhost:5001/api/maintenance/link-discord/by-email
Content-Type: application/json

{
  "email": "your-email@example.com",
  "discordId": 123456789012345678,
  "discordUsername": "YourDiscordName"
}
```

**Link by User ID:**
```bash
POST https://localhost:5001/api/maintenance/link-discord/by-user-id
Content-Type: application/json

{
  "userId": "your-aspnet-identity-user-id",
  "discordId": 123456789012345678,
  "discordUsername": "YourDiscordName"
}
```

---

### Verify Admin Access

After completing either approach, verify your admin account:

```bash
GET https://localhost:5001/api/maintenance/diagnostics
```

You should see your account in the `adminUsernames` list and `discordLinkedUsers` should be at least 1.

---

## Using the API Endpoints

### Check System Diagnostics

Get comprehensive system health information:

```bash
GET https://localhost:5001/api/maintenance/diagnostics
```

**Response:**
```json
{
  "totalUsers": 5,
  "totalAdmins": 1,
  "adminUsernames": ["admin-user"],
  "totalRoles": 4,
  "allRoles": ["Admin", "Moderator", "Premium", "User"],
  "discordLinkedUsers": 3,
  "emailConfirmedUsers": 2,
  "systemReady": true,
  "recentUsers": 2
}
```

### Check if Admin Exists

Quickly check if the system has at least one admin:

```bash
GET https://localhost:5001/api/maintenance/has-admin
```

**Response:**
```json
{
  "hasAdmin": true
}
```

### List All Roles

Get all application roles:

```bash
GET https://localhost:5001/api/maintenance/roles
```

**Response:**
```json
{
  "roles": ["Admin", "Moderator", "Premium", "User"]
}
```

### Discord Account Management

#### Unlink Discord Account

Remove Discord linking from a user:

```bash
POST https://localhost:5001/api/maintenance/unlink-discord
Content-Type: application/json

{
  "userId": "your-aspnet-identity-user-id"
}
```

**Response:**
```json
{
  "message": "Discord account unlinked successfully",
  "userId": "your-aspnet-identity-user-id"
}
```

---

## Using the Service Programmatically

### From a Controller or Service

```csharp
public class MyController : ControllerBase
{
    private readonly IAdminMaintenanceService _maintenanceService;

    public MyController(IAdminMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    public async Task<IActionResult> SetupAdmin()
    {
        // Ensure roles exist
        await _maintenanceService.EnsureRolesExistAsync();

        // Option 1: Create admin with Discord in one step (recommended)
        var user = await _maintenanceService.CreateAdminWithDiscordAsync(
            "admin@example.com",
            "admin",
            "SecurePassword123!",
            123456789012345678, // Your Discord ID
            "YourDiscordName"
        );

        if (user != null)
        {
            return Ok("Admin created successfully with Discord link");
        }

        // Option 2: Promote existing user and link Discord separately
        var promoted = await _maintenanceService.PromoteUserToAdminByEmailAsync("admin@example.com");
        if (promoted)
        {
            await _maintenanceService.LinkDiscordAccountByEmailAsync(
                "admin@example.com",
                123456789012345678,
                "YourDiscordName"
            );
            return Ok("Admin promoted and Discord linked");
        }

        return BadRequest("Failed to create admin");
    }
}
```

### From a Background Service or Startup

```csharp
public class DatabaseSeeder : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public DatabaseSeeder(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var maintenanceService = scope.ServiceProvider
            .GetRequiredService<IAdminMaintenanceService>();

        // Ensure all roles exist
        await maintenanceService.EnsureRolesExistAsync();

        // Check if we need to create an admin
        if (!await maintenanceService.HasAdminAccountAsync())
        {
            // Promote initial admin from configuration
            var initialAdminEmail = configuration["InitialAdmin:Email"];
            if (!string.IsNullOrEmpty(initialAdminEmail))
            {
                await maintenanceService.PromoteUserToAdminByEmailAsync(initialAdminEmail);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

---

## Security Considerations

### Development vs Production

The Maintenance API endpoints have a built-in safety mechanism:

```json
{
  "Maintenance": {
    "SetupModeEnabled": true
  }
}
```

- **Development**: Set `SetupModeEnabled: true` to enable admin promotion endpoints
- **Production**: Set `SetupModeEnabled: false` to disable admin promotion endpoints after initial setup

### Recommended Production Setup

1. **Initial Setup Phase**:
   - Enable setup mode
   - Create your admin account
   - Promote yourself to admin
   - Verify admin access works

2. **Post-Setup Phase**:
   - Disable setup mode in configuration
   - Remove or secure the maintenance endpoints with authentication
   - Consider removing the MaintenanceController entirely
   - Use the admin web interface for all future user management

3. **Alternative: Use Configuration-Based Initial Admin**:
   Instead of using the API, you can configure an initial admin in `appsettings.json`:

   ```json
   {
     "InitialAdmin": {
       "Email": "your-email@example.com",
       "DiscordId": "123456789012345678"
     }
   }
   ```

   Then create a startup service that automatically promotes this user on first run.

### API Security Options

For production use of maintenance endpoints:

1. **IP Whitelisting**: Restrict access to specific IPs
2. **API Keys**: Require an API key header for authentication
3. **Admin Authorization**: Require admin role (chicken-and-egg for initial setup)
4. **Remove Endpoints**: Delete MaintenanceController after setup is complete

---

## Troubleshooting

### "User not found"

If you receive a "User not found" error when trying to promote to admin:

1. Verify the user account exists:
   ```bash
   GET https://localhost:5001/api/maintenance/diagnostics
   ```

2. Check that you're using the correct identifier (email, username, Discord ID, or user ID)

3. If using Discord ID, ensure the account is linked (has a DiscordUserId value)

### "Setup mode is disabled"

If you receive this error in production:

1. Set `Maintenance:SetupModeEnabled: true` in your configuration
2. Restart the application
3. Perform the admin promotion
4. Set `Maintenance:SetupModeEnabled: false` again
5. Restart the application

### "Failed to create Admin role"

This typically indicates a database connection issue:

1. Verify your database connection string
2. Ensure migrations have been applied
3. Check that Identity tables exist in the database

---

## Best Practices

1. **Create Admin First**: Always create at least one admin account before deploying to production

2. **Disable Setup Mode**: After initial setup, disable the setup mode to prevent unauthorized admin promotion

3. **Use Audit Logging**: Monitor the logs for admin promotion events (they are logged as warnings)

4. **Test Admin Access**: After promotion, verify you can access admin-only features

5. **Document Your Admin**: Keep a secure record of admin account credentials

6. **Multiple Admins**: Consider creating multiple admin accounts for redundancy

---

## Related Documentation

- [Security Architecture](../02-architecture/04-security-architecture.md) - Overall security design
- [User Management API](./user-management-api.md) - User and role management endpoints
- [Authentication System](../03-implementation-plans/01-authentication-registration-system.md) - Registration flow

---

## Maintenance Service Standard Roles

The service creates these standard roles when `EnsureRolesExistAsync()` is called:

| Role Name   | Description                                      | Typical Use Cases                           |
|-------------|--------------------------------------------------|---------------------------------------------|
| Admin       | Full system access                               | System administration, user management      |
| Moderator   | Content moderation capabilities                  | Moderation commands, user moderation        |
| Premium     | Access to premium features                       | Premium bot commands, VIP features          |
| User        | Standard user access                             | Basic bot commands, standard features       |

You can add custom roles using ASP.NET Identity's `RoleManager<IdentityRole>` if needed.

---

## Example: Complete Initial Setup Scripts

### Option 1: Create Admin with Discord (Recommended)

Here's a complete example using curl to create an admin with Discord integration:

```bash
#!/bin/bash

BASE_URL="https://localhost:5001"
YOUR_EMAIL="your-email@example.com"
YOUR_USERNAME="admin"
YOUR_PASSWORD="YourSecurePassword123!"
YOUR_DISCORD_ID=123456789012345678  # Replace with your Discord ID
YOUR_DISCORD_NAME="YourDiscordName"

# Step 1: Ensure roles exist
echo "Creating roles..."
curl -X POST "$BASE_URL/api/maintenance/ensure-roles" \
  -H "Content-Type: application/json"

# Step 2: Create admin account with Discord link in one step
echo "\nCreating admin account with Discord..."
curl -X POST "$BASE_URL/api/maintenance/create-admin-with-discord" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$YOUR_EMAIL\",
    \"username\": \"$YOUR_USERNAME\",
    \"password\": \"$YOUR_PASSWORD\",
    \"discordId\": $YOUR_DISCORD_ID,
    \"discordUsername\": \"$YOUR_DISCORD_NAME\"
  }"

# Step 3: Verify admin exists
echo "\nVerifying admin account..."
curl -X GET "$BASE_URL/api/maintenance/diagnostics"

echo "\nSetup complete! Your admin account is created and linked to Discord."
```

### Option 2: Promote Existing User and Link Discord

If you already have a user account:

```bash
#!/bin/bash

BASE_URL="https://localhost:5001"
YOUR_EMAIL="your-email@example.com"
YOUR_DISCORD_ID=123456789012345678  # Replace with your Discord ID
YOUR_DISCORD_NAME="YourDiscordName"

# Step 1: Ensure roles exist
echo "Creating roles..."
curl -X POST "$BASE_URL/api/maintenance/ensure-roles" \
  -H "Content-Type: application/json"

# Step 2: Promote existing user to admin
echo "\nPromoting user to admin..."
curl -X POST "$BASE_URL/api/maintenance/promote-admin/by-email" \
  -H "Content-Type: application/json" \
  -d "{\"email\": \"$YOUR_EMAIL\"}"

# Step 3: Link Discord account
echo "\nLinking Discord account..."
curl -X POST "$BASE_URL/api/maintenance/link-discord/by-email" \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$YOUR_EMAIL\",
    \"discordId\": $YOUR_DISCORD_ID,
    \"discordUsername\": \"$YOUR_DISCORD_NAME\"
  }"

# Step 4: Verify admin exists and Discord is linked
echo "\nVerifying admin account..."
curl -X GET "$BASE_URL/api/maintenance/diagnostics"

echo "\nSetup complete! Your account is promoted to admin and linked to Discord."
```

**Make the script executable and run it:**
```bash
chmod +x setup-admin.sh
./setup-admin.sh
```

### How to Find Your Discord ID

1. Open Discord
2. Go to User Settings → Advanced
3. Enable "Developer Mode"
4. Right-click your username anywhere in Discord
5. Select "Copy User ID"
6. Paste the ID into the script (it's a long number like `123456789012345678`)

---

**Document Control**:
- **Author**: System Administrator
- **Last Updated**: 2025-10-20
- **Next Review**: 2026-01-20

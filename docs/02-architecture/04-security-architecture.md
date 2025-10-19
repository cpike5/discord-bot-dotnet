# Security Architecture

**Version:** 1.0
**Last Updated:** 2025-10-18
**Status:** Approved

---

## 1. Overview

This document outlines the security architecture for integrating ASP.NET Identity authentication and authorization with Discord.Net bot commands. The system implements a **Linked Accounts Model** where Discord users authenticate through the web application, and bot command permissions are validated against ASP.NET Identity roles.

### Key Principles

- **Single Source of Truth**: ASP.NET Identity is the authoritative system for all application permissions
- **Explicit Account Linking**: Discord users must explicitly link their Discord account to an application account
- **Centralized Permission Management**: All role and permission assignments happen through the web interface
- **Fail-Secure**: Unlinked Discord users receive access denied messages for restricted commands
- **Separation of Concerns**: Discord server roles and application roles remain independent

---

## 2. Architecture Strategy

### 2.1 Linked Accounts Model

The system maintains a bidirectional mapping between Discord User IDs and ASP.NET Identity users:

```
Discord User (Discord ID: 123456789)
           ↕ [Account Link]
ASP.NET Identity User (ApplicationUserId: guid-xxx)
           ↓
Identity Roles: ["Admin", "Moderator", "Premium"]
```

**How it works:**

1. User authenticates via web application (Discord OAuth2)
2. ASP.NET Identity user is created/retrieved
3. Discord User ID is stored on the Identity user record
4. Admin assigns roles via web interface
5. Bot commands query Identity roles using Discord User ID lookup

---

## 3. Security Domains

### 3.1 ASP.NET Identity (Application Permissions)

**Purpose**: Manages application-level authentication and authorization

**Scope**:
- Web application access
- Bot command permissions
- Data access rights
- Feature flags (premium, beta features)

**Roles** (examples):
- `Admin` - Full system access
- `Moderator` - Content moderation capabilities
- `Premium` - Access to premium features
- `User` - Standard user access

**Authentication Methods**:
- Discord OAuth2 (primary)
- Email/Password (optional)
- Two-Factor Authentication (supported via Identity)

### 3.2 Discord Server Roles

**Purpose**: Discord native server management

**Scope**:
- Discord server permissions (kick, ban, manage channels)
- Voice channel access
- Channel visibility
- Discord-specific features

**Important**: Discord server roles do NOT grant application permissions (except for server ownership/admin)

---

## 4. Account Linking Flow

### 4.1 Initial Registration

```
┌─────────────┐
│ Discord Bot │
│             │
│ /register   │──┐
└─────────────┘  │
                 │ 1. Generate invite code
                 │    (sent via DM)
                 ↓
        ┌────────────────┐
        │  Web App       │
        │  /Account/     │
        │  Register      │
        └────────────────┘
                 │
                 │ 2. User enters code
                 │    + sets password
                 ↓
        ┌────────────────┐
        │ ASP.NET        │
        │ Identity       │
        │                │
        │ User created   │
        │ DiscordUserId  │
        │ = 123456789    │
        └────────────────┘
```

**Process**:

1. User runs `/register` command in Discord
2. Bot checks if Discord user is already linked
   - If linked: sends "already registered" message
   - If not linked: generates time-limited invite code
3. Bot sends DM with code and registration URL
4. User visits web registration page, enters code
5. Code validation:
   - Code exists and not expired
   - Code not already used
   - Discord user not already linked
6. User completes registration (sets username/password)
7. `ApplicationUser.DiscordUserId` field populated with Discord ID
8. Account is now linked

### 4.2 Subsequent Authentication

```
┌─────────────┐
│ User visits │
│ web app     │
└─────────────┘
       │
       ↓
┌─────────────────┐
│ Discord OAuth2  │
│ Login           │
└─────────────────┘
       │
       ↓
┌─────────────────┐
│ Identity user   │
│ retrieved by    │
│ Discord ID      │
└─────────────────┘
       │
       ↓
┌─────────────────┐
│ User logged in  │
│ with roles      │
└─────────────────┘
```

---

## 5. Bot Command Authorization

### 5.1 Custom Precondition Attribute

Discord.Net commands will use a custom `RequireIdentityRole` attribute to enforce permissions:

```csharp
[RequireIdentityRole("Admin")]
[SlashCommand("purge", "Delete messages")]
public async Task PurgeAsync(int count) { ... }
```

**Implementation Flow**:

```
User executes command
        ↓
Discord.Net invokes precondition
        ↓
[RequireIdentityRole] attribute
        ↓
1. Get Discord User ID from context
2. Query ApplicationUser by DiscordUserId
3. If user not found → DENY (send ephemeral message)
4. If user found → check Identity roles
5. If required role present → ALLOW
6. If role missing → DENY (send ephemeral message)
```

### 5.2 Unlinked User Behavior

**Policy**: If a Discord user is not linked to an application account, they receive an access denied message for restricted commands.

**Message Template** (ephemeral):
```
❌ Access Denied

This command requires an application account.
Please run /register to create an account.
```

**Implementation**:
- All role-restricted commands check for linked account first
- Public commands (like `/help`, `/register`) remain accessible
- Error messages are ephemeral (only visible to user)

### 5.3 Discord Server Owner/Admin Privilege

**Policy**: Discord server owners and administrators automatically receive `Admin` role in the application.

**Implementation**:

1. When a Discord server owner/admin runs `/register`:
   - Standard account creation flow
   - After account creation, check guild permissions
   - If user has `GuildPermission.Administrator` or is guild owner:
     - Automatically assign `Admin` role in Identity
     - Log this action for audit trail

2. For existing linked accounts:
   - Periodic background job checks guild owners/admins
   - Ensures they have `Admin` role
   - Syncs changes (e.g., if admin permissions revoked in Discord)

**Rationale**: Server owners have ultimate control over the Discord server, so they should have administrative access to bot features.

---

## 6. Role Management

### 6.1 Assignment Interface

**Policy**: Role management is done exclusively through the web interface.

**Access Control**:
- Only users with `Admin` role can assign/revoke roles
- Available in web admin panel: `/Admin/Users`

**Features**:
- View all users and their current roles
- Assign multiple roles to a user
- Revoke roles from a user
- View Discord username/ID for each user
- Audit log of role changes

**UI Requirements**:
- Confirmation dialog before role changes
- Display last login time
- Show linked Discord account info
- Filter/search by username or Discord ID

### 6.2 Initial Admin Assignment

**Bootstrap Process**:

Since role assignment requires an admin account, the first admin must be created manually:

**Option 1: Database Seed**
- Create initial admin user in database migration
- Use known Discord ID of server owner
- Pre-assign `Admin` role

**Option 2: Configuration Override**
- Store initial admin Discord ID in `appsettings.json`
- On first run, check if user with that Discord ID exists
- Auto-promote to `Admin` if found

**Option 3: Command Line Tool**
- Include CLI tool to promote user to admin
- Example: `dotnet run -- promote-admin <discord-id>`

**Recommended**: Option 2 (configuration) for development, Option 3 (CLI) for production.

---

## 7. Permission Levels

### 7.1 Application Roles

| Role        | Description                                      | Permissions                                    |
|-------------|--------------------------------------------------|------------------------------------------------|
| `Admin`     | System administrators                            | Full access to all features and commands       |
| `Moderator` | Content moderators                               | Moderation commands, user management           |
| `Premium`   | Premium/VIP users                                | Access to premium commands and features        |
| `User`      | Standard registered users                        | Basic commands                                 |
| (none)      | Unlinked Discord users                           | Public commands only (`/help`, `/register`)    |

### 7.2 Command Permission Matrix

| Command Group       | Required Role | Fallback Behavior                          |
|---------------------|---------------|--------------------------------------------|
| `/register`         | None          | Public                                     |
| `/help`             | None          | Public                                     |
| `/profile`          | User          | Ephemeral: "Register first"                |
| `/admin *`          | Admin         | Ephemeral: "Admin access required"         |
| `/moderate *`       | Moderator     | Ephemeral: "Moderator access required"     |
| `/premium *`        | Premium       | Ephemeral: "Premium subscription required" |

### 7.3 Hybrid Permissions

Some commands may require BOTH Discord permissions AND application roles:

```csharp
[RequireUserPermission(GuildPermission.ManageMessages)] // Discord check
[RequireIdentityRole("Moderator")]                      // App check
[SlashCommand("purge", "Bulk delete messages")]
public async Task PurgeAsync(int count) { ... }
```

**Use Case**: Ensures user has both server-level permissions and application-level authorization.

---

## 8. Implementation Components

### 8.1 Database Schema Extensions

**ApplicationUser** (extends IdentityUser):
```csharp
public class ApplicationUser : IdentityUser
{
    public ulong? DiscordUserId { get; set; }      // Unique
    public string? DiscordUsername { get; set; }
    public string? DiscordDiscriminator { get; set; }
    public string? DiscordAvatarHash { get; set; }
    public DateTime? DiscordAccountLinkedAt { get; set; }

    // Relationships
    public virtual ICollection<InviteCode> GeneratedInviteCodes { get; set; }
}
```

**Indexes**:
- Unique index on `DiscordUserId`
- Index on `DiscordUsername` for search

### 8.2 Service Layer

**IUserAuthorizationService**:
```csharp
public interface IUserAuthorizationService
{
    Task<ApplicationUser?> GetUserByDiscordIdAsync(ulong discordId);
    Task<bool> IsInRoleAsync(ulong discordId, string role);
    Task<IEnumerable<string>> GetUserRolesAsync(ulong discordId);
    Task<bool> IsLinkedAsync(ulong discordId);
    Task SyncDiscordServerAdminsAsync(IGuild guild);
}
```

**Implementation Notes**:
- Cache role lookups (5-minute sliding expiration)
- Use `UserManager<ApplicationUser>` for role operations
- Invalidate cache on role changes

### 8.3 Precondition Attribute

**RequireIdentityRoleAttribute**:
```csharp
public class RequireIdentityRoleAttribute : PreconditionAttribute
{
    private readonly string _roleName;

    public RequireIdentityRoleAttribute(string roleName)
    {
        _roleName = roleName;
    }

    public override async Task<PreconditionResult> CheckRequirementsAsync(
        IInteractionContext context,
        ICommandInfo commandInfo,
        IServiceProvider services)
    {
        var authService = services.GetRequiredService<IUserAuthorizationService>();
        var discordId = context.User.Id;

        // Check if user is linked
        if (!await authService.IsLinkedAsync(discordId))
        {
            return PreconditionResult.FromError(
                "❌ Access Denied\n\n" +
                "This command requires an application account.\n" +
                "Please run `/register` to create an account.");
        }

        // Check role
        if (await authService.IsInRoleAsync(discordId, _roleName))
        {
            return PreconditionResult.FromSuccess();
        }

        return PreconditionResult.FromError(
            $"❌ Access Denied\n\n" +
            $"This command requires the '{_roleName}' role.");
    }
}
```

**Key Features**:
- Async role checking
- Clear error messages
- DI-based service resolution
- Caching support in service layer

---

## 9. Security Considerations

### 9.1 Account Takeover Prevention

**Risks**:
- Malicious user tries to link someone else's Discord account
- User loses access to Discord account

**Mitigations**:
1. **Invite code generation requires Discord bot interaction**
   - Only the actual Discord user can run `/register`
   - Code sent via DM (private to user)

2. **Code expiration**
   - Codes expire after 24 hours
   - Reduces window for code theft

3. **One-time use codes**
   - Codes marked as used immediately
   - Cannot be reused

4. **Account recovery**
   - Users can unlink/relink Discord accounts via web
   - Requires email verification
   - Logs all linking/unlinking events

### 9.2 Role Elevation Prevention

**Risks**:
- User attempts to escalate their own privileges
- Malicious admin assigns unauthorized roles

**Mitigations**:
1. **Role assignment requires Admin role**
   - Only admins can modify roles
   - Admins cannot remove their own admin role

2. **Audit logging**
   - All role changes logged with timestamp, admin user, target user
   - Available in admin audit log view

3. **Protected roles**
   - `Admin` role changes require confirmation dialog
   - Minimum 1 admin user enforced

### 9.3 Discord Impersonation

**Risks**:
- User changes Discord username to impersonate admin
- User transfers account to someone else

**Mitigations**:
1. **Use Discord ID, not username**
   - Discord IDs are immutable
   - Username changes don't affect authorization

2. **Periodic sync of Discord usernames**
   - Background job updates cached Discord usernames
   - Helps with audit trails and admin UI

3. **Display both username and ID in admin UI**
   - Admins can verify correct user by ID

### 9.4 Caching Security

**Risks**:
- Stale cache shows old roles after revocation
- User retains access after role removal

**Mitigations**:
1. **Short cache duration**
   - 5-minute sliding expiration on role cache
   - Balance between performance and security

2. **Active cache invalidation**
   - When roles changed via admin UI, invalidate user's cache entry
   - Use cache key pattern: `user_roles:{discordId}`

3. **Critical operations bypass cache**
   - High-security commands can force fresh database lookup
   - Example: financial transactions, user bans

### 9.5 Token Security

**Discord Bot Token**:
- Stored in User Secrets (development)
- Environment variables (production)
- Never committed to source control
- Rotate if exposed

**Discord OAuth2 Credentials**:
- Client ID and Secret stored securely
- Redirect URIs whitelist enforced
- State parameter for CSRF protection

---

## 10. Monitoring and Auditing

### 10.1 Audit Events

Log the following events for security auditing:

| Event                        | Data Logged                                                           |
|------------------------------|-----------------------------------------------------------------------|
| Account linked               | Discord ID, Username, Application User ID, Timestamp, IP Address      |
| Account unlinked             | Discord ID, Application User ID, Timestamp, Admin User ID (if forced) |
| Role assigned                | Target User, Role Name, Admin User, Timestamp                         |
| Role revoked                 | Target User, Role Name, Admin User, Timestamp                         |
| Failed authorization attempt | Discord ID, Command, Required Role, Timestamp                         |
| Admin command executed       | Discord ID, Command, Parameters, Timestamp                            |
| Invite code generated        | Discord ID, Code (hashed), Timestamp                                  |
| Invite code used             | Code (hashed), Application User ID, Timestamp                         |

### 10.2 Monitoring Metrics

Track for security monitoring:

- Failed authorization attempts per user (rate limiting trigger)
- Number of unlinked users attempting restricted commands
- Role assignment frequency (detect unusual activity)
- Admin command execution frequency
- Invite code generation rate

### 10.3 Alerts

Configure alerts for:

- Multiple failed authorization attempts from same user (>10 in 5 minutes)
- Bulk role assignments (>5 in 1 minute)
- Admin role assignment/revocation
- Unusual command patterns (e.g., rapid-fire admin commands)

---

## 11. Future Enhancements

### 11.1 Multi-Guild Support

Current design assumes single Discord server. For multi-guild:

- Add `GuildId` to account links
- Support one Discord user → multiple guild-specific accounts
- Guild-specific roles (e.g., "Admin in Guild A", "User in Guild B")

### 11.2 Advanced Authorization

- **Claims-based authorization**: Fine-grained permissions beyond roles
- **Time-limited roles**: Premium expires after 30 days
- **Conditional access**: IP whitelisting, geographic restrictions
- **Rate limiting per role**: Different rate limits for different roles

### 11.3 External Identity Providers

- Support additional OAuth providers (Google, GitHub)
- Link multiple external accounts to one application account
- Discord remains primary for bot commands

### 11.4 Permission Delegation

- Allow admins to delegate specific permissions
- Example: "User A can assign 'Moderator' role but not 'Admin'"
- Role hierarchy and permission inheritance

---

## 12. Testing Strategy

### 12.1 Unit Tests

- `RequireIdentityRoleAttribute` precondition logic
- `UserAuthorizationService` role checking
- Cache invalidation logic
- Account linking validation

### 12.2 Integration Tests

- Full `/register` flow (bot → web → database)
- Discord OAuth2 login flow
- Role assignment → command authorization
- Automatic admin assignment for server owners

### 12.3 Security Tests

- Attempt to use expired invite codes
- Attempt to reuse invite codes
- Attempt to link already-linked Discord account
- Attempt to execute admin commands without role
- Attempt to escalate privileges via API manipulation

---

## 13. Migration Path

For existing Discord bots with users:

### Phase 1: Add Account Linking (Optional)
- Deploy new system with account linking feature
- All commands remain public initially
- Users can voluntarily link accounts

### Phase 2: Role Assignment
- Admins assign roles to linked users
- Commands still public (soft launch)

### Phase 3: Enforce Permissions
- Enable `RequireIdentityRole` on commands
- Unlinked users see "register required" messages
- Provide migration support period (e.g., 30 days)

### Phase 4: Mandatory Linking
- All commands (except `/register`, `/help`) require linked account
- Full security model in effect

---

## 14. References

- [ASP.NET Identity Documentation](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)
- [Discord.Net Preconditions](https://discordnet.dev/guides/int_framework/preconditions.html)
- [Discord OAuth2 Documentation](https://discord.com/developers/docs/topics/oauth2)
- [ADR-001: Use Interaction Framework](../05-adr/001-use-interaction-framework.md)

---

## Appendix A: Configuration Example

```json
{
  "Bot": {
    "Token": "...",
    "InviteCodeExpirationHours": 24,
    "RegistrationUrl": "https://yourapp.com/Account/Register"
  },
  "Security": {
    "RoleCacheDurationMinutes": 5,
    "InitialAdminDiscordId": "123456789012345678",
    "RequireAccountLinkingForAllCommands": false
  },
  "Discord": {
    "OAuth2": {
      "ClientId": "...",
      "ClientSecret": "...",
      "RedirectUri": "https://yourapp.com/signin-discord"
    }
  }
}
```

---

## Appendix B: Database Migration Checklist

When implementing this architecture:

- [ ] Add `DiscordUserId` (ulong?) to `ApplicationUser`
- [ ] Add `DiscordUsername` (string?) to `ApplicationUser`
- [ ] Add `DiscordDiscriminator` (string?) to `ApplicationUser`
- [ ] Add `DiscordAvatarHash` (string?) to `ApplicationUser`
- [ ] Add `DiscordAccountLinkedAt` (DateTime?) to `ApplicationUser`
- [ ] Create unique index on `DiscordUserId`
- [ ] Create index on `DiscordUsername`
- [ ] Seed initial `Admin` role in Identity
- [ ] Create `AuditLog` table for security events
- [ ] Update `InviteCode` entity with `UsedByApplicationUserId` foreign key

---

**Document Control**:
- **Author**: System Architect
- **Approved By**: (pending)
- **Next Review**: 2025-11-18

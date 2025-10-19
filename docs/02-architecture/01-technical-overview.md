# Technical Specifications

**Version:** 1.0
**Last Updated:** 2025-10-18

---

## 1. Technology Stack

### Backend
- **.NET 8.0** - Runtime and framework
- **ASP.NET Core Blazor Server** - Web UI
- **ASP.NET Core Identity** - Authentication & user management
- **Entity Framework Core 8.0** - ORM
- **SQL Server** - Database
- **Discord.Net** - Discord bot framework
  - `Discord.Net.WebSocket` - Discord client
  - `Discord.Net.Interactions` - Slash command framework

### Architecture Pattern
- **Clean Architecture** / **Layered Architecture**
- **Repository Pattern** - Data access abstraction
- **Service Layer** - Business logic
- **DTO Pattern** - Data transfer objects
- **Modular Command System** - Extensible interaction modules (see [COMMAND_ARCHITECTURE.md](COMMAND_ARCHITECTURE.md))

---

## 2. Project Structure

```
src/DiscordBot/
├── DiscordBot.Core/                    # Core business logic (no UI dependencies)
│   ├── Entities/                       # Database entities
│   │   ├── InviteCode.cs
│   │   └── (ApplicationUser in Blazor project)
│   ├── DTOs/                          # Data transfer objects
│   │   ├── InviteCodeDto.cs
│   │   └── CreateInviteCodeDto.cs
│   ├── Repositories/                   # Data access layer
│   │   ├── IInviteCodeRepository.cs
│   │   └── InviteCodeRepository.cs
│   ├── Services/                       # Business logic
│   │   ├── IBotControl.cs
│   │   ├── BotHostedService.cs        # Background service (cpikeBotService)
│   │   ├── IInviteCodeService.cs
│   │   └── InviteCodeService.cs
│   ├── Modules/                        # Discord Interaction Framework modules
│   │   ├── RegisterModule.cs          # /register command
│   │   └── AdminModule.cs             # /admin commands
│   ├── Configuration/                  # Configuration models
│   │   └── BotOptions.cs
│   └── Extensions/
│       ├── ServiceCollectionExtensions.cs      # Main DI registration
│       └── InteractionServiceExtensions.cs     # Module registration helpers
│
└── DiscordBot.Blazor/                  # Web application
    ├── Components/                     # Blazor components
    ├── Data/                          # EF Core DbContext & migrations
    │   ├── ApplicationDbContext.cs    # Extended with InviteCode DbSet
    │   ├── ApplicationUser.cs         # Extended with Discord fields
    │   └── Migrations/
    └── Program.cs                     # DI configuration
```

---

## 3. Architecture Layers

### 3.1 Data Layer (Entities)
- **Location**: `DiscordBot.Core/Entities`
- **Responsibility**: Database entity models (EF Core)
- **Dependencies**: None (POCOs)

### 3.2 Repository Layer
- **Location**: `DiscordBot.Core/Repositories`
- **Responsibility**: Data access abstraction
- **Dependencies**: Entities, `ApplicationDbContext` (injected)
- **Pattern**: Interface + Implementation

### 3.3 Service Layer
- **Location**: `DiscordBot.Core/Services`
- **Responsibility**: Business logic, orchestration
- **Dependencies**: Repositories, DTOs
- **Pattern**: Interface + Implementation

### 3.4 Presentation Layer
- **Discord Bot**: `DiscordBot.Core/Modules` (Interaction Framework)
- **Web UI**: `DiscordBot.Blazor/Components`
- **Dependencies**: Services (via DI), DTOs

---

## 4. Key Components

### 4.1 BotHostedService (cpikeBotService)

**Location**: `DiscordBot.Core/Services/BotHostedService.cs`

**Responsibility**:
- Implements `IHostedService`
- Manages Discord client lifecycle
- Registers global slash commands
- Handles Discord events
- Performs maintenance tasks

**Dependencies**:
- `DiscordSocketClient`
- `InteractionService`
- `IServiceProvider` (for DI in modules)
- `ILogger<BotHostedService>`
- `IConfiguration` (for bot token)

**Key Methods**:
```csharp
Task StartAsync(CancellationToken cancellationToken)
  - Login to Discord
  - Start client
  - Wire up events

Task StopAsync(CancellationToken cancellationToken)
  - Graceful shutdown

Task OnReadyAsync()
  - Register commands globally

Task HandleInteractionAsync(SocketInteraction interaction)
  - Execute interaction via InteractionService
```

---

### 4.2 InviteCodeService

**Location**: `DiscordBot.Core/Services/InviteCodeService.cs`

**Interface**: `IInviteCodeService`

**Methods**:
```csharp
Task<InviteCodeDto> GenerateInviteCodeAsync(ulong discordUserId, string discordUsername)
  - Checks for existing active code
  - Generates new code (format: XXXX-XXXX-XXXX)
  - Sets 24-hour expiration
  - Saves to database
  - Returns DTO

Task<InviteCodeDto?> ValidateInviteCodeAsync(string code)
  - Finds code
  - Validates not used, not expired
  - Returns DTO or null

Task MarkCodeAsUsedAsync(string code, string applicationUserId)
  - Updates IsUsed, UsedAt, UsedByApplicationUserId
  - Saves to database

Task<bool> HasActiveCodeAsync(ulong discordUserId)
  - Checks if user has unused, non-expired code

Task<bool> IsDiscordUserRegisteredAsync(ulong discordUserId)
  - Checks if Discord user already has linked account
```

---

### 4.3 InviteCodeRepository

**Location**: `DiscordBot.Core/Repositories/InviteCodeRepository.cs`

**Interface**: `IInviteCodeRepository`

**Methods**:
```csharp
Task<InviteCode?> GetByCodeAsync(string code)
Task<InviteCode?> GetActiveCodeByDiscordUserIdAsync(ulong discordUserId)
Task<IEnumerable<InviteCode>> GetAllActiveCodesAsync()
Task<InviteCode> CreateAsync(InviteCode inviteCode)
Task UpdateAsync(InviteCode inviteCode)
Task DeleteExpiredCodesAsync(DateTime olderThan)
```

---

### 4.4 RegisterModule

**Location**: `DiscordBot.Core/Modules/RegisterModule.cs`

**Inheritance**: `InteractionModuleBase<SocketInteractionContext>`

**Commands**:
```csharp
[SlashCommand("register", "Get an invite code to register on the web application")]
public async Task RegisterAsync()
  - Defer response (ephemeral)
  - Check if user already registered
  - Generate invite code via service
  - Send DM with code and URL
  - Follow up with confirmation
```

---

### 4.5 AdminModule

**Location**: `DiscordBot.Core/Modules/AdminModule.cs`

**Inheritance**: `InteractionModuleBase<SocketInteractionContext>`

**Precondition**: `[RequireUserPermission(GuildPermission.Administrator)]`

**Commands**:
```csharp
[Group("admin", "Administrative commands")]
public class AdminModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("generate-code", "Manually generate an invite code for a user")]
    public async Task GenerateCodeAsync(IUser user)

    [SlashCommand("revoke-code", "Revoke an active invite code")]
    public async Task RevokeCodeAsync(string code)

    [SlashCommand("list-codes", "List all active invite codes")]
    public async Task ListCodesAsync()
}
```

---

## 5. Configuration

### 5.1 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DiscordBot;..."
  },
  "Bot": {
    "Token": "", // Set via User Secrets in dev
    "InviteCodeExpirationHours": 24,
    "RegistrationUrl": "https://localhost:5001/Account/Register"
  }
}
```

### 5.2 User Secrets (Development)

```json
{
  "Bot": {
    "Token": "YOUR_DISCORD_BOT_TOKEN"
  }
}
```

### 5.3 Environment Variables (Production)

- `Bot__Token` - Discord bot token
- `ConnectionStrings__DefaultConnection` - Database connection

---

## 6. Dependency Injection Setup

### DiscordBot.Core Registration

```csharp
services.AddDiscordBot(configuration);
// Registers:
// - DiscordSocketClient (Singleton)
// - InteractionService (Singleton)
// - BotHostedService (Hosted Service)
// - IInviteCodeService -> InviteCodeService (Scoped)
// - IInviteCodeRepository -> InviteCodeRepository (Scoped)
```

### DiscordBot.Blazor Registration

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDiscordBot(builder.Configuration);
```

---

## 7. Discord Bot Configuration

### Required Intents
```csharp
GatewayIntents.Guilds | GatewayIntents.GuildMembers
```

### OAuth2 Scopes
- `bot`
- `applications.commands`

### Bot Permissions
- Send Messages (for DMs)
- Use Slash Commands

---

## 8. Security Considerations

### Invite Code Generation
- Use `RandomNumberGenerator.GetBytes()` for cryptographic randomness
- Base32 encoding for readability (excludes ambiguous characters)
- Format: `XXXX-XXXX-XXXX` (12 characters + 2 hyphens)

### Database Constraints
- Unique constraint on `InviteCode.Code`
- Unique constraint on `ApplicationUser.DiscordUserId`
- Foreign key constraint on `InviteCode.UsedByApplicationUserId`

### Validation Rules
- Invite code must exist
- Invite code must not be expired
- Invite code must not be already used
- Discord user must not already have an account
- Discord user can only have one active code at a time

---

## 9. Error Handling

### Discord Bot
- Failed DM: Reply with ephemeral message to use `/register` again
- Already registered: Inform user they already have an account
- Rate limiting: Implement cooldown on `/register` command

### Web Application
- Invalid code: Show error on registration page
- Expired code: Provide link to Discord to request new code
- Database errors: Log and show generic error message

---

## 10. Performance Considerations

- **Index on Code lookup**: O(1) lookup via unique index
- **Index on DiscordUserId**: Fast validation queries
- **Background cleanup**: Weekly job to delete old expired codes
- **Connection pooling**: EF Core default connection pooling
- **Singleton bot client**: Reuse single Discord connection

---

## 11. Future Technical Considerations

- Add caching layer for frequently accessed data
- Implement distributed caching for multi-instance deployments
- Add metrics/telemetry (Application Insights)
- Implement audit logging
- Add health check endpoints
- Docker containerization
- CI/CD pipeline setup

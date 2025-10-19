# Command Architecture & Module System

**Version:** 1.0
**Last Updated:** 2025-10-18

---

## 1. Overview

The command system is designed to be modular and extensible, allowing developers to add new command modules by simply creating new class libraries and registering them via dependency injection.

## 2. Design Goals

- **Modularity**: Commands organized into separate modules/assemblies
- **Auto-discovery**: Automatic registration of command modules
- **Extensibility**: Easy to add new command libraries without modifying core code
- **Convention-based**: Follow conventions to minimize configuration
- **Testability**: Modules can be tested independently
- **Type Safety**: Compile-time checks for command definitions

---

## 3. Architecture

### 3.1 Module Structure

```
Solution Structure:
├── DiscordBot.Core                    # Core framework
│   ├── Services/
│   │   └── BotHostedService.cs        # Registers modules from DI
│   └── Extensions/
│       └── InteractionServiceExtensions.cs
│
├── DiscordBot.Commands.Core           # Built-in commands (NEW)
│   ├── Modules/
│   │   ├── RegisterModule.cs
│   │   └── HelpModule.cs
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
│
├── DiscordBot.Commands.Admin          # Admin commands (NEW)
│   ├── Modules/
│   │   ├── InviteCodeAdminModule.cs
│   │   ├── UserAdminModule.cs
│   │   └── ServerAdminModule.cs
│   └── Extensions/
│       └── ServiceCollectionExtensions.cs
│
└── DiscordBot.Commands.Custom         # Future: Custom commands
    └── ...
```

### 3.2 Module Discovery Patterns

We'll support **three discovery patterns**:

#### Pattern 1: Explicit Registration (Recommended for Production)
Modules are explicitly registered in DI configuration.

```csharp
// In Program.cs or Startup
builder.Services.AddDiscordBot(builder.Configuration)
    .AddCoreCommands()      // Extension method from DiscordBot.Commands.Core
    .AddAdminCommands();    // Extension method from DiscordBot.Commands.Admin
```

**Pros:**
- Explicit control over what's loaded
- Clear dependencies in code
- Better performance (no reflection scanning)
- Easy to conditionally load modules

**Cons:**
- Must remember to add new modules

---

#### Pattern 2: Assembly Scanning (Auto-discovery)
Automatically discover modules in referenced assemblies.

```csharp
builder.Services.AddDiscordBot(builder.Configuration)
    .AddCommandModulesFromAssemblies(
        typeof(RegisterModule).Assembly,        // DiscordBot.Commands.Core
        typeof(InviteCodeAdminModule).Assembly  // DiscordBot.Commands.Admin
    );
```

**Pros:**
- Automatic discovery of all modules in assembly
- Less configuration code

**Cons:**
- Slight startup performance hit
- Less explicit

---

#### Pattern 3: Convention-Based Auto-discovery
Scan all assemblies matching naming convention.

```csharp
builder.Services.AddDiscordBot(builder.Configuration)
    .AddCommandModulesFromPattern("DiscordBot.Commands.*");
```

**Pros:**
- Fully automatic - just reference the package
- Great for plugin architectures

**Cons:**
- Most "magic" - harder to debug
- Potential performance impact on startup

---

## 4. Implementation Design

### 4.1 Module Registration Interface

Each command library provides an extension method for registration:

```csharp
// DiscordBot.Commands.Core/Extensions/ServiceCollectionExtensions.cs
public static class CoreCommandsServiceCollectionExtensions
{
    public static IServiceCollection AddCoreCommands(this IServiceCollection services)
    {
        // Register any services needed by this module
        services.AddScoped<IInviteCodeService, InviteCodeService>();

        // Register the module for discovery
        services.AddInteractionModule<RegisterModule>();
        services.AddInteractionModule<HelpModule>();

        return services;
    }
}
```

```csharp
// DiscordBot.Commands.Admin/Extensions/ServiceCollectionExtensions.cs
public static class AdminCommandsServiceCollectionExtensions
{
    public static IServiceCollection AddAdminCommands(this IServiceCollection services)
    {
        services.AddInteractionModule<InviteCodeAdminModule>();
        services.AddInteractionModule<UserAdminModule>();
        services.AddInteractionModule<ServerAdminModule>();

        return services;
    }
}
```

### 4.2 Core Extension Method

```csharp
// DiscordBot.Core/Extensions/InteractionServiceExtensions.cs
public static class InteractionServiceExtensions
{
    private static readonly List<Type> _moduleTypes = new();

    public static IServiceCollection AddInteractionModule<T>(this IServiceCollection services)
        where T : InteractionModuleBase
    {
        _moduleTypes.Add(typeof(T));
        return services;
    }

    public static async Task RegisterModulesAsync(
        this InteractionService interactionService,
        IServiceProvider serviceProvider)
    {
        foreach (var moduleType in _moduleTypes)
        {
            await interactionService.AddModuleAsync(moduleType, serviceProvider);
        }

        // Register globally (or per guild based on config)
        await interactionService.RegisterCommandsGloballyAsync();
    }
}
```

### 4.3 BotHostedService Integration

```csharp
// DiscordBot.Core/Services/BotHostedService.cs
public class BotHostedService : IHostedService
{
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    private async Task OnReadyAsync()
    {
        _logger.LogInformation("Bot is ready. Registering command modules...");

        // Automatically register all discovered modules
        await _interactionService.RegisterModulesAsync(_serviceProvider);

        _logger.LogInformation("Command modules registered successfully");
    }
}
```

---

## 5. Module Metadata (Optional Enhancement)

For more advanced scenarios, modules can provide metadata:

```csharp
// DiscordBot.Core/Attributes/CommandModuleAttribute.cs
[AttributeUsage(AttributeTargets.Class)]
public class CommandModuleAttribute : Attribute
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool RequiresDatabase { get; set; }
    public string[] RequiredPermissions { get; set; }

    public CommandModuleAttribute(string name)
    {
        Name = name;
    }
}
```

Usage:
```csharp
[CommandModule("Admin Commands",
    Description = "Administrative commands for server management",
    RequiresDatabase = true,
    RequiredPermissions = new[] { "Administrator" })]
public class AdminModule : InteractionModuleBase<SocketInteractionContext>
{
    // ...
}
```

---

## 6. Configuration-Based Module Loading

For maximum flexibility, support enabling/disabling modules via configuration:

```json
{
  "Bot": {
    "Token": "...",
    "Modules": {
      "CoreCommands": {
        "Enabled": true
      },
      "AdminCommands": {
        "Enabled": true,
        "RequireGuildAdmin": true
      },
      "CustomCommands": {
        "Enabled": false
      }
    }
  }
}
```

```csharp
// Conditional registration based on config
builder.Services.AddDiscordBot(builder.Configuration);

if (builder.Configuration.GetValue<bool>("Bot:Modules:CoreCommands:Enabled"))
{
    builder.Services.AddCoreCommands();
}

if (builder.Configuration.GetValue<bool>("Bot:Modules:AdminCommands:Enabled"))
{
    builder.Services.AddAdminCommands();
}
```

---

## 7. Assembly Scanning Implementation (Pattern 2)

```csharp
// DiscordBot.Core/Extensions/InteractionServiceExtensions.cs
public static IServiceCollection AddCommandModulesFromAssemblies(
    this IServiceCollection services,
    params Assembly[] assemblies)
{
    foreach (var assembly in assemblies)
    {
        var moduleTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => typeof(InteractionModuleBase).IsAssignableFrom(t));

        foreach (var moduleType in moduleTypes)
        {
            services.AddInteractionModule(moduleType);
        }
    }

    return services;
}

private static IServiceCollection AddInteractionModule(
    this IServiceCollection services,
    Type moduleType)
{
    _moduleTypes.Add(moduleType);
    return services;
}
```

Usage:
```csharp
builder.Services.AddDiscordBot(builder.Configuration)
    .AddCommandModulesFromAssemblies(
        typeof(RegisterModule).Assembly,
        typeof(AdminModule).Assembly
    );
```

---

## 8. Recommended Approach

### For This Project (Phase 1):

**Use Pattern 1: Explicit Registration**

```csharp
// Program.cs
builder.Services.AddDiscordBot(builder.Configuration)
    .AddCoreCommands()
    .AddAdminCommands();
```

**Rationale:**
- Clear and explicit
- Easy to understand and debug
- No performance overhead
- Simple to test
- Can add auto-discovery later if needed

---

## 9. Project Structure (Recommended)

### Phase 1: Keep It Simple
```
src/DiscordBot/
├── DiscordBot.Core/               # Framework + All Commands
│   ├── Entities/
│   ├── DTOs/
│   ├── Repositories/
│   ├── Services/
│   ├── Modules/                   # All command modules here
│   │   ├── RegisterModule.cs
│   │   └── AdminModule.cs
│   └── Extensions/
└── DiscordBot.Blazor/
```

### Phase 2+: When You Have Many Commands
```
src/DiscordBot/
├── DiscordBot.Core/               # Framework only
├── DiscordBot.Commands.Core/      # Basic user commands
├── DiscordBot.Commands.Admin/     # Admin commands
├── DiscordBot.Commands.Moderation/ # Moderation commands
├── DiscordBot.Commands.Fun/       # Fun/entertainment commands
└── DiscordBot.Blazor/
```

---

## 10. Module Lifecycle Hooks (Future Enhancement)

For advanced scenarios, modules can implement lifecycle hooks:

```csharp
public interface ICommandModule
{
    Task OnModuleRegisteredAsync();
    Task OnModuleUnloadedAsync();
    Task OnCommandExecutedAsync(string commandName, IInteractionContext context);
}
```

Example:
```csharp
public class AdminModule : InteractionModuleBase<SocketInteractionContext>, ICommandModule
{
    public async Task OnModuleRegisteredAsync()
    {
        _logger.LogInformation("Admin module registered");
        await InitializeAdminRolesAsync();
    }

    public async Task OnCommandExecutedAsync(string commandName, IInteractionContext context)
    {
        await LogAdminActionAsync(commandName, context.User);
    }
}
```

---

## 11. Testing Strategy

### Unit Testing Modules

```csharp
public class RegisterModuleTests
{
    private readonly Mock<IInviteCodeService> _mockInviteService;
    private readonly RegisterModule _module;

    [Fact]
    public async Task RegisterAsync_UserNotRegistered_SendsDM()
    {
        // Arrange
        var inviteCode = new InviteCodeDto { Code = "TEST-CODE-1234" };
        _mockInviteService
            .Setup(x => x.GenerateInviteCodeAsync(It.IsAny<ulong>(), It.IsAny<string>()))
            .ReturnsAsync(inviteCode);

        // Act
        await _module.RegisterAsync();

        // Assert
        _mockInviteService.Verify(x => x.GenerateInviteCodeAsync(
            It.IsAny<ulong>(),
            It.IsAny<string>()),
            Times.Once);
    }
}
```

### Integration Testing Module Registration

```csharp
public class ModuleRegistrationTests
{
    [Fact]
    public async Task AddCoreCommands_RegistersAllModules()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddDiscordBot(mockConfig)
            .AddCoreCommands();

        var provider = services.BuildServiceProvider();
        var interactionService = provider.GetRequiredService<InteractionService>();

        // Act
        await interactionService.RegisterModulesAsync(provider);

        // Assert
        var modules = interactionService.Modules;
        Assert.Contains(modules, m => m.Name == "RegisterModule");
        Assert.Contains(modules, m => m.Name == "HelpModule");
    }
}
```

---

## 12. Example: Adding a New Command Module

### Step 1: Create the Module Class

```csharp
// DiscordBot.Core/Modules/PingModule.cs
public class PingModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "Check if the bot is responsive")]
    public async Task PingAsync()
    {
        await RespondAsync($"Pong! Latency: {Context.Client.Latency}ms");
    }
}
```

### Step 2: Register in Extension Method

```csharp
// DiscordBot.Core/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddDiscordBot(this IServiceCollection services, IConfiguration configuration)
{
    // ... existing code ...

    // Register modules
    services.AddInteractionModule<RegisterModule>();
    services.AddInteractionModule<AdminModule>();
    services.AddInteractionModule<PingModule>();  // ADD THIS LINE

    return services;
}
```

### Step 3: Done!
The module will be automatically discovered and registered on bot startup.

---

## 13. Summary & Recommendation

### Recommended Implementation Path:

1. **Phase 1 (Now)**:
   - Keep all modules in `DiscordBot.Core/Modules/`
   - Use simple explicit registration in `ServiceCollectionExtensions`
   - Register modules one-by-one in `AddDiscordBot()`

2. **Phase 2** (When you have 10+ modules):
   - Split into separate command libraries
   - Add extension methods per library (`.AddCoreCommands()`, etc.)
   - Still explicit registration

3. **Phase 3** (If needed):
   - Implement assembly scanning for true auto-discovery
   - Add configuration-based module loading
   - Implement module lifecycle hooks

This approach provides:
- ✅ Simple to start with
- ✅ Easy to understand and maintain
- ✅ Scales as project grows
- ✅ No over-engineering
- ✅ Testable at every phase

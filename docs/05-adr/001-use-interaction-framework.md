# 001. Use Discord.Net Interaction Framework for Slash Commands

**Date:** 2025-10-18
**Status:** Accepted
**Deciders:** Project Team

---

## Context

Discord bots can handle commands using several approaches:
1. Traditional text-based prefix commands (e.g., `!register`)
2. Manual slash command handling via Discord API
3. Discord.Net Interaction Framework (attribute-based slash commands)

We need a maintainable, scalable way to implement Discord slash commands for user registration and admin functions.

## Decision

We will use the **Discord.Net Interaction Framework** (`Discord.Net.Interactions` package) with an attribute-based module system for all Discord commands.

Commands will be organized into modules that inherit from `InteractionModuleBase<SocketInteractionContext>` and use attributes like `[SlashCommand]` for command definitions.

## Consequences

### Positive

- **Modern approach**: Aligns with Discord's push toward slash commands
- **Type safety**: Compile-time checks for command parameters
- **Familiar pattern**: Similar to ASP.NET Core MVC/API controllers (attribute-based routing)
- **Built-in DI**: Automatic dependency injection into command modules
- **Rich features**: Supports autocomplete, choices, subcommands, user/role parameters
- **Clean syntax**: Declarative command definitions
- **Extensible**: Easy to add new command modules

### Negative

- **Additional dependency**: Requires `Discord.Net.Interactions` package
- **Learning curve**: Team needs to learn Interaction Framework patterns
- **Global command delay**: Global command registration can take up to 1 hour (mitigated by using guild commands in development)

### Neutral

- Commands are registered at bot startup (one-time operation)
- Requires specific OAuth2 scope (`applications.commands`)

## Alternatives Considered

### Alternative 1: Text-Based Prefix Commands

**Description**: Use Discord.Net.Commands for traditional `!command` style commands.

**Pros**:
- Familiar pattern for older bots
- Instant registration (no Discord API delays)
- Well-documented

**Cons**:
- Discord is deprecating this approach
- Less discoverable for users
- No built-in parameter validation UI
- No autocomplete support
- Clutters chat with command invocations

**Rejected because**: Discord is moving away from this pattern, and slash commands provide better UX.

### Alternative 2: Manual Slash Command Handling

**Description**: Manually handle `SlashCommandExecuted` events and route to handlers.

**Pros**:
- Full control over command routing
- No framework overhead

**Cons**:
- Significant boilerplate code
- Manual parameter parsing and validation
- No compile-time safety
- Hard to maintain as commands grow
- Reinventing the wheel

**Rejected because**: The Interaction Framework provides all needed features without boilerplate.

## References

- [Discord.Net Interaction Framework Documentation](https://docs.discordnet.dev/guides/int_framework/intro.html)
- [Command Architecture Design](../02-architecture/03-command-system.md)

# 003. Use Repository Pattern for Data Access

**Date:** 2025-10-18
**Status:** Accepted
**Deciders:** Project Team

---

## Context

The application needs to access database entities (InviteCode, ApplicationUser) from multiple layers:
- Discord bot command modules
- Blazor web UI components
- Background services

We need to decide how to structure data access to ensure:
- Testability (ability to mock data access)
- Separation of concerns (business logic vs data access)
- Maintainability (consistent data access patterns)
- Flexibility (ability to change data access implementation)

## Decision

We will implement the **Repository Pattern** for data access, with:
- Interface-based repository contracts (`IInviteCodeRepository`)
- Concrete EF Core implementations (`InviteCodeRepository`)
- Repository registration in dependency injection
- Service layer consuming repositories (not DbContext directly)

Structure:
```
DiscordBot.Core/
├── Repositories/
│   ├── IInviteCodeRepository.cs      # Interface
│   └── InviteCodeRepository.cs       # EF Core implementation
└── Services/
    ├── IInviteCodeService.cs         # Business logic interface
    └── InviteCodeService.cs          # Uses IInviteCodeRepository
```

## Consequences

### Positive

- **Testability**: Easy to mock repositories in unit tests
- **Separation of concerns**: Clear boundary between data access and business logic
- **Abstraction**: Services don't depend on EF Core directly
- **Consistency**: Standardized data access patterns
- **Single responsibility**: Repositories only handle data access
- **Flexibility**: Can swap implementations (e.g., Dapper, caching layer)
- **Explicit contracts**: Repository interfaces document available operations

### Negative

- **More code**: Additional abstraction layer requires more files
- **Indirection**: Extra layer between services and data
- **Potential over-engineering**: For simple CRUD, DbContext might suffice
- **Learning curve**: Developers must understand the pattern

### Neutral

- Repository implementations will be in `DiscordBot.Core` but use `ApplicationDbContext` from `DiscordBot.Blazor` (via DI)
- Each entity gets its own repository (no generic repository)

## Alternatives Considered

### Alternative 1: Direct DbContext Usage

**Description**: Services inject and use `ApplicationDbContext` directly.

**Pros**:
- Less code and fewer files
- Direct access to EF Core features
- No extra abstraction
- Faster to implement

**Cons**:
- Harder to test (must mock DbContext)
- Services tightly coupled to EF Core
- Business logic mixed with data access
- No clear data access contract
- Difficult to add caching or alternate implementations

**Rejected because**: Violates separation of concerns and makes testing difficult.

### Alternative 2: Generic Repository Pattern

**Description**: Single generic `IRepository<T>` with CRUD methods for all entities.

**Pros**:
- Less code (one repository for all entities)
- Consistent interface across entities
- Easy to implement

**Cons**:
- Leaky abstraction (exposes IQueryable)
- Often leads to anemic repositories
- Doesn't capture entity-specific operations well
- Can become bloated with special cases
- "One size fits all" rarely fits well

**Rejected because**: Tends to leak EF Core concerns and doesn't express domain operations clearly.

### Alternative 3: Specification Pattern

**Description**: Use specification objects to encapsulate query logic.

**Pros**:
- Very flexible query composition
- Reusable query logic
- Testable specifications

**Cons**:
- Significant complexity for simple queries
- Steeper learning curve
- More abstractions to manage
- Overkill for this project's needs

**Rejected because**: Too complex for the current scope; can be added later if needed.

## References

- [Repository Pattern (Martin Fowler)](https://martinfowler.com/eaaCatalog/repository.html)
- [Technical Architecture Overview](../02-architecture/01-technical-overview.md)

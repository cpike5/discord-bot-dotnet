# 002. Use SQLite for Development Database

**Date:** 2025-10-18
**Status:** Accepted
**Deciders:** Project Team

---

## Context

The project needs a database for storing invite codes and user data. Different database systems are suited for different environments (development, testing, production).

The original project template used SQL Server, which requires:
- SQL Server installation (LocalDB or full instance)
- Additional configuration for developers
- Platform-specific limitations (Windows-focused)

We need a database strategy that:
- Is easy to set up for new developers
- Works cross-platform
- Supports production databases (MySQL/PostgreSQL)
- Minimizes development friction

## Decision

We will use **SQLite** as the default database for local development and testing, while supporting MySQL/MariaDB and PostgreSQL for production deployments.

Database provider selection will be configuration-based using the `Database:Provider` setting in `appsettings.json`.

## Consequences

### Positive

- **Zero setup**: SQLite requires no installation or configuration
- **Portable**: Database is a single file that can be committed or shared
- **Cross-platform**: Works on Windows, macOS, Linux
- **Fast for development**: In-process database, no network overhead
- **Easy testing**: In-memory mode for unit tests
- **Good for CI/CD**: No need to spin up database containers for simple tests
- **File-based backups**: Simple copy of `.db` file

### Negative

- **Single writer limitation**: Only one process can write at a time (not an issue for this app)
- **No network access**: Can't connect remotely (fine for local dev)
- **Different from production**: Production uses MySQL/PostgreSQL (mitigated by proper EF Core abstractions)
- **Limited concurrency**: Not suitable for high-load production (we're not using it for that)

### Neutral

- Requires developer awareness of database provider differences
- Migrations must be tested against all target databases
- Entity Framework abstractions handle most compatibility issues

## Alternatives Considered

### Alternative 1: SQL Server (LocalDB)

**Description**: Continue using SQL Server LocalDB as templated.

**Pros**:
- Already configured in template
- Familiar to .NET developers
- Good tooling (SSMS)

**Cons**:
- Windows-only (or complex Docker setup)
- Requires installation and configuration
- Heavier resource usage
- More complex for beginners
- Overkill for local development

**Rejected because**: Adds unnecessary complexity and platform limitations for development.

### Alternative 2: Docker-based MySQL/PostgreSQL

**Description**: Use Docker containers for development databases matching production.

**Pros**:
- Exact parity with production
- Good isolation
- Industry standard practice

**Cons**:
- Requires Docker installation and knowledge
- Higher barrier to entry for new contributors
- More complex setup process
- Resource overhead
- Container management complexity

**Rejected because**: Adds too much setup friction for development. We'll use this for integration testing instead.

### Alternative 3: In-Memory Database Only

**Description**: Use EF Core's in-memory provider for development.

**Pros**:
- Zero configuration
- Very fast
- Built into EF Core

**Cons**:
- Not a real database (lacks constraints, triggers, etc.)
- Data doesn't persist between runs
- Can't inspect data with SQL tools
- Masks potential production issues

**Rejected because**: Too different from real databases; useful only for unit tests.

## References

- [Database Strategy Documentation](../02-architecture/02-database-design.md)
- [Entity Framework Core SQLite Provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)

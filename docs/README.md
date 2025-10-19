# Discord Bot Documentation

Welcome to the Discord Bot with Web Application Integration documentation.

---

## 📋 Documentation Index

### 📖 Planning & Requirements
Start here to understand what we're building and why.

- **[Product Requirements](01-planning/01-product-requirements.md)** - Project goals, features, and success metrics
- **[Functional Specifications](01-planning/02-functional-specs.md)** - User stories, flows, and detailed requirements

### 🏗️ Architecture & Design
Technical design and architecture decisions.

- **[Technical Overview](02-architecture/01-technical-overview.md)** - Tech stack, project structure, and component design
- **[Database Design](02-architecture/02-database-design.md)** - Schema, ERD, multi-database strategy, and migrations
- **[Command System](02-architecture/03-command-system.md)** - Modular command architecture and registration patterns

### 📚 Guides
How-to guides for developers (coming soon).

- Getting Started *(planned)*
- Development Setup *(planned)*
- Database Setup *(planned)*
- Adding Commands *(planned)*
- Deployment *(planned)*

### 📖 Reference
Technical reference documentation (coming soon).

- Configuration Options *(planned)*
- API Reference *(planned)*
- Troubleshooting *(planned)*

### 🗂️ Architecture Decision Records
Important architectural decisions and their rationale.

- **[ADR Index](05-adr/README.md)** - Complete list of architecture decisions
  - [001: Use Interaction Framework](05-adr/001-use-interaction-framework.md)
  - [002: Use SQLite for Development](05-adr/002-use-sqlite-for-development.md)
  - [003: Use Repository Pattern](05-adr/003-use-repository-pattern.md)

### 📐 Standards
Documentation and coding standards.

- **[Documentation Standards](DOCUMENTATION_STANDARDS.md)** - How we organize and write documentation

---

## 🚀 Quick Start for New Developers

1. **Understand the project**: Read [Product Requirements](01-planning/01-product-requirements.md)
2. **Learn the architecture**: Review [Technical Overview](02-architecture/01-technical-overview.md)
3. **Review design decisions**: Check [ADR Index](05-adr/README.md)
4. **Understand the database**: See [Database Design](02-architecture/02-database-design.md)
5. **Learn command system**: Study [Command System](02-architecture/03-command-system.md)

---

## 🎯 Project Overview

### What We're Building

A Discord bot integrated with an ASP.NET Core Blazor web application that provides:
- Invite-code-based user registration via Discord `/register` command
- One-to-one mapping between Discord users and web accounts
- Administrative commands for invite code management
- 24-hour invite code expiration

### Key Features

- 🎟️ **Discord `/register` command** - Users request invite codes via Discord
- 🌐 **Web registration** - Users register on web app using invite codes
- 🔗 **Discord linking** - One Discord user = one web account
- 👨‍💼 **Admin commands** - Manage invite codes via Discord
- ⏱️ **Auto-expiration** - Codes expire after 24 hours

### Technology Stack

| Layer | Technology |
|-------|------------|
| Runtime | .NET 8.0 |
| Web Framework | ASP.NET Core Blazor Server |
| Discord Framework | Discord.Net (WebSocket + Interactions) |
| ORM | Entity Framework Core 8.0 |
| Database (Dev) | SQLite |
| Database (Prod) | MySQL/MariaDB or PostgreSQL |
| Authentication | ASP.NET Core Identity |

### Architecture Patterns

- **Clean/Layered Architecture** - Clear separation of concerns
- **Repository Pattern** - Abstracted data access
- **Service Layer** - Business logic isolation
- **DTO Pattern** - Data transfer objects
- **Modular Commands** - Extensible command system

---

## 📊 Project Status

| Aspect | Status |
|--------|--------|
| **Phase** | Documentation & Planning |
| **Version** | 1.0 (Draft) |
| **Last Updated** | 2025-10-18 |

---

## 🛠️ Development Workflow

### Documentation First
We document before we code. All major features and decisions should be documented in this directory.

### Adding New Documentation

1. Follow the [Documentation Standards](DOCUMENTATION_STANDARDS.md)
2. Use the proper naming convention: `[number]-[kebab-case].md`
3. Place in the appropriate directory
4. Update this index
5. Link from related documents

### Making Architecture Decisions

1. Use the [ADR template](05-adr/template.md)
2. Document the decision in `05-adr/`
3. Update the [ADR index](05-adr/README.md)
4. Reference from relevant technical docs

---

## 📚 Additional Resources

- [Discord.Net Documentation](https://docs.discordnet.dev/)
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/)

---

## 🤝 Contributing

When contributing documentation:

1. Read [Documentation Standards](DOCUMENTATION_STANDARDS.md)
2. Follow the established structure
3. Keep documents concise and focused
4. Update the index when adding new docs
5. Use proper markdown formatting

---

## 📝 Future Documentation

As the project evolves, we'll add:

- 📖 Getting started guide
- 🔧 Development environment setup
- 💾 Database setup and migrations guide
- ➕ How to add new commands
- 🚀 Deployment guide
- 🔍 API reference documentation
- ❓ Troubleshooting guide
- 👥 Contributing guidelines

---

**Need help?** Check the relevant section above or review the [Documentation Standards](DOCUMENTATION_STANDARDS.md).

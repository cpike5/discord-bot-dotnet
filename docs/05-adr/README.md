# Architecture Decision Records (ADR)

This directory contains Architecture Decision Records for the Discord Bot project.

## What is an ADR?

An Architecture Decision Record (ADR) captures an important architectural decision made along with its context and consequences. ADRs help teams understand why certain technical choices were made and provide historical context for future decisions.

## ADR Index

| Number | Title | Status | Date |
|--------|-------|--------|------|
| [001](001-use-interaction-framework.md) | Use Discord.Net Interaction Framework for Slash Commands | Accepted | 2025-10-18 |
| [002](002-use-sqlite-for-development.md) | Use SQLite for Development Database | Accepted | 2025-10-18 |
| [003](003-use-repository-pattern.md) | Use Repository Pattern for Data Access | Accepted | 2025-10-18 |

## Creating a New ADR

1. Copy [template.md](template.md)
2. Name it with the next number: `NNN-decision-name.md`
3. Fill out all sections
4. Add entry to the index above
5. Commit with the ADR

## ADR Statuses

- **Proposed**: Under consideration
- **Accepted**: Decision has been made and is active
- **Deprecated**: No longer applicable but kept for history
- **Superseded**: Replaced by a newer ADR (link to replacement)

## Guidelines

- One decision per ADR
- Be concise but complete
- Include context and consequences
- Never edit an accepted ADR (create a new one instead)
- Link to related ADRs

# Folio

Club reporting and document management (.NET 10, Blazor, Clean Architecture).

Clubs of a league (the demo seed uses ~30 Czech ice hockey clubs with fictional data) report **player transfers**, **budgets** (spending and income) and **season reports** to a league office. Folio opens reporting periods, snapshots the participating clubs from a registry, lets club users confirm documents or fill versioned Excel packages, and keeps an audit trail of everything.

## Solution structure

Open **`Folio.sln`** in Visual Studio.

If the build fails with *"Folio.Application.dll could not be found"*:
1. Close Visual Studio
2. Run `Clean-And-Build.bat` in the repo root
3. Reopen `Folio.sln`

```
src/
  Folio.Domain/           Entities, enums
  Folio.Application/      DTOs, services, static mappers, interfaces
  Folio.Infrastructure/   File storage, registry client, in-memory + SQL (Dapper) repos
  Folio.Api/              REST API
  Folio.Web/              Blazor Web App (Interactive Server)
tests/                    xUnit tests (Application, Infrastructure)
database/                 SQL Server scripts (schema, stored procedures, seed data)
contracts/                Auto-generated OpenAPI contract (on API build)
```

See [AI/Architect.md](AI/Architect.md) for the full specification and [PERMISSIONS.md](PERMISSIONS.md) for roles and access control.

## Prerequisites

- .NET 10 SDK
- SQL Server (optional — the default provider is in-memory, see [database/README.md](database/README.md))

## Run locally

Terminal 1 — API:

```bash
dotnet run --project src/Folio.Api
```

Terminal 2 — Web UI:

```bash
dotnet run --project src/Folio.Web
```

- API: https://localhost:7280
- Web: https://localhost:7299

In Development the app runs with seeded demo users (switch via `ActiveDevUser` in `src/Folio.Web/appsettings.Development.json`) and a **fake registry** that serves ~30 hockey clubs, so no external system is needed.

## Design prototype

Static HTML mockup: [FolioApp.html](FolioApp.html) (reference for the CSS in `src/Folio.Web/wwwroot/folio.css`).

## AI tooling

Project knowledge is documented in two places so that different AI coding tools can use it:

- **`CLAUDE.md`** (repo root) — auto-loaded by Claude Code. Condensed architecture/conventions plus Claude-Code-specific setup (`.claude/agents/`, `/feature-done`).
- **`AI/Architect.md`** and **`AI/Skills/*.md`** — full specification and per-topic conventions, tool-agnostic.

Both describe the same architecture and must be kept in sync.
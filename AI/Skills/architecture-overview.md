# Skill: Folio Architecture Overview

Use this skill whenever you need to reason about where new code belongs in the Folio solution.

## Layers (Clean Architecture)

- `src/Folio.Domain` — Entities, enums, domain rules. **No external dependencies.**
- `src/Folio.Application` — DTOs, interfaces (`Interfaces/Repositories`, `Interfaces/Services`, `Interfaces/Integrations`), services, static mappers (`Mappings/`). Depends on Domain only.
- `src/Folio.Infrastructure` — Implements Application interfaces: SQL (Dapper, `Persistence/Sql`), in-memory (`Persistence/InMemory`), file storage (`Storage/`), external integrations (`Integrations/`). Wired via `DependencyInjection.cs`.
- `src/Folio.Api` — Thin REST controllers, DI wiring, OpenAPI contract generation (`contracts/contract.yaml`).
- `src/Folio.Web` — Blazor Web App (.NET 10, Interactive Server). Calls the API via `HttpClient`, references `Folio.Application` for DTOs. Uses Radzen.Blazor for grids/dialogs and a custom `FolioApp.css` design system (primary `#312E81`, accent `#0891B2`).

## Rules to enforce

1. No AutoMapper — always add static mapping methods in `Application/Mappings/`.
2. Persistence provider is switched via config (`Persistence:Provider` = `Sql` or `InMemory`) in `Folio.Infrastructure/DependencyInjection.cs`. Any new repository/service needs both an in-memory and a SQL implementation registered there.
3. All new NuGet packages must use free/open-source licenses.
4. UI text is English-only (international approvers).
5. API contract (`contracts/contract.yaml`) regenerates automatically on `Folio.Api` build — never hand-edit it.

## Reference

Full context lives in `AI/Architect.md` and `PERMISSIONS.md`. Read them before large feature work.

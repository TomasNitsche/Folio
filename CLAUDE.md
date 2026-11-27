# Folio — Club Reporting Confirmation Automation

This file is auto-loaded by Claude Code at the start of every session in this repo. It condenses `AI/Architect.md` and `AI/Skills/*.md` — read those files directly for full detail.

> **Keep in sync:** other developers on this project use other AI tools (Cursor, Copilot, ...) pointed at `AI/Architect.md` and `AI/Skills/*.md`, not at this file. Whenever architecture, conventions, or workflow change here, make the same edit in the relevant `AI/*.md` file(s) — and vice versa. Don't let the two drift apart.

## What Folio does

Folio manages the periodic reports clubs submit to a league office — player transfers, budgets (spending and income), season reports (modular, more types may be added). The demo seed uses ~30 Czech ice hockey clubs. It replaces manual Excel/email workflows with central tracking, versioned document packages, and an audit trail. **UI is English-only.**

Core flow: a Reporting lead opens a period → Folio snapshots entities from **Registry** (system of record) at that date → lead selects which entities to include → company users confirm (sign a manual) or fill a package (Excel, versioned, until marked `Final`) → Folio tracks status via dashboard/grid and audit log.

Full functional spec, roles/permissions matrix, file storage layout, and open questions: `AI/Architect.md`. Permission matrix: `PERMISSIONS.md`.

Two cross-cutting rules that are easy to miss:

- **Grid status/updated are derived, not stored** — `EntityWorkflowState.DeriveStatus`/`LastActivityAt` compute them from an entity's documents *and* its latest package (Draft → Pending once the static reference is up → Confirmed when final). Don't read `EntityPackage.Status`/`UpdatedAt` straight into the grid.
- **Default period** — one period per confirmation type carries `IsDefault`; it is preselected everywhere and is the only period company users (Confirmer, Pack filler) may upload into. Enforced in `EntityAccessService`, set via `POST /api/periods/{id}/default`.

## Solution architecture (Clean Architecture)

```
Folio.sln
├── src/
│   ├── Folio.Domain/           Entities, enums, domain rules — no external dependencies
│   ├── Folio.Application/      DTOs, interfaces (Interfaces/{Repositories,Services,Integrations}),
│   │                            services, static mappers (Mappings/) — depends on Domain only
│   ├── Folio.Infrastructure/   Implements Application interfaces: SQL (Dapper, Persistence/Sql),
│   │                            in-memory (Persistence/InMemory), file storage (Storage/),
│   │                            external integrations (Integrations/). Wired in DependencyInjection.cs
│   ├── Folio.Api/              Thin REST controllers, DI wiring, OpenAPI contract generation
│   └── Folio.Web/              Blazor Web App (.NET 10, Interactive Server), calls Api via HttpClient,
│                                references Application for DTOs
├── tests/
│   ├── Folio.Application.Tests/   xUnit, references Application only — no mocking framework, plain
│   │                                objects/in-memory fakes (see existing tests before adding a new one)
│   └── Folio.Infrastructure.Tests/  xUnit, references Infrastructure — same conventions as above
└── contracts/
    └── contract.yaml            Auto-generated OpenAPI spec on every Folio.Api build — never hand-edit
```

Target framework: **.NET 10**. Namespaces follow folder structure (`Folio.<Project>.<Folder>`).

## Rules to enforce

1. **No AutoMapper** — always add static mapping methods in `Application/Mappings/*Mapper.cs`.
2. Persistence provider is switched via config (`Persistence:Provider` = `Sql` or `InMemory`) in `Folio.Infrastructure/DependencyInjection.cs`. Any new repository/service needs **both** an in-memory and a SQL implementation registered there.
3. All new NuGet packages must use **free/open-source** licenses.
4. UI text is **English-only**.
5. `contracts/contract.yaml` regenerates automatically on `Folio.Api` build — never hand-edit it; add endpoints/DTOs in Application and rebuild instead.
6. Any state change, upload, or version creation must be logged via the audit mechanism (`AuditEntry` / `SqlAuditRepository` / in-memory equivalent).
7. Prefer minimal, targeted diffs; match existing formatting/brace style already in the touched file.

## Blazor UI (Folio.Web)

- Interactive Server render mode, applied **globally** via `<Routes @rendermode="InteractiveServer" />` in `App.razor` so `MainLayout` (sidebar toggle) is interactive as well — never put `@rendermode` on individual pages.
- Prefer **Radzen.Blazor** components for grids/dialogs/forms.
- Styling goes through `FolioApp.css`; brand colors: primary `#312E81`, accent `#0891B2`.

## Registry integration

`IRegistryEntityClient.GetEntitiesValidOnAsync(DateOnly date, ct)` is the contract (`Folio.Application.Interfaces.Integrations`). Implementation is selected via `Registry:Provider` in `DependencyInjection.cs` (defaults to `Dev`/`InMemory` based on `Persistence:Provider` when unset):

- `Http` → `HttpRegistryEntityClient` — real integration: `HttpClientFactory` + Windows integrated auth (`HttpClientHandler { UseDefaultCredentials = true }`) + `Microsoft.Extensions.Http.Resilience`, config under `Registry:*`. Calls `GET folio-integration/entities?asOfDate=yyyy-MM-dd` and maps the response to `RegistryEntitySnapshot` via `RegistryEntityMapper` (static, no AutoMapper). `RegistryOptionsValidator` fails startup fast when `Registry:Provider=Http` and `Registry:BaseUrl` is missing or lacks its trailing `/`.
- `Dev` → `DevRegistryEntityClient`, `InMemory` (default) → `InMemoryRegistryEntityClient` — fakes serving ~30 fictional hockey clubs from `Persistence/FakeClubDirectory.cs`; keep for local dev/test, do not delete. `RegistryEntityClientStub` is a minimal stub with simulated latency. Don't hardcode `Registry:Provider` in the shared `appsettings.json`.
- `RegistryEntityMapper.ToSnapshot` uses `CompanyCode` as the entity's short display `Code` (falling back to `DisplayValue`); `RegistrationNumber` is carried to entity-selection time but not persisted onto `PeriodEntity`.
- Use a plain REST/`HttpClient` call, not an MCP server.

Full detail: `AI/Skills/registry-integration.md`.

## Versioning

The version comes from the CI pipeline (`azure-pipelines.yml`): `versionPrefix` (`0.0` until production, then `1.0`) + a counter → build number (e.g. `0.0.7`), passed to the build as `-p:Version=...`; a successful build of `main` tags the commit with it. Local builds are `0.0.0-local` (`Directory.Build.props`). The UI reads it via `Folio.Web/Services/AppVersion` and shows it in the sidebar footer menu `AppInfoMenu` (*System info*). Never hard-code a version in code; to change the major/minor, change `versionPrefix` in the pipeline. The pipeline identity needs permission to push tags. Details: `AI/Architect.md` → *Versioning & build*.

## Development user switch

In the `Development` environment the top bar shows a **DEV** user dropdown (`MainLayout`) listing the users from `DevUsers` in `Folio.Web/appsettings.Development.json`. Choosing one mutates the singleton `CurrentUserOptions` (used by `CurrentUserDelegatingHandler` for the API calls) and reloads the page, so roles and assignment-scoped access can be tried without real authentication. It never renders outside Development.

## Intro splash

A spinning 3D cube with the app name is shown while the app loads: static markup in `Components/App.razor`, styles in `wwwroot/splash.css`, timing in `wwwroot/splash.js` (hides after Blazor started, the layout rendered and one full cube turn). Plain HTML/CSS/JS, no dependencies.

## Claude Code project setup

- `.claude/agents/` — `folio-code-reviewer`, `folio-test-writer` subagents.
- `.claude/commands/feature-done.md` — `/feature-done` runs both in parallel after a feature is implemented.
- `AI/Prompts/` — ad-hoc prompt templates (code review, new feature) usable with any AI tool.

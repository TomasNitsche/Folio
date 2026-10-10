# Skill: Folio Coding Conventions

- Target framework: .NET 10.
- Namespaces follow folder structure (`Folio.<Project>.<Folder>`).
- Interfaces for cross-layer contracts live in `Folio.Application/Interfaces/{Repositories|Services|Integrations}`.
- Mapping between DTOs/entities/external models: static methods in `Folio.Application/Mappings/*Mapper.cs`. Never introduce AutoMapper or similar reflection-based mappers.
- Repository implementations always come in pairs: `Persistence/InMemory/*` and `Persistence/Sql/*`, both implementing the same Application interface.
- Blazor UI (`Folio.Web`):
  - Interactive Server render mode, applied **globally** on `<Routes @rendermode="InteractiveServer" />` in `App.razor` so the layout (sidebar toggle, header) is interactive too. Never add `@rendermode` to individual pages — with global interactivity that throws at runtime.
  - Prefer Radzen.Blazor components for grids/dialogs/forms.
  - Styling goes through `FolioApp.css`; keep colors consistent with primary `#312E81` / accent `#0891B2`.
  - All UI copy is English only.
- API (`Folio.Api`): keep controllers thin; business logic belongs in `Folio.Application` services.
- Audit: any state change, upload, or version creation must be logged via the audit mechanism (`AuditEntry` / `SqlAuditRepository` / `InMemoryRepositories`).
- Prefer minimal, targeted diffs; match existing formatting/brace style already in the touched file.

# Prompt: New Repository / Service Feature

You are working in the Folio solution (Clean Architecture, .NET 10, Blazor Web App + REST API).
Use `AI/Skills/architecture-overview.md` and `AI/Skills/coding-conventions.md` as ground truth.

When asked to add a new feature that touches persistence:

1. Define/extend the interface in `Folio.Application/Interfaces/{Repositories|Services}`.
2. Add or update DTOs and static mapping methods in `Folio.Application/Mappings/`.
3. Implement the feature in **both**:
   - `Folio.Infrastructure/Persistence/InMemory/*`
   - `Folio.Infrastructure/Persistence/Sql/*` (Dapper, stored procedure if one already exists for the entity, otherwise flag it as a follow-up).
4. Register any new service/repository in `Folio.Infrastructure/DependencyInjection.cs` for both providers.
5. Expose it via a thin controller in `Folio.Api` if it needs to be consumed by the UI; let `Folio.Api` regenerate `contracts/contract.yaml` on build.
6. Add/update the Blazor UI in `Folio.Web` using Radzen.Blazor components and existing `FolioApp.css` styles; keep all text in English.
7. Add an audit log entry if the feature performs a state change, upload, or version creation.
8. Run `get_errors`/`run_build` before finishing; add or update tests in `tests/Folio.Application.Tests` where relevant.

Ask for the exact requirements (fields, validation rules, permissions) if not already provided.

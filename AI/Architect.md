# Project: Folio — Club Reporting & Document Management

> **Keep in sync:** `CLAUDE.md` (repo root) is a condensed copy of this file and `AI/Skills/*.md`, auto-loaded by Claude Code every session. If you change architecture, conventions, or workflow here, make the same edit in `CLAUDE.md` — and vice versa. Don't let the two drift apart.

## Context

Folio manages the periodic reports that clubs submit to a league office (player transfers, budgets, spending and income, season reports) and the confirmations that go with them. The application is **English-only** (international approvers). The demo seed uses ~30 fictional-data Czech ice hockey clubs.

Current pain points:
- Manual Excel-based confirmation workflows
- Email-driven coordination
- No central tracking or audit trail

Folio centralizes confirmations, eliminates manual steps, tracks status, and supports auditability.

---

## Report types

Each type is a separate module in the left navigation. Period granularity differs per type:

| Type | Code | Period granularity | Example period code |
|------|------|-------------------|---------------------|
| Player transfers | `TRANSFERS` | **Monthly** | `2026M07` |
| Budgets | `BUDGET` | Monthly | `2026M06` |
| Season reports | `ANNUAL` | Yearly | `2025` |

Additional confirmation types may be added later (modular design).

---

## Period opening workflow (snapshot model)

1. A **Reporting lead** (Folio user) opens a new period (e.g. `2026M07`).
2. Folio calls **Registry** (system of record) via REST endpoint and retrieves all entities valid on the snapshot date:
   - Entity ID
   - Entity name
   - Entity code
3. The lead **selects which clubs** to include in the period.
4. Selected entities are persisted as a **snapshot** in the database.
5. Changes in Registry after the snapshot date are **ignored** — work always uses the frozen snapshot.

### Default period

Exactly one period per confirmation type can be flagged **default** (`ReportingPeriod.IsDefault`) — the one the
reporting team currently wants confirmed. The first period opened for a type becomes the default automatically;
a Reporting lead or Administrator moves the flag with `POST /api/periods/{id}/default` ("Set … as default" in the
period header). Consequences:

- The dashboard opens on the default period for every user instead of on the newest one.
- Club users (Confirmer, Pack filler) may still browse other periods, but those are **read-only**: uploading and
  marking final is blocked in `EntityAccessService` and the upload zones are hidden in the UI. If no period of that
  type is flagged, nothing is locked.
- Folio-internal roles are not restricted by the flag.

---

## Document & package workflow

### Folio users (Contributors)
- Upload **static reference files** (manuals, templates) per period / entity type.
- Assign **responsible persons** with permissions per entity.

### Club users (Approvers / Contributors)
Two permission-based roles per entity:

| Role | Action |
|------|--------|
| **Confirmer** | Download manual → read → sign → upload signed document |
| **Pack filler** | Download Excel package → fill in → upload back |

### Package versioning
- Each Excel upload from a company creates a **new package version** (1, 2, 3…).
- Versions can be edited until marked **`Final`**.
- Grid columns: Entity, ID, Code, **Version**, **Is Final**, Status, Responsible person, Updated.

---

## File storage

Files are stored on a configured network/local path:

```
{ReportFilesPath}/
  {PeriodCode}/          e.g. 2026M07, 2026M06, 2025
    {EntityCode}/
      static/            Folio-uploaded reference files
      signed/            Signed confirmations
      packages/          Excel packages (versioned filenames)
```

On period creation, the period folder is created automatically.

Static files may also be pulled from shared folders in some cases (integration TBD).

---

## Roles & permissions

| Role | Capabilities |
|------|-------------|
| **Administrator** | Manage periods, entities, users, confirmation types |
| **Contributor** (Folio) | Open periods, upload static files, assign users |
| **Confirmer** (Club) | Download/sign/upload signed documents |
| **Pack filler** (Club) | Download/fill/upload Excel packages |
| **Reporting lead** | Open periods, select snapshot entities |

Role-based and assignment-scoped access enforced on API and UI. See [PERMISSIONS.md](PERMISSIONS.md) for the full matrix.

---

## Functional requirements

### 1. Period management
- Create period with code, date range, confirmation type
- Fetch entities from Registry, select subset, save snapshot
- Period states: `Draft`, `Active`, `Closed`

### 2. Package workflow
- States: `Draft`, `Pending`, `Confirmed`, `Rejected`
- The status shown in the grid is **derived from the whole entity**, not only from its package
  (`EntityWorkflowState.DeriveStatus`): `Draft` until the reporting team uploads the static reference,
  `Pending` while documents are being added, `Confirmed` once the package is marked final, `Rejected` when the
  package was rejected.
- The grid's `Updated` column is the newest of all document uploads and package changes
  (`EntityWorkflowState.LastActivityAt`); it is `null`/`—` while nothing has happened.
- Version counter per entity per period
- `IsFinal` flag on version (immutable after final)

### 3. Notifications (placeholder)
- New confirmation request
- Status change
- Overdue reminders

### 4. Dashboard & grid
- KPI cards: total, pending, confirmed, draft
- Filterable grid by period, status, entity
- Detail panel: documents, workflow steps, responsible persons

### 5. Audit log
- Every state change, upload, version creation logged

---

## Solution architecture (Clean Architecture)

```
Folio.sln
├── src/
│   ├── Folio.Domain/           Entities, enums, domain rules
│   ├── Folio.Application/      DTOs, interfaces, services, static mappers
│   ├── Folio.Infrastructure/   Registry client, file storage, SQL (Dapper)
│   ├── Folio.Api/              REST API host
│   └── Folio.Web/              Blazor Web App (Interactive Server)
└── contracts/
    └── contract.yaml            Auto-generated OpenAPI spec on API build
```

### Layer rules
- **Domain** — no external dependencies
- **Application** — depends on Domain only; defines use cases
- **Infrastructure** — implements Application interfaces (DB, files, Registry)
- **Api** — thin controllers, DI wiring
- **Web** — Blazor UI, calls Api via HttpClient; references Application for DTOs

### Mapping
- **No AutoMapper** — static mapping methods in `Application/Mappings/`
- All NuGet packages must be **free/open-source** licenses

### Frontend
- **Blazor Web App** (.NET 10, Interactive Server)
- **Radzen.Blazor** for data grids, dialogs, form controls where it fits
- Custom CSS (`FolioApp.css` design system) for layout, colors, branding
- Colors: primary `#312E81`, accent `#0891B2`

### API contract
- `contracts/contract.yaml` regenerated on every **Folio.Api** build (OpenAPI 3)
- FE and BE stay in sync; add endpoints/DTOs in Application, rebuild to refresh contract

### Database
- SQL Server
- Stored procedures for persistence (future)
- Initial skeleton uses in-memory repositories until DB schema is ready

### External integrations
- **Registry** — entity master data (REST, snapshot on period open)
- **Shared folders** — optional document source (TBD)

---

## Key use cases

1. **Open period** — Lead fetches Registry entities, selects clubs, saves snapshot, creates folder
2. **Upload static files** — Folio contributor uploads manuals/templates
3. **Assign users** — Set confirmer and pack filler per entity
4. **Confirm** — Company user downloads, signs, uploads signed doc
5. **Fill package** — Company user uploads Excel; new version created
6. **Mark final** — User or Folio marks version as final
7. **Monitor** — Dashboard shows progress across entities

---

## Non-functional requirements
- Audit log on every change
- Excel export capability (future)
- API-first design
- English UI only

---

## Open questions
- Registry environment config for the `Http` provider — Folio calls `GET folio-integration/entities?asOfDate=...`; only `Registry:BaseUrl` (with trailing `/`) is configured per environment. The response carries `RegistrationNumber` and `CompanyCode`; `RegistryEntityMapper` uses `CompanyCode` as the entity `Code`. `RegistrationNumber` is carried up to entity-selection time but not yet persisted onto `PeriodEntity`. See `AI/Skills/registry-integration.md`.
- Validation rules for uploaded Excel packages
- Shared folder integration details

---

## Implementation status
- [x] Visual HTML/CSS prototype (`FolioApp.html`)
- [x] Solution skeleton (Domain, Application, Infrastructure, Api, Web)
- [x] SQL Server schema + stored procedures (`database/`)
- [x] Dapper repositories + Dev Registry substitute (`DevCompanies`, ~30 fake hockey clubs)
- [x] Dev seed service (users, companies, demo period)
- [x] Role + assignment permissions (API enforcement, entity filtering) — see [PERMISSIONS.md](PERMISSIONS.md)
- [x] Production Registry integration — `HttpRegistryEntityClient` (REST, Windows integrated auth), selected via `Registry:Provider=Http` (see Open questions)
- [ ] Authentication (phase 2 — SSO for internal, magic link for company users)
- [ ] Email notifications

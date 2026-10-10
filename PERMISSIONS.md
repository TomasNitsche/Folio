# Folio — Permissions & Access Control

This document describes how roles, entity assignments, and API enforcement work in Folio (phase 1). Authentication (login) is planned for phase 2.

See also [Architect.md](Architect.md) and [database/README.md](database/README.md).

---

## Roles

| Role | Code | Audience |
|------|------|----------|
| **Administrator** | `Administrator` | Full system administration |
| **Reporting lead** | `ReportingLead` | Opens periods, coordinates reporting |
| **Contributor** | `Contributor` | Folio staff — static documents, assignments |
| **Confirmer** | `Confirmer` | Company user — signs confirmations |
| **Pack filler** | `PackFiller` | Company user — uploads Excel packages |

---

## Permission matrix

| Capability | Administrator | Reporting lead | Contributor | Confirmer | Pack filler |
|------------|:-------------:|:--------------:|:-----------:|:---------:|:-----------:|
| View all entities in a period | yes | yes | yes | — | — |
| View assigned entities only | — | — | — | `CanConfirm` | `CanFillPackage` |
| Upload static reference (step 1) | yes | yes | yes | — | — |
| Assign confirmer / pack filler | yes | yes | yes | — | — |
| Upload signed confirmation (step 2) | yes | — | — | assigned | — |
| Upload Excel package | yes | — | — | — | assigned |
| Mark package final | yes | — | — | — | assigned |
| Download documents | all entities | all | all | assigned | assigned |
| Open period | yes | yes | — | — | — |
| Mark a period as default | yes | yes | — | — | — |
| Administration (users) | yes | — | — | — | — |

**Assigned** means the user has an `EntityAssignments` row for that `PeriodEntityId` with the matching flag (`CanConfirm` or `CanFillPackage`).

**Default period.** One period per confirmation type is flagged as the one being collected. Confirmers and pack fillers can open and read any period, but they can only upload into the default one — other periods are read-only for them. Folio-internal roles are not restricted this way. If no period of that type is flagged, nothing is locked.

A pack filler assigned to four companies sees only those four in the grid and KPI summary. The same applies to confirmers.

---

## Entity assignments

Each company in a period (`PeriodEntities`) can have up to one **confirmer** and one **pack filler**:

```
EntityAssignments
  PeriodEntityId  →  company snapshot in a period
  UserId          →  AppUsers.Id
  CanConfirm      →  true for confirmer
  CanFillPackage  →  true for pack filler
```

Assignments are set by Administrator, Reporting lead, or Contributor via the entity detail panel (**Save assignments**). The assignee must already exist as a user with the matching role (`Confirmer` or `PackFiller`).

---

## Document workflow & who acts

```
1. Static reference     → Contributor / Lead / Admin
2. Signed confirmation  → Confirmer (assigned) or Admin
3. Excel package (v0…)  → Pack filler (assigned) or Admin
4. Mark final           → Pack filler (assigned) or Admin
```

Enforcement is applied in **Application services** (not only in the UI). Unauthorized actions return **HTTP 403**.

---

## Development user switching

In Development, the Web app impersonates a user from `appsettings.Development.json`:

```json
"ActiveDevUser": "karel.gott",
"DevUsers": {
  "karel.gott": {
    "UserId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4",
    "UserName": "karel.gott",
    "AppRole": "Confirmer"
  }
}
```

Change `ActiveDevUser` and restart **Folio.Web** to switch identity. The Web client sends `X-Folio-User-Id` and `X-Folio-User-Role` headers to the API on every request.

Seed user IDs are defined in `DevSeedData.cs` and `database/003_SeedData.sql`.

---

## Implementation (phase 1)

| Layer | Responsibility |
|-------|----------------|
| `RolePermissions` | Role-level capability checks |
| `EntityAccessService` | Assignment-scoped filtering and `EnsureCanAccess` |
| `ICurrentUserAccessor` | Current user from request headers (dev) / JWT (future) |
| `PackageService`, `DocumentService`, … | Call `EntityAccessService` before reads/writes |

---

## Phase 2 — Authentication (planned)

| User group | Recommended approach | Rationale |
|------------|---------------------|-----------|
| Administrator, Reporting lead, Contributor | **Entra ID / Windows SSO** | Internal staff; group membership handles turnover |
| Confirmer, Pack filler | **Invitation / magic link** by email | No per-person account provisioning; one-time or short-lived token → session/JWT |

Windows-integrated sign-in with a PIN per person works on a corporate network, but PINs can be shared and onboarding is manual — unsuitable for external club users. Folio will use magic links for club roles instead.

`ICurrentUserAccessor` from phase 1 will be backed by JWT claims once auth is implemented.

# Folio — Database & Stored Procedures

Developer guide for those familiar with **Entity Framework**. Folio does not use EF — data access goes through **SQL Server stored procedures** and **Dapper** (a thin layer that maps rows to C# objects).

---

## How it differs from EF

| Entity Framework | Folio |
|------------------|--------|
| `DbContext`, `DbSet<T>` | Tables + stored procedures in SQL |
| LINQ `.Where(...)` | `EXEC Folio_Period_SELECT_ByType @ConfirmationType` |
| `SaveChanges()` | Specific SPs: `Folio_Period_INSERT`, `Folio_Package_UPDATE`, … |
| Migrations (`dotnet ef migrations`) | SQL scripts in the `database/` folder |
| Change tracking | Explicit INSERT/UPDATE in code |

**Dapper** in C# simply calls the SP and maps the result:

```csharp
var rows = await conn.QueryAsync<PeriodRow>(
    "Folio_Period_SELECT_ByType",
    new { ConfirmationType = (byte)type },
    commandType: CommandType.StoredProcedure);
```

Implementation: `src/Folio.Infrastructure/Persistence/Sql/`

---

## Prerequisites

- SQL Server or **LocalDB** (included with Visual Studio)
- SSMS (SQL Server Management Studio) or `sqlcmd`
- .NET 10 SDK

---

## Step 1 — Create the database

### Option A: BAT file (recommended — no PowerShell policy required)

Double-click or run from cmd:

```cmd
cd /d "M:\Projects\Návrh řešení\FolioApp\database"
Setup-Database.bat
```

### Option B: PowerShell (requires script execution enabled)

```powershell
Set-ExecutionPolicy -Scope CurrentUser RemoteSigned
.\Setup-Database.ps1
```

### Option C: Manually in SSMS

1. Open SSMS → connect to `localhost\SQLEXPRESS`
2. Run the files **in this order**:
   1. `000_CreateDatabase.sql`
   2. `001_Schema.sql`
   3. `002_StoredProcedures.sql`
   4. `003_SeedData.sql` — users + companies (Registry substitute)

---

## Step 2 — Configure the application

In `src/Folio.Api/appsettings.Development.json`:

```json
{
  "Persistence": {
    "Provider": "Sql"
  },
  "ConnectionStrings": {
    "Folio": "Server=localhost\\SQLEXPRESS;Database=Folio;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "DevSeedOnStartup": true
}
```

| Setting | Meaning |
|---------|---------|
| `Persistence:Provider` | `Sql` = database, `InMemory` = no DB (quick testing) |
| `ConnectionStrings:Folio` | Connection string to SQL Server |
| `DevSeedOnStartup` | On API startup, runs the seeder (see below) |

For a production SQL Server change the connection string, e.g.:

```
Server=your-server;Database=Folio;User Id=...;Password=...;TrustServerCertificate=True
```

---

## Step 3 — Run and seed data

```bash
dotnet run --project src/Folio.Api
```

### Seeding overview — what runs where

| Source | When | What it seeds | Target |
|--------|------|---------------|--------|
| **`database/003_SeedData.sql`** | Once, during `Setup-Database.ps1` | Users + companies (`DevCompanies`) | SQL tables |
| **`DevSeedData.cs`** | — | **Data definitions only** (C# constants) | Nowhere directly — source of truth for the C# seeder |
| **`DevSeedService.cs`** | API startup (`DevSeedOnStartup: true`) or `POST /api/dev/seed` | Users, companies, demo period `2026M01` | SQL or InMemory (based on `Persistence:Provider`) |
| **`DataSeeder.cs`** | *(unused)* | Old InMemory seed | Legacy — not wired up |

**Why two places (`003_SeedData.sql` + `DevSeedData.cs`)?**

- **SQL script** — populates the DB immediately at setup time, even without the app running (SSMS, `sqlcmd`).
- **C# seeder** — same data on API startup, works in **InMemory** mode too; additionally creates the **demo period** with entities, package versions and assignments (which the SQL script does not do).

User and company data is **duplicated** in both places — in SQL mode this is fine (`IF NOT EXISTS` / `AddIfNotExists`). Update both places, or consolidate into just the C# seeder over time.

The **demo period** (`2026M01`) exists **only** in `DevSeedData.DemoPeriodEntities` + `DevSeedService` logic — it is created only if the period does not yet exist in the DB.

With `DevSeedOnStartup: true` the API automatically:

1. **Seeds users** (admin, reporting lead, confirmers, pack fillers)
2. **Seeds companies** into `DevCompanies` (Registry substitute in Development)
3. **Creates the demo period** `2026M01` with 5 entities, package versions and assigned users (if it does not already exist)

### Manual seed via API

```http
POST https://localhost:7280/api/dev/seed?includeDemoPeriod=true
```

(Development environment only.)

### What does the Registry substitute do in dev?

| Environment | `IRegistryEntityClient` | Company source |
|-------------|---------------------|----------------|
| Development + Sql | `DevRegistryEntityClient` | `DevCompanies` table (SP `Folio_DevCompany_SELECT_ValidOn`) |
| Development + InMemory | `InMemoryRegistryEntityClient` | In-memory seed data |
| Production (future) | `RegistryEntityClient` | Real Registry REST API |

When **opening a period** (`POST api/periods/open`) the application:

1. Calls the Registry client → list of companies valid as of `SnapshotDate`
2. Lead selects companies → **snapshot** is saved to `PeriodEntities`
3. Period folder is created on disk (`ReportFilesPath/2026M07/...`)

---

## Table structure

```
Users                 — users and roles
DevCompanies          — company catalogue (dev only, simulates Registry)
ReportingPeriods      — periods (2026M07, 2025, …)
PeriodEntities        — company snapshot within a period
EntityPackages        — Excel package versions (Version, IsFinal, Status)
StoredDocuments       — metadata of uploaded files
EntityAssignments     — who can confirm / fill a package
AuditEntries          — audit log
```

---

## Stored procedures — overview

| Procedure | Purpose |
|-----------|---------|
| `Folio_DevCompany_SELECT_ValidOn` | Companies valid as of a given date (Registry dev) |
| `Folio_DevCompany_MERGE` | Upsert company into the dev catalogue |
| `Folio_User_SELECT_All` | List of active users |
| `Folio_User_INSERT` | New user (idempotent) |
| `Folio_Period_SELECT_ByType` | Periods by type (Transfers/Budget/Annual) |
| `Folio_Period_INSERT` | New period |
| `Folio_PeriodEntity_INSERT` | Company snapshot in a period |
| `Folio_Package_SELECT_Latest` | Latest package version |
| `Folio_Package_UPDATE` | Final / package status |
| `Folio_Document_INSERT` | File metadata |
| `Folio_Assignment_INSERT` | Assign user to an entity |
| `Folio_Audit_INSERT` | Audit entry |

---

## Adding a new table / stored procedure (instead of an EF migration)

1. Update `database/001_Schema.sql` (or create a new `004_*.sql`)
2. Add the SP to `database/002_StoredProcedures.sql`
3. Add an interface in `Folio.Application/Interfaces/Repositories/`
4. Implement it in `Folio.Infrastructure/Persistence/Sql/`
5. Register it in `DependencyInjection.cs`
6. Run the SQL script against the database
7. `dotnet build src/Folio.Api` — refreshes `contracts/contract.yaml`

---

## Verification

After seeding, in SSMS:

```sql
USE Folio;
SELECT * FROM Users;
SELECT * FROM DevCompanies;
SELECT * FROM ReportingPeriods;
SELECT pe.Name, ep.Version, ep.IsFinal, ep.Status
FROM PeriodEntities pe
JOIN EntityPackages ep ON ep.PeriodEntityId = pe.Id;
```

API:

- `GET https://localhost:7280/api/users`
- `GET https://localhost:7280/api/periods?type=1` (Transfers)
- `GET https://localhost:7280/api/periods/registry-entities?snapshotDate=2026-07-01`

---

## Seed users (Development)

| UserName | Role | Purpose |
|----------|------|---------|
| `test.testovic` | Administrator | Testing / full access |
| `folio.admin` | Administrator | Administration |
| `folio.lead` | ReportingLead | Opening periods |
| `karel.gott` | Confirmer | Signing documents |
| `jan.novak` | Confirmer | Signing documents |
| `marie.vesela` | Confirmer | Signing documents |
| `petr.horak` | PackFiller | Filling Excel packages |
| `eva.kralova` | PackFiller | Filling Excel packages |
| `folio.uploader` | Contributor | Uploading static files |

---

## Dev user switching (Web UI)

To switch between users without restarting, edit a **single key** in `src/Folio.Web/appsettings.Development.json`:

```json
"ActiveDevUser": "folio.lead"
```

All pre-configured dev users are defined in the same file under `DevUsers` (include `UserId` matching seed data). Available options:

| ActiveDevUser | Role |
|---------------|------|
| `test.testovic` | Administrator |
| `folio.lead` | ReportingLead |
| `karel.gott` | Confirmer |
| `petr.horak` | PackFiller |
| `folio.uploader` | Contributor |

---

## Tips from the EF world

- **DbContext** → `DbConnectionFactory` + repository classes
- **Navigation properties** → loaded explicitly (see `SqlPeriodEntityRepository` — packages + assignments)
- **Transactions** → `BeginTransactionAsync` in `AddRangeAsync` (period open)
- **Seed data** → `003_SeedData.sql` + C# `DevSeedService` for the demo period

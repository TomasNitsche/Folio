# Skill: Registry Integration

The "registry" is the external system of record for entity master data (Entity ID, Name, Code) — in the demo setup
these entities are ice hockey clubs. Folio only reads from it, once per period, when a period is opened (snapshot model).

## Contract

`Folio.Application.Interfaces.Integrations.IRegistryEntityClient`

```csharp
Task<IReadOnlyList<RegistryEntitySnapshot>> GetEntitiesValidOnAsync(DateOnly date, CancellationToken ct = default);
```

`Folio.Infrastructure/DependencyInjection.cs` selects the implementation via `Registry:Provider`
(`Http` | `Dev` | `InMemory`). When unset it defaults to `Dev` if `Persistence:Provider=Sql`, otherwise `InMemory`,
so fakes are the default for local development, tests and CI:

| Implementation | Provider | Source of entities |
|----------------|----------|--------------------|
| `InMemoryRegistryEntityClient` | `InMemory` | `DevSeedData.Companies` (fake hockey clubs) |
| `DevRegistryEntityClient` | `Dev` | `dbo.DevCompanies` table via stored procedure |
| `RegistryEntityClientStub` | — (not wired by default) | `FakeClubDirectory` with simulated latency — minimal reference stub |
| `HttpRegistryEntityClient` | `Http` | a real REST endpoint (see below) |

The fake entities (~30 Czech hockey clubs) live in one place: `Persistence/FakeClubDirectory.cs`. Keep it as the single
source for the dev seed, the in-memory client, the stub and the SQL seed (`database/003_SeedData.sql`).

## HttpRegistryEntityClient

- Calls `GET {BaseUrl}folio-integration/entities?asOfDate=yyyy-MM-dd` and expects
  `[{ "entityId": int, "displayValue": string, "registrationNumber": string?, "companyCode": string? }]`.
- `RegistryEntityMapper.ToSnapshot` (`Folio.Application/Mappings`) uses `companyCode` as the short display `Code`
  (falling back to `displayValue`) and carries `registrationNumber` through to entity-selection time. It is not persisted
  on `PeriodEntity` (no DB column yet). Mapping is static — no AutoMapper.
- Config section `Registry` (`RegistryOptions`): `Provider`, `BaseUrl`, `TimeoutSeconds`. `RegistryOptionsValidator` +
  `.ValidateOnStart()` fail startup when `Provider=Http` and `BaseUrl` is missing or lacks the trailing `/`
  (without it `HttpClient` drops the last path segment, e.g. `api`).
- Auth: the typed `HttpClient` uses `HttpClientHandler { UseDefaultCredentials = true }` (Windows integrated auth with the
  process identity), so no secrets live in Folio's config. Replace the handler in `DependencyInjection.cs` if your
  registry uses a different scheme.
- `.AddStandardResilienceHandler()` (`Microsoft.Extensions.Http.Resilience`) adds timeout/retry/circuit-breaker because
  the call sits in the period-open critical path.
- `HttpRegistryEntityClientTests` (`tests/Folio.Infrastructure.Tests`) pins the relative URL and the response mapping with a
  stub `HttpMessageHandler`.
- Don't set `Registry:Provider` in the shared `appsettings.json`; opt a specific environment into `Http` explicitly
  (environment-specific appsettings, user-secrets or an env var).

Do not delete the fake clients — they stay the default for local dev/tests and CI.

## Decision: REST endpoint vs MCP server

A plain REST call via `HttpClient` is the right choice. MCP servers expose tools to AI agents; they are not meant for
service-to-service data fetching inside an ASP.NET Core app. Registry lookup is a deterministic, synchronous business call
made by the Application layer during period creation.

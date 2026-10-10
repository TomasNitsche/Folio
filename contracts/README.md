# API Contract

This folder contains the **OpenAPI contract** between `Folio.Web` and `Folio.Api`.

## Files

| File | Description |
|------|-------------|
| `contract.yaml` | Primary contract file (regenerated on API build) |
| `contract.json` | Same content in JSON format |
| `Folio.Api.json` | Raw build output from `Microsoft.Extensions.ApiDescription.Server` |

## Regeneration

Build the API project:

```bash
dotnet build src/Folio.Api/Folio.Api.csproj
```

The contract is auto-generated from controller endpoints and Application DTOs.

## Note

Until a YAML serializer is added, `contract.yaml` contains OpenAPI JSON (valid for Swagger/OpenAPI tooling). Use `contract.json` interchangeably.

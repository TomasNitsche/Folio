# Prompt: Code Review Against Folio Conventions

Review the given diff/file against `AI/Skills/architecture-overview.md` and
`AI/Skills/coding-conventions.md`. Flag violations such as:

- Domain project referencing Application/Infrastructure/Api/Web.
- Application project referencing Infrastructure, Api, or Web.
- Use of AutoMapper or reflection-based mapping instead of static mappers in `Application/Mappings/`.
- New repository/service added for only one persistence provider (missing InMemory or Sql counterpart).
- Fat controllers in `Folio.Api` containing business logic that belongs in `Folio.Application`.
- Non-English UI text in `Folio.Web`.
- Missing audit log entries for state changes, uploads, or version creation.
- Hand-edited `contracts/contract.yaml` (should be auto-generated on `Folio.Api` build).
- New NuGet packages with non-permissive/non-OSS licenses.

Summarize findings as a short list with file/line references and a concrete suggested fix for each.

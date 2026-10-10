---
name: folio-test-writer
description: Writes or updates xUnit unit tests for a feature just implemented in the Folio solution. Invoked by /feature-done after a feature is implemented, or on demand for any changed file/class.
tools: Read, Write, Edit, Bash, Glob, Grep
---

You write **xUnit** unit tests for the **Folio** solution (.NET 10, Clean Architecture). You are given either an explicit diff/description of what changed, or a set of changed files/classes to cover — discover the rest yourself with `git diff` / `git status` and by reading the changed files.

## Conventions to follow (match existing tests, don't invent new patterns)

Look at `tests/Folio.Application.Tests/` first (e.g. `PackageWorkflowTests.cs`, `RolePermissionsTests.cs`, `EntityAccessServiceTests.cs`, `RoleDefinitionsTests.cs`) before writing anything:
- Plain xUnit, `[Fact]` / `[Theory]` + `[InlineData]`. No mocking framework is referenced in the test project today — don't add Moq/NSubstitute unless a dependency genuinely can't be exercised directly (e.g. an interface with real side effects); prefer constructing real objects or small in-memory fakes, consistent with the rest of the codebase's InMemory-repository pattern.
- Test classes are `public sealed class <Thing>Tests`, one class per unit under test, named after the class/module being tested with a `Tests` suffix.
- Test method names are `MethodOrBehavior_expected_outcome` in snake-ish lower-case after the first segment, matching the existing style (e.g. `CountCompleted_counts_workflow_milestones_not_raw_files`).
- Build test fixtures with small private static helper methods (e.g. `Doc(...)` in `PackageWorkflowTests.cs`) rather than repeating object-initializer boilerplate across tests.
- `GlobalUsings.cs` already has `global using Xunit;` — don't re-add that using per file.
- Only `Folio.Application.Tests` exists today, referencing `Folio.Application` (which pulls in `Folio.Domain`). If the code you're testing lives in `Folio.Infrastructure`, `Folio.Api`, or `Folio.Web`, say so explicitly in your final report instead of silently skipping it or wiring up a new test project without being asked — flag it as a gap.

## What to cover

Prioritize:
1. New/changed public methods on Application services, static mappers, and domain rule helpers (e.g. `RolePermissions`, `EntityAccessService`, package versioning/naming logic).
2. Edge cases the feature touches: permission boundaries (assigned vs unassigned entity), version/final-state transitions, period state transitions, audit log entries being written on state changes.
3. Bug-shaped cases: empty/null collections, boundary version numbers, case sensitivity on codes/names.

Don't write tests that just restate the implementation (e.g. asserting a static constant equals itself) or duplicate an existing test's coverage.

## Process

1. Read the changed source files and the closest existing test file for the same area.
2. Add tests to the existing test class file if one already covers that unit; otherwise create a new `<Thing>Tests.cs` file in `tests/Folio.Application.Tests/` following the naming/structure above.
3. Run `dotnet test tests/Folio.Application.Tests/Folio.Application.Tests.csproj` and fix any failures or compile errors before finishing.
4. Report back: which files you added/changed, a one-line description of each new test, the `dotnet test` result, and any coverage gaps you flagged (e.g. logic in a layer with no test project yet).

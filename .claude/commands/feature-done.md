---
description: Run code review and unit-test generation for a just-finished feature in Folio
argument-hint: [optional: short feature description or path/branch scope]
---

You are orchestrating the end-of-feature check for the **Folio** solution. The user just finished implementing a feature and invoked `/feature-done $ARGUMENTS`.

## 1. Scope the change

Determine what changed:
- Run `git status` and `git diff` (and `git diff <base-branch>...HEAD` if the work is already committed on a branch) to find the actual changed files.
- If `$ARGUMENTS` narrows the scope (a description, a path, a branch name), use it to focus the diff — but still verify against real `git` output, don't rely on the argument text alone.
- If there are no changes at all, tell the user there's nothing to review and stop here.

Build a short, concrete summary of the change (what files, what layers, what the feature does) — both subagents below start with no context, so you must hand them this summary plus the diff yourself.

## 2. Fan out review + tests in parallel

Launch both in a single message with two `Agent` tool calls so they run concurrently:

- `subagent_type: "folio-code-reviewer"` — prompt: the feature summary, the full diff (or `git diff` command output you already gathered), and which files/layers were touched. Ask it to review against Folio's architecture/conventions and report findings via `ReportFindings`.
- `subagent_type: "folio-test-writer"` — prompt: the same feature summary and diff, plus which classes/methods are new or changed. Ask it to add/update xUnit tests and report what it added and any coverage gaps.

Do not run these sequentially — they're independent and should overlap.

## 3. Synthesize and report back

Once both return, give the user one consolidated summary (not two separate dumps):
- Review findings, grouped by severity, each with file/line and the concrete fix — or "no findings" if the reviewer found nothing.
- Tests added/updated, with the `dotnet test` result the test-writer reported, and any flagged coverage gaps (e.g. a layer with no test project).
- If the reviewer found anything above trivial severity, or tests failed, call it out clearly at the top rather than burying it — don't let a clean-sounding summary hide a real problem.

Do not silently apply the reviewer's suggested fixes yourself — report them and let the user decide, unless the user's original request for this command explicitly asked you to also fix issues.

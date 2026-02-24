---
name: pr-description
description: Use this skill when the user asks to generate, write, or create a pull request (PR) description/body/summary from repository changes. Prefer staged changes; otherwise use unstaged changes.
---

## Purpose
Generate a GitHub Pull Request description based on the current repository changes.

## Execution (MUST FOLLOW)
1. Re-check the repository state by reading `git status` output (porcelain or equivalent).
2. Determine which changes to describe:
   - Prefer **staged** changes (Git index) if any exist.
   - Otherwise use **unstaged** working tree changes.
3. Identify the affected files from the chosen change set.
4. For each affected file, read the relevant changes (diff or file content) sufficiently to understand:
   - what changed,
   - why it likely changed,
   - which layer it belongs to (Domain/Application/Infrastructure/WebApp).
5. If no staged or unstaged changes exist, output exactly:
No changes detected.
and stop.

## Rules
- Do NOT summarize these instructions.
- Do NOT restate the rules.
- Output the PR description only.
- Do not reference already merged commits or unrelated history.
- Base the PR strictly on the current detected change set (staged or unstaged).

## Project constraints (Beagl)
- Clean Architecture layers: Domain, Application, Infrastructure, WebApp
- Expected errors use Result<T> (not exceptions)
- Exceptions only for domain invariant violations (DomainException)
- EF Core + DbContext remain in Infrastructure; no lazy loading
- Async end-to-end; no `.Result` / `.Wait()`
- Tests: xUnit + FluentAssertions
- Follow `.editorconfig`

## Output format (STRICT)
- Output the PR description only.
- Wrap the entire output inside a single fenced markdown code block.
- Use ```markdown as the fence.
- Do not add any text before or after the code block.

## Summary
...

## Motivation
...

## Changes
- ...

## Architectural Impact
- Domain: ...
- Application: ...
- Infrastructure: ...
- WebApp: ...

## Database / Migration
... (or N/A)

## Testing
- ... (or N/A)

## How to validate
- ...

## Breaking Changes
... (or None)

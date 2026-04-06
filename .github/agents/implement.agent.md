---
name: Implement
description: "Automate the implementation of a GitHub issue in the current codebase, strictly following Beagl coding guidelines, best practices, SOLID principles, Clean Architecture, and ensuring testability."
tools: [read, edit, search, execute, todo, agent, web]
model: Claude Opus 4.6 (copilot)
argument-hint: "Provide a GitHub issue number, e.g. #42"
---

# Implement Agent

## Purpose
Automates the implementation of a GitHub issue in the current codebase. Triggered by commands like `/implement #XX` or `Implement issue #XX`, it:
- Reads and summarizes the specified issue.
- Plans actionable todos for implementation.
- Executes code changes, refactors, and generates new files as needed.
- Compiles the codebase and runs all tests.
- Iterates until the implementation is complete, all tests pass, and the issue is resolved.

## Workflow
1. **Issue Intake**: Fetch and summarize the issue.
2. **Scope Alignment**: Validate the requested behavior against the first two sections of `README.md` (project description and main features), which are the source of truth for product scope.
3. **Planning**: Break down requirements into actionable todos.
4. **Implementation**: Make code changes, generate files, refactor, and update tests.
5. **Validation**: Compile and run all tests, fix errors until green.
6. **Completion**: Mark todos as completed and confirm implementation.

## Guidelines
Refer to the scoped instruction files under `.github/instructions/` for coding, architecture, testing, data access, UI, and localization standards.

Product-scope guardrails:
- Use the first two sections of `README.md` as the product-scope source of truth.
- If an issue conflicts with `README.md`, pause implementation and surface the mismatch clearly.
- Prefer implementations that directly support the documented feature domains before adding adjacent or speculative behavior.

## Relevant Instruction Files
- [architecture.instructions.md](../instructions/architecture.instructions.md)
- [async.instructions.md](../instructions/async.instructions.md)
- [error-handling.instructions.md](../instructions/error-handling.instructions.md)
- [csharp.instructions.md](../instructions/csharp.instructions.md)
- [testing.instructions.md](../instructions/testing.instructions.md)
- [efcore.instructions.md](../instructions/efcore.instructions.md) when Infrastructure persistence is involved
- [ui-design.instructions.md](../instructions/ui-design.instructions.md) for WebApp UI changes
- [localization.instructions.md](../instructions/localization.instructions.md) for localization and resource changes

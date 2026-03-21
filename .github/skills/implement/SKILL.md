---
name: implement
description: Automate the implementation of a GitHub issue in the current codebase, strictly following Beagl coding guidelines, best practices, SOLID principles, Clean Architecture, and ensuring testability.
---

# Implement Skill

## Purpose
Automates the implementation of a GitHub issue in the current codebase. Triggered by commands like `/implement #XX` or `Implement issue #XX`, it:
- Reads and summarizes the specified issue.
- Plans actionable todos for implementation.
- Executes code changes, refactors, and generates new files as needed.
- Compiles the codebase and runs all tests.
- Iterates until the implementation is complete, all tests pass, and the issue is resolved.

## Workflow
1. **Issue Intake**: Fetch and summarize the issue.
2. **Planning**: Break down requirements into actionable todos.
3. **Implementation**: Make code changes, generate files, refactor, and update tests.
4. **Validation**: Compile and run all tests, fix errors until green.
5. **Completion**: Mark todos as completed and confirm implementation.

## Guidelines
Refer to [copilot-instructions.md](../../copilot-instructions.md) for all coding, architecture, testing, and quality standards. All implementation must strictly follow these instructions.
For frontend changes, use the "UI Design System" section in that file as the single source of truth.
For localization requirements, use the "Localization (i18n)" section in that file as the single source of truth.

## Invocation
- `/implement #XX`
- `Implement issue #XX`

## File
This skill is defined in `.github/skills/implement/SKILL.md`.

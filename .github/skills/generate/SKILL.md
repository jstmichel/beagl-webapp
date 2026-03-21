---
name: generate
description: Generate a new C# file (class, record, interface, etc.) strictly following Beagl coding guidelines, best practices, SOLID principles, Clean Architecture, and ensuring testability.
---
# Generate Skill

## Purpose
Generate a new C# file (class, record, interface, etc.) strictly following Beagl coding guidelines, best practices, SOLID principles, Clean Architecture, and ensuring testability.

---

## Workflow
1. **Input:** User specifies a class, record, interface, or similar to generate, e.g. `/generate a class that does XY`.
2. **Analysis:**
   - Interpret user requirements and clarify intent.
   - Design the type to fit Beagl architecture and conventions.
3. **Generate:**
    - Create the implementation file in the correct project and folder structure based on context (e.g., Domain, Application, Infrastructure, WebApp).
    - Generate the corresponding unit test file in the proper test project and folder structure (e.g., tests/Beagl.Domain.Tests, tests/Beagl.Application.Tests).
    - Follow all coding guidelines from `.github/copilot-instructions.md` (single source of truth) for both files.
    - For UI files, follow the centralized UI design system in `.github/copilot-instructions.md` section "UI Design System".
   - For localization requirements, use `.github/copilot-instructions.md` section "Localization (i18n)" as the single source of truth.
    - Mark methods as static when they do not use instance data.
    - If a class only contains static methods and holds no state, define the class as static.
    - Unit test file:
       - Use xUnit, Moq, and FluentAssertions
       - Only unit tests (no integration tests)
       - Cover public methods and critical logic
       - Mock dependencies and external services
       - Follow Beagl testing conventions
4. **Output:**
    - Create the implementation file in the correct project and folder structure in the workspace.
    - Create the unit test file in the proper test project and folder structure in the workspace.
    - Summarize key design choices and file locations in chat.

---

## Restrictions
- Only new files are created.
- No cross-file changes, renames, or interface updates.
- No breaking changes to external contracts.
- No business logic changes unless required for testability or guideline compliance.

---

## Example Usage
- "/generate a class that does XY"
- "/generate a record for immutable DTO"
- "/generate an interface for service abstraction"

---

## Skill Metadata
name: generate
applyTo: chat command /generate
scope: file creation only
output: generated file content + summary

---

## References
- .github/copilot-instructions.md
- .editorconfig
- Beagl coding guidelines

---

## Author
Jonathan St-Michel

---

MIT License - Copyright (c) 2025 Jonathan St-Michel

---
name: refactor-file
description: Refactor a single file in context to enforce Beagl coding guidelines, SOLID, Clean Architecture, and testability. No changes to other files.
---
# Refactor File Skill


## Purpose
Refactor a single file in context to strictly follow Beagl coding guidelines, best practices, SOLID principles, Clean Architecture, and ensure testability. No changes are made to other files.

---

## Workflow
1. **Input:** User specifies a file to refactor.
2. **Analysis:**
   - Review file for violations of Beagl guidelines, SOLID, Clean Architecture, and testability issues.
   - Identify unused code, static state, poor separation of concerns, and non-testable patterns.
3. **Refactor:**
   - Apply changes only within the specified file.
    - Enforce:
       - File-scoped namespace
       - Use C# primary constructor syntax for classes unless custom logic is required in the constructor body
       - Explicit types (no `var`)
       - Remove unused `using` statements
       - Use dependency injection for services
       - No static state
       - Small, cohesive methods
       - Mark methods as static when they do not use instance data
       - If a class only contains static methods and holds no state, define the class as static.
       - PascalCase for types/members, camelCase for locals/parameters
       - XML documentation for public members
       - File header as per `.editorconfig`
       - Spaces for indentation (4 spaces)
       - Max line length: 120
       - Braces on new line
       - Nullable reference types enabled
       - No EF Core entities outside Infrastructure
       - No blocking calls, all I/O async
       - Result<T> for expected errors
       - No exceptions for validation/business errors
       - Prefer records for immutable DTOs
       - Remove UI concerns from validation
       - Structured logging with `ILogger<T>`
   - Make the file testable:
     - Decouple dependencies
     - Use interfaces for external services
     - Avoid static methods/state
     - Ensure public methods are unit-testable
4. **Output:**
   - If refactor is needed: return the refactored file content and summarize key changes and improvements.
   - If no refactor is needed: output only a statement such as "The file is already conform to Beagl guidelines and requires no changes."

---

## Restrictions
- Only the file in context is modified.
- No cross-file changes, renames, or interface updates.
- No breaking changes to external contracts.
- No business logic changes unless required for testability or guideline compliance.

---

## Example Usage
- "Refactor [file] to follow Beagl guidelines and make it testable."
- "Apply SOLID and Clean Architecture to this file only."

---

## Skill Metadata
name: refactor-file
applyTo: single file in context
scope: file-level only
output: refactored file content + summary

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

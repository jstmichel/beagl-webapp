---
applyTo: "**/*.cs"
description: "C# conventions, code quality expectations, and logging rules for Beagl."
---

# C# Instructions

Apply these conventions to all C# files.

## Language and Style

- Do not suppress nullable warnings without reason.
- Always use file-scoped namespace declarations.
- Always use braces (`{}`) for all control flow statements (`if`, `else`, `for`, `foreach`, `while`, `do`), even for single-line bodies.
- Always use C# primary constructor syntax for classes unless custom logic is required in the constructor body.
- Always use explicit types instead of `var` for all variable declarations.
- When using explicit types, simplify variable creation using `new()` where valid.
- Prefer collection expressions over explicit collection initializers when practical, for example `[..items]` instead of `items.ToArray()` when it improves clarity.
- Use explicit access modifiers on all types and members, including interface members.
- Remove unused `using` statements.
- Prefer records for immutable DTOs.
- Use PascalCase for types and members.
- Use camelCase for locals and parameters.
- Prefix all private fields, including `static readonly`, with `_` and use camelCase.
- Prefix interfaces with `I`.
- Mark methods as `static` when they do not use instance data.
- If a class only contains static methods and holds no state, define the class as `static`.

## Required File Header

- Add the file header defined in `.editorconfig` to generated C# files:
  - `MIT License - Copyright (c) 2025 Jonathan St-Michel`

## Logging

- Use `ILogger<T>` for logging.
- Use structured logging.
- Do not log sensitive information.

## Testability by Design

- Design code to be unit-testable by default.
- Depend on abstractions for external concerns (persistence, time, random, network, file system).
- Inject collaborators through constructors instead of creating them directly in business logic.
- Keep validation and decision logic in methods with clear inputs/outputs and minimal side effects.
- When behavior is added or changed, add or update unit tests in the same change.
- Do not automatically create integration tests; add them only when explicitly requested.

## Code Coverage Exclusions

- Use `[ExcludeFromCodeCoverage]` (from `System.Diagnostics.CodeAnalysis`) to exclude types or members from coverage — do not rely on coverlet file-pattern exclusions for this purpose.
- Apply `[ExcludeFromCodeCoverage]` at the **class level** for:
  - EF Core entity POCO classes (no behavior to test)
  - `DbContext` subclasses (schema config only; requires a real database)
  - Static constants-only classes (no executable logic)
  - Infrastructure bootstrap helpers (composition root wiring)
- Apply `[ExcludeFromCodeCoverage]` at the **method level** for individual methods that are infrastructure glue with no testable logic.
- For `Program.cs` (top-level statements), use the assembly-level form: `[assembly: ExcludeFromCodeCoverage]` placed after the `using` directives.
- Keep coverlet `.runsettings` `ExcludeByFile` patterns limited to generated files only: migrations, model snapshots, and Razor-generated files (`*.g.cs`, `*.b.cs`).

## Quality Bar

- Follow `.editorconfig` strictly.
- All analyzer warnings are treated as errors.
- Use spaces for indentation.
- Max line length is 120.
- Braces go on new lines.
- XML documentation is required for public members.
- Code must be written in English, including identifiers, comments, and documentation.

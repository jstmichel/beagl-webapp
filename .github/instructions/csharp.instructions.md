---
applyTo: "**/*.cs"
description: "C# conventions, code quality expectations, and logging rules for Beagl."
---

# C# Instructions

Apply these conventions to all C# files.

## Language and Style

- Do not suppress nullable warnings without reason.
- Always use file-scoped namespace declarations.
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

## Quality Bar

- Follow `.editorconfig` strictly.
- All analyzer warnings are treated as errors.
- Use spaces for indentation.
- Max line length is 120.
- Braces go on new lines.
- XML documentation is required for public members.
- Code must be written in English, including identifiers, comments, and documentation.

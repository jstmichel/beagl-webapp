---
name: xunit-tests
description: Generate xUnit + FluentAssertions unit tests for selected C# code (preferred) or for new/changed behavior inferred from the staged diff.
---

## Goal
Generate unit tests that match repository standards.

## Input priority
1) Selected code in the editor (preferred)
2) Staged git diff (infer what needs tests)
If neither exists: output exactly `No code selected or staged changes detected.` and stop.

## Standards
- Use xUnit + FluentAssertions
- Respect Result<T> pattern for expected failures (assert success/failure and payload/errors)
- Domain tests must not depend on Infrastructure
- Application tests must mock repositories/external services
- Avoid testing implementation details; focus on behavior

## Relevant Instruction Files
- `.github/instructions/error-handling.instructions.md`
- `.github/instructions/csharp.instructions.md`
- `.github/instructions/testing.instructions.md`

## Coverage requirements
- Happy path
- Edge cases
- Failure cases

## Rules
- Output must be test code only. Do not summarize instructions or explain reasoning.
- Include only necessary `using` directives.
- Use clear test method names (underscores allowed).
- Assume nullable reference types are enabled.

## Output format
- Provide complete test classes ready to paste.
- If multiple files are appropriate, output separate code blocks with headings indicating filenames.

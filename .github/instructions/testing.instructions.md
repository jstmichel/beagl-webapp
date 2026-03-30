---
applyTo: "tests/**/*.cs"
description: "Testing framework, coverage expectations, and test-layer rules for Beagl."
---

# Testing Instructions

Apply these rules to all test code.

- Use xUnit and FluentAssertions.
- Add unit tests for all critical logic.
- Any generated or modified production behavior must include corresponding unit tests in the same change.
- Cover happy path, edge cases, and failure cases.
- Domain tests must not depend on Infrastructure.
- Application tests must mock repositories and external services.
- Mirror the source structure under `tests/`.
- Keep tests behavior-focused rather than implementation-focused.
- Prefer unit tests by default. Do not add integration tests, UI tests, or end-to-end tests automatically.
- Add integration tests only when explicitly requested by the developer.
- All test assemblies are excluded from code coverage via `[assembly: ExcludeFromCodeCoverage]` in `tests/Directory.Build.props`. Do not add per-class `[ExcludeFromCodeCoverage]` attributes in test code.

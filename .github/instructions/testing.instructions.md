---
applyTo: "tests/**/*.cs"
description: "Testing framework, coverage expectations, and test-layer rules for Beagl."
---

# Testing Instructions

Apply these rules to all test code.

- Use xUnit and FluentAssertions.
- Add unit tests for all critical logic.
- Cover happy path, edge cases, and failure cases.
- Domain tests must not depend on Infrastructure.
- Application tests must mock repositories and external services.
- Mirror the source structure under `tests/`.
- Keep tests behavior-focused rather than implementation-focused.

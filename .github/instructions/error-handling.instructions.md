---
applyTo: "src/**/*.cs"
description: "Result pattern, validation ownership, and exception rules for Beagl source code."
---

# Error Handling Instructions

## Expected Errors

- Use the `Result<T>` pattern for expected errors in all layers.
- Do not use exceptions for validation or business errors.
- Only catch exceptions when explicitly handling them.
- Use custom exception types when relevant.

## Domain Exceptions

- Only throw exceptions in Domain aggregates, entities, and value objects for invariant violations.
- Domain exceptions must inherit from `DomainException`.

## Validation Ownership

- Input validation belongs in Application.
- Domain enforces invariants only.
- Do not mix validation logic with UI concerns.

## Localization Compatibility

- Backend failures should expose stable error codes so the UI can localize by code.

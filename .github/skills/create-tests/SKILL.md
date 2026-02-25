---
name: create-tests
description: Scans the codebase to identify critical logic, business rules, and public methods/classes that lack unit tests, following Beagl's Clean Architecture and testing guidelines, and generates xUnit + FluentAssertions test skeletons for them.
---

# Agent Skill: Unit Test Suggestions

## Overview
This skill launches an agent that scans the entire codebase and suggests what should be unit tested. It identifies critical logic, business rules, and public methods/classes that are candidates for unit testing, following Beagl's Clean Architecture and testing guidelines.

## Usage
- Invoke this skill to receive a prioritized list of code elements (classes, methods, services, etc.) that should be covered by unit tests.
- The agent will:
  - Analyze all layers (Domain, Application, Infrastructure, WebApp)
  - Exclude auto-generated, trivial, or UI-only code
  - Focus on business logic, domain entities, application services, and repository interfaces
  - Suggest test types: happy path, edge cases, failure cases
  - Reference files and line numbers for each suggestion

## Output
- Returns a Markdown report listing:
  - Symbol name (class/method/service)
  - File and line reference
  - Reason for testing (business logic, validation, error handling, etc.)
  - Suggested test types

## Implementation
- Use search_subagent to explore the workspace for public classes, methods, and business logic
- Apply Beagl's testing guidelines (see .github/copilot-instructions.md)
- Exclude Infrastructure implementations and WebApp UI code from unit test suggestions
- Prioritize Domain and Application layers
- For each suggestion, automatically generate xUnit + FluentAssertions unit test skeletons
- Place generated tests in the appropriate test project and folder, mirroring the source structure:
  - Domain → tests/Beagl.Domain.Tests/
  - Application → tests/Beagl.Application.Tests/
  - Infrastructure → tests/Beagl.Infrastructure.Tests/
  - WebApp → tests/Beagl.WebApp.Tests/
- Create new test files if needed, or append to existing files
- Use file-scoped namespace declarations matching the folder structure (e.g., tests/Beagl.Infrastructure.Tests/Users/Entities → namespace Beagl.Infrastructure.Tests.Users.Entities;)
- Use explicit types instead of var for all variable declarations
- Add the file header as defined in .editorconfig to every generated file: MIT License - Copyright (c) 2025 Jonathan St-Michel
- Ensure proper namespace and class naming conventions
- Do not overwrite existing tests unless explicitly requested

## Example Output
```
### Unit Test Suggestions

- [AnimalAggregate](src/Beagl.Domain/AnimalAggregate.cs#L10)
  - Reason: Domain business rules and invariants
  - Suggested tests: Happy path, edge cases, invariant violations

- [CreateAnimalHandler](src/Beagl.Application/CreateAnimalHandler.cs#L15)
  - Reason: Application service logic and input validation
  - Suggested tests: Valid input, invalid input, repository errors

- [IAnimalRepository](src/Beagl.Domain/IAnimalRepository.cs#L5)
  - Reason: Repository interface contract
  - Suggested tests: Mocked repository, failure cases
```

## Skill File Location
- Place this file at `.github/skills/unit-test-suggestions/SKILL.md`

## Author
GitHub Copilot

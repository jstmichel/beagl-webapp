# Beagl – Copilot Instructions

Beagl-WebApp is an open source application for animal centers.

Use the first two sections of `README.md` (project description and main features) as the source of truth for product scope. If this file and `README.md` differ, follow `README.md`.

This file is the always-on orientation document for the repository. Keep it short and stable. Put detailed guidance in scoped files under `.github/instructions/` so the AI receives only the rules relevant to the current task.

---

# Project Overview

- Product purpose: open source application for animal centers that centralizes operations for animals, owners, employees, adoption, payments, and reporting.
- Main functional domains:
    - Employee and profile management
    - Owner and animal tracking
    - Adoption workflow
    - Owner self-service portal, including annual fee payment
    - Reporting and statistics
    - Authentication and role management
    - Centralized data management and integrations
- Primary stack: C# / .NET 10
- Architecture style: Clean Architecture
- Main projects:
    - `src/Beagl.Domain`
    - `src/Beagl.Application`
    - `src/Beagl.Infrastructure`
    - `src/Beagl.WebApp`
- Tests mirror the source structure under `tests/`
- Repository layout should continue to include `README.md`, `src/`, `tests/`, and required configuration files.

---

# Instruction Map

Use these scoped instruction files as the source of truth for detailed guidance:

- `.github/instructions/architecture.instructions.md`
    - Clean Architecture boundaries, layering rules, repository placement, project structure
- `.github/instructions/async.instructions.md`
    - Async flow, cancellation, non-blocking rules
- `.github/instructions/error-handling.instructions.md`
    - `Result<T>`, validation ownership, exception rules, error-code expectations
- `.github/instructions/csharp.instructions.md`
    - C# conventions, code quality, file headers, logging, static guidance
- `.github/instructions/efcore.instructions.md`
    - EF Core and Infrastructure data access rules
- `.github/instructions/testing.instructions.md`
    - xUnit, FluentAssertions, test coverage expectations, test-layer boundaries
- `.github/instructions/ui-design.instructions.md`
    - Brand tokens, CRUD UI patterns, detail panel structure, accessibility expectations, and Blazor versus Razor Pages selection guidance for the WebApp
- `.github/instructions/localization.instructions.md`
    - i18n rules, resource organization, bilingual requirements, localization key patterns

---

# Universal Rules

- Follow `.editorconfig` strictly.
- Keep changes minimal, focused, and consistent with the existing codebase.
- Code must be written in English, including identifiers, comments, and documentation.
- Repository changes should respect existing project boundaries and folder structure.

---

# Commit Messages

- Use Conventional Commits in lowercase.
- Use a single header line.
- Keep the title under 72 characters.
- Add bullet points in the body for grouped changes when useful.
- Add `BREAKING CHANGE:` in the body if applicable.

---

# Skill Authoring Guidance

- Skills should reference only the smallest relevant instruction files instead of treating this file as a monolith.
- Put reusable domain knowledge in scoped instruction files, not duplicated across many skills.
- Keep skill files focused on workflow and trigger behavior; keep coding standards in `.github/instructions/`.

# Beagl – Copilot Instructions (C# / .NET 10)

Beagl is a modern CRM for animal centers.

This file is the single source of truth for coding guidelines used by skills.

---

# 1. Architecture

- Follow Clean Architecture with strict layer separation:
  - Domain
  - Application
  - Infrastructure
  - WebApp
- Respect SOLID principles.
- Prefer composition over inheritance.
- Avoid static state.
- Use dependency injection for all services.

## Layer Boundaries

- Domain must not reference Infrastructure or WebApp.
- Application must not reference WebApp.
- Infrastructure may reference Domain and Application.
- Mapping between layers must occur in Application.
- Do not expose EF Core entities outside Infrastructure.
- Repository interfaces live in Domain.
- Repository implementations live in Infrastructure.

---

# 2. Async and Concurrency

- All I/O operations must be asynchronous.
- Do not use `.Result`, `.Wait()`, or blocking calls.
- Avoid `async void`.
- Use `CancellationToken` in application services and handlers.
- Async must flow from WebApp down to Infrastructure.

---

# 3. Error Handling

- Use the `Result<T>` pattern for expected errors in all layers.
- Do not use exceptions for validation or business errors.
- Only throw exceptions in Domain aggregates/entities/value objects for invariant violations.
- Domain exceptions must inherit from `DomainException`.
- Only catch exceptions when explicitly handling them.
- Use custom exception types when relevant.

---

# 4. Validation

- Input validation belongs in Application layer.
- Domain enforces invariants only.
- Do not mix validation logic with UI concerns.

---

# 5. Entity Framework Core

- DbContext must remain in Infrastructure.
- Use Fluent API for configuration.
- Do not use lazy loading.
- Use `AsNoTracking()` for read-only queries.
- Do not leak DbContext outside Infrastructure.
- Keep queries efficient and explicit.

---



# 6. C# Conventions

- Do not suppress nullable warnings without reason.
- Always use file-scoped namespace declarations for all C# files (not block-scoped).
- Always use C# primary constructor syntax for classes unless custom logic is required in the constructor body.
- Always use explicit types instead of `var` for all variable declarations.
- When using explicit types, simplify variable creation using the new type() syntax (IDE0090), e.g., `MyType myVar = new();`.
- Prefer collection expressions over explicit collection initializers (IDE0300–IDE0305), e.g., `[..items]` instead of `items.ToArray()`.
- Add the file header as defined in `.editorconfig` to every generated file:
  - `MIT License - Copyright (c) 2025 Jonathan St-Michel`
- Use explicit access modifiers on all types and members, including interface members (IDE0040).
- Remove unused `using` statements.
- Prefer records for immutable DTOs.
- Use PascalCase for types/members.
- Use camelCase for locals/parameters.
- Prefix all private fields (including `static readonly`) with `_` and use camelCase (IDE1006), e.g., `_myField`.
- Prefix interfaces with `I`.
- Keep methods small and cohesive.
- Limit file and function size.

---

# 7. Logging

- Use `ILogger<T>` for logging.
- Use structured logging.
- Do not log sensitive information.

---

# 8. Testing

- Use xUnit + FluentAssertions.
- Add unit tests for all critical logic.
- Cover:
  - Happy path
  - Edge cases
  - Failure cases
- Domain tests must not depend on Infrastructure.
- Application tests must mock repositories.

---

# 9. Code Quality

- Follow `.editorconfig` strictly.
- All analyzer warnings are treated as errors.
- Use spaces for indentation (4 spaces per `.editorconfig`).
- Max line length: 120.
- Braces on new line.
- XML documentation required for public members.
- Code must be written in English (identifiers, comments, documentation).

---

# 10. Folder Structure

Projects:

- `src/Beagl.WebApp`
- `src/Beagl.Infrastructure`
- `src/Beagl.Application`
- `src/Beagl.Domain`

Tests mirror source structure under `tests/`.

Repositories must include:
- `README.md`
- `src/`
- `tests/`
- Required config files

---

# 11. Commit Messages

- Use Conventional Commits (lowercase).
- Single header per commit.
- Keep title under 72 characters.
- Summarize related changes in bullet points in body.
- Add `BREAKING CHANGE:` in body if applicable.

---

# 12. UI Design System

- UI/UX guidance is centralized in this file and is the single source of truth for frontend decisions.
- Use the official Beagl logo asset at `src/Beagl.WebApp/wwwroot/images/beagl_logo.png` for brand marks in the UI.
- Primary brand color is RGB(78, 170, 171) (`#4EAAAB`) and must be represented as the main design token for accents and primary actions.
- Build interfaces with a clear visual direction and reusable design tokens.
- Define and use CSS variables for color, spacing, radii, shadows, and motion timings.
- Prefer expressive type stacks and intentional hierarchy; avoid default-looking layouts.
- Keep accessibility first: color contrast, focus states, semantic structure, and keyboard navigation.
- Use responsive layouts by default with mobile-first breakpoints.
- Use meaningful motion (page entrance, staggered reveal, panel transitions) rather than excessive micro-animations.
- Preserve existing visual language when updating an established surface; only apply broad redesign when explicitly requested.
- Keep style logic centralized (global stylesheet/design tokens) and avoid scattered one-off inline styles.
- For data-heavy CRUD screens, avoid permanent narrow side panes and avoid long inline bottom detail editors; prefer modal dialogs for quick create/edit/details and dedicated routes/pages for complex or long forms.
- For list row actions in CRUD tables, prefer icon-only, borderless action buttons; keep them accessible with explicit `aria-label` and `title` attributes.
- For icon-only action buttons, use the primary brand color for neutral actions and a red destructive variant for delete actions.
- In CRUD dialogs, avoid duplicate dismissal controls: do not show both a top close button and a footer cancel/close button at the same time; keep a single secondary dismissal action in the footer.

---

# 13. Localization (i18n)

- All backend errors and UI text (excluding database content) must be localization-ready.
- Always provide English (default) and French resource entries for every user-facing string.
- Backend/domain/application failures must expose stable error codes (e.g., `users.invalid_email`) so UI can localize by code.
- UI should resolve backend error codes through localization resources, with backend message as fallback only.
- The app must select language from browser preferences via request localization with supported cultures `en` and `fr`.
- Keep localized text centralized in resource files organized by concern (e.g., layout, home, users, errors) rather than one large shared file; avoid hardcoded user-facing strings in components/pages/services.

## Localization Resource Convention

- Use concern-specific marker classes in `src/Beagl.WebApp/Resources/` (e.g., `LayoutResource`, `HomeResource`, `UsersResource`, `ErrorResource`).
- Use `IStringLocalizer<TConcernResource>` in each page/component/service, matching the concern of that file.
- Resource file naming must follow this pattern:
  - Default culture: `Resources.{Concern}Resource.resx`
  - French culture: `Resources.{Concern}Resource.fr.resx`
- Add new keys to the smallest relevant concern file; do not place all keys in a global shared file.
- If a string changes in one language, update the corresponding key in both English and French in the same change.
- Keep backend error code keys (e.g., `users.invalid_email`) in the users/domain concern resources used by the UI.
- French translations must use proper French orthography with accents (e.g., `é`, `à`, `è`, `ç`) in UTF-8 resource files; do not strip accents.

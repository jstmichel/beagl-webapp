# Beagl – Copilot Instructions (C# / .NET 10)

Beagl is a modern CRM for animal centers.

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

- Enable nullable reference types.
- Do not suppress nullable warnings without reason.
- Use file-scoped namespace declarations.
- Use explicit access modifiers.
- Remove unused `using` statements.
- Prefer records for immutable DTOs.
- Use PascalCase for types/members.
- Use camelCase for locals/parameters.
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

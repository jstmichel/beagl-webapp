---
applyTo: "src/Beagl.Infrastructure/**/*.cs"
description: "EF Core and database access rules for Beagl infrastructure code."
---

# EF Core Instructions

Apply these rules to Infrastructure data access code.

- `DbContext` must remain in Infrastructure.
- Prefer primary constructors for `DbContext` and similar infrastructure classes unless constructor logic is required.
- Use Fluent API for configuration.
- Do not use lazy loading.
- Use `AsNoTracking()` for read-only queries.
- Do not leak `DbContext` outside Infrastructure.
- Keep queries efficient and explicit.

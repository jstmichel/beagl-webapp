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

## Migrations

- Generate an EF Core migration whenever `DbSet` properties, entity types, or Fluent API configurations are added or changed.
- Run: `dotnet ef migrations add <Name> --project src/Beagl.Infrastructure --startup-project src/Beagl.WebApp`
- Use descriptive PascalCase names (e.g. `AddBreeds`, `AddOwnerPayments`).
- Review the generated `Up` and `Down` methods for correctness before committing.
- Never skip migration generation when schema changes are present.
- After generating a migration, add an `[ExcludeFromCodeCoverage] partial class <MigrationName>` entry to `src/Beagl.Infrastructure/Migrations/MigrationCoverageExclusions.g.cs` to exclude the generated migration code from coverage.

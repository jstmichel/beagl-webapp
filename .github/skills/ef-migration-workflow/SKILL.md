---
name: ef-migration-workflow
description: "Generate and configure an EF Core migration for Beagl. Use when adding or modifying DbSet properties, entity configurations, or Fluent API mappings in ApplicationDbContext. Covers command, migration naming, Up/Down review, and coverage exclusion."
---

# EF Core Migration Workflow — Beagl

## When to Use

Apply when the task adds or modifies `DbSet` properties, entity configurations, or Fluent API mappings. Skip when no schema changes are involved.

See `.github/instructions/efcore.instructions.md` for the full EF Core data access rules.

## Procedure

1. Implement all entity classes, repository classes, and `DbContext` changes first.
2. Run the migration command from the workspace root:
   ```
   dotnet ef migrations add <MigrationName> --project src/Beagl.Infrastructure --startup-project src/Beagl.WebApp
   ```
3. Use a descriptive PascalCase name (e.g. `AddBreeds`, `AddOwnerPayments`).
4. Review the generated `Up`/`Down` methods for correctness.
5. If the migration is empty or incorrect, delete it and diagnose the `DbContext` configuration.
6. Exclude the migration from coverage — add to `src/Beagl.Infrastructure/Migrations/MigrationCoverageExclusions.g.cs`:
   ```csharp
   [ExcludeFromCodeCoverage] partial class <MigrationName> { }
   ```

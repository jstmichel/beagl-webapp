---
name: new-migration
description: Create a new Entity Framework Core migration in the workspace using the dotnet CLI, following Beagl coding guidelines and project conventions.
---
# New Migration Skill

## Purpose
Create a new Entity Framework Core migration in the workspace using the dotnet CLI, following Beagl coding guidelines and project conventions.

## Relevant Instruction Files
- `.github/instructions/architecture.instructions.md`
- `.github/instructions/csharp.instructions.md`
- `.github/instructions/efcore.instructions.md`

---

## Workflow
1. **Input:** User specifies a migration name, e.g. `/new-migration AddUserTable` or "create a new migration named AddUserTable".
2. **Analysis:**
   - Check if there are any pending model changes requiring a migration.
   - If no migration is needed, output only "No migration needed."
3. **Migration Creation:**
    - Execute the migration command:
       dotnet ef migrations add <migration-name> --project src/Beagl.Infrastructure --startup-project src/Beagl.WebApp
       as documented in README.md.
   - Follow migration documentation and conventions from Beagl .md files.
   - Ensure migration files are created in the proper folder structure.
   - Summarize the migration result and file locations in chat.

---

## Restrictions
- Only creates new migrations.
- No cross-file changes, renames, or interface updates.
- No breaking changes to external contracts.
- No business logic changes unless required for migration.

---

## Example Usage
- "/new-migration AddUserTable"
- "create a new migration named AddUserTable"

---

## Skill Metadata
name: new-migration
description: Create a new Entity Framework Core migration in the workspace using the dotnet CLI, following Beagl coding guidelines and project conventions.

---

## References
- .github/instructions/architecture.instructions.md
- .github/instructions/csharp.instructions.md
- .github/instructions/efcore.instructions.md
- .editorconfig
- Beagl migration documentation

---

## Author
Jonathan St-Michel

---

MIT License - Copyright (c) 2025 Jonathan St-Michel

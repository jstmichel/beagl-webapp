---
name: design
description: Generate PlantUML diagram code in chat for a user-provided description, following Beagl guidelines for clarity and structure.
---
# Design Skill

## Purpose
Generate PlantUML diagram code in chat for a user-provided description, following Beagl guidelines for clarity and structure.

## Relevant Instruction Files
- `.github/instructions/architecture.instructions.md`

---

## Workflow
1. **Input:** User specifies a diagram description, e.g. `/design a diagram for user authentication flow` or "design a diagram <description>".
2. **Analysis:**
   - Interpret the description and clarify the diagram type (class, sequence, activity, etc.).
   - Design the diagram to fit Beagl architecture and conventions.
3. **Generate:**
   - Output PlantUML code in chat for the requested diagram.
   - Ensure clarity, proper naming, and structure.
4. **Output:**
   - Return PlantUML code in chat and summarize key design choices.

---

## Restrictions
- Only outputs PlantUML code in chat.
- No file creation or workspace changes.
- No breaking changes to external contracts.
- No business logic changes unless required for diagram clarity.

---

## Example Usage
- "/design a diagram for user authentication flow"
- "design a diagram for animal adoption workflow"

---

## Skill Metadata
name: design
applyTo: chat command /design
scope: diagram generation only
output: PlantUML code in chat + summary

---

## References
- `.github/instructions/architecture.instructions.md`

---

## Author
Jonathan St-Michel

---

MIT License - Copyright (c) 2025 Jonathan St-Michel

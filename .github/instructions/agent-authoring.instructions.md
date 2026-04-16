---
applyTo: ".github/**/*.md"
description: "Skill, agent, and prompt authoring guidance for Beagl. Applies when editing any customization file under .github/."
---

# Agent Authoring Instructions

## Instruction Architecture

- `copilot-instructions.md` is the always-on orientation document. Keep it short and stable — only project identity, domain map, and universal rules.
- Put detailed coding rules in scoped files under `.github/instructions/` with targeted `applyTo` patterns.
- Skills carry portable workflow knowledge that is reusable across projects. Keep coding standards in `.github/instructions/`, not duplicated in skills.
- Agents and prompts reference skills for on-demand workflows. They do not duplicate the skill content inline.

## Skill Reference Pattern

When an agent or prompt relies on a workflow defined in a skill, reference it by name rather than repeating the steps:

```
Apply the `skill-name` skill.
```

When a project instruction file covers the same content, the project file takes precedence. Reference the skill as a fallback for portability:

```
Follow `.github/instructions/efcore.instructions.md` when present; otherwise apply the `ef-migration-workflow` skill.
```

## Agent Design Rules

- Agent `description` must be self-contained and discovery-friendly. Use "Use when:" trigger phrases.
- Do not write "Called by X only" — any agent should be directly invocable.
- Each agent's "Instruction Source of Truth" section must use dynamic discovery (`all files under .github/instructions/`), not a hardcoded file list.
- Always include a portable fallback in "Instruction Source of Truth" that references skills rather than prose.

## Prompt Design Rules

- Prompts should be a single focused trigger. Keep the body to one or two sentences.
- Use `mode:` to target the right agent or subsystem directly.
- Do not embed workflow logic in prompts — that belongs in the agent or skill.

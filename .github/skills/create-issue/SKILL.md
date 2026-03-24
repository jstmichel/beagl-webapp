---
name: create-issue
description: Use this skill when the user asks to create, draft, or write a GitHub issue for a new feature. This skill is interactive: it asks one intake question at a time, maintains a visible todo list, then outputs separate copyable Markdown sections.
---

## Goal
Create a GitHub issue draft with three separate copyable sections:
- title
- description
- suggested labels

The description must include acceptance criteria that are concrete enough to guide implementation with minimal ambiguity.

## Trigger
- Use this skill when the user writes `/create-issue`.
- Use this skill when the user asks to create, draft, or write a GitHub issue for a feature or enhancement.
- When triggered, run a structured intake, then generate the issue content.

## Workflow
This skill is a guided, multi-turn workflow.

### Phase 1: Intake
- Ask questions one by one.
- Show the full intake todo list before or alongside the current question so the user can see what remains.
- Mark completed items clearly as answers are collected.
- Ask only one unanswered question per turn.
- If the user already provided information, mark that item complete and skip asking it again.

Use this intake todo list:
1. Feature summary
2. Problem or value
3. Scope and expected behavior
4. Acceptance criteria
5. Out of scope
6. Affected area

### Phase 2: Draft
- Rewrite the collected answers into a clean, user-friendly GitHub issue draft.
- Reformulate rough answers into concise, structured language that is easier for both humans and AI agents to read.
- Preserve the user's intent. Improve clarity, not meaning.

### Phase 3: Review
- Allow the user to refine the generated title, description, or labels.
- If the user changes one answer, regenerate the affected sections consistently.

## Intake Rules
- Questions must follow best practices: short, specific, and unambiguous.
- Ask for missing information, not broad essays.
- Prefer concrete prompts about outcome, scope, and constraints.
- Prefer concrete prompts that help derive testable acceptance criteria.
- When choices are helpful, present them as square-bracket options, for example:
  - [1] Web UI
  - [2] Application logic
  - [3] Domain model
  - [4] Infrastructure or database
  - [5] Other
- When choices are provided, the user may answer with the number only.
- After each user answer, briefly normalize it into clearer wording internally and use that normalized form in the final issue draft.
- Do not dump all questions at once.
- If the user gives a broad or vague answer, ask a narrow follow-up question instead of inventing important implementation details.

## Question Order
Use this default order unless the user already answered some parts:

### 1. Feature summary
Ask for the feature in one sentence.

Example:
What feature should this issue describe?

### 2. Problem or value
Ask what problem it solves or what value it adds.

Example:
Why is this needed, and who benefits from it?

### 3. Scope and expected behavior
Ask for the expected behavior and key requirements.

Example:
What should happen when this feature is implemented?

### 4. Acceptance criteria
Ask for observable, verifiable outcomes.

Example:
What must be true for this issue to be considered done?

Prefer concise, testable statements such as UI behavior, defaults, validation, persistence, filtering logic, and empty states.

### 5. Out of scope
Ask what should explicitly not be included in this issue.

Example:
What is explicitly out of scope for this issue?

Prefer non-goals and exclusions over vague notes. If there are still important unresolved points, capture them briefly only when they materially affect implementation.

### 6. Affected area
Ask the user which part of the product is mainly affected.

Prefer choices when useful:
- [1] Web UI / Blazor
- [2] Application layer
- [3] Domain layer
- [4] Infrastructure / database
- [5] Multiple areas
- [6] Not sure

## Output Format
After intake, output exactly three sections, each in its own Markdown code block so the user can copy them independently.

### Section 1: Title
Use a short, specific issue title.

```text
<generated title>
```

### Section 2: Description
Use clear GitHub issue Markdown.

```markdown
## Summary

<Short, clear summary of the feature request>

## Problem

<Why this is needed and what pain point or opportunity it addresses>

## Proposed Scope

- <Requirement or expected behavior>
- <Requirement or expected behavior>

## Acceptance Criteria

- <Testable expected outcome>
- <Testable expected outcome>

## Out of Scope

- <Explicit non-goal or exclusion>

## Notes

- <Constraint, dependency, or open question>
```

### Section 3: Labels
Suggest only labels that exist in the repository.
Use the repository's available labels as the source of truth.
Recommend only labels that clearly match the issue.
Prefer a small, high-signal set.

```text
enhancement
layer:blazor
tests
```

## Label Guidance
- Use existing repository labels only.
- Prefer `enhancement` for new features unless the request is actually a refactor, documentation change, chore, or bug.
- Suggest one layer label when the affected area is clear:
  - `layer:blazor`
  - `layer:application`
  - `layer:domain`
  - `layer:infrastructure`
- Add other labels only when justified, such as:
  - `database`
  - `design`
  - `documentation`
  - `refactor`
  - `tests`
- Do not invent labels.

## Critical Behavior
- During intake, do not generate the final issue sections yet.
- During intake, each response to the user must include:
  - the current todo list status
  - one current question only
- When enough information is collected, generate the three final sections.
- The final wording must be cleaner and more structured than the raw user answers.
- Keep the output practical and ready to paste into GitHub.
- Do not omit acceptance criteria in the final description.
- If the user does not provide enough detail for meaningful acceptance criteria, ask follow-up questions before generating the final draft.
- Prefer a dedicated `Out of Scope` section for non-goals instead of mixing them into notes.

## Title rules
- Generate the title from the collected intent, problem, and scope.
- Keep it short, clear, and specific.
- Prefer outcome-oriented titles.
- Avoid vague titles like "Improve feature" or "Update app".

## Description Rules
- Use short sections and bullets.
- Avoid repetition.
- Avoid speculative implementation details unless the user asked for them.
- Distinguish clearly between required behavior and open questions.
- Write in a way that a teammate or AI coding agent can act on without re-reading the full chat history.
- Acceptance criteria must be observable and testable.
- Prefer behavior-focused criteria over technical implementation instructions.
- Include defaults, state changes, edge cases, and success conditions when relevant.
- Use `Out of Scope` for explicit exclusions and non-goals.
- Use `Notes` only for constraints, dependencies, assumptions, or open questions that still matter.

## Interaction Style
- Be concise.
- Be structured.
- Reformulate user input into clearer language as the draft is built.
- If an answer is ambiguous, ask a narrow follow-up question before moving on.

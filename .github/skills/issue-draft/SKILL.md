---
name: issue-draft
description: Use this skill when the user asks to create, draft, or write a GitHub issue. This skill is interactive: it asks questions first, then outputs a final Markdown issue.
---

## Goal
Create a GitHub issue draft (title + description + acceptance criteria).

## Critical behavior (MUST)
- This is a two-phase interaction:
  1) Ask questions (intake)
  2) Generate the final issue Markdown
- DO NOT generate any issue markdown in phase 1.
- In phase 1, output ONLY the questions.
- After the user answers, generate the final issue.

## Phase 1: Intake questions (ask in this order)
Ask up to 6 questions. If the user already provided an answer, skip that question.

1) Issue type (bug / feature / chore / tech debt)?
2) One-sentence goal/problem statement?
3) Expected outcome / definition of done (1–2 sentences)?
4) Key constraints (architecture layer, Result<T>, security, perf, compatibility, etc.)?
5) Out of scope?
6) Verification (tests, manual steps)?

## Phase 2: Output (only after answers)
Rules:
- Output ONLY one fenced code block using ```markdown
- No text before or after the code block.
- Must include:
  - Title
  - Description
  - Acceptance Criteria (>= 3 checkbox items, testable)
- If any answer is missing, make minimal assumptions and label them clearly under Description as “Assumptions”.

## Title rules
- Keep it short and specific.
- Prefer: "<type>: <short outcome>" (e.g., "bug: fix adoption fee not updating owner balance")

Template:

```markdown
# <Title>

## Description
<Problem, context, goal, expected outcome.>
Assumptions:
- ...

## Acceptance Criteria
- [ ] ...
- [ ] ...
- [ ] ...

## Notes (optional)
- Constraints:
- Out of scope:
- Verification:
```

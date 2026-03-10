---
name: create-feature
description: Use this skill when the user asks to create, draft, or write a GitHub issue for a new feature. This skill is interactive: it asks questions first, then outputs a final Markdown issue.
---


## Goal
Create a GitHub issue draft (title + description + acceptance criteria).

## Trigger
- Use this skill when the user writes `/create-feature`.
- When triggered, use the Feature Specification template for intake and issue generation.



## Feature Specification Intake (for /create-feature)
When using `/create-feature`:
- If you provide text after the command, it is treated as the goal (Overview / Goal).
  - The skill will assign the goal, generate a concise title, and ask the remaining questions:
    1) Business Requirements (user stories, acceptance criteria)?
    2) Functional Requirements (expected behavior)?
    3) Non-Functional Requirements (optional)?
    4) Open Questions / Risks?
- If you do not provide any text, the skill will ask all questions:
    1) Feature Title?
    2) Overview / Goal?
    3) Business Requirements (user stories, acceptance criteria)?
    4) Functional Requirements (expected behavior)?
    5) Non-Functional Requirements (optional)?
    6) Open Questions / Risks?

If the user provides answers up front, skip those questions.

## Output (for /create-feature)
After intake, generate the issue using this template:

```markdown
# <Feature Title>

## Overview / Goal
<Overview>

## Business Requirements
<Business Requirements>

## Functional Requirements
<Functional Requirements>

## Non-Functional Requirements
<Non-Functional Requirements>

## Open Questions / Risks
<Open Questions / Risks>
```



## Critical behavior (MUST)
- This is a multi-phase interaction:
  1) Ask questions (intake)
  2) Generate the draft issue Markdown
  3) Allow iterative edits and review
  4) When user confirms, create the issue in GitHub
  5) Assign to project, prompt for milestone, and add labels

- DO NOT generate any issue markdown in phase 1.
- In phase 1, output ONLY the questions.
- After the user answers, generate the draft issue.
- Allow user to edit or iterate on the draft until satisfied.
- When ready, create the issue in the repository using GitHub API.
- Fetch available projects, milestones, and labels from the repo.
- Prompt user to select project, milestone, and labels.
- Assign the issue accordingly.

## Title rules
- Keep it short and specific.
- For features, use the feature title provided.

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

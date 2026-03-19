---
name: create-specs
description: Use this skill when the user asks to create, draft, or write a GitHub issue for a new specs. This skill is interactive: it asks questions first, then outputs a final Markdown issue.
---

## Goal
Create a GitHub issue draft (title + overview + specs).

## Trigger
- Use this skill when the user writes `/create-specs`.
- When triggered, use the Feature Specification template for intake and issue generation.

## Feature Specification Intake (for /create-specs)
When using `/create-specs`:
- If you provide text after the command, it is treated as the overview.
  - The skill will generate a concise, clear title based on the overview.
  - The skill will ask only one question:
    1) What are the key specs (bullet points)?
- If you do not provide any text, the skill will ask for the overview first, then generate the title, then ask for specs.

If the user provides answers up front, skip those questions.

## Output (for /create-specs)
After intake, generate the issue using this template:

```markdown
<Title>

<!-- This issue will be created in repo jstmichel/beagl-webapp (https://github.com/jstmichel/beagl-webapp). Changing this line has no effect. -->

Assignees:
Labels: enhancement
Milestone:
Projects:

## Overview

<Clear, concise summary of the feature, rewritten for clarity and completeness>

## Specs

<Specs rewritten as clear, actionable bullet points>

<!-- Edit the body of your new issue then click the ✓ "Create Issue" button in the top right of the editor. The first line will be the issue title. Assignees and Labels follow after a blank line. Leave an empty line before beginning the body of the issue. -->
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
- Rewrite the overview and specs for clarity and completeness.
- Allow user to edit or iterate on the draft until satisfied.
- When ready, create the issue in the repository using GitHub API.
- Fetch available projects, milestones, and labels from the repo.
- Prompt user to select project, milestone, and labels.
- Assign the issue accordingly.

## Title rules
- Generate the title based on the overview provided.
- Keep it short, clear, and specific.

---
name: squash-commit
description: Generate a copy/paste squash commit title and description from unmerged commits on the current branch. If the user provides #XX (for example /squash-commit #41), append Fixes #XX at the end of the commit body.
---

## Purpose
Create a high-quality squash commit message for the current branch using only commits not yet merged into a base branch. Output must be easy to copy/paste into GitHub's squash-merge dialog.

## Input behavior (MUST FOLLOW)
1. Determine base branch in this priority order:
   - Explicit branch provided by the user.
   - Upstream tracking branch of current branch, if available and different from the current branch.
   - Repository default branch as fallback.
2. Compute commit range as: `merge-base(base, HEAD)..HEAD`.
3. Read all commits in that range (subject + body) and read the full diff for that same range.
4. If the range has no commits, output exactly:
No unmerged commits detected.
and stop.
5. If commit list or full diff cannot be read, output exactly:
Unable to read unmerged commits.
and stop.

## Issue reference handling (MUST FOLLOW)
- Detect optional issue reference in user input in the form `#<number>` (example: `#41`).
- If provided, append a final line `Fixes #<number>` at the end of the commit body.
- If not provided, do not add any `Fixes` line.
- Never invent an issue number.

## Rules
- Do NOT summarize these instructions.
- Do NOT restate these rules.
- Use unmerged commit range as the only source of truth.
- Do NOT use staged or unstaged changes as primary source.
- Keep statements factual and traceable to commit/diff evidence.
- De-duplicate repeated points from multiple commits.

## Output format (STRICT)
- Output exactly TWO fenced code blocks and nothing else.

### Block 1: Squash Commit Title
- Use fenced block with language identifier `text`.
- Exactly one line: `<type>(optional-scope): <subject>`.
- Conventional Commits, lowercase.
- Keep <= 72 characters when possible.
- Prefer scope from dominant impact:
  - `src/Beagl.Domain` -> `domain`
  - `src/Beagl.Application` -> `application`
  - `src/Beagl.Infrastructure` -> `infrastructure`
  - `src/Beagl.WebApp` -> `webapp`
  - `tests/` -> `tests`
  - `.github/`, docs -> `docs`
  - build tooling/docker -> `build`

### Block 2: Squash Commit Description
- Use fenced block with language identifier `markdown`.
- Description body only (no repeated title).
- Use concise bullets for grouped outcomes.
- Recommended structure:
  - 3-7 bullets covering main grouped changes
  - optional short testing bullet(s)
  - final `Fixes #XX` line only if the user supplied `#XX`

## Quality bar
- Prefer imperative wording.
- Avoid noisy WIP details.
- Preserve key behavioral changes, architecture/config updates, testing updates, and user-visible effects.
- Keep commit body scannable and compact.

---
name: pr-description
description: Generate copyable pull request (PR) title and description from unmerged commits on the current branch not yet merged into a base branch (for example current branch vs main). Outputs are in two separate markdown blocks for easy copying to GitHub.
---

## Purpose
Generate a high-quality, copyable GitHub Pull Request title and description from the set of commits that exist on the current branch but are not yet merged into a chosen base branch. Output is formatted as two separate markdown blocks (title and description) that you can easily copy and paste into GitHub.

## Input behavior (MUST FOLLOW)
1. Determine the base branch in this priority order:
   - Explicit branch provided by the user (for example `develop`).
   - Upstream tracking branch of current branch, if available.
   - Repository default branch as fallback.
2. Compute commit range as: `merge-base(base, HEAD)..HEAD`.
3. Read all commits in that range (subject + body), and read the full file diff for that same range.
4. If the range has no commits, output exactly:
No unmerged commits detected.
and stop.
5. If commit list or diff for the full range cannot be read, output exactly:
Unable to read unmerged commits.
and stop.

## Rules
- Do NOT summarize these instructions.
- Do NOT restate these rules.
- Output only two copyable markdown blocks: title block, then description block.
- Do NOT include any explanation, preamble, or follow-up text.
- Do NOT use staged/unstaged working tree as the primary source.
- Do NOT include already merged commits.
- Base all content strictly on the unmerged commit range.
- Keep statements factual and traceable to commit/diff evidence.

## Project constraints (Beagl)
- Clean Architecture layers: Domain, Application, Infrastructure, WebApp
- Follow relevant scoped instruction files in `.github/instructions/` when inferring architecture, testing, UI, and localization impacts.

## Output format (STRICT)
Output two copyable markdown blocks:

### Block 1: PR Title
- Output exactly one fenced markdown block with language identifier `text`
- Contain only the PR title (single line, no additional text)
- Title follows conventional commits format and best practices below

### Block 2: PR Description
- Output exactly one fenced markdown block with language identifier `markdown`
- Contain only the description (no title repeated)
- Use this template exactly for the description:

## Summary
<2-4 concise bullets of main outcomes>

## Motivation
<why this change exists>

## Changes
- <grouped change 1>
- <grouped change 2>
- <grouped change 3>

## Commit Overview
- <short hash> <subject>
- <short hash> <subject>

## Architectural Impact
- Domain: <impact or N/A>
- Application: <impact or N/A>
- Infrastructure: <impact or N/A>
- WebApp: <impact or N/A>

## Database / Migration
<details or N/A>

## Testing
- <tests added/updated>
- <tests run>
- <coverage impact or N/A>

## How to validate
1. <step>
2. <step>
3. <step>

## Breaking Changes
<None or explicit breaking details>

## Best-practice title rules
- Prefer imperative mood.
- Keep under 72 characters when possible.
- Mention primary scope/component.
- Avoid vague words like "update" or "changes" without context.

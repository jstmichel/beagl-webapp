---
name: pr-description
description: Generate a pull request (PR) title and description from unmerged commits, with optional direct creation on GitHub if `gh` CLI is installed. Use when the user asks to generate, write, or create a PR from commits on the current branch not yet merged into a base branch (for example current branch vs develop).
---

## Purpose
Generate a high-quality GitHub Pull Request title and description from the set of commits that exist on the current branch but are not yet merged into a chosen base branch. Optionally create the PR directly on GitHub if `gh` CLI is available.

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
- Output only the PR content (title + description), no extra commentary.
- Do NOT use staged/unstaged working tree as the primary source.
- Do NOT include already merged commits.
- Base all content strictly on the unmerged commit range.
- Keep statements factual and traceable to commit/diff evidence.

## Project constraints (Beagl)
- Clean Architecture layers: Domain, Application, Infrastructure, WebApp
- Follow relevant scoped instruction files in `.github/instructions/` when inferring architecture, testing, UI, and localization impacts.

## Output format (STRICT)
- Output the result inside exactly one fenced markdown block.
- Use `markdown` as the fence language.
- Do not add text before or after the code block.
- Inside the markdown block, use this template exactly.

# PR Title
<single line, conventional and concise>

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

## Create PR on GitHub (optional)
After outputting the PR preview, the skill automatically detects the GitHub repository from your current git remote and offers to create the PR.

1. **Check if `gh` CLI is installed:**
   ```bash
   which gh
   ```
   - If found: Continue to step 2
   - If not found: Output the helper message below and stop

2. **If `gh` CLI is NOT installed, output exactly:**
   ```
   GitHub CLI (`gh`) is not installed. To create the PR directly:

   Option 1 (Recommended): Install GitHub CLI
   - macOS: brew install gh
   - Linux: https://github.com/cli/cli/blob/trunk/docs/install_linux.md
   - Windows: choco install gh
   - Then authenticate: gh auth login

   Option 2: Create PR manually
   - Go to https://github.com/{owner}/{repo}/compare/{base}...{current-branch}
   - Copy the above PR content and paste it into GitHub's web UI
   ```

3. **If `gh` CLI IS installed, after showing the preview, ask:**
   ```
   Would you like to create this PR on GitHub now? (yes/no)
   ```

4. **If user answers "yes", run:**
   ```bash
   gh pr create \
     --title "{title from PR content}" \
     --body "{body - everything after title}" \
     --base {base branch}
   ```

5. **Output the result:**
   - Success: Show PR URL from `gh` output
   - Failure: Show error message and instructions to create manually

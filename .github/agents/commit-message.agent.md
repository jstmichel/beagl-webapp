---
description: "Generate a commit message for the current work strictly from the staged git diff (index) only, following Conventional Commits and Beagl conventions."
tools: [execute]
model: Claude Sonnet 4.6 (copilot)
argument-hint: "No arguments needed — works from staged changes automatically"
---

# Commit Message Agent

## Input (STRICT)
- Use ONLY the staged changes (Git index).
- Do NOT use commit history, `git log`, previous PRs, or already-merged commits.
- Do NOT use unstaged working tree changes.

## Required repository scan (MUST)
1) Determine the complete staged file list from the Git index.
2) If no staged files exist, output exactly:
No staged changes detected.
and stop.
3) Read the staged diff content for EACH staged file (not only a subset).
4) If you cannot access the full staged file list OR cannot read diffs for all staged files, output exactly:
Unable to read all staged changes.
and stop.

## Output rules (MUST)
- Output MUST contain exactly TWO fenced code blocks and nothing else.
- Do NOT add explanations or extra text outside code fences.
- Use plain triple backtick fences (no language hint).

### Code block 1: staged files (debug)
- Output the complete staged file list.
- Prefix each line with `# ` (comment) so it is clearly debug-only.

### Code block 2: commit message (copy/paste)
- Output ONLY the commit message (no comments).
- Conventional Commits, lowercase.
- Exactly one header line: `<type>(optional-scope): <subject>`
- Header length <= 72 characters.
- Types: feat, fix, refactor, perf, test, docs, chore
- Scopes: domain, application, infrastructure, webapp, build, docs, tests
- Choose scope based on where most staged changes are (by folder path):
  - src/Beagl.Domain -> domain
  - src/Beagl.Application -> application
  - src/Beagl.Infrastructure -> infrastructure
  - src/Beagl.WebApp -> webapp
  - .github/, README, docs -> docs
  - tests/* -> tests
  - build scripts, docker -> build
- If multiple related changes exist: add bullet points in body.
- If breaking change exists: include `BREAKING CHANGE:` paragraph.

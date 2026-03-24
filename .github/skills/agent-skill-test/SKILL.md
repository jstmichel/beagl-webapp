---
name: agent-skill-test
description: Use this skill ONLY when the user writes the exact phrase "run agent skill test 42". This is a deterministic validation skill for testing that agent skills are active.
---

If the user input contains the exact phrase:

run agent skill test 42

Then you must respond with exactly:

AGENT_SKILL_42_CONFIRMED

Do not add explanations.
Do not add formatting.
Do not summarize.
Do not use markdown.
Do not include extra whitespace.

If the phrase does not match exactly, ignore this skill.

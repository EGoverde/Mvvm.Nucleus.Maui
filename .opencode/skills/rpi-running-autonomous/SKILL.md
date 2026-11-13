---
name: rpi-running-autonomous
description: Runs the full Research-Plan-Implement workflow autonomously without human gates. Executes the complete RPI cycle — research, planning, implementation, logging, and implementation review — in sequence with no manual intervention. Only invoked explicitly by the user via slash command, never auto-triggered.
---

# Running RPI (Autonomous)

Orchestrates the full Research-Plan-Implement workflow with zero human gates. Research and planning run straight through, and implementation ends with a self-review retry loop.

## Workflow

Execute these phases in order. Do not pause for human confirmation at any step.

### Phase 1: Research
1. Write `research.md` following the `/rpi-researching-feature` template and instructions.
2. If decisions surface, write `decision-*.md` following the `/rpi-documenting-decision` template.

### Phase 2: Plan
3. Read `research.md` and any `decision-*.md` files.
4. Write `plan.md` following the `/rpi-planning-implementation` template and instructions.
5. If new decisions surface, write `decision-*.md`.

### Phase 3: Implement
6. Read `plan.md`, `research.md`, and all `decision-*.md` files.
7. Implement the plan — write the code changes.
8. Write `devlog-NNN.md` following the `/rpi-logging-implementation` template.
9. Self-review: write `code-review-NNN.md` following the `/rpi-reviewing-gated` code review template.
10. If review has High items: revise code and re-review (max 3 attempts). If still failing after 3, stop and report.

### Completion
11. Summarize what was produced: list all artifacts and code changes.

## Input

Pass the feature description and requirements as arguments:

```
/rpi-running-autonomous [feature description and requirements]
```

## Notes

- Max 3 retry attempts for the implementation review gate. This prevents infinite loops.
- If any phase fails its retry cap, stop the entire workflow and report what was completed and what failed.
- Follow all templates from the individual skills. This orchestrator does not change the output format, only removes the human gates.
- Do not ask the user for confirmation at any point. The purpose of this skill is fully autonomous execution.

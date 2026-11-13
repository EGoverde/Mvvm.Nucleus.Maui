---
name: rpi-implementing-plan
description: Executes the implementation described in plan.md, writing the actual code changes. Checks for plan.md, research.md, and decision records for full context. Auto-invokes rpi-logging-implementation and rpi-reviewing-gated when done. Best invoked after a plan has been approved.
---

# Implementing Plan

Executes an approved plan by writing code. Reads `plan.md` and produces code changes in the project.

## Workflow

1. **Check for plan** — Look for `plan.md` in the project root.
   - **Found:** Read it. Summarize: "I found plan.md: [summary]. I'm about to implement [N steps]. Proceed?" Wait for approval.
   - **Not found:** Inform the user: "No plan.md found. I need a plan to implement. Consider running `/rpi-planning-implementation` first." Stop.

2. **Gather full context** — Also read if available:
   - `research.md` — background context
   - `decision-*.md` — decisions that constrain implementation
   - Existing code that will be modified

3. **Implement** — Follow the Implementation Approach section of plan.md step by step. Write the code.

4. **Auto-invoke devlog** — When implementation is complete, invoke `/rpi-logging-implementation` to capture what was done. Do not ask the user.

5. **Auto-invoke review** — After the devlog is written, invoke `/rpi-reviewing-gated` to self-review the code changes. Do not ask the user.

## Notes

- `plan.md` is effectively required. Without it, this skill has nothing to implement.
- Follow the plan. If you discover the plan is wrong or incomplete during implementation, note it as a deviation — do not silently diverge.
- Do not refactor, add features, or "improve" code beyond what the plan specifies.

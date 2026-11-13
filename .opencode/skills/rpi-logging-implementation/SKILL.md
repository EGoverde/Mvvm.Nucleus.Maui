---
name: rpi-logging-implementation
description: Writes a devlog (devlog-NNN.md) summarizing what was implemented, tricky parts, decisions made, and deviations from the plan. Auto-invoked after rpi-implementing-plan completes. Checks plan.md and recent git changes for context.
---

# Logging Implementation

Captures implementation context in a devlog. Produces `devlog-NNN.md` in the project root.

## Workflow

1. **Determine devlog number** — Check for existing `devlog-NNN.md` files. Increment NNN (zero-padded to 3 digits, starting at 001).

2. **Gather context** — Read:
   - `plan.md` — what was the intent?
   - Recent git diff / uncommitted changes — what actually happened?
   - Any `decision-*.md` files created during implementation

3. **Write devlog** — Use the template below. Focus on what is not obvious from reading the code or the diff.

4. **Continue** — Control returns to the invoking skill (typically `/rpi-implementing-plan`, which then triggers `/rpi-reviewing-gated`).

## Template

```markdown
# Devlog: [Brief Title]

**Date:** [Date]
**Implementing:** plan.md

## What Was Done
- [Completed work]

## Tricky Parts
- [Challenges encountered and how they were resolved]

## Decisions Made
- [Implementation decisions not covered in the plan]

## Deviations from Plan
- [Any changes from the original plan and why]

## Next Steps
- [If incomplete, what remains]
```

## Notes

- This skill is auto-invoked. Do not wait for human approval.
- Keep it concise. The devlog captures context that would be lost, not a restatement of the diff.
- If there were no tricky parts or deviations, say so briefly rather than padding.

---
name: rpi-reviewing-gated
description: Performs a structured self-review of an artifact (research.md, plan.md, or code changes) with severity-rated findings and a pass/revise verdict. Auto-triggered after implementation and available for manual use on research or planning artifacts. Loops back automatically on failure.
---

# Gated Review

Polymorphic self-review skill. Auto-detects the artifact to review and produces a severity-rated assessment with a binary pass/revise verdict.

## Workflow

1. **Detect artifact** — Scan the project root for the most recently modified reviewable artifact:
   - `research.md` → research review
   - `plan.md` → plan review
   - Recent git changes (uncommitted or last commit) → code review
   - If multiple exist, review the most recently modified one.

2. **Confirm target** — State: "I'm about to review [artifact]. Proceeding with review." This is informational, not a gate — do not wait for approval.

3. **Determine review number** — Check for existing `*-review-NNN.md` files for this artifact type. Increment NNN (zero-padded to 3 digits, starting at 001).

4. **Write review** — Use the appropriate template below. Be honest and critical. Do not rubber-stamp.

5. **Verdict** — If the review contains any **High** severity items:
   - State the verdict: "Needs revision. [summary of high items]."
   - Loop back: re-read the original artifact, apply fixes, then re-review (up to 3 attempts).
   - If still failing after 3 attempts, stop and report to the user.
   - If the review passes (no High items), state: "Review passed. Ready for human review."

## Templates

### Research Review (research-review-NNN.md)

```markdown
# Research Review

**Reviewer:** AI
**Date:** [Date]
**Reviewing:** research.md

## Summary
[One paragraph assessment]

## Strengths
- [What the research does well]

## Gaps

### High
- [Critical missing information that blocks planning]

### Medium
- [Important gaps that should be addressed]

### Low
- [Nice-to-have information]

## Questions
- [Clarifications needed]

## Recommendation
[ ] Ready for human review
[ ] Needs revision (see gaps above)
```

### Plan Review (plan-review-NNN.md)

```markdown
# Plan Review

**Reviewer:** AI
**Date:** [Date]
**Reviewing:** plan.md

## Summary
[One paragraph assessment]

## Strengths
- [What the plan does well]

## Concerns

### High
- [Critical issues that could cause failure]

### Medium
- [Significant risks or unclear areas]

### Low
- [Minor issues or suggestions]

## Suggestions
- [Improvements or alternatives]

## Recommendation
[ ] Ready for human review
[ ] Needs revision (see concerns above)
```

### Code Review (code-review-NNN.md)

```markdown
# Code Review

**Reviewer:** AI
**Date:** [Date]
**Reviewing:** [file list or commit hash]

## Summary
[One paragraph assessment]

## What Works Well
- [Positive aspects]

## Issues

### High
- [ ] [Critical bugs or security issues]

### Medium
- [ ] [Significant problems]

### Low
- [ ] [Minor improvements]

## Recommendation
[ ] Ready for human review
[ ] Needs revision (see issues above)
```

## Notes

- This skill is auto-invoked after implementation. It can also be run manually for research or planning artifacts.
- Be genuinely critical. A review that always passes is worthless.
- The 3-attempt retry cap prevents infinite loops.

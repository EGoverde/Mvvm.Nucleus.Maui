---
name: rpi-planning-implementation
description: Creates a structured implementation plan (plan.md) for a feature or task, detailing technical design, approach, and alternatives. Checks for research.md to build upon. Best invoked after research is complete or when jumping straight to planning without prior research.
---

# Planning Implementation

Creates a structured implementation plan. Produces `plan.md` in the project root.

## Workflow

1. **Check for research** — Look for `research.md` (and any `research-*.md` variants) in the project root.
   - **Found:** Read all research files. Summarize: "I found [research files] covering [topics]. I'm about to plan implementation for [feature]. Proceed?" Wait for approval.
   - **Not found:** Inform the user: "No research files found. I'm about to plan implementation for [feature] without prior research. Proceed? (Consider running `/rpi-researching-feature` first.)" Wait for approval.

2. **Determine output filename** — Check if `plan.md` already exists in the project root.
   - **Does not exist:** Output will be `plan.md` (the primary plan).
   - **Exists and this plan is for the same topic:** Ask whether to overwrite or build on it.
   - **Exists and this plan is for a different/secondary topic** (e.g. a regression fix, an edge case discovered during implementation): Output will be `plan-<descriptor>.md` where `<descriptor>` is a short camelCase label you choose to describe the secondary topic (e.g. `plan-regressionFix.md`, `plan-authEdgeCase.md`). If a matching `research-<descriptor>.md` exists, use the same descriptor. Inform the user of the chosen filename.

3. **Check for decisions** — Read any `decision-*.md` files for context that should inform the plan.

4. **Write plan file** — Use the template below. Write to the filename determined in step 2. Ground the plan in the research findings and decisions if available.

5. **Document decisions** — If planning surfaces new decision points, invoke `/rpi-documenting-decision`.

6. **Finish** — When the plan file is complete, stop and return control to the user. Do not auto-invoke `/rpi-reviewing-gated`.

## Template

ALWAYS use this structure for plan.md:

```markdown
# Plan: [Feature Name]

**Status:** Draft | In Review | Approved | Implemented
**Author:** [Name]
**Created:** [Date]

## Summary
[One paragraph]

## Motivation
[Why is this needed?]

## Goals
- [What this achieves]

## Non-Goals
- [What this does not address]

## Technical Design
[Detailed approach]

## Implementation Approach
[Step-by-step implementation plan, key files to modify, order of operations]

## Alternatives Considered
[Other approaches and why not chosen]
```

## Notes

- `research.md` is a soft dependency. The skill works without it but produces better plans with it.
- The Implementation Approach section should be specific enough that `/rpi-implementing-plan` can follow it step by step.
- Do not over-plan. Match plan detail to task complexity.

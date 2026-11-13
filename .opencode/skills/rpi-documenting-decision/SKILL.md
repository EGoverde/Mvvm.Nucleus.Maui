---
name: rpi-documenting-decision
description: Creates an ADR-style decision record (decision-*.md) when a feature or task surfaces a choice between multiple valid approaches. Auto-invoked during research or planning phases. Reads research.md and plan.md for context if available.
---

# Documenting Decision

Creates structured decision records when multiple valid approaches exist. Produces `decision-[name].md` in the project root.

## Workflow

1. **Check context** — Read `research.md` and `plan.md` if they exist, to understand the surrounding work.

2. **Write decision record** — Use the template below. Name the file `decision-[kebab-case-topic].md`.

3. **Continue** — This skill does not trigger any follow-up. Control returns to the invoking skill.

## Template

```markdown
# Decision: [Brief Title]

**Status:** Proposed | Accepted | Deprecated | Superseded
**Date:** [Date]
**Deciders:** [Who made the decision]

## Context
[What problem or question prompted this decision?]

## Options

### Option 1: [Name]
[Brief description]

**Pros:**
- [Advantage]

**Cons:**
- [Disadvantage]

### Option 2: [Name]
[Brief description]

**Pros:**
- [Advantage]

**Cons:**
- [Disadvantage]

## Decision
[Which option was chosen and why]

## Consequences
### Positive
- [Benefits]

### Negative
- [Drawbacks or tradeoffs]
```

## Notes

- This skill is auto-invoked. Do not wait for human approval.
- Only create a decision record when there is a genuine choice between distinct approaches. Do not create one for obvious or single-option decisions.
- Keep it concise. Two options with clear pros/cons is better than five with vague ones.

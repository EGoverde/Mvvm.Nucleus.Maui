---
name: rpi-researching-feature
description: Conducts structured research for a feature or task, producing a research.md with requirements, architecture, constraints, and open questions. Best invoked when starting new work that needs investigation before planning.
---

# Researching Feature

Conducts structured research before implementation planning. Produces `research.md` in the project root.

## Workflow

1. **Confirm scope** — Summarize what you are about to research and what the user provided as input. Ask: "I'm about to research [topic] covering [scope]. Proceed?" Wait for approval before continuing.

2. **Determine output filename** — Check if `research.md` already exists in the project root.
   - **Does not exist:** Output will be `research.md` (the primary research file).
   - **Exists and this research is for the same topic:** Ask whether to overwrite or build on it.
   - **Exists and this research is for a different/secondary topic** (e.g. a regression discovered during implementation): Output will be `research-<descriptor>.md` where `<descriptor>` is a short camelCase label you choose to describe the secondary topic (e.g. `research-regressionFix.md`, `research-authEdgeCase.md`). Inform the user of the chosen filename.

3. **Check for context** — Look for existing artifacts in the project root:
   - Any relevant code, configs, or docs in the project that inform the research

4. **Write research file** — Use the template below. Write to the filename determined in step 2. Focus on what Claude does not already know from reading the codebase. Do not pad with obvious information.

5. **Document decisions** — If research surfaces a decision point with multiple valid options, invoke `/rpi-documenting-decision` to capture it.

6. **Finish** — When the research file is complete, stop and return control to the user. Do not auto-invoke `/rpi-reviewing-gated`.

## Template

ALWAYS use this structure for research.md:

```markdown
# Research: [Brief Title]

**Requester:** [Who asked]
**Date:** [Date]

## Requirements

### Original Request
[The ask verbatim or paraphrased]

### Context
[Context that came with the request]

### Open Questions
- [Questions to explore]

## System Architecture

### Related Components
[Overview of existing systems this feature touches]

### Data Flow
[How data moves through the relevant systems]

### Constraints
[Technical or business constraints to consider]

## Prior Art
[Similar implementations, patterns, or references]
```

## Notes

- This is the entry point for the RPI workflow. It has no soft dependencies.
- Keep research focused and concise. If the task is small, the research should be small.
- Do not fabricate architecture or constraints. If you are unsure, list it as an open question.

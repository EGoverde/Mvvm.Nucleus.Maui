---
name: rpi-archiving-artifacts
description: Collects RPI markdown artifacts (research.md, plan.md, decision-*.md, devlog-*.md, *-review-*.md) into a dated folder, and can optionally also pull in related ad hoc markdown docs and any other files the user names (scripts, logs, code, datasets, binaries) that belong to the same feature or investigation. Generates a one-page summary plus a manifest explaining every artifact, and removes the originals from the project root. Run manually at the end of a feature, or to archive a bug investigation that never went through the RPI cycle, for transfer to a separate repository.
---

# Archiving Artifacts

Collects everything related to a finished feature or investigation — RPI-generated markdown, other markdown docs on the same topic, and any supporting files the user calls out (scripts, logs, code, datasets, binaries) — into one dated folder with a summary that explains every piece, ready for archival in a separate repository.

## Delegation

This is file organization and summarization, not work that needs frontier-level reasoning. Don't run the Workflow below yourself — delegate it to a subagent on a cheaper model:

Call `Agent` with `subagent_type: "general-purpose"` and `model: "sonnet"`. The subagent starts with no memory of this conversation, so its `prompt` must be fully self-contained: include (a) the user's original archiving request verbatim, (b) the current working directory / repo root, and (c) the entire Workflow, Summary Template, and Notes sections below, pasted in full. Don't just tell it to "follow SKILL.md" — it can't see this conversation or re-invoke the skill itself.

The subagent may need to ask the user clarifying questions (folder name, ambiguous file matches) via `AskUserQuestion` — that's expected and works normally even from a subagent. When it finishes, relay its final report back to the user as this skill's result; don't redo or duplicate its work.

## Workflow

1. **Identify core RPI artifacts** — Scan the project root for the fixed patterns:
   - `research.md` and `research-*.md`
   - `plan.md` and `plan-*.md`
   - `decision-*.md`
   - `devlog-*.md`
   - `*-review-*.md` (research-review, plan-review, code-review)

2. **Check for a topic in the invocation** — If the user's request names a feature, bug, or topic (e.g. "archive the somrt stall bug"), pull out keywords from it now. They drive steps 3-4 and suggest the folder name.

3. **Optionally discover related markdown** — Run this step when either (a) a topic was given in step 2, or (b) step 1 found nothing (a fallback so the skill still works for investigations that never used the RPI naming convention, like an ad hoc `*-findings.md`). Search the project root for other `*.md` files whose filename or content plausibly relates to the topic keywords or to the artifacts already found in step 1 (shared slug/prefix, same subject matter). Present the candidates to the user and confirm before including any of them — never sweep in `README.md`, `CLAUDE.md`, or unrelated notes just because a keyword loosely matches.

4. **Pull in files the calling prompt asks for** — If the user names specific files, folders, scripts, logs, datasets, or other non-markdown artifacts — directly, or via a topic description that points at an obviously-related existing folder (e.g. a `<topic>-artifacts/` directory sitting next to the findings doc) — add them to the include list. If it's unclear exactly which files the user means, confirm the final list with them before moving anything.

5. **Stop if nothing was found** — If steps 1, 3, and 4 all come up empty, stop and inform the user.

6. **Determine folder name** — Unless it's already clear from step 2, ask the user for a short feature/topic description (1-3 words). Create the folder name as `YYYYMMDD-featureDescription` using today's date and the description in camelCase. Example: `20260515-asyncJobQueue`.

7. **Create the archive folder** in the project root.

8. **Move artifacts**, preserving enough structure to stay navigable:
   - Core RPI markdown (step 1) and related ad hoc markdown (step 3) → archive root, original filenames.
   - A directory named in step 4 → move it into the archive root as-is, preserving its internal structure (e.g. an existing `<topic>-artifacts/` folder with its own `scripts/`, `datasheets/`, `code/` subfolders moves in whole, unchanged).
   - Loose non-markdown files named in step 4 → group into sensible category subfolders under the archive root by kind (e.g. `scripts/`, `logs/`, `code/`, `data/`, `docs/`). Use judgment — don't force a taxonomy that doesn't fit what's actually there; a single stray file with no siblings can just sit at the archive root.

9. **Generate summary** — Read all moved markdown and write `summary.md` inside the archive folder, using the template below. For non-markdown artifacts, describe what each one is and why it's relevant (infer from filename, from what the markdown says about it, or ask the user) — never list a filename with no explanation. Target roughly one page for the narrative sections (~40-60 lines); the artifact manifest can run longer since it has to cover everything archived. Distill, don't concatenate.

10. **Report** — List everything archived and where it landed. Remind the user to copy or move the folder to their archival repository.

## Summary Template

```markdown
# Feature Summary: [Feature Name]

**Date:** [YYYY-MM-DD]
**Repository:** [name of current repo]

## Overview
[2-3 sentence high-level description of what was researched/investigated, planned, and built or found]

## Key Decisions
- [Important decisions made and their rationale, distilled from decision-*.md files, if any]

## What Was Built / Found
- [Concrete deliverables and changes, distilled from plan.md and devlog — or key findings and conclusions, distilled from ad hoc investigation docs]

## Architecture & Design
[Brief description of the technical approach taken, if applicable]

## Review Findings
- [Notable findings from reviews, especially anything that required revision]

## Lessons Learned
- [Tricky parts, surprises, or deviations from the plan — from the devlog or investigation notes]

## Archive Contents
- `filename.md` — [one line: what it is, e.g. "root-cause investigation doc; root cause confirmed in the final section"]
- `scripts/foo.py` — [one line: what it does and why it's here]
- `<topic>-artifacts/` — [one line: what the folder holds, e.g. "probe scripts, bisect log, PHY datasheets, and the bare test app used to isolate the stall"]
- [... one line per archived item, markdown and non-markdown alike]
```

## Notes

- This skill is manually invoked. It is never auto-triggered.
- Never archive `CLAUDE.md`, `README.md`, or other clearly project-level (not feature-specific) files, even if they match a topic keyword.
- Confirm with the user before moving anything found via fuzzy topic matching (step 3) or parsed from free text (step 4) — only the fixed-pattern RPI artifacts from step 1 can be moved without confirmation.
- If a `.gitignore` entry exists for any of the artifact patterns, mention it to the user but still proceed with archiving.
- The summary should stand on its own — someone reading only `summary.md` should understand what happened and know what every other file in the folder is, without opening them.
- An ad hoc investigation doc that never went through the RPI cycle (no research.md/plan.md, just a `*-findings.md`) is valid input on its own — treat it like a devlog for summary purposes: distill status, root cause, and next steps rather than reproducing its full timeline.

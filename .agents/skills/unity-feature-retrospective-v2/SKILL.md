---
name: unity-feature-retrospective-v2
description: Review the completed TroughGame feature-development process and propose a few evidence-based workflow improvements or none. Run only when explicitly dispatched at the new workflow's retrospective stage or directly requested. Do not change rules, reopen implementation, publish to Notion or create reports.
---

# Feature Retrospective V2

## Entry and Inputs

Run at `retrospective` with permission to enter this stage, or on a direct developer
request to use this skill. In the feature workflow it follows whole-feature acceptance
and completed/not-applicable Notion finalization. The primary factual source is
`.ai/features/<feature>/process-history.md`, maintained by the coordinator. Also use
the compact workflow outcomes, explicit developer feedback and the event sources
referenced in that history.
Review the development process, not the game's implementation or accepted scope.

## Task

1. Identify concrete process problems that caused unnecessary work, unclear decisions,
   repeated corrections, inconsistent responses or avoidable tool/context usage.
   Read process history and relevant feedback; follow a referenced source only to
   establish a specific fact. Missing/empty history is not proof of a problem-free
   process. If other supplied evidence is insufficient, state that limitation without
   inventing events or reconstructing them from guesses. Do not replay every stage
   or audit the whole repository.
2. Distinguish a correctable workflow problem from a one-off implementation bug,
   already resolved issue or unsupported impression. Check the relevant instruction
   when needed: an existing rule that was ignored may need a targeted clarification,
   not another copy of the same rule. State uncertainty where evidence is insufficient.
3. Propose at most three useful improvements. For each, name the observed problem,
   supporting event/reference, exact instruction/tool change, owning file/skill and
   expected benefit. Prefer a small correction or removal of redundant work. Do not
   invent general rules from a single bug or add documents/gates without a demonstrated
   need. If no supported improvement is useful, say so rather than filling a quota.

## Response Standard

Use the developer's language and present:

1. **Process result.** One short sentence stating whether useful improvements were
   identified and how many. No implementation recap or chronological work journal.
2. **Proposals.** Give each proposal a stable ID such as `RET-01`. Describe the
   problem and concrete evidence (history entry/source or explicit developer feedback),
   then the exact proposed change and its target.
   For an instruction edit, show the suggested wording or removal, not just an
   abstract recommendation. Explain the expected effect without invented savings
   or measurements. Use a list of separate `RET-*` entries, not a table; omit the
   section if empty.
3. **Developer decision.** Ask which proposals to accept, reject or defer, and whether
   to finish this workflow. If there are none, ask only for confirmation to finish.
   Acceptance of a proposal does not imply that it has already been implemented.

## Output and Completion

The output is the brief retrospective and developer decisions in chat. No retrospective
MD/template, file/state edits, test runs, Editor control, Notion calls, subagents or
Git mutations belong here. Process history is read-only for this skill; do not append
conclusions or proposals to it. The coordinator owns records and completion.

When the developer accepts the retrospective and authorizes finishing, return that
decision to the coordinator. Rejected/deferred proposals or no proposals are valid
outcomes; applying improvements is not a prerequisite to feature completion.
Any approved rule/tool change is separate authorized work on the named target,
subject to its applicable checks, not an automatic self-edit within this stage.
Do not reopen accepted feature work or start another workflow to apply a lesson.
Silence is not a decision; completed game development and process suggestions remain
distinct even when an improvement is deferred.

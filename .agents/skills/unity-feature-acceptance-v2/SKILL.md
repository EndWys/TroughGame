---
name: unity-feature-acceptance-v2
description: Summarize delivery of a complete TroughGame feature against agreed requirements and obtain developer whole-feature acceptance. Run only when explicitly dispatched at the new workflow's acceptance stage or directly requested. Reuse prior results; do not repeat tests/reviews, write reports or finalize Notion.
---

# Whole-Feature Acceptance V2

## Entry and Inputs

Run at `acceptance` with permission to enter this stage, or on a direct developer
request to use this skill. Inputs are the feature ID, accepted `requirements.md`,
`feature-plan.md`, relevant accepted decisions and the coordinator's actual subtask
review/playtest results. This stage concerns the whole feature, not another review
or playtest of the same implementation.

## Task

1. Compare the delivered result with the current accepted requirements and plan.
   Use the existing subtask handoffs, review acceptance and developer playtest
   responses. Follow specific references only where needed to establish a fact;
   do not rescan the project or repeat the previous stages' reports.
2. Account for every agreed requirement/acceptance criterion and planned subtask.
   Identify the delivered behavior and supporting accepted subtask/case. Check for
   missing results, unresolved findings or integration requirements not covered by
   the existing outcomes. Accepted individual subtasks alone do not prove an
   untested cross-subtask behavior; report the specific gap without running tests.
3. Separate agreed exclusions/deferred scope from unfinished required behavior.
   Retain actual limitations and developer decisions. Do not silently narrow a
   requirement, convert a missing result into a pass or treat an agent summary as
   developer acceptance. An unclear fact needs a focused clarification, not a guess.
4. Present a concise whole-feature result for the developer's acceptance or specific
   feedback. Outstanding mandatory work prevents a ready-for-acceptance conclusion;
   return the gap to the coordinator instead of fixing it or starting another stage.

## Response Standard

Use the developer's language and present:

1. **Feature result.** A short account of the delivered functionality and how its
   parts work together. Do not repeat per-file inventories, architecture or routine
   successful technical checks.
2. **Requirements coverage.** A compact table: `RQ/AC`, delivered behavior, supporting
   accepted subtask/playtest case. Group related criteria where this avoids repetition,
   retaining their IDs. Include only actual support; make a missing result visible.
3. **Gaps or agreed limitations.** If present, state the concrete remaining behavior,
   impact and needed developer decision, distinguishing required work from explicitly
   deferred/out-of-scope items. No empty section or generic verification warnings.
4. **Developer decision.** With no mandatory gaps, ask to accept the feature as a
   whole or identify behavior needing correction. With gaps, ask only for the missing
   result or scope decision needed to resolve them. Do not repeat approval requests
   for accepted subtasks or claim the final decision has already been made.

## Output and Completion

Return the whole-feature summary, coverage, limitations and actual developer
acceptance basis in chat. No new MD/template, file/state edits, test/build runs,
Editor control, Notion calls, subagents or Git mutations belong to this stage.
The coordinator records the outcome and owns authorized workflow transitions.

The stage is accepted when required feature scope has supported accepted outcomes,
mandatory gaps are resolved and the developer explicitly accepts the whole-feature
result. Silence, task completion flags and earlier subtask acceptance are insufficient.
On feedback, preserve accepted results and identify the affected requirement/subtask
for the coordinator. Do not reopen unrelated work or invoke `unity-bugfix`.
Whole-feature acceptance does not itself publish to Notion, complete the remaining
workflow or authorize another transition without the developer's permission.

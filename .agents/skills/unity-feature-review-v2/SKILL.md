---
name: unity-feature-review-v2
description: Review one implemented TroughGame subtask against accepted requirements, architecture and project rules, then obtain developer diff acceptance. Run only when explicitly dispatched at the new workflow's review stage or directly requested. Do not fix code, create reports or start another stage.
---

# Feature Review V2

## Entry and Inputs

Run at `review` with permission to enter this stage, or on a direct developer
request to use this skill. Inputs are the feature ID, selected `FEAT-*`, accepted
requirements/plan/architecture, the implementation handoff, actual technical check
results and the task's changes with their comparison base.

## Task

1. Establish the actual change set: staged/unstaged edits, new untracked files and
   affected assets with their `.meta` files. Use the task's base and scope; do not
   treat all working-tree changes or earlier accepted work as this subtask's diff.
   Resolve ambiguous boundaries before reviewing the affected changes.
2. Read the relevant diff and surrounding callers/contracts, not just the author's
   summary. Compare behavior and integration with accepted requirements and this
   subtask's architecture. Follow AGENTS, architecture rules and conventions; inspect
   relevant scene, UI and lifecycle rules only where the change needs them.
3. Check logic correctness, input/state validation, ownership/dependency direction,
   lifecycle and initialization errors, cleanup/error paths and affected contracts.
   Inspect network authority/peer roles, configuration, prefab/scene references or
   UI bindings where applicable. For network changes, review transmitted data size
   and representation, update/RPC frequency, redundant replication and recipient
   scope against actual gameplay needs. Review excessive allocations and unnecessarily
   high algorithmic complexity, considering call frequency and expected data volume.
   Base performance findings on concrete code; do not invent runtime measurements.
   Check that changed assets and code form a coherent implementation.
4. Examine the available technical check results and test coverage of the change.
   Missing required evidence is a concrete gap; successful compilation alone does
   not establish code correctness. If live asset inspection is needed,
   use the installed Unity CLI skill read-only against the intended project.
5. Retain issues supported by concrete evidence; exclude speculative bugs, invented
   requirements and unrelated refactoring. On authorized re-review, inspect the
   correction and affected interactions, retaining unresolved findings without
   restarting unrelated work.

## Response Standard

Use the developer's language and present the following in order:

1. **Review result.** One short line stating the number of proposed corrections.
   No explanation of the subtask, review scope or successful technical checks.
2. **Findings.** Use a table: ID, type, severity, short problem description. Assign
   unique feature-scoped IDs such as `FEAT-01-REV-001`; preserve them across revisions
   and never reuse an ID for a different issue. Type names the primary issue category.
   Order findings by severity.
   Each description names the concrete fault and effect, with a clickable file/line
   or asset/object reference supporting it. Do not place the proposed fix in this table.
3. **Proposed solutions.** List every finding by the same ID and show the concrete
   logic change, not an abstract action plan. Describe current → proposed behavior;
   identify entities to change/create/remove and their resulting responsibilities.
   Where call flow changes, show the proposed chain as an ordered list of
   `Object/Class.Method()` — role/action. Include a short proposed code change or
   clearly labeled pseudocode for the key correction, with the affected file/method;
   specify asset/reference values when the fix concerns Unity integration instead.
   Omit unchanged aspects; do not invent entities or code just to fill these items.
   Explain why the change resolves the finding and note consequential contract
   changes/tradeoffs.
   If no concrete solution is established, say what is missing. If developer input
   is needed, ask the exact question and identify which part of the solution it
   determines; do not disguise an unresolved choice as an implementation-ready fix.
4. **Developer decision.** Ask which finding IDs should be fixed and which proposed
   solutions are authorized, rejected or need clarification. With no findings, omit
   the findings/solutions sections and ask for a simple continue/change instruction.

Use severity consistently: `P0` critical (blocks core use or causes severe loss),
`P1` high (substantial correctness/contract failure), `P2` medium (localized defect
or meaningful performance issue), `P3` low (minor issue or improvement). Justify the
level through concrete impact; severity does not itself authorize a correction.
If missing scope/evidence prevents review completion, report that specific blocker
instead of presenting an incomplete review as a clean result. Do not add a routine
reviewed-scope section or repeat the implementation handoff.

## Output and Completion

Deliver the response in chat only. This stage is read-only: no file edits, review
reports/templates/manifests, test runs, experiments, Notion calls, subagents or Git
mutations. Do not invoke `unity-bugfix`. The coordinator records decisions and routes
authorized corrections within the same subtask; this skill does not select stages.

Completion requires the developer's decision on findings and acceptance of the
reviewed diff or permission to continue. Pending authorized corrections/re-review
or material review gaps keep the stage open. Explicit deferrals/rejections remain
visible to the coordinator; they neither prove a fix nor revise accepted requirements
or architecture. A clean agent review or silence is not acceptance. Later material
edits require review of those changes; prior acceptance covers only the reviewed diff.

Return control to the coordinator. An explicit continue instruction permits its
stated next transition; diff acceptance alone does not grant a transition or
establish Play Mode/retest acceptance.

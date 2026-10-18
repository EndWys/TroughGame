---
name: unity-feature-planning-v2
description: Research existing TroughGame mechanisms and obtain developer acceptance of an ordered, verifiable feature subtask plan. Run only when explicitly dispatched at the new workflow's planning stage or directly requested. Do not design subtask architecture, implement code or start other stages.
---

# Feature Planning V2

## Entry and Inputs

Run at `planning` after requirements acceptance and permission to proceed, or on a
direct developer request to use this skill. Inputs are the feature ID, accepted
`requirements.md`, relevant developer decisions and any existing `feature-plan.md`.
If requirements are unaccepted or materially incomplete, report the exact gap;
do not infer approval or rewrite the requirements yourself. The coordinator owns
any return to another stage.

## Task

1. Read the accepted requirements and applicable project architecture/conventions.
   Identify which responsibilities and integration boundaries actually matter.
   Inspect those areas of code, configuration, assets and tests; follow relevant
   callers and implementations. Detailed scene, UI and lifecycle design belongs to
   the architecture/implementation stages, not this planning task.
   Use the installed Unity CLI skill for necessary read-only Editor inspection;
   do not change scenes/assets, run experiments or start a fallback Editor.
2. Identify existing mechanisms to reuse or extend. Name relevant assets, scripts,
   types or objects, their roles and how they affect the plan. Filesystem paths
   are not required in the findings or plan.
   Separate observed facts, inferences and unresolved questions. Do not dump searches
   or inventory unrelated project systems.
3. Divide the work into focused `FEAT-*` subtasks with manageable review scope.
   Prefer smaller coherent changes over a large multi-system diff, without fragmenting
   trivial edits into artificial tasks. Use responsibility and integration boundaries;
   each subtask must be understandable and reviewable on its own. Each delivers an observable
   outcome with a defined scope, related `RQ-*`/`AC-*`, justified dependencies and
   short description of how its result can be checked (for example a gameplay
   scenario, an asset inspection or an automated test). This is not a detailed test plan.
   Do not split solely by interfaces, DTOs or files, or disguise overlapping scope.
   Cover every requirement and criterion; do not add mechanics outside the contract.
4. Resolve unknowns that change scope, ownership feasibility or task order before
   asking for plan acceptance. Other bounded design details can be assigned to the
   appropriate architecture stage. Name each material uncertainty and its effect;
   ask a concrete question rather than choosing an unsupported solution. No exact
   class signatures, exhaustive file inventories or implementation estimates here.
   For each subtask explain the logic it adds, the proposed operating concept,
   assets expected to be created/changed and key entities with their roles. Label
   proposed entities as proposed; do not present them as existing project mechanisms.
5. Prepare `.ai/features/<feature>/feature-plan.md` using the
   [feature plan template](templates/feature-plan.md). Present the useful findings and entire
   proposed plan in chat, then request acceptance or corrections. Revise this same
   proposal based on feedback. Edit this same document in place; do not create
   numbered revisions, alternate plan files or separate change reports.

## Response Standard

Use the developer's language and present:

- Research summary: relevant existing mechanisms, their names/roles and reuse/new
  work rationale. Keep only findings that affect the plan.
- Brief description of each subtask before the overview table: purpose and added
  logic; how the mechanism will work at concept level; expected created/changed
  assets, if any; key existing/proposed entities and what each is responsible for.
  This is a working concept, not code, final architecture or an exhaustive change list.
- One plan table: `FEAT-ID`, observable outcome, covered `RQ/AC`, scope, dependencies
  with their reason, and how to check the result. Make task order explicit.
- Blocking questions or material risks, if any; no empty sections.
- The exact acceptance decision requested. Routing after this result belongs to
  the coordinator; this skill does not prescribe or start the next stage.

A file link supplements the proposal; it does not replace it. Describe future
checks as planned, not passed. Do not present detailed architecture, raw logs or
an artificial collection of alternative plans.

## Output and Completion

This skill changes only `feature-plan.md`; it creates no state, requirements, decision,
architecture, subtask or extra report files. Resolve a missing feature ID before
choosing a destination. The coordinator owns state and subsequent dispatch.
On an authorized plan revision, preserve completed work, stable task IDs and prior
acceptance; show the exact proposed delta in the same document. Do not add, remove,
split or reorder accepted tasks on your own. An unchanged accepted plan needs no
new approval, rewrite or additional report.

Return the plan document, proposed task order, unresolved blockers if any and actual
developer acceptance basis when available. Missing material input means `blocked`;
a complete proposal awaiting acceptance means `waiting_for_approval`. The stage is
accepted only when blocking planning questions are resolved and the developer
accepts the plan. Return the result to the coordinator without selecting another stage.
No code/asset changes, test runs, commits, subagents or Notion calls belong here.

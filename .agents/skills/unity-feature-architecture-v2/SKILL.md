---
name: unity-feature-architecture-v2
description: Design and obtain developer acceptance of a technical solution for one agreed TroughGame feature subtask. Run only when explicitly dispatched at the new workflow's architecture stage or directly requested. Do not implement, rewrite the feature plan or start another stage.
---

# Feature Architecture V2

## Entry and Inputs

Run at `architecture` with permission to enter this stage, or on a direct developer
request to use this skill. Inputs are the feature ID, selected `FEAT-*`, accepted
`requirements.md`, accepted `feature-plan.md` and relevant existing decisions.
Check that the subtask is in the accepted plan and its prerequisites are available.
Report a missing input or dependency without choosing a different task or changing
the plan. The coordinator owns task selection and workflow state.

## Task

1. Inspect the selected subtask's actual integration points: relevant scripts,
   types, assets, objects and their callers. Follow AGENTS, architecture rules and
   naming conventions. Use scene/UI/lifecycle guidance only where this solution
   needs it. Reuse planning findings; inspect further only to settle design details.
2. Explain current behavior and the proposed change. Define the involved entities,
   their roles and ownership, what is reused/changed/created and the execution chain
   from trigger to observable result. Describe each distinct key chain when there
   are several. Distinguish existing entities from proposals.
   Choose a coherent solution for this subtask; do not invent unrelated extension
   points, speculative systems or alternative designs without a real tradeoff.
3. Specify the boundaries implementation must respect: affected scripts/types and
   assets/objects, responsibility and dependency direction, significant data and
   cross-type contracts. Show signatures or serialized fields where they determine
   integration/compatibility. Include short illustrative code/pseudocode for the key
   implementation decisions; do not write full implementations or enumerate trivial helpers.
   Identify the owning context/feature. Names and roles are sufficient unless a
   location is needed to disambiguate or agree a new asset's placement.
4. Cover relevant runtime details: DI/initialization, readiness and cleanup,
   subscriptions/async work, network authority and peer roles, serialized references
   or UI state ownership. Omit inapplicable detail. For runtime UI, use
   `unity-ui-toolkit` for the architecture/resource proposal within this same stage;
   do not start its prototype/implementation phases or create separate documents.
5. State how to verify the proposed behavior and its affected boundaries. Identify
   relevant automated checks and runtime/manual scenarios, with expected outcomes.
   Resolve unknowns that affect behavior, ownership or integration before requesting
   acceptance; report the precise question instead of presenting assumptions as facts.
6. Prepare `.ai/features/<feature>/subtasks/<subtask>/architecture.md` using the
   [architecture template](templates/architecture.md). Present the solution in chat
   and request developer acceptance or corrections. Edit this same document on
   feedback; do not create numbered revisions or additional architecture reports.

## Response Standard

Use the developer's language. Give a concise but complete preview of the planned
implementation, so the developer can understand its important behavior and design
before code exists. Present these items in order, omitting only inapplicable detail:

1. **Subtask and scope:** the selected FEAT, its result and agreed boundary.
2. **Expected behavior:** what will happen in the game, current → proposed behavior,
   including consequential conditions and outcomes.
3. **Key execution chains:** trigger → participating entities/actions → state change
   or observable result. Show every distinct key chain; do not compress multiple
   mechanisms into one vague sequence. Use a short explanation or diagram as useful.
4. **Entity tables:** separate scripts/types, prefabs and other assets into their
   own tables; add scene-object or other categories only when needed. Each row names
   the entity, state (`reuse`, `change`, `new`), and concrete role/change: why it is
   needed, how it is used and what it will do. Name the owner and important contracts,
   data or serialized references where consequential. Do not mix unlike categories
   in one table or list unrelated entities. Clearly distinguish proposed additions
   from existing entities; no empty tables.
5. **Key code/pseudocode fragments:** illustrate the few most important decisions,
   interactions or algorithms, with a brief explanation of what each demonstrates.
   Keep fragments focused; do not expand them into a full implementation. Label
   pseudocode and proposed APIs; do not imply that sketches are existing/compiled code.
6. **Dependencies on other systems/features:** name each key dependency, why it is
   needed, how/where this subtask uses it and the relevant boundary/contract. Explain
   the interaction rather than only naming a system or DI dependency.
7. **Runtime details:** relevant ownership, initialization, lifetime/cleanup, network
   roles/authority or UI behavior needed to understand the solution.
8. **Decisions, risks and questions:** meaningful tradeoffs and unresolved choices.
9. **Verification:** what will be checked and expected observable results.
10. **Acceptance:** the exact architecture decision requested from the developer.

A file link supplements this preview; it does not replace it. Keep existing scope,
contracts, runtime details, risks and verification visible while using these expanded
items; do not trade them away for tables or snippets. Avoid repeating the whole
feature plan or generic checklists. Proposed checks are not passed checks. Routing
after this result belongs to the coordinator. Code sketches belong only in the
response and architecture document; source/asset implementation remains out of scope.

## Output and Completion

Change only this subtask's `architecture.md`. Do not edit code/assets, requirements,
the plan, decisions, state or other subtasks. Resolve missing feature/subtask IDs
before choosing a destination. No test runs, experiments, commits, subagents or
Notion calls belong here. If live inspection is needed, use the installed Unity CLI
skill read-only; do not start an Editor or modify objects to test a design.

For a requested revision, preserve the prior acceptance basis and identify the
material change in the same document. A change to agreed requirements or task scope
must be reported to the coordinator for a developer decision, not silently accepted
as architecture. Unchanged approved design needs no repeated approval or rewrite.

Return the architecture document, remaining blockers if any and actual developer
acceptance basis when available. Missing material input or unresolved design choices
mean `blocked`; a complete proposal awaiting acceptance means `waiting_for_approval`.
The stage is accepted only when blocking choices are resolved and the developer
accepts the solution. Return control without selecting or starting another stage.

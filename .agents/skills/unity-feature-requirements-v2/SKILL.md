---
name: unity-feature-requirements-v2
description: Prepare and obtain developer acceptance of a TroughGame feature's behavior, scope and acceptance criteria. Run only when explicitly dispatched at the new workflow's requirements stage or directly requested by the developer. Do not research implementation, design architecture or start other stages.
---

# Feature Requirements V2

## Entry and Inputs

Run at `requirements` after the developer starts `unity-feature-development-v2`,
or on an explicit developer request to use this stage skill. Inputs are the feature
ID, selected specification/text, developer answers and any existing requirements.
Do not read the old workflow or its reference files.

## Notion Source Selection

When the developer selects Notion as a source, this stage owns the intake in both
coordinator-driven and direct invocation. No separate developer command for the
Notion skill is needed. Reuse a supplied usable packet for the selected sources.
Otherwise load [notion-feature-requirements](../notion-feature-requirements/SKILL.md)
and dispatch its isolated worker with the repository root, absolute skill path,
selected source URLs/IDs or page titles, and only necessary feature context/questions.
Follow that skill's read-only MCP and handoff rules; never call Notion MCP directly.
An ambiguous title requires source selection, not guessing. Incomplete intake may
request only the specific missing source through the same skill; do not repeat
already usable reads or refresh sources on resume or ordinary requirement edits.
Missing access or unavailable delegation blocks source-dependent work; report the
exact gap. For local documents or developer-provided text, do not invoke Notion.

## Task

1. Read the supplied sources and relevant existing requirements. Extract the intended
   player/game outcome, explicit behavior, constraints, exclusions and dependencies.
   Keep a source reference for each requirement: document section, source URL or
   developer statement. Distinguish source facts from proposed interpretation.
2. Form short, stable `RQ-*` requirements and observable `AC-*` acceptance criteria
   linked to them. A criterion describes conditions/action and expected behavior,
   not the presence of a class, method or asset. Mark newly derived criteria as
   proposals until accepted. Do not invent mechanics or convert guesses into scope.
3. Ask only questions whose answers change behavior, scope or acceptance. Explain
   the missing/conflicting fact and its impact. Omit questions already answered by
   the supplied sources. If no source describes the requested feature at all, ask
   for that input before creating a speculative requirements document.
4. Prepare `.ai/features/<feature>/requirements.md` using the
   [requirements template](templates/requirements.md). Replace its instructions
   with actual content and omit empty optional sections. Keep unresolved material
   questions visible; the document is a draft until accepted.
5. Present the complete concise proposal in chat and request acceptance or corrections.
   Resolve feedback within this stage. Record acceptance only for the exact content
   the developer accepted; document/source status and silence are not acceptance.

## Response Standard

Use the developer's language. Present these items in order, omitting empty ones:

- Goal: the expected game outcome in one or two sentences.
- Requirements: each `RQ-*`, its behavior/constraint and source.
- Acceptance criteria: each `AC-*`, the observable condition and related `RQ-*`.
- Scope boundaries/dependencies and blocking questions, if any.
- The exact decision requested and the proposed next stage, `planning`.

Use short lists or a compact table as appropriate. A file link supplements the
proposal; it does not replace it. Do not include implementation inventories,
architecture options, invented estimates, raw source dumps or claims of passed tests.

## Output and Completion

The only feature file this skill changes is `requirements.md`. Use the stage skill's
template; do not create research, decisions, architecture, reports or state files.
The coordinator owns state and transitions. On a direct invocation without a feature
ID, resolve the ID with the developer before choosing a destination.
For an existing accepted document, preserve IDs and acceptance history; propose only
the requested changes and require acceptance of a material revision. Unchanged
accepted requirements need no repeated approval or new file.

Return the requirements path, unresolved questions if any, and actual acceptance
basis (developer decision/reference) when available. Source gaps mean `blocked`;
a complete unaccepted proposal means `waiting_for_approval`. The stage is accepted
only after material questions are resolved and the developer accepts its content.
Hand that result back to the coordinator. Planning is next, but this skill never
starts it. Requirements acceptance alone does not grant permission to transition.
No code/asset edits, tests, commits or remote writes are part of this stage.
The isolated read-only Notion worker above is its only permitted subagent.

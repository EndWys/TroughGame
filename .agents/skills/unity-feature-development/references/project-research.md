# Project Research Reference

## Purpose

Use this reference before feature decomposition. The goal is a short, evidence-based Research Brief and initial feature plan that connect approved behavior to the relevant existing project systems before any subtask architecture is proposed. The same applicable content is shown to the developer in chat and preserved in the feature artifact.

Research is conditional: inspect only concerns that can affect the requested feature. Do not pad the result with empty areas or raw search output.

Record the approved brief in `.ai/features/<feature>/research.md` and its compact status/reference in `state.yaml`.

## Requirements source check

For each material requirement and Acceptance Criterion, record its authoritative source: document section/page, approved developer answer or existing approved decision.

Before asking a requirements question:

1. Re-read the authoritative document and search its relevant section.
2. State the exact source that was checked.
3. Ask only if the source is absent, contradicts another authority, or leaves a behavior or implementation-impacting choice genuinely open.

Do not ask a developer to repeat a requirement that is directly specified by an authoritative source.

## Research Brief

Use only relevant rows.

| Requirement / AC | Requirement authority | Concern and search basis | Existing evidence or negative finding | Reuse decision | Planning implication |
| --- | --- | --- | --- | --- | --- |
| `RQ-##` | `<GDD section/page or decision>` | `<responsibility/data boundary>` | `<concrete path/type/API, or bounded search result>` | `<reuse, extend or new>` | `<scope, ordering, risk or validation consequence>` |

The `Existing evidence` column must name a verifiable artifact when one exists: path, type, interface, method, prefab, ScriptableObject, installer or test utility. A negative finding states the searched responsibility and location, not merely “nothing found”.

## Relevant concern prompts

Consider these only when the feature can interact with them:

- gameplay rules, income/rewards/inventory and player state;
- Fusion synchronization, peer ownership, reconnect and peer authority;
- UI/View ownership, navigation, prefabs, serialized references and visual assets;
- configuration, ScriptableObjects, DI registration and initialization ownership;
- persistence, backend/API, telemetry and localization;
- existing tests, validation utilities and realistic Play Mode scenarios.

For a changed public contract, also search its implementations, callers and parallel variants before proposing a signature or behavior change.

## From research to plan

### Confirmed integration map

When it materially determines ownership, scope or subtask ordering, show the discovered end-to-end path:

```text
<existing trigger>
  -> <existing owner>
  -> <existing model or system>
  -> <existing event, state update or replication>
  -> <existing View or observable result>
```

Name real discovered types, methods, events and Unity objects. Omit this map when the change does not have a relevant cross-system path.

### Proposed initial feature plan

| ID | Verifiable outcome | Included scope | Reuse / expected change | Dependencies | Validation |
| --- | --- | --- | --- | --- | --- |
| `FEAT-01` | `<concrete result>` | `<systems, known files and Unity objects>` | `<reuse, extend or new>` | `<predecessor and reason, if any>` | `<automated and Play Mode>` |

The plan must name:

- the existing mechanisms that each subtask will reuse or extend;
- the evidence-based dependency ordering between subtasks;
- scope boundaries justified by ownership or a verified non-impact;
- a spike or a developer decision for each material unresolved unknown.

Every listed subtask is a proposal until the developer approves the initial plan. Do not create it in state, begin its local architecture or modify project files before that approval.

### Execution order

Show the dependency sequence when there is more than one subtask:

```text
FEAT-01 -> FEAT-02 -> FEAT-03
```

State the reason for each dependency; do not use arrows as a substitute for it.

### Scope boundaries and unresolved decisions

Include only relevant items:

| Topic | Evidence | Effect | Required developer decision |
| --- | --- | --- | --- |
| `<scope boundary or real unknown>` | `<research finding>` | `<effect on plan or behavior>` | `<smallest concrete decision, only if needed>` |

Observed evidence, inference and proposal must remain distinguishable. Do not present an unverified assumption as an existing project convention. Omit empty boundary, risk or decision sections.

## Developer-facing research and plan approval

Present the complete applicable Research Brief, integration map and proposed initial plan in chat before requesting approval. Do not reduce the response to raw search logs or a link to `research.md`: the developer must be able to review the evidence, scope, ordering, risks and validation without opening the artifact.

The approval request states that approval records the listed subtasks as the initial feature plan and starts local architecture for the first subtask. A request for changes revises research or the plan before any subtask work begins.

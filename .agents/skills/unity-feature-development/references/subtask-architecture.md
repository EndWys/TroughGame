# Subtask Architecture Reference

## Purpose

Use this reference before local-architecture approval. The plan must make the intended implementation reviewable before any code or Unity-managed state changes: what will change, who owns it, how it is invoked, and how it will be verified. The same applicable content is shown to the developer in chat and preserved in the feature artifact.

Include only sections that apply to the approved subtask. Do not create “none”, “not applicable” or speculative file rows.

Record the approved plan in `.ai/features/<feature>/subtasks/FEAT-XX.md` and link it from the matching `state.yaml` subtask entry.

## Required plan contents

### 1. Scope and outcome

State the concrete observable result, included systems and explicit exclusions. An exclusion needs a short reason only when it is a plausible adjacent concern discovered during research.

### 2. Change inventory

| Path | Object / type | Status | Responsibility / behavior | API / data | Dependencies |
| --- | --- | --- | --- | --- | --- |
| `<file or asset>` | `<type, object or feature>` | `existing / reuse`, `extend`, `new` or `new asset` | `<what it will do>` | `<method, state, event, field or serialized reference>` | `<callers, owners or required systems>` |

List every expected production file and Unity-managed asset. Separate a new file from a modified one. A conditional item must name its condition. `existing / reuse` means it is an intentional integration point, not an unchanged file that happens to exist nearby.

### 3. Data, state and lifecycle

Describe the concrete behavior from trigger to observable result. Name real `Type.Member(...)`, events, models and Views rather than an abstract sequence. State data ownership, state transitions, initialization prerequisites, subscription or cleanup rules and authority boundaries when they apply.

For every new or changed cross-type contract, state its signature or serialized form, data owner, callers, implementations and compatibility effect. A contract includes an interface, public/internal method used across types, property, event, SignalBus signal, data field or serialized reference. If an existing contract is reused unchanged, name it and its role rather than restating its implementation.

Show the execution chain when it materially clarifies the change. For example:

```text
<player action / network event>
  -> <owner system or handler>
  -> <service/model mutation>
  -> <signal or model update>
  -> <view/controller update>
  -> <observable result>
```

Explain any cross-feature dependency, direction of ownership and initialization prerequisite. Use the lifecycle dependency map when `unity-lifecycle-safety.md` applies.

### 4. Unity / DI / serialized integration

When the subtask changes Unity-managed state, name the exact scene/prefab/GameObject/component/serialized field/installer/ScriptableObject/localization entry/visual asset and describe the required binding or hierarchy change. State whether the object is modified, created or removed.

### 5. Design choices and extension points

Name a pattern, principle or extension point only when it materially shapes the solution. State why it fits this feature and how a future extension would use it. Do not force a pattern into a simple change.

### 6. Risks, decisions and scope conflicts

List only material risks, unresolved choices and actual scope conflicts. For every developer decision, state its effect and ask the smallest concrete question that unblocks implementation. Do not create an empty risk or scope-conflict section when there is nothing to report.

### 7. Validation

Map each relevant change to the narrowest validation: automated test, connected-Editor check, Inspector/Console check or reproducible Play Mode scenario. State the expected evidence and any manual role for the developer.

## Approval gate

Stop for an architecture decision when an essential owner, public contract, lifecycle precondition, Unity object or integration boundary cannot be established from approved requirements and research. Do not defer those choices to implementation.

## Developer-facing architecture handoff

Present the same applicable sections and change inventory in the architecture response before requesting approval. Do not reduce the response to a link or a generic summary: the developer must be able to review the scope, concrete objects, behavior, integration, risks and validation without opening `subtasks/FEAT-XX.md`. The artifact is the durable copy of the approved plan, not the primary place where its content is revealed.

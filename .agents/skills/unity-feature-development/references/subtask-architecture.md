# Subtask Architecture

## Purpose

Before code/Unity edits, expose what changes, who owns it, how it executes, and
how it is checked. Save approved architecture in `subtasks/FEAT-XX.md` and link
from the subtask. Present all applicable content before approval; omit empty/
speculative rows.

## Plan Content

1. **Scope/outcome:** observable behavior, included systems, explicit exclusions.
   Explain exclusions of plausible neighboring areas.
2. **Change inventory:** all expected production files/Unity assets, separated
   into new/changed/intentionally reused. Conditional items name conditions,
   not invented filenames.
3. **Data/lifecycle:** actual `Type.Member`, events, models/Views, data owners,
   transitions, readiness, authority, and cleanup.
4. **Unity/DI/serialization:** exact scenes/prefabs/GameObjects/components,
   fields, installers, config, localization/visual assets, bindings/hierarchy;
   specify creation, modification, or removal.
5. **Design choices:** only consequential patterns/extension points, reason,
   and future usage. Do not force patterns onto simple edits.
6. **Risks/decisions:** material risks, open choices, scope conflict, impact,
   minimal question. Omit empty sections.
7. **Validation:** narrow checks for each change, expected evidence, and necessary
   manual developer role.

| Path | Type / Object | Change | Responsibility / Behavior | API / Data | Dependencies |
| --- | --- | --- | --- | --- | --- |
| File/asset | Actual name | Reuse / extend / create | Concrete role | Method/field/event/reference | Callers/owners/systems |

For new/changed cross-type contracts show signatures/serialized shape, owner,
callers, implementations, and compatibility: interfaces, `public`/`internal`
methods, properties, events, SignalBus, data, and serialized references.
Unchanged contracts only need their name and role.
Use an actual execution chain when useful:
action/network event → owner/handler → service/model → signal/state → View/controller
→ observable outcome. Explain cross-feature dependencies and prerequisites.
Use the lifecycle reference where applicable.

## Approval and Result

For runtime UI use [unity-ui-toolkit](../../unity-ui-toolkit/SKILL.md) and its
[architecture forms](../../unity-ui-toolkit/references/response-formats.md).
Show hierarchy, asset search/inventory, selected presentation/data/input contracts, per-view
lifetime, scaling and collection strategy as applicable. Include these in this
one approval; do not add a second architecture gate. The approved implementation
sequence includes visual prototype acceptance before complete functional wiring.
MVVM is the default; an MVP proposal includes concrete benefit, the compact MVVM
alternative, View contract and one state/action owner in this same approval.

Do not defer material owners/contracts/lifecycle prerequisites/Unity objects/
integration boundaries to implementation. Missing evidence requires a technical
question/experiment and a stop. The developer sees scope, objects, behavior,
integration, risks, and checks without opening a file. Artifacts retain the
accepted solution rather than hiding it from review.

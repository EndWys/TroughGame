# FEAT-01 Architecture Revision 9 — Component Availability

## Approval and Scope

APR-009, developer 2026-10-16: «Такой вид контракта мне нравиться» with the
explicit correction that CombatHurtboxComponent.IsComponentValid must use only
`_origin != null`, without a UnityEngine.Object lifetime pattern.
This accepts the preceding concrete proposal. Keep FEAT-01 and the five-subtask
plan; review revision 2 remains historical and unaccepted as a complete diff.

## Contract and Execution

Add readonly IsInitialized, HasValidNetworkContext and IsComponentAvailable to
INetworkEntityComponent. Both existing bases expose the same properties:
availability is initialization AND valid parent/Object/Runner AND
isActiveAndEnabled AND protected virtual IsComponentValid (default true).
Evaluation short-circuits before local rules when prerequisites are false.
Remove the narrower processor.CanExecute method to avoid two availability meanings.
The processor retains initialization/reset and network validity responsibilities.

Public readiness getters are pure. NetworkTick/ClientRender use TryBeginCallback:
if available execute the hook; otherwise only initialized + invalid network
context invokes the existing idempotent OnNetworkContextInvalidated hook.
Disable or a false local validity rule skips callbacks without resetting local
state or the contact ledger. Init and ResetLocalRuntime remain unconditional;
authority/forward-only rules stay in the owning components. Disable pauses
callbacks, not network time, and does not automatically cancel an action.

Hurtbox IsAvailable aliases IsComponentAvailable; its WorldShape getter throws
InvalidOperationException while unavailable. Protected IsComponentValid is
exactly `_origin != null`. Mandatory origin and shape validation remain in Init.
There is no destroyed-Unity-object probe beyond the developer-approved simple
interface null comparison. No automatic validation of Unity object lifetimes is
claimed by that rule.

## Exact Change Inventory

All production sources are under Assets/ProjectCore/Contexts/GameCore/Features.

| File | Exact change / compatibility |
| --- | --- |
| NetworkEntities/Scripts/Abstract/INetworkEntityComponent.cs | Add three readonly availability properties; migrate the two test doubles. |
| NetworkEntities/Scripts/Abstract/BaseNetworkEntityComponent.cs | Public network validity and common availability, virtual local predicate, shared callback-entry guard. |
| NetworkEntities/Scripts/Abstract/BaseNetworkEntityBehaviourComponent.cs | Same contract/guard for NetworkBehaviour consumers. |
| NetworkEntities/Scripts/Other/Processors/NetworkEntityComponentLifecycleProcessor.cs | Remove CanExecute; retain context check and lifecycle behavior. |
| Combat/Scripts/Components/CombatHurtboxComponent.cs | Common availability alias, simple origin predicate, strict WorldShape getter. |
| GameCore/Tests/Editor/NetworkEntityComponentLifecycleTests.cs | Update processor API assertions/test double; verify readiness/invalid callback distinction and unconditional reset. |
| GameCore/Tests/Editor/CombatTargetQueryServiceTests.cs | WorldShape requires initialized/spawned availability; remove offline component geometry reads and keep pure geometry coverage. Add common test-double properties. |
| TechnicalPrototype/Tests/Play/PlayerCombatPhasePlayModeTests.cs | Move component/world geometry assertions to actual spawned object; verify both bases' disable/init/reset availability, dynamic local validity, actual network-context guard, and probe disable/re-enable dedup within one execution. |

No new production type, serialized field, DI binding, prefab, scene, asset, meta,
assembly or package change. Existing ICombatHurtbox and combat snapshot APIs remain.

## Validation

- Connected Editor compilation and full Edit Mode.
- Existing tick-driven Solo fixture plus spawned availability/geometry and
  same-execution probe disable/re-enable checks. Keep assertions on origin/offset,
  ignored visual/root scale and Z, rotation, contact uniqueness and reset.
- Runtime build, strict architecture and tracked git diff --check.
- Separate audit and complete revision 3 review snapshot. Existing strict PNG
  baseline and pending owner/Host-client/live replay/Fusion pool checks stay visible.

Before implementation all 150 revision 2 source hashes matched; real index empty;
HEAD d9d3914e6b54c62659b0cdeacbcbf18d12dec90b. Preserve unrelated assets/stashes.

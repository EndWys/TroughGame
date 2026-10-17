# FEAT-01 Revision13 — Passive Player Mediator Correction

## Current Acceptance / Temporary Limitation

APR-014: owner explicitly accepts this exact solution TEMPORARILY and requests
implementation. PlayerActionControllerComponent is temporary actor-level coordination,
not the approved final general architecture. Keep this limitation visible for later
review; it does not authorize an additional redesign or subtask now. Runtime/prefab
implementation and validation are underway. Historical proposal below retained.

## Initial Status / Authority

Developer explicitly requires PlayerMediatorComponent to route Notify to handlers,
with no gameplay/input/action logic in its NetworkTick. This role correction is
accepted input. The concrete replacement owner/composition below is proposed,
awaiting architecture approval; no production/Unity edits performed. Continue the
same FEAT-01, unchanged five-subtask plan. No bug skill/diagnostic state for this
architecture feedback. R12 two-file source hashes match, index empty.

## Proposed Ownership / Execution

Create TechnicalPrototype PlayerActionControllerComponent as the player-level
buffered-command owner. Move the existing mediator OnNetworkTick cycle into it,
without changing action admission policy/timing: read already captured input from
PlayerInputSourceComponent, update existing latest direction state, expire/read
independent buffer, respect full Dodge and attack recovery restrictions, consume
one eligible command, call attack StartAttack or force movement Dodge.

This actor-level composition may refer to Combat and Movement; neither feature
nor movement state/processors acquire a dependency on the other feature. Combat
still reports attack start/stop through the existing mediator; Dodge still reports
phase/effect lifetime through the mediator. Input source only reads/buffers network
input, Combat only executes attacks, Movement owns Dodge. No new input buffer,
state/timer/config, command priority, feature or generic framework.

Mediator keeps Notify, existing handlers and their required DI/initialization/reset
infrastructure. Remove its OnNetworkTick override, combat/dodge/input-source fields,
latest-direction/facing DI dependencies. Handlers retain Movement transitions,
buffer completion/cancellation and finite invulnerability mapping. No hidden Update,
FixedUpdate, coroutine or externally driven replacement tick method in mediator.

## Exact Inventory / Contracts

| Path (below Assets/ProjectCore/Contexts/TechnicalPrototype) | Change / public contract |
| --- | --- |
| Features/Player/Scripts/Components/PlayerActionControllerComponent.cs + .meta | New sealed BaseNetworkEntityComponent; inherited lifecycle only, no new public method/interface/event. Serialized PlayerInputSourceComponent _inputSource, PlayerCombatActionComponent _combatAction, PlayerMovementStateMachine _movementStateMachine, PlayerDodgeState _dodgeState. DI IInputBufferController, IMovementStateDataAccessor<PlayerMovementState>, IPlayerFacingDirectionAccessor, optional ICombatActionMutator. |
| Features/Player/Scripts/Components/PlayerMediatorComponent.cs | Remove OnNetworkTick; keep handlers/Notify/lifecycle. Remove serialized _combatAction/_dodgeState/_inputSource and DI ICombatActionMutator/IPlayerFacingDirectionAccessor. |
| Features/Player/Scripts/Components/PlayerNetworkEntityComponent.cs | Add serialized PlayerActionControllerComponent _actionControllerComponent and exactly one CreateComponents entry after mediator, before movement. Preserve accepted R12 getter. |
| Features/Player/Assets/Prefabs/Prefab_Player_NetworkEntity.prefab | Add controller on root through Editor; wire its four colleagues and root reference; resave removed mediator fields. Preserve GUIDs, existing children/components/configs. |
| Tests/Editor/PlayerPrefabTests.cs | Verify controller/root/colleague composition in existing prefab fixture. |
| Tests/Play/PlayerCombatPhasePlayModeTests.cs | Existing9 regressions; new behavior scenario: queued command plus mediator.NetworkTick does not consume/start; controller.NetworkTick consumes/starts through existing attack notification path. |

Existing PlayerNetworkEntityInstaller bindings suffice: controller is injected as
part of GameObjectContext and uses existing root/InputBuffer interfaces. No new
installer binding or assembly/package/config/scene migration.

## Lifecycle / Unity

Root component order becomes input, hurtbox, attack, mediator, action controller,
movement, body, probe, camera, target. Relative attack-update/command-admission/
movement order is identical to R11/R12. Init validates configured references only;
no colleague gameplay during Init/reset. Controller authority/readiness guards are
preserved from prior cycle; no mutable local gameplay source of truth introduced.
Prefab addition/meta generated and wired through connected Editor. Do not hand-edit
asset semantic changes. Existing mediator handler readiness guards remain.

## Validation / Handoff

Connected compile, full Edit, existing9 Solo plus new passive-mediator/controller
behavior scenario; runtime build, strict existing PNG baseline, diff check and
focused review. Verify direction changes, buffer expiry while locked, attack window,
full Dodge Recovery, finite effect expiry, cleanup and single component tick.
Host/client/live replay/pool limitations remain explicit. No commit before full
accepted FEAT gates; no Notion publication of this development-process correction.

## Concrete Unapplied Code Proposal

FEAT-01-passive-mediator-r13-proposed.patch contains the exact proposed mediator
removals, complete new controller class and root field/tick-list additions. The
current OnNetworkTick body is relocated unchanged to the controller. Unity prefab
wiring and regression fixture edits remain as specified above, to be implemented
only after architecture acceptance. This patch is a review artifact, not applied
production code. No new public method/interface/payload or installer binding.


## R13 Temporary Mediator Correction Implemented — 2026-10-17

APR-014 temporary architecture implemented: mediator has no gameplay NetworkTick;
new controller owns original admission cycle, root/prefab wired through Editor.
Compile passed, Edit674/674 and Solo10/10 passed, build0errors/10warnings, strict
same4PNG ASSET003, finaldiffcheck passed. Focused7-file audit P0/P1/P2=0/0/0.
Owner diff/Play acceptance pending, full FEAT audit/closure not inferred. Temporary
solution limitation retained; no Notion call, commit or next FEAT.

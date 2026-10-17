# FEAT-01 Implementation Architecture Revision 11

## Authorization / Reconciliation

APR-011, developer 2026-10-17: «Реализаем этот план» explicitly authorizes the
agreed correction plan, including APR-010 requirements revision6. No repeat
approval requested for these decisions. Keep FEAT-01/five-part plan.149/150
review3 hashes match; one inspected user edit is trailing whitespace in the
processor being replaced. Index empty, HEAD d9d3914e6b54c62659b0cdeacbcbf18d12dec90b.
Fresh Notion operation combat-r11-implementation-refresh-20261017 completed with
request SHA256 0aaaa0b9350dbf123e777455dc139b31d9aaa1cf2c4b0d04b198fa21ac45cf25;
no delta, spec23:11:57.412Z/task2026-10-12T17:30:47.137Z unchanged. Metadata
completeness unavailable; no writes. R9 evidence is historical.

## Exact Implementation / Ownership

- Rename/move IMovementOriginAccessor + meta through Editor to NetworkEntities
  Abstract/IEntityPositionAccessor, Position Vector2. Root implements root worldXY;
  body injects it and collision Position adds offset once. Combat consumes root
  position; remove installer body-origin bindings/unused fields. Update tests.
- PlayerCombatActionComponent becomes attack-only: no movement body, Dodge profile,
  buffer or input source. StartAttack(Vector2) owns attack state/facing and sends
  PlayerAttackStatePayload through IPlayerMediator; attack tick completes expired
  attacks. CancelAction sends stopped/clear-input intent via mediator. Preserve
  snapshot/resolver common interfaces and attack timings/network model.
- Existing PlayerMediatorComponent owns entity-level command admission via existing
  independent IInputBufferController. It tracks latest input direction, permits
  attack recovery chaining, prevents transitions out of incomplete Dodge, consumes
  only valid buffered command, starts attack or forces Dodge through existing
  ChangeState. No generic override/provider or separate buffer. Existing action
  completion ticks before mediator, then movement/body/probe retains order.
- PlayerMovementStateMachine loses all Combat reads. Its existing BeforeMovementUpdate
  only reconciles local entered state with replicated movement state; AfterNextStateEnter
  records local activation. Attack/Dodge references may be omitted for mover-only
  composition. Public ChangeState remains inherited; no redundant wrapper.
- Attack Enter zeros body velocity, no attack processor; remains Attack until mediator.
- Dodge owns phased movement/config/timing. Extend DodgeMovementConfig with
  preparation/recovery defaults0.03/0.10; retain existing DurationSeconds0.20 as
  Active for legacy consumers. Add DodgeTimingData (three quantized tick lengths)
  and DodgeMovementPhase. Extend existing timer mutator (not its common accessor)
  with StateTimerEndTick and StartStateTimerTicks. Root retains one existing
  Networked MovementStateTimer; uses CreateFromTicks(sum phase ticks), preserving
  exact boundaries. Tick-derived phases via end tick minus total length. No new
  Dodge clock/network phase/cache. Init validates config without reading Runner;
  runtime entry compiles timings; restored running timer is not restarted.
- Rename old PlayerActionMovementProcessor to PlayerDodgeMovementProcessor through
  Editor, movement-only body/config/current-phase/timer access. Dodge state sends
  PlayerDodgePhasePayload(phase,activeEndTick) every simulated tick; Exit sends end.
  Recovery must finish fully. Movement/facing logic outside those states unchanged.
- Add minimal ICombatInvulnerabilityAccessor/Mutator, backed by root Networked
  TickTimer InvulnerabilityTimer. Mediator maps Dodge Active interval to a finite
  effect deadline, inactive/end to clear. IsInvulnerable computes unexpired timer;
  missing off message cannot extend lifetime. Root reset/spawn clears timer. This
  supplies requested state plumbing, not FEAT-02 damage/HP/hitstun/death logic.
- Root order remains input,hurtbox,action,mediator,movement,body,probe,camera,target.
  Initializers do not notify unready colleagues; mediator runtime handlers guard
  component readiness/authority. Unconditional local reset sends no network commands.
  Root body cleanup owns velocity reset. No animation handlers called from new
  simulation payloads; existing render notifications stay presentation-only.

## Source / Asset Inventory

Existing: both Combat components, BaseNetworkEntityRoot, TransformMovementBody,
PlayerCombatAction, PlayerMediator, PlayerMovementStateMachine, PlayerAttackState, PlayerIdleState,
PlayerDodgeState, Player/Enemy installers, Player root, DodgeMovementConfig,
MovementConfigValidation, IMovementStateTimerMutator; renamed position interface
and player processor; six affected existing Editor/Play fixture sources.
New: position interface moved with meta; Movement DataHolders/Data/DodgeTimingData,
Movement Enums/DodgeMovementPhase, Static/Utilities/DodgePhaseUtility; Combat Abstract invulnerability accessor/mutator;
Player mediator payloads PlayerAttackStatePayload and PlayerDodgePhasePayload.
Editor migrates Player/baseEnemy prefab references (mediator colleagues, removed
fields), verifies inherited Dash variant, imports new script metas. Dodge config
asset retains old values with new defaults. Old Combat Dodge profile is removed
from live actor wiring; serialized legacy common enum remains compatible.
No assembly/package/scene/ProjectContext changes. No new subtask/feature.

## Verification / Handoff

Pure phase/tick-wrap/buffer tests, mediator/DI no-Combat movement graph, root-only
position and geometry tests; connected compile, full Edit, affected Solo phases,
Dodge full Recovery/queued expiry/deadline/Attack reentry and previous6 scenarios;
build/strict/tracked diff check; separate audit and complete review4 snapshot.
No Host/client/live replay/real Fusion pool claim without execution. Required
owner full diff/Play acceptance pending; strict PNG baseline remains visible.
No stage/commit before required gates. Notion accepted implementation facts via
isolated worker after verification; main owns Git/Unity/state. Parallel packages
remain spatial and mediator/movement; implementation is kept in main agent under
current skill's ownership restriction.

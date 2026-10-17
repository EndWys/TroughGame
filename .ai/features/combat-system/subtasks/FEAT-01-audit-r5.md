# FEAT-01 Full Code Audit — Review5

## Scope / Immutable Evidence

Full current FEAT-01 source snapshot: FEAT-01-review-r5-manifest.json / .patch.
Digest `053bf5b931ab037047b6ff80e277c09c4ab5cef6abeca01882f75178e4ca2968`. Includes current working-tree timer API refinement.
Audit inspected all changed/new responsibility, spatial, timing, effect, actor,
lifecycle and composition paths since review3, and carries forward review3/review2
inspection for exact hash-matching unchanged paths. This is a full-scope incremental
audit, not a claim that old evidence tests a new source snapshot.

APR-015 accepts the focused temporary R13 implementation diff. It does not prove
owner Host/client/manual execution or final whole-FEAT acceptance. No source/asset
was changed by this audit. External timer changes were preserved, not overwritten:
EndTick moved to accessor, mutator keeps ticks-only start/stop, old float start
removed, Enemy accessor and two fixtures updated. New current-source checks run.

## Inspected Areas / Conclusions

- Root-owned IEntityPositionAccessor supplies XY world position independent of
  movement. Body collision offset applied once; hit/hurt geometry uses root origin.
  Moved interfaces/processors retain prior script meta/GUID. No obsolete code
  references to old origin/action movement processor or test-only contact event.
- Availability/lifecycle remains the approved universal contract, pure getters,
  local validity `_origin != null`, unconditional Init/reset and rollback/reverse
  child cleanup. Disable/local invalidity preserves contact ledger; network context
  invalidation clears output. Hash-matching earlier lifecycle inspection retained.
- All timer accessor/mutator implementations and consumers inspected, including
  Enemy inherited IEnemyDashStateAccessor and GameCore/TP test doubles. Current
  code has no StartStateTimer(float) call; EndTick null-normalization retained.
  Compiled phases have validated finite positive durations and bounded total ticks;
  half-open phase/window boundaries and tick-counter wrap tests retained.
- CombatAction executes attacks only, validates the selected attack definition,
  stores execution ID/start/direction in network state, derives phases from ticks,
  emits facts through mediator. Attack Enter owns zero velocity. No Combat snapshot
  dependency in Movement state machine, Dodge state or its processor.
- One input buffer retains last eligible command and finite lifetime. Callback
  sequence distinguishes simultaneous presses; held buttons do not repeat. Queries
  expire pending commands while locked, attack recovery window permits chaining,
  Dodge waits full Recovery; next attack uses last nonzero captured direction.
- TEMPORARY actor-level controller owns admission only, with existing readiness/
  authority guards. Mediator has no gameplay tick; Notify dispatch/handlers connect
  attack status to movement and Dodge phases to finite invulnerability. Root order
  remains input -> attack -> admission -> movement -> body -> contact probe, with
  explicit unified initialization before callback execution. No new hidden startup.
- Dodge uses one replicated movement timer. Local activation reconciliation avoids
  Exit/restarting/unlocking restored Dodge during state correction; pure timing
  controls velocity. Separate replicated invulnerability deadline cannot outlive
  its end tick if the end notification is absent. Full client prediction/replay
  and mid-action respawn/reinitialization are not claimed tested by local cache tests.
- Active contacts remain authoritative forward-only; replay skips preserve ledger,
  unique entity/execution pairs prevent repeats, source/self/unavailable filtering
  and unregister cleanup retained. Root caches registered identity for teardown.
  Damage/HP/life-state validation/AI/feedback/match flow belong to later approved FEATs.
- Prefab/DI/config composition inspected with exact source/asset hashes and existing
  Editor/Play integration tests. New controller registered once; root field/colleagues
  correct, movement-only graph still requires no Combat binding. Root identity,
  action/accessor/resolver bindings stay coherent. No new .asmdef/package/scene/
  ProjectContext edit. Shared API independence/namespaces remain within conventions.
- Existing Play fixture observations occur on forward ticks with caught assertion
  propagation and cleanup; test maintenance does not relax phase/buffer criteria.
  Local reset/despawn/fresh spawn/disabled callback cases are distinguished from
  actual Fusion pool reuse/disconnect/reconciliation. Missing network coverage stays
  visible as an acceptance gap, not a passed result or an invented defect.

## Findings

P0:0. P1:0. P2:1, previously recorded and unchanged.

| ID | Location | Level | Evidence / consequence | Recommended action |
| --- | --- | --- | --- | --- |
| AUD-001 | CombatTargetQueryService.FillTargetIds, lines27–33 | P2 | Per query allocates candidate List, captured predicate and HashSet; actual crowd cost not profiled. | Profile a representative crowd before deciding on collection/traversal reuse. No optimization or investigation performed without scope. |

Temporary controller architecture is explicitly owner-accepted debt, not silently
promoted to final reusable design and not a new finding requiring redesign now.
No new actionable findings. Existing four unrelated ASSET003 PNG-name errors remain
a known strict baseline; do not rename assets or hide failed Strict status.

## Validation / Remaining Gates

Actual current-source Compile/Edit/Play/build/strict/diff outcomes are indexed in
state.yaml and FEAT-01-review-r5-result.md after commands complete. Source hashes
must still match before reuse; a changed source invalidates corresponding checks.
Full snapshot/diff owner review and required Host/client/actual network lifecycle
Play acceptance remain distinct from the accepted focused R13 correction. No full
FEAT/subtask completion, checkpoint/commit, push or Notion workflow publication.

## Verification Completion Within Approved Plan

Full audit found that the already-approved command/gameplay-ID independence case
had only pure profile-validation coverage. Added one fixture-only Solo scenario:
valid buffered Attack command resolves gameplay ActionId300 (>127), snapshot/root
resolver agree and buffer consumes normally. It clones per-instance definitions,
restores originals/cancels in finally and destroys temporary clones. No production
code/assets/test hook changed. This is completion of planned verification, not a
bug diagnosis or an invented requirement. Gameplay/input models match frozen hashes;
retained contracts-layout.json confirms unchanged buffer8bytes/2words, action20bytes/
5words; no new bandwidth claim. Source digest/complete patch updated before final
verification. One Play request was not accepted during reload/server reset and is
recorded unavailable, not failed; resumed current suite result retained separately.

# BUG-003 — R11 Dodge Solo Scenarios Timeout

## Current Status — Proven Cause / Approved Fix

APR-012: owner «Да, исправь getter и заверши проверки» approved the exact
StateTimerEndTick normalization. One-line production fix applied; tests actualized,
full verification passed (Compile, Edit674/674, Solo9/9). Owner diff/retest acceptance recorded by APR-013; no commit.
Original timeout-only classification below was withdrawn; runtime cause was later
proved independently by actual assembly behavior and immediate-entry regression.

## Classification Correction — 2026-10-17

Developer rejected treating a failed test alone as a confirmed game bug and
explicitly authorized completing fixture actualization within FEAT-01. The
BUG-003 investigation/approval proposal below is historical and withdrawn; no
runtime defect is established by the recorded timeout. Current work/result is
tracked in subtasks/FEAT-01-test-actualization-r12.md under the same host state.
No skill/rule files are changed by this task-specific correction.

## Historical Status / Host

Diagnostic plan revision1 awaiting developer approval. Embedded in FEAT-01 at
implementation; no standalone state/new FEAT/checkpoint. R11 implementation is
partial, prior R9 review evidence historical. No temporary probes installed or
new diagnostic reruns executed. Existing BUG-001/002 review gates retained.

## Expected / Actual

Requirements revision6 / APR-010 / implementation authorization APR-011: Movement
owns Dodge phases, full Recovery, independent buffer; finite Active-only network
invulnerability. New Solo scenarios should execute Dodge and observe its lifetime.
One complete filtered suite run on Unity6000.3.14f1, Fusion Single authority:
9 total,6 passed,3 new Dodge scenarios failed with only the tick-sequence timeout.
The baseline timeout does not identify which step or component stopped advancing.
No assertions or behavior may be weakened to force a pass.

Failing cases: SoloDodgeUsesMovementTimerAndWaitsForFullRecoveryBeforeAttack;
SoloRepeatedDodgeWaitsUntilRecoveryFinishes;
SoloInvulnerabilityExpiresWithoutTheDodgeEndNotification. All report
RunTickSequence timeout5seconds, with no captured callback assertion exception.
Original six attack/contact/availability scenarios passed. Compile and full
Edit674/674 passed. Runtime build0errors/8dependencywarnings; strict retains four
unrelated ASSET003 images; current tracked diff check passed after already-approved
empty m_Name formatting restoration for newly added prefab object IDs.

## Hypotheses / Estimates

| Hypothesis | Likelihood/confidence | Evidence/gap | Probe distinction | Conditional fix |
| --- | --- | --- | --- | --- |
| H1: Dodge command is not admitted or runtime colleague reference/state differs | Medium/low confidence | All three new Dodge cases timeout; earlier six and prefab compile/Edit checks pass. No per-tick runtime state trace. | Pending command/lock vs mediator readiness, actor movement state and configured Dodge identity. | Small/medium; exact branch/binding not known. |
| H2: Dodge enters but timer/phase/transition resets or fails to finish | Medium/low confidence | New owner/timer path not covered by old six; pure phase boundary tests pass. Need actual end ticks. | Movement state/target tick/current phase across ticks and completion. | Small/medium; timing/lifecycle path. |
| H3: New fixture step conditions miss an otherwise successful execution | Lower/low confidence | Existing AfterTick driver works for original six, new three use new predicates. | Trace step changes vs state/timer transitions. | Small; test-only correction if proven, preserve all criteria. |

Separate static initialization concern: generic movement Init selects Idle, while
new Dodge ownership requires preserving restored state/timer during a mid-action
reinitialization. This has not been proven to cause these timeouts. Include the
already-visible initializer/entered-state information in analysis; do not silently
apply an unapproved production fix or claim actual live replay coverage.

## Proposed EXP-001 Revision1

Modify only TechnicalPrototype/Tests/Play/PlayerCombatPhasePlayModeTests.cs with
temporary bounded test-only records in the representative repeated-Dodge scenario
and its tick driver. Capture tick, fixture step, movement state, action ID,
buffer command/lock/expiry, movement timer target, current Dodge phase,
mediator/state-machine availability, and finite invulnerability deadline. Read
Dodge phase only while its movement state is active. Output a bounded ring/list
(maximum64 state/step changes, plus initial/final state) on failure/completion.
No production log/event, prefab/config/behavior change, sleep/timing extension,
assertion relaxation or new public test-only API.

Execute at most3 filtered Solo runs of SoloRepeatedDodgeWaitsUntilRecoveryFinishes
via Tools/AI/validate-unity.ps1 on this connected Editor. Stop early once one trace
distinguishes mechanism; transport/reload failures are unavailable attempts and
reported separately. Keep prior failed suite. Cost small instrumentation + bounded
runs; remove temporary probe after diagnosis/approved fix. Then present evidence
and exact fix proposal; diagnosis approval does not authorize production changes.

## Final Non-Diagnostic Check Refresh

Final source build passed: 0 errors / 8 dependency warnings. Final Strict scanned
581 scripts / 173 resources and retained the same four unrelated Enemy PNG ASSET003.
Tracked diff check passed, index empty, no EditorBuildSettings changes remain.
All169 current manifest hashes and21 frozen common contract hashes matched.
No instrumentation, extra Play run, assertion relaxation, timeout extension or
production fix was performed after the failed suite.

## Actual Runtime Cause Established During Authorized Test Maintenance

The original classification from a timeout alone was invalid. Subsequent owner-
authorized test maintenance established a separate, concrete runtime mechanism.
Representative forward-tick failure context: step1, state Locomotion, movement
timer target0, pending command0, lockfalse, mediatoravailabletrue, movementavailabletrue.
Connected Editor eval of installed Fusion assembly: TickTimer.TargetTick type is
Nullable<Int32>; TickTimer.None.TargetTick returns0 while None.IsRunning isfalse.
The installed XML says absent timer target isnull, but actual assembly behavior
was checked and differs. Evidence: r12-timer-contract.json, r12-state-play.json.

PlayerNetworkEntityComponent.StateTimerEndTick forwards TargetTick directly.
PlayerDodgeState.Enter checks HasValue; target0 is therefore treated as an already
started timer. No new timer starts. DodgePhaseUtility sees target0 already elapsed,
so the movement processor exits in that same tick. This explains the command being
consumed/lockreleased while no sustained Dodge is observed. Cause established;
not a stale assertion or a mere timeout classification.

Exact minimal production proposal: StateTimerEndTick => MovementStateTimer.IsRunning
? MovementStateTimer.TargetTick : null. Existing running/expired timer behavior
retained, no timer/phase config or ownership change. Only this getter plus existing
Play fixture actualization/regression; no new public API/prefab/package changes.
Requested through async question in current chat; approval pending. Prior proposed
EXP-001 approval is withdrawn, not retroactively inferred. No production fix yet.

Tests now require immediate movement-state entry, non-Combat execution, locked
independent buffer and future movement deadline. The finite-effect scenario also
uses directional input. Recovery/buffer-expiry/missing-off assertions remain.
An attempted updated test run was unavailable due to server connection reset;
not counted as an assertion failure. Retry is underway.

## APR-012 Implementation / Regression

Only production change: PlayerNetworkEntityComponent.StateTimerEndTick returns
null when MovementStateTimer.IsRunning isfalse; otherwise returns TargetTick.
No phase/config/authority/messaging/serialization/ownership changes.

Play fixture preserves scenarios, adds immediate entry/non-Combat/lock/future-deadline
checks, absent/completed deadline assertions, direction precondition for the effect
scenario, and failure context. No timeout extension/assertion removal/production probe.
Before-fix actualized repeated-Dodge test failed immediately: expected Dodge,
actual Idle (20261017-131218-601-4306b5e6-PlayMode/result.json). Old 5-second timeout
replaced by an actionable failure; actual complete phase behavior still must pass.
Checks after fix in progress; no passed outcome inferred from source presence.

## Verification / Focused Review Result

APR-012 exact correction and fixture maintenance verified: connected Compile passed,
full Edit674/674 and affected Solo9/9 passed. Before-fix actualized regression failed
expected Dodge/actual Idle; after fix all3Dodge+previous6 cases passed. Build0errors/
10dependencywarnings; strict same4unrelatedPNG ASSET003; final tracked diffcheck passed.
Focused exact two-file audit P0/P1/P2=0/0/0. Full report/evidence and patch are in
subtasks/FEAT-01-test-actualization-r12.md / .patch / -review.md / -manifest.json.
No temporary trace ring/public hook/log added; only useful fixture failure context
retained. Owner final diff/retest acceptance pending. No commit or silent return
past full FEAT-01 audit, no next FEAT. Initial premature classification remains
historically corrected; current runtime conclusion rests on concrete mechanism.


## Owner Acceptance / Publication Boundary — 2026-10-17

APR-013: developer «Подтверждаю» accepts revision12 exact two-file delta and
executed Compile/Solo9/Edit674 results. Embedded BUG-003 review/retest complete;
return to pending full FEAT-01 audit. No full FEAT acceptance, separate commit,
next-subtask readiness or new manual/network execution inferred.

Owner clarified Notion is game/project documentation, specifications and task
board, not a feature-development process journal. Local workflow stages, approvals,
diagnostic iterations, audits/test runs and technical handoffs belong in state.yaml,
feature/bug Markdown and evidence. This acceptance is recorded locally only.
Canonical publication boundary updated in docs/notion/feature-integration.md and
referenced by AGENTS/feature/Notion/bug instructions. No Notion worker/MCP operation
or remote edit was performed. Existing remote history was not silently rewritten.

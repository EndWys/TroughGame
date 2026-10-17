# BUG-002 Р Р†Р вЂљРІР‚Сњ Early-Input Play Mode Failure

Host: FEAT-01. Origin: architecture revision 8 implementation verification.
Stage: diagnostic_plan. Revision: 1. Status: awaiting diagnostic approval.
No production fix is approved. Independent approved FEAT-01 work may continue.

## Expected and Actual

The accepted finite input lifetime is 0.15 seconds; input buffered near attack
onset must expire before the recovery combo window. The revised Play fixture
completed 1/2: contact uniqueness/DI/despawn passed, early-input test failed with
"Expired early input chained later. Expected: 1 But was: 2".
Run: .ai/evidence/combat-system/r8-validation/20261016-164046-661-4974b5bc-PlayMode/result.json.
Unity 6000.3.14f1, Single mode, TechnicalPrototype, current uncommitted revision 8
over HEAD d9d3914e6b54c62659b0cdeacbcbf18d12dec90b. Observed by agent.

## Read-Only Analysis

PlayerCombatPhasePlayModeTests waits for ActionId != 0 in a frame coroutine,
then buffers the second command without checking phase or age. A frame can
contain several network ticks; a late observation could make that command
legally survive until the combo window. This is a leading hypothesis, not proof
of the failed run's phase. Runtime InputBufferController is unchanged; Edit Mode
664/664 passed. Neither fact proves actual tick ordering in the failed run.

| Hypothesis | Likelihood / Confidence | Evidence / Missing Data | Conditional Fix |
| --- | --- | --- | --- |
| H1: coroutine observes first execution late, then sends a valid late command | Leading, medium | Test does not enforce early phase; failing run has no recorded injection tick/phase | Small test-only tick synchronization correction |
| H2: runtime preserves buffered command too long | Low, tentative | Failure is consistent but controller unchanged and unit tests pass; actual expiration timer unavailable | Unknown until mechanism is identified |

## Diagnostic Plan EXP-001, Revision 1

Only PlayerCombatPhasePlayModeTests.cs may receive temporary test-only output.
Record runner Tick, action StartedTick/ExecutionId/phase, buffered CommandId,
BufferedCommandTimer.RemainingTime and lock at first/second input injection,
first execution observation and an unexpected execution change. Run only
SoloAttackLocksPositionExpiresEarlyInputAndChainsWithNewDirection, maximum three
attempts, retaining every result. No production, asset, SDK, timing or assertion
changes. Distinguish late injection (H1) from an expiration violation (H2).
Remove temporary output after analysis under the subsequently approved fix.
The plan was presented in chat with hypothesis/expected-actual tables and an
async approval request. Silence is not approval; no probe has executed yet.

## Independent Work

Approved readiness/reset regression, source audit, asset formatting normalization
under APR-005 and review artifact preparation are independent of this failure.
Full green Play Mode and final review readiness are not claimed.

## Additional Approved Lifecycle Check Observation

The independently added same-component reset/reinitialize Play scenario also failed
(0/1), expecting a nonzero ActionId. Evidence:
.ai/evidence/combat-system/r8-validation/20261016-164321-605-ac31f49c-PlayMode/result.json.
This is recorded without assigning a cause or broadening EXP-001 instrumentation.
It reinforces the need to distinguish tick dispatch/input injection timing from
actual runtime failure. No assertions have been weakened.

EXP-001 launch attempt 1 was not accepted by Pipeline: run_tests returned a
network request error during Editor reload. No instrumented scenario executed.
Editor status subsequently ready/stopped, compilation completed without errors;
retry under unchanged approved plan. Evidence: .ai/evidence/combat-system/bug002-exp001-attempt1.json.

## EXP-001 Result and Fix Proposal

Launch attempt 2 executed and reproduced the original failure (0/1). Evidence:
.ai/evidence/combat-system/bug002-diagnostics/20261016-164709-309-2c305a22-PlayMode/result.json
and .ai/evidence/combat-system/bug002-exp001-ticks.log.
The first attack started at tick 121; coroutine observed it at 144 in Recovery,
then submitted the supposed early input at tick 144 with TTL=0.15. The next
execution started at 148. This confirms H1 for the original failure: the test
created valid late input and incorrectly expected expiration. No production
expiration violation was observed. One actual diagnostic run was sufficient;
no third launch is needed. The independently failing lifecycle scenario has
no equally detailed observation; do not claim its exact cause proven.

Stage: fix_proposal, revision 1, awaiting developer approval.
Only PlayerCombatPhasePlayModeTests.cs changes: a test-only SimulationBehaviour/
IAfterTick driver executes phase-dependent input and records/asserts state on
every server forward tick; coroutine waits for completion/error. Preserve the
original expiry, movement/facing lock, allowed combo/new direction and cancellation
assertions. Synchronize shared CompleteAttack and the new same-component reset
scenario through the same driver to avoid missing entire actions between frames.
The driver captures test assertion errors for the coroutine to report; no
production hooks/events, altered balance/assertions, prefab changes or SDK edits.
Remove temporary BUG002 logging. Complexity medium, test-only. Verification:
three original-scenario runs, full Play fixture and full Edit Mode, compile,
existing project checks and combined diff review/retest.

The proposal was presented with exact tick evidence and an async approval request.
No fix implementation is authorized by the diagnostic approval alone.

## Fix Approval

APR-008: developer «подтверждаю. продолжай работу» accepts the exact test-only
fix revision 1 and continuation to FEAT-01 after its checks. Stage: implementation.
No production fix or contract change is authorized/needed.

## Implemented Correction and Verification

APR-008 correction is implemented only in PlayerCombatPhasePlayModeTests.cs.
All temporary BUG002 traces were removed. The test-only AfterTick driver executes
assertions/input on every server forward tick and surfaces assertion errors to
the coroutine. Existing expiry, lock/facing, new-direction combo, cancellation,
contact and lifecycle assertions remain. First-injection Preparation assertion
strengthens the original early-input precondition.

Original failed scenario passed three independent repetitions; full Solo fixture
passed 3/3 including contact/despawn/fresh spawn and same-instance component
reset/reinitialize. Final Edit Mode 664/664 and compilation passed. Evidence index:
.ai/evidence/combat-system/r8-final-results.json. Runtime build 0 errors/8 warnings;
tracked diff check passed; strict has the same four unrelated ASSET003 errors.
Combined audit of review revision 2 has no P0/P1; prior P2 query allocations remains.
The independent lifecycle failure now passes with tick observations, but its exact
old execution history was not captured, so that specific historical cause is not
claimed independently proven.

APR-008 authorizes continuation after this correction's checks. Host resumes
FEAT-01 developer_review with one full review revision 2 snapshot; bug stays in
verification_review until matching developer diff/retest acceptance. No bug or
feature closure, commit or separate checkpoint. Host/client, live replay and real
Fusion pool reuse remain unexecuted.

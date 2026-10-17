# BUG-004: First Client Attack Prediction Not Accepted by Host

## Status / Host

Stage: deferred; revision:4; host:combat-system/FEAT-01,INT-005.
Conclusion: owner deferred FIX-R1 via APR-019 and ended this investigation.
EXP-002 confirms a loss mechanism after a controlled250ms delay, beyond the150ms
command lifetime. It does not prove a within-lifetime contract violation or the
original R15 cause. No production fix implemented; accepted Review5 unchanged.
Investigation closed by owner deferral, not by a remediation claim. Earlier
diagnostic proposals/results below are retained as history.

## Expectation / Observation

Accepted attack contract: an eligible valid attack command is consumed into an
authoritative execution and replicated; input-authority prediction can be corrected
to the authoritative result. A correction by itself is not a defect.

Unity6000.3.14f1, separate Editor Host and Windows Development Client. Virtual
Gamepad first attack left,release,second attack right,Dodge right. Client passes
through existing local reader/network input. First sequence trace:

| Peer / Tick | Action / Execution / Started / Direction | Fact |
| --- | --- | --- |
| Client4107 |2/1/4107/left|Predicted first attack|
| Client4112 |0/0/0/zero|Authoritative correction removed prediction|
| Host through this interval |0/0/0/zero|No matching first authoritative execution observed|
| Host and Client4165 |2/1/4165/right|Following attack accepted and replicated|

First run client/host traces3484/3577frames (host/client respectively), max sampling
gaps28/29ticks. Later unobserved repeat sequence was requested while a proposed
tick observer recorded zero rows; do not count that as a measured reproduction.
Raw source: `.ai/evidence/combat-system/r15-network/host-trace.jsonl`,
`client-trace.jsonl`, `network-summary.json`. Host final ExecutionId1 after two
Client pulses supports one accepted execution, beyond merely missing a sampled
Preparation frame. No input payload/transport/replay trace exists yet.

## Read-Only Analysis

PlayerNetworkInputComponent.OnInput captures transitions from LocalInputProvider
and sends LatestActionCommandId in PlayerInputData. Capture resets button
transitions. PlayerInputSourceComponent.OnNetworkTick buffers commands obtained
from GetInput; missing input returns early. No cause is proved by this code.
Following attack and Dodge work across peers, weakening a complete authority/DI
failure. No production fix proposed.

## Hypotheses

| ID / Hypothesis | Likelihood / Confidence | Evidence / Gap | Conditional Fix Complexity |
| --- | --- | --- | --- |
| H1 Short command pulse absent/late in authoritative tick input |Medium/low|Prediction removed within5ticks; server accepted later input. No sent/received payload evidence.|Unknown; small if capture bug,medium if command transport semantics change.|
| H2 QA virtual-device/timing setup induced first-sample discrepancy |Medium/low|First synthetic press; frame sampling/CLI pauses and no warmed-device comparison.|Small QA correction; gameplay change unnecessary if confirmed.|
| H3 Host receives correct command but action admission rejects it |Low/low|Following attack/Dodge work; no per-tick admission record.|Small/medium depending proven rejected-state condition.|

## Approved Diagnostics EXP-001 R1 (APR-017 / Execution Complete)

1. Temporarily instrument only TP PlayerNetworkInputComponent.OnInput and
   PlayerInputSourceComponent.OnNetworkTick. OnInput: preserve the exact existing
   payload, record peer/runner tick/frame,MoveDirection,DodgePressed,LatestActionCommandId.
   InputSource: record GetInput success/payload before buffering plus root
   ActionId/ExecutionId/StartedTick,BufferedCommandId,lock and movement state.
   No new events, network fields or gameplay decisions.
2. Add one temporary compile-guarded static QA recorder at
   Assets/ProjectCore/Contexts/TechnicalPrototype/Features/Player/Scripts/Other/CombatNetworkDiagnosticProbe.cs
   (+Unity meta). Use buffered local JSONL, bounded capture, no per-tick console
   stack traces. All calls/recorder guarded UNITY_EDITOR||DEVELOPMENT_BUILD.
   Recorder is removed with all temporary calls before any final diff/commit.
3. Compile and build the same Development client once. Up to three separate-peer
   runs: original short pulse; warmed virtual device plus isolated short presses;
   warmed device plus longer held press. Keep existing attack/Dodge timings,
   gameplay/network configs and transport unchanged. Compare sent payload ->
   authoritative GetInput -> buffer/admission -> action state on both peers.
4. Stop early when a discriminating result proves harness vs input delivery vs
   admission; otherwise stop after third run and report uncertainty. Investigation
   costs one build and up to three short sessions; logging overhead is a limitation.
5. Flush/stop recording, shut down peers, remove all temporary source/meta/calls,
   restore temporary build settings and verify Review5 hashes/compilation. Retain
   only ignored evidence and local process records. Diagnostic approval permits
   these probes only; any production fix needs a separate exact proposal.

## EXP-001 Actual Results

Evidence root: `.ai/evidence/combat-system/bug004-exp001/`.
All three separate Editor Host / Development Player Client sessions joined with
StartGame Ok. Each used two attacks and one Dodge; warmup/held duration followed
the approved matrix. Full send/receive/buffer JSONL retained for both processes.

| Run | Input Conditions | Host Command Ticks (left/right/Dodge) | Authoritative Attack Executions | Result |
| --- | --- | --- | --- | --- |
|1|Original1s warmup,0.06s press|2274/2331/2390|1@2274,left;2@2331,right|Same commands/actions on Client; no loss|
|2|Neutral device warmup3s,0.06s press|980/1037/1096|1@980,left;2@1037,right|Same commands/actions on Client; no loss|
|3|Neutral device warmup3s,0.25s press|1060/1130/1200|1@1060,left;2@1130,right|Same commands/actions on Client; no loss|

Client source callback recorded549/796/816 actual replay invocations respectively
(1098/1592/1632 receive+buffer records, not distinct replay ticks). These establish
real replay execution of the input-source path, not hit-probe replay-ledger proof.
Host had29/21/30 forward ticks without input for the remote actor; none was an
attack/Dodge command tick. Missing input alone therefore does not identify the
original lost command. All held durations emitted each command on exactly one
tick. Final actor state in each capture idle/unlocked/buffer0/execution2.

Updated hypotheses: H1 remains a leading medium-likelihood, limited-confidence
explanation (single-tick command + real missing-input ticks, but no overlapping
press/missing tick observed). H2 deterministic first-device/warmup failure weakened:
both original and warmed setups pass; transient QA/runtime timing still possible.
H3 general admission failure weakened by six correctly accepted attacks. No
production fix is justified as the original-case remediation yet.

Infrastructure attempts are distinct from the three sessions: initial build lacked
Pipeline runtime config because its enabling eval timed out; its Player never
joined a network session and was stopped. Verified config and rebuilt the same QA
binary (0errors/486warnings,37357ms). First unusable build0errors/487warnings retained.
Run2 orchestration initially hit a null-result parser error after entering Play;
resumed the same processes before StartGame, with no extra session or input sequence.
These were QA setup corrections, not additional gameplay experiments or game bugs.

## Cleanup / Current Integrity

All sessions shut down; both registries empty after each shutdown. All three
Players exited, Editor stopped on original Preloader. Two instrumented sources
restored byte-for-byte from verified baselines. Temporary recorder and Unity meta
deleted through AssetDatabase. No retained events/test hooks/network fields.
Runtime Pipeline config removed, generated Pipeline assets removed, preloaded
input asset list and URP runtime settings restored. Rendering/ProjectSettings
semantic diff empty. Some Unity newline-only Git status entries remain unchanged.

All175 accepted Review5 hashes match, indexempty; cleanup connected Compile passed
(`cleanup-validation/20261017-202231-505-6cb72a92-Compile/result.json`).
Rendering cleanup eval timed out but actual effects were checked, not blindly
replayed; final semantic diff and later readback are authoritative. Prior full
Edit674/Solo11 remain prior results for the restored source, not new test runs.
No gameplay fix, commit, new FEAT, Notion publication, or readiness claim.

## Approved EXP-002 R1 (APR-018 / Execution Complete)

Purpose: distinguish a command lost at its authoritative input deadline from an
admission failure. Controlled client hitch is a conditional reproducer; it cannot
by itself prove that an identical hitch caused the original observation.

| Probe | Exact Delta / Conditions | Discriminating Data | Budget / Exit |
| --- | --- | --- | --- |
|EXP-002|Reapply the same temporary OnInput/InputSource logs. Only temporary CombatNetworkDiagnosticProbe gains a one-shot PauseFirstClientCommandMilliseconds=250 switch: after logging the first nonzero Client payload inside RecordSent, pause that Client thread250ms before OnInput returns. Record pause begin/end; never pause Host. No network/project/balance configuration or payload changes.|Compare first command tick in Client send/receive to Host GetInput at that exact tick, subsequent command0 samples, buffer and ActionState. Second attack is an unpaused control. Missing press tick followed by unprocessed intent supports H1; received nonzero command without execution supports H3; successful first execution weakens this reproducer.|One usable Development build and at most one separate-peer session with the original left/right/Dodge sequence. Stop after capture; remove all probes/settings, restore hashes/compile. No production fix authorized.|

If data proves the failure mechanism, present the minimal exact fix with a
regression and peer retest plan. Otherwise report uncertainty without expanding
the experiment automatically. Potential input-intent retention changes are not
pre-approved or presented as a fix for the original case.

## EXP-002 Actual Results

One usable Development build0errors/0warnings,55247ms; one joined Host/client
session. Evidence root:`.ai/evidence/combat-system/bug004-exp002/`.
An initial readonly readiness window ended while Editor was still in Preloader;
resumed the same processes after startup completed, before any StartGame. No
second session or duplicate input sequence. Config-removal eval timed out but its
actual absent-file effect was verified before peer start, not blindly replayed.

| Event | Observed Data | Conclusion |
| --- | --- | --- |
|First Client OnInput payload|Attack2,left,tick842|Correct first command formed|
|One Client pause|250.5701ms,begin/end tick842/frame6296|Approved hitch actually executed once; Host not paused|
|Host source at842|GetInput=false,command0,buffer0,action0|First command absent at its authoritative deadline; not rejected by admission|
|First valid Host input after gap|tick869,command0|No retained intent in a subsequent available frame|
|Control right attack|Host/Client command915,execution1 started915|Normal admission still works|
|Dodge control|Host/Client command972,clean Idle after993|Adjacent movement action works|

Client source callback had677 replay receive records. No first left attack state
was observed in the pre-controller receive records, so no predicted animation
claim. Mechanism is the one-frame sender pulse plus receiver's unavailable-input
return. The pause exceeds150ms; accepting the first command at869 would already
violate the original finite lifetime. This does not prove that an identical delay
caused R15, or that stale commands should be executed.

Original accepted Review5 hashes restored; temporary logger source/meta removed,
config/settings restored,Players exited,Editor stopped. Cleanup Compile passed:
`cleanup-validation/20261017-203655-115-50f7a713-Compile/result.json`.
Final integrity/readback/diff check retained under this evidence root.

## Fix Proposal R1

See [fix-proposal-r1.md](fix-proposal-r1.md) for the exact10 production files,
API/state/DI/actor/deadline/replay rules,three test files,common-contract amendment
and before/after peer regression. Complexity medium; changes are Input-owned.
No mediator/movement/combat coupling or timing/balance change is proposed.
Proposal retained for reference only; deferred by owner via APR-019.

## Decision / Return

Developer approved EXP-001 R1 (APR-017), initially requiring a fix before FEAT-01
completion. EXP-001 and EXP-002 (APR-018) completed; all probes were removed.
After clarification that250ms exceeds the150ms lifetime, owner said:
«Я бы пока проигнорировал этот фикс. Пропустим», then «Продолжи» (APR-019).
This supersedes the earlier requirement to fix this candidate before completion.
FIX-R1 is deferred, never approved or implemented. No new experiment is authorized.
The earlier mandatory-fix proposal was premature: delivery loss under this expired
input condition is not proof of incorrect behavior within the accepted lifetime.
INT-005 returns to play_mode_verification. Full Play acceptance, remaining coverage
limits and FEAT-01 completion remain separate; no accepted checkpoint is created.
No Notion publication: this is local development/validation process data.

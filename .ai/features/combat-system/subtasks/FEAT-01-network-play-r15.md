# FEAT-01 Separate-Peer Play Verification R15

## Accepted Baseline

APR-016: developer accepted full Review5 with "Принимаю дифф". Accepted source digest:
`053bf5b931ab037047b6ff80e277c09c4ab5cef6abeca01882f75178e4ca2968`.
All 175 manifest file hashes still match; index remains empty. No gameplay source,
prefab, scene or balance change was implemented in this verification turn.
FEAT-01 and the five-subtask plan remain active and unchanged.

## Executed Setup

Unity 6000.3.14f1; existing connected Editor PID40132 as Host. Separate Windows
Development Player PID8172 as Client, batchmode/nographics, using the existing
Preloader/TechnicalPrototype initialization, SceneContext DI, object provider and
player spawn factories. Photon session private/not listed, two participants.
Both StartGame results returned Ok; Host ActivePlayers was [1,2].

Pipeline build completed in223278ms,554497630bytes,0errors/492warnings. Full report
retained in ignored evidence; counts include shader and vendor/sample warnings.
This is a local QA binary, not a release/build-quality acceptance claim.

Temporary Pipeline config enableInBuilds was removed after building, restoring its
initial absence. Stock runtime eval filters out ProjectCore/Fusion references;
QA wrapper supplied those existing assemblies to the installed Roslyn compiler.
All QA scripts/compiled artifacts live in ignored `.ai/evidence/`, not Assets.

## Scenarios and Actual Results

Evidence root: `.ai/evidence/combat-system/r15-network/`.

| Scenario | Expected | Actual / Evidence |
| --- | --- | --- |
| Separate peers | Host and Client join one session, two spawned players | Passed: host/client-start-result.json, host-connected-state.json |
| Host attacks replicated | Matching action ID/execution/start tick/direction on Client | Passed for action2,executions1/2,start ticks3839/3912,right; network-summary.json |
| Authority-only contacts | Host produces contacts; Client produces none | Host unique samples(source67108864,target67108865,exec1,tick3851),(same pair,exec2,tick3924). Client0 contacts over3577 frame samples. These are samples, not exhaustive per-tick counting. |
| Client attack through actual input route | Virtual Gamepad -> LocalInputProvider -> Fusion PlayerInputData -> server action | Partial: right attack1/start4165 matches Host and Client. Earlier left prediction1/start4107 corrected to idle/execution0 at4112; no matching Host execution. Cause unconfirmed; BUG-004 candidate investigation. |
| Host and Client Dodge | Finite state timer, Active-only finite invulnerability, full Recovery | Observed Host-owned state end3993/effect end3987; Client-owned state end4243/effect end4237. Client-owned samples cover Preparation4223/24,false;Active4225..4236,true;Recovery4237..4242,false. Both return Idle/unlocked with no pending command. Proxy presentation lag is retained in raw samples. |
| Despawn during attack | Unregister old actor, replicate removal, clean new spawn | Passed: old action2/lockedtrue; old67108865 removed, replacement67108866. New attack/execution/command0,unlocked,no timers/invulnerability/contacts. Both peer traces show old pair -> Host alone -> replacement pair. lifecycle-result.json and *-lifecycle-trace.jsonl |
| Same-instance pooling | Real pooled instance reuse | Not executed: current ZenjectNetworkObjectProvider inherits Instantiate/Destroy provider; actual old object destroyed,new instance differs. Do not label fresh spawn as pool reuse. |
| Client disconnect | Host removes remote actor; Client registry empties | Passed: Client registry2->0; Host ActivePlayers[1],registry[67108864]. client-shutdown.json,host-after-client-disconnect.json |
| Host shutdown | Registry emptied | Passed: registry1->0; host-shutdown.json |
| Replay callback coverage | Observe actual forward/replay ticks | Not established. Two transient dynamically compiled IAfterTick observers installed but wrote zero rows; attributed retry also wrote zero. Excluded as proof; no game-defect inference from failed instrumentation. Frame trace shows authoritative correction but cannot prove execution of replay guards. |

The frame sampler's maximum tick gap was28 Host/29 Client. It cannot establish
every-tick ordering, absence of unsampled contacts, or the cause of the lost first
Client prediction. No packet loss/latency settings were modified.

## Cleanup / Checks

Both runners shut down, Client process exited and Editor stopped. Editor scene
restored to its original clean Preloader. Generated Pipeline assets/empty child
folder deleted through AssetDatabase; project runtime config absent again.
Build-added PlayerSettings preloaded input asset restored to the initial empty list
through PlayerSettings API. URP build-generated runtime settings list restored to
its initial empty list through reimport/SerializedObject/save. Final Rendering and
ProjectSettings semantic diffs are empty; Unity newline-only rewrites may remain
visible in Git status. No unrelated user changes were restored or removed.

Final diff check passed. Review5 source hashes unchanged, staged paths empty.
Prior current-source Compile/Edit674/Solo11/runtime-build checks remain applicable;
they were not rerun and are not relabeled as network validation. Strict's four
existing unrelated PNG ASSET003 blockers remain. No commit, next FEAT, Notion
publication or feature-readiness claim.

## Next Gate

Host/client Play gate remains open for review of actual results and coverage limits.
The earlier request for BUG-004 diagnostics was satisfied by EXP-001/EXP-002.
Owner subsequently deferred FIX-R1 and ended that investigation (APR-019).

## Current Disposition — 2026-10-17

See [BUG-004 case](../bugs/BUG-004/case.md). EXP-001's three sessions delivered all
commands. EXP-002 observed a missing command after one250.57ms Client pause; that
exceeds the150ms lifetime and does not prove an in-lifetime contract violation or
the original R15 cause. Owner said «Я бы пока проигнорировал этот фикс. Пропустим»,
then «Продолжи». No production fix applied; probes removed, Review5 unchanged.

Return to FEAT-01 Play acceptance with the original partial/untested results above
preserved. Actual input-source replay callbacks were subsequently recorded, but
hit-probe replay-ledger execution remains unproved. Same-instance reuse remains
inapplicable to the current Instantiate/Destroy provider. Owner deferral accepts
neither all Play results nor full FEAT-01 completion. Plan unchanged, no commit.

# FEAT-01 R16 Hurtbox Debug Implementation and Review

## Delivered Behavior / Scope

APR-020 implemented. Actual hurtbox shapes are green at rest; an authoritative
contact makes the target red for0.20s. The receipt survives the next tick clearing
the probe list and remains inspectable after expiry (tick/execution/source).
Host/Solo produces flashes; Client contours only, without new replicated state.
The actual contact list, hit detection, ledger, gameplay contracts and damage
semantics remain unchanged. Existing mediator/controller architecture is preserved.

## Exact Inventory

Eight production/test/asset paths (new script and its meta counted separately):

| Area | Files | Actual Change |
| --- | --- | --- |
| GC Combat | CombatDebugVisualizationComponent.cs and Editor-generated meta | Base network lifecycle/availability, existing registry lookup on contact tick, local unscaled highlight deadline, actual hurtbox circle/box drawing and diagnostic receipt. OnEnable/disable only registration and local cleanup; reset/context invalidation clear receipts. |
| TP Player | PlayerNetworkEntityComponent.cs; Prefab_Player_NetworkEntity.prefab | One serialized debug colleague immediately after probe in component order; Editor-added component and hurtbox/probe/root links. |
| TP Enemy | EnemyNetworkEntityComponent.cs; Prefab_Enemy_NetworkEntity.prefab | Same debug colleague initialized/reset by root; own hurtbox and no probe. Existing dash variant inherits all links without an asset edit. |
| Template DebugVisualization | Config_Template_DebugVisualization_Settings.asset | Idle green and Contact red styles; depth test off so debug outlines remain visible. |
| TP Play tests | PlayerCombatPhasePlayModeTests.cs | New actual-contact integration case. Completed tick-driver sequences stop immediately within a frame. New fixture freezes enemy movement and uses NetworkTransform.Teleport for authoritative setup/miss positions. |

Prefab/config mutations ran through the connected Editor. Eight Editor-generated
trailing-whitespace lines were normalized without semantic changes; final prefab
readback confirms links on Player, Enemy and inherited dash Enemy. Original metas,
old components and prefab inheritance preserved. No new installer/service/event/RPC.

## Actual Checks

Evidence root: `.ai/evidence/combat-system/r16-hurtbox/`.

| Check | Result | Evidence / Limit |
| --- | --- | --- |
| Connected Compile | Passed; final test helpers also recompiled current sources | validation/20261017-215907-733-40a70ff0-Compile/result.json |
| Relevant Edit prefab tests |9/9 passed| validation/20261017-220809-812-b2984fb4-EditMode/result.json |
| Affected Solo combat Play suite |12/12 passed| validation/20261017-220621-201-80b74c3b-PlayMode/result.json |
| New integration case | Same-tick target receipt; flash after list clearing; expiry; later execution; miss; disable and despawn/new spawn cleanup | SoloHurtboxHighlightCapturesContactBeforeItClearsAndResetsOnDespawn in the above suite |
| Runtime build |0errors/10dependencywarnings| build.log; runtime sources unchanged afterward |
| Strict architecture |Same4unrelated Enemy PNG ASSET003 errors; no new errors| architecture.log;583scripts/173resources |
| Editor prefab/style readback |All3 prefab views have one debug component and correct links; green/red styles| prefab-readback.json; final-readback-run.json |
| Actual Solo rendering |Red target circle and green player circle; target returns green after expiry | visual-ready.json,visual-expired.json,hurtboxes-contact.png,hurtboxes-idle.png; both screenshots visually inspected |
| Final diff/index |Passed, index empty| diff-check-final.log; no commit |

The live rendering capture used the real scene/DI/provider and real probe contacts.
Target receipt tick193,execution1,source67108864; emitted target circle center
(2.65,-0.35),radius0.25,color(1,0,0,1). Player circle center(2,-0.35),radius0.25,
color(0,1,0,1). After waiting0.30s the target highlight is false and its receipt
persists. Editor pause was used for screenshots. Initial baseline ScreenCapture
included Fusion's startup overlay and is not used as the green visual proof.

## Test and Tooling Corrections

First Solo run10/12: completed availability test callback repeated before the
frame resumed (counters4 instead of0); new miss fixture produced a contact after
direct Transform reassignment. Corrected test driver completion gating and new
fixture's network teleport/frozen movement; final12/12 above. No production bug
claim or bug workflow was created from test failures.

An initial compile request was unavailable during server reload; later Compile
passed. First multi-name Edit filter selected0tests, so it is not a pass; corrected
PrefabTests filter executed9. One repeat Play request stalled on an obsolete CLI
endpoint, with no new active run; only that exact owned CLI process was stopped,
Editor preserved, retry completed12/12. Retain not_completed helper result.

Prefab mutation succeeded but its first result serialization failed on Unity Color;
readback was repeated without replaying the mutation. A temporary visual QA script
initially referenced an internal console interface; its compilation failed before
execution, then a reflection-only QA correction ran. No tooling/diagnostic source
was added to Assets. All scripts/evidence remain ignored and local.

## Code Audit / Limits / Next Gate

Separate main-agent review of the8path delta against APR-020: availability/DI,
init/tick order, actual query source, server/authority/forward guards, same Runner,
disabled target handling, receipt expiry, despawn/context invalidation, prefab
inheritance, color/shape draw paths, allocations and scope. No new actionable
P0/P1/P2 findings. Previous unrelated P2 AUD-001 query allocations remain unchanged.

R16 Host/client peers were not rerun. Client contours-only limitation is intentional;
no networked hit-flash behavior is claimed. Earlier network coverage and unproved
hit-probe replay-ledger execution remain as recorded in R15/BUG-004. FIX-R1 remains
deferred; this visual change does not remediate it. Full Edit674 is a prior result,
not this iteration's run; this iteration ran the relevant9 prefab checks.

Editor stopped and original Preloader restored. Temporary live-only movement/UI/
channel settings were discarded with Play exit. No persistent balance/config change
other than approved channel styles. No Notion write, accepted checkpoint, commit,
FEAT-01 completion or FEAT-02 transition. Developer reviews the R16 delta, then
executes the updated manual handoff; prior full Review5 approval remains valid.

## Owner Acceptance — 2026-10-17

APR-021 accepts the current diff; APR-022 records the developer's reported manual
Play execution and acceptance: «я выполнил и проверку дифа и я все подтверждаю и
плеймод проверку». This supersedes the pending review/manual gate in earlier text.
Exact peer mode/new network execution is not specified. See
subtasks/FEAT-01-owner-acceptance-r16.md (feature-root relative). INT-006 returns
to the plan/checkpoint step. Plan unchanged; strict's four unrelated ASSET003
errors remain failed, so no automatic commit/FEAT-01 completion yet. No Notion write.

# FEAT-01 Manual Play Handoff

## Scope and Current Status

Developer execution is pending. This is an instruction, not a passed test record.
Ready: one test attack, fixed attack pose/facing, three phases, recovery combo
subwindow, finite latest-action buffer, movement-owned phased Dodge, debug contacts.
HP/damage/hitstun/death, three attack variants, final placeholder animation/impact
and match flow belong to later subtasks. Current animation does not prove phase timing.
FIX-R1 remains deferred by APR-019; no loss/hitch experiment is requested.

## Setup and Controls

1. Open Assets/ProjectCore/Contexts/Preloader/Assets/Scenes/Scene_Preloader.unity.
2. Enter Play, wait for TechnicalPrototype initialization and Fusion startup GUI.
3. Click Start Single Player; wait for the player, then focus the Game view.
4. WASD moves and supplies attack direction; left mouse button attacks; Space
   with a direction starts Dodge. Mouse position does not aim this test attack.
5. Open Console with the Console launcher or BackQuote (key below Escape).
   In DebugViz navigate Player > Combat, State or Input. Enable corresponding
   channel toggles for world drawings. Combat values: Phase, Execution, Combo Window,
   Contacts This Tick; Input values: Buffered Command, Buffered Command Timer, Locked.
   Close Console before pressing attack buttons so UI clicks do not confuse input.
   Also navigate Combat > Hurtboxes and enable both Idle and Contact. Real hurtbox
   outlines are green; Host/Solo contacts make the target red for0.20s. Last Contact
   Tick/Execution/Source remain inspectable after the flash. Client contours remain
   green because no contact flash is replicated in R16.
   Use world labels and reopen Console for values; Unity Pause/Step can help inspect
   brief phases. Exact tick boundaries/duplicate-contact counts remain automated checks.

## Manual Scenarios

| ID | Steps | Expected Result |
| --- | --- | --- |
| M1 | Move right with D, attack once, then hold A without another attack. | Position/facing stay fixed through the attack; after completion movement left resumes. Phase order Preparation/Active/Recovery/None; a new execution starts once. |
| M2 | Attack, release LMB, immediately press/release it again during Preparation, then wait at least1s. | Early pending command expires after150ms; no second attack starts automatically. |
| M3 | Attack, release LMB; near the end of Recovery hold A and press attack again. Repeat if the brief window is missed. | A fresh buffered attack starts in the combo window facing left, before full completion of the original attack. No current-attack turning. |
| M4 | Hold LMB for1s from idle without releasing. | One attack, no automatic repeating chain from a held button. |
| M5 | Hold D and press/release Space once. Try attacking or pressing Space again near the start of Dodge, then stop sending actions. | Short Preparation, movement in Active, stationary Recovery; early commands expire, no early Dodge cancellation or stuck lock. Movement resumes after complete Recovery. |
| M6 | Enable DebugViz > Combat > Hurtboxes > Idle and Contact. In Solo press F7 for a normal enemy (F9 for dash enemy). Attack with enemy inside the forward box; compare a distant attack. F8 removes enemies; spawn again. | Real hurtboxes have green outlines. On overlap the contacted target is red for about0.20s then green; a miss creates no new flash. New spawn has no old highlight. Last Contact Tick/Execution/Source remain inspectable; Contacts This Tick remains transient. No HP reduction/death expected. Manual observation does not prove per-tick deduplication. |
| M7 | Stop Play, reopen Preloader if necessary, enter Play and Start Single Player again. Repeat attack/Dodge. | New session starts without a stale attack, buffer or movement lock. |

Attack config: Preparation0.20s, Active0.10s, Recovery0.30s; combo interval
0.15..0.25s into Recovery (approximately0.45..0.55s from attack start, end exclusive,
subject to tick quantization). Buffer150ms. Dodge0.03/0.20/0.10s, speed8 in Active.
Invulnerability state exists in Dodge Active, but actual damage rejection is FEAT-02
and cannot be demonstrated by HP loss in this slice.

## Optional Two-Window Repeat

Use two graphical instances of the current revision, each started through Preloader.
Before connection enter the same nonempty Room name in Fusion GUI. First instance:
Start Host; second: Start Client. Repeat M1/M3/M5 on each owning window; both peers
should show the same action/direction and eventual unlocked movement. Use F7/F8 on
Host for enemy creation/removal and confirm both windows agree. Closing Client
should remove its actor from Host. This requires a graphical Client; the previously
executed automated headless sessions do not constitute developer manual execution.
No second instance is needed for M1..M7. Pool reuse and actual hit-probe replay-ledger
proof are not claimed by these manual steps.

## Return Data

Developer reports M1..M7 passed/failed/not run, mode (Solo/Host/Client), unexpected
behavior with the exact input sequence and Console error text if any. Accept only
after actual execution; no all-results approval is requested before this handoff.
Earlier automated results: Edit674/674, Solo11/11 and separate-peer measurements
in FEAT-01-network-play-r15.md. These were not rerun for this instruction.

## Owner Acceptance — 2026-10-17

APR-021 accepts the current diff; APR-022 records the developer's reported manual
Play execution and acceptance: «я выполнил и проверку дифа и я все подтверждаю и
плеймод проверку». This supersedes the pending review/manual gate in earlier text.
Exact peer mode/new network execution is not specified. See
subtasks/FEAT-01-owner-acceptance-r16.md (feature-root relative). INT-006 returns
to the plan/checkpoint step. Plan unchanged; strict's four unrelated ASSET003
errors remain failed, so no automatic commit/FEAT-01 completion yet. No Notion write.

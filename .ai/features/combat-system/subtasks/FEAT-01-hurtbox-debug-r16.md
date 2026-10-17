# FEAT-01 Hurtbox Contact Visualization — Revision 16

## Status and Request

Approved by developer APR-020 (current chat: «да подтверждаю»); implementation
and verification in progress. Existing Review5 source approval and APR-019 FIX-R1
deferral remain unchanged. This is a requested
debug-visibility improvement within FEAT-01, not a confirmed gameplay defect.

Owner reports Contacts This Tick rarely changes and requests hurtbox outlines
that change color on contact. CombatHitProbeComponent clears Contacts every forward
tick; its processor emits each target only once per execution. A frame/HUD observer
can therefore miss that legitimate single-tick result. Do not alter hit detection,
buffer lifetime, contacts, damage semantics or the meaning of the existing counter.

## Exact Inventory

Paths are relative to Assets/ProjectCore/Contexts unless stated otherwise.

| File / Asset | Exact Change |
| --- | --- |
| GameCore/Features/Combat/Scripts/Components/CombatDebugVisualizationComponent.cs (+Editor-generated meta) | New BaseNetworkEntityComponent, IDebugDrawable. Serialized required CombatHurtboxComponent, optional CombatHitProbeComponent, positive finite highlight duration default0.20s. DI NetworkEntityRegistry. Reads the real WorldShape; draws circle or rotated box. Local receipt timestamp/source/execution/tick only; no Networked field. |
| TechnicalPrototype/Features/Player/Scripts/Components/PlayerNetworkEntityComponent.cs | Serialized debug colleague; append immediately after hit probe in CreateComponents so capture executes in the same authoritative forward tick. Existing component order otherwise preserved. |
| TechnicalPrototype/Features/Enemy/Scripts/Components/EnemyNetworkEntityComponent.cs | Serialized same debug colleague; append to CreateComponents for Init/reset/readiness. No contact source needed yet. |
| TechnicalPrototype/Features/Player/Assets/Prefabs/Prefab_Player_NetworkEntity.prefab | Add/wire debug component, own hurtbox and probe, root reference through Editor. |
| TechnicalPrototype/Features/Enemy/Assets/Prefabs/Prefab_Enemy_NetworkEntity.prefab | Add/wire debug component, own hurtbox, root reference through Editor. Check inherited dash variant without overriding inheritance unnecessarily. |
| Template/Features/DebugVisualization/Assets/Configs/Config_Template_DebugVisualization_Settings.asset | Add Combat/Hurtboxes/Idle green and Combat/Hurtboxes/Contact red channel styles through Editor. |
| TechnicalPrototype/Tests/Play/PlayerCombatPhasePlayModeTests.cs | One integration scenario checks same-tick receipt, visibility after list clearing, expiry, later execution, misses, disable and despawn/new spawn cleanup. |
| .ai/features/combat-system/subtasks/FEAT-01-manual-play.md | Replace fleeting-contact-only manual instructions with visible hurtbox/held-contact check. |

No new feature, installer binding, service, event, RPC, gameplay interface or frozen
contract amendment. Existing hit probe/hurtbox and player/enemy debug components
remain behaviorally unchanged. The new component belongs to existing GC Combat
and consumes existing Template debug infrastructure.

## Execution Chain and Lifetime

1. Existing root initialization initializes the new colleague. OnEnable/OnDisable
   only register/unregister IDebugDrawable, following existing debug registration.
2. Player tick: hit probe creates authoritative forward contacts -> new debug
   colleague reads matching ContactTick and Contacts immediately -> registry
   TryGetComponent<CombatDebugVisualizationComponent> resolves the target -> target
   records a local unscaled highlight deadline (now+0.20s).
3. Receipt requires available source/target components in the same Runner. Proxy
   and replay callbacks never produce contact receipts. A new contact may refresh
   the deadline; no retained entity references, scene service registration or queue.
4. DrawDebug reads the actual available hurtbox.WorldShape and uses red while its
   local deadline is active, green otherwise. Position follows the target's current
   shape, not the source's old hitbox. Optional Last Contact Tick/Execution/Source
   values provide a persistent diagnostic receipt without changing Contacts This Tick.
5. Reset, network-context invalidation and disable clear local highlights/receipts.
   Root initialization/reset remains owner; no hidden Awake/Start initialization.
   Drawing unavailable hurtboxes returns without accessing WorldShape.

Host/Solo displays actual-contact flashes. Client displays its hurtbox contours
but cannot infer authoritative contacts from overlap; no replicated flash is added.
Contact means geometry/ledger acceptance here, not damage (FEAT-02 owns damage).

## Validation and Manual Handoff

Compile, relevant prefab/composition Edit tests, existing Solo combat Play suite,
runtime build, strict baseline and diff check. Through connected Editor verify
both prefab roots/links and inherited dash variant. Actual Solo Play: spawn enemy,
attack at range/overlap and inspect live debug state/drawing commands for green ->
red held across render refresh -> green, consecutive executions and despawn/new
spawn cleanup. Report actual executed checks and remaining limits separately.

Developer steps after implementation: Preloader -> Play -> Start Single Player;
Console/BackQuote -> DebugViz -> Combat -> Hurtboxes, enable both channels; close
Console; F7 enemy/F9 dash enemy; WASD direction and LMB attack; F8 cleanup. Verify
red outline on the contacted target for about0.20s, no red on a miss, no stale red
after cleanup/restart. Independent final diff review and Play acceptance remain.

Scope: existing FEAT-01, existing five-subtask plan. Notion unchanged: debug workflow
and verification records remain local. Executed results are recorded separately.

# Combat Research and Initial Plan

Date: 2026-10-15. Requirements: accepted revision 4, APR-001.
Research is complete for initial decomposition. The five-part plan below was
approved by the developer (APR-002). FEAT-01–FEAT-05 are now recorded in state;
FEAT-01 architecture is proposed separately and has not yet been approved.

## Authority and Source Check

Side-conversation proposal update, 2026-10-15: FEAT-01 architecture revision 7
adds the developer-requested buffer-model refactoring. Keep the existing Networked
InputBufferStateModel and separate expiry TickTimer; pack the execution lock into
bit 0 and pending command ID into bits 1..7 of one byte (0 empty, 1..127 valid).
Reuse the ordinary InputBufferCommandDescriptor and existing ushort-facing API;
reject out-of-range IDs, preserve independent lock/command updates, and verify
the actual Fusion layout and snapshot behavior. This records the requested plan
change, not approval of the complete architecture or a measured bandwidth gain.

The developer also accepted this action contract hierarchy:
ICombatActionStateAccessor + ICombatActionSnapshotAccessor ->
ICombatActionAccessor -> ICombatActionMutator. All four DI contracts resolve to
the same player root; narrow consumers receive only the required interface.
The root delegates derived snapshot evaluation to its explicitly referenced
PlayerActionComponent, which reads source state without calling the snapshot
getter recursively. No independent snapshot mutator or separate DI implementation
is introduced. This replaces the proposed ICombatActionStateMutator name and
split root/component accessor bindings; full architecture approval stays pending.

The developer accepted separate identifier domains: buffered CommandId expresses
input intent (one Attack command for all attack variants), while gameplay ActionId
identifies the concrete profile selected by the executor at action start.
The 1..127 packed-input limit does not apply to ushort ActionId. ExecutionId is
the actor-local execution counter; combine it with EntityId for contact identity.
FEAT-01 resolves Attack to its test profile; FEAT-03 extends gameplay selection
without adding concrete attack IDs to input. Validate and test these domains
independently, including CommandId != ActionId and ActionId above 127.

The developer confirmed removal of the proposed descriptor-returning peek API.
Reuse IInputBufferController and InputBufferController unchanged: executors check
their known intent with HasBufferedCommand and consume through BeginAction at
the allowed phase gate, using EndAction for the prior execution. Gameplay action
execution receives boolean eligibility/consumption results, not descriptors or
input containers. Retain regression tests and the separately accepted packed-model
refactoring; this does not authorize implementation or complete architecture approval.

RQ-01–RQ-13 and AC-01–AC-13 refer to `requirements.md`. DEC-001–DEC-008
retain the developer's behavior choices. Notion's Combat specification was
freshly read by the isolated documentation worker: no source drift from
`2026-10-15T09:42:13.677Z` before recording requirements acceptance.
Postwrite version: `2026-10-15T10:08:04.160Z`. This accepted research start,
not the implementation plan or architecture.

## Research Brief

Paths in this document are repository-relative. `GC` means
`Assets/ProjectCore/Contexts/GameCore`; `TP` means
`Assets/ProjectCore/Contexts/TechnicalPrototype`; `Template` means
`Assets/ProjectCore/Contexts/Template`.

| RQ / AC | Authority | Actual mechanism / bounded finding | Reuse or extension | Planning impact |
| --- | --- | --- | --- | --- |
| RQ-01 / AC-01 | DEC-001, DEC-006 | `GC/Features/Input/Scripts/Managers/Controllers/InputBufferController.cs` stores one command through `IInputBufferStateMutator`. `InputBufferStateModel` has one ID, expiry TickTimer and lock. `PlayerNetworkInputComponent.OnInput` sends only MoveDirection and DodgePressed; `PlayerInputSourceComponent.NetworkTick` buffers Dodge only. `InputActions_TechnicalPrototype.inputactions` has Move and Dodge only. | Extend this buffer and input chain; do not add a parallel queue or command-priority system. Preserve current input capture while action execution is locked. | Attack input, action direction, action phases and recovery-window consumption must be introduced together. Existing 0.15 s Dodge buffer expiry must be reconciled with input retained across phases at architecture approval. |
| RQ-01, RQ-04 / AC-01, AC-04 | DEC-001, DEC-002, DEC-006 | `PlayerDodgeState.Enter` consumes the pending action and starts one MovementStateTimer; `DodgeProcessor` reads current direction and exits at expiry. `DodgeMovementConfig` has speed and one duration; no phase windows. `PlayerLocomotionState` updates facing before Dodge transition. | Reuse movement processors/body, but integrate the common phase/lock rules explicitly. | A three-phase Dodge cannot be achieved by adding HP immunity to the existing single timer alone. Input/body/facing order must be checked in FEAT-01. |
| RQ-02 / AC-02 | Combat spec and phase task | `CombatFeature` binds DamageableEntityService and DamageSourceEntityService only. Their base contracts expose damage amount/type or durability/application; no active attack execution or hit deduplication implementation was found in active GC/TP code. | Extend GameCore Combat using entity IDs and component lookup; keep collision/movement body separate from hurtboxes. | First deliverable needs an observable contact trace with real actor hurtboxes and phase-gated hitboxes, before HP changes. |
| RQ-03, RQ-07 / AC-03, AC-07 | S3, DEC-005, DEC-008 | `NetworkEntityRegistry` resolves stable entity IDs; `NetworkEntitySpawner` delegates spawn/despawn to existing factories. GC damage base classes are also consumed by legacy `Contexts/Prototype/Features/Player` implementations. Active TP roots have no health state. | Reuse factories/registry and preserve legacy base-contract compatibility. New active runtime must not depend on Prototype. | Authoritative HP/death must be added to active actors, not copied by introducing a dependency on the old prototype. Enemy targeting currently tests target registration, not alive/dead state. |
| RQ-03, RQ-05, RQ-06, RQ-09 / AC-03, AC-05, AC-06, AC-09 | DEC-002, DEC-007 | `BaseNetworkEntityRoot` initializes ordered components in Spawned, ticks them in FixedUpdateNetwork and presents in Render. Player simulation runs on State/Input Authority; EnemyBehaviourStateMachine ticks only on State Authority. Base root Despawned unregisters the root but offers no per-component teardown callback. | Keep authority and presentation separate; define cleanup/reset in the changed owners rather than adding a broad framework redesign. | HP/hit history must remain authoritative and feedback must not replay during correction. Cleanup on death, despawn, scene disposal and restart requires explicit architecture. |
| RQ-11 / AC-11 | DEC-003 | EnemyBehaviourStateType is Disabled/TargetSelection/Waiting/Chasing. Chasing state uses chase-ability processors and has no attack action. EnemyDashChaseAbilityProcessor emits a movement request from target/distance/cooldown. EnemyNetworkEntityComponent stores dash movement timers. | Extend enemy ability/action coordination; consume shared attack contracts. | Dash hitbox attachment may follow the visual pose, but its execution/phase must not be inferred from the movement state or DashActionTimer. Shared configuration may coordinate working values. |
| RQ-08, RQ-12 / AC-08, AC-12 | DEC-002, DEC-004 | Live prefab inspection found four-direction player Idle/Walk/Dodge clips, controlled by PlayerAnimatorComponent and PlayerMediatorComponent. Both enemy prefabs have a SpriteRenderer and no Animator. No attack/death clips exist under TP. Current sprite assets are 1024x1024, 100 PPU; final rendered size also depends on hierarchy transforms. | Reuse actual character references/directional presentation, extend only related animation ownership. Generate and import temporary attack/death frames after the resource plan is approved. | Art preparation, animation integration, trails, phase alignment, flash/shake and visual acceptance are mandatory work, not a final optional polish pass. |
| RQ-08 / AC-08 | DEC-004, UI standard | Scene dependencies include Template ScreenNavigation/Popup UIDocument roots and PanelSettings with empty catalogs. Editor AssetDatabase enumeration found no root CommonUI/font resources under `Assets/ProjectCore/Assets` at inspection. | Reuse existing UI hosts and generic mechanisms. Add feature-owned HP widgets with per-view presentation ownership; project UI standard defaults to MVVM. | Show a visual prototype before completing functional UI wiring; no new global HUD singleton or direct dependencies on private UI from another concrete context. |
| RQ-13 / AC-13 | DEC-005, DEC-008 | EnemySpawnComponent supports Host/Solo spawn/despawn and F7/F8/F9 debugging only; no periodic match state was found. PlayerSpawnComponent handles player join/leave and owns four spawn points but no restart restoration. | Compose a TP scene-owned Match capability using existing public spawn/registry contracts; extend narrow public operations where required. | Match flow depends on life/death and completed actor integration, and needs Host-only popup commands plus replicated state. Do not reuse a scene reload as an unapproved substitute for revive/reset. |
| RQ-09, RQ-10 / AC-09, AC-10 | S1–S4 and accepted contract | Existing Editor tests cover input, movement, facing, prefab configuration, targeting and registry. `Template/Tests/Play/ApplicationStartupPlayModeTests.cs` covers Preloader, scene composition, Solo spawn/despawn and scene recreation. No active Combat test suite was found in GC/TP tests. | Extend the existing Editor/Play assemblies and `.asmref` arrangement; add no `.asmdef`. | Add meaningful behavior tests per deliverable; verify actual Solo/2/4 at subtask gates and a final full journey. Code inspection/build is not network evidence. |

Negative findings above are bounded to active GC/TP runtime code, their referenced
assets, TP animation asset queries, and current test directories. Legacy health
code and debug sample health values were found elsewhere; these are not proof
of a working current Combat implementation.

## Existing Integration Paths

Current player path:

`PlayerNetworkInputComponent.OnInput -> PlayerInputData ->
PlayerInputSourceComponent.NetworkTick -> InputBufferController /
PlayerMovementStateMachine -> movement processors ->
TransformMovementBodyComponent -> MovementSimulationService`.

Facing/state rendering is driven by replicated PlayerNetworkEntityComponent
properties through PlayerMediatorComponent into PlayerAnimatorComponent.
Attack phases must run before affected facing/movement decisions without
preventing input collection. Exact changes belong to FEAT-01 architecture.

Current enemy path:

`EnemyBehaviourStateMachine (State Authority) -> EnemyBehaviourChasingState ->
chase ability processor -> EnemyInputSourceComponent ->
EnemyMovementStateMachine -> movement body`.

Proposed complete path:

`input / enemy intent -> actor action execution -> phase and active hitbox ->
authoritative hit validation -> HP / hitstun / death -> replicated state ->
actor animation and HP UI`.

Proposed match path:

`Host hotkey -> existing Popup host + per-view ViewModel -> Match domain
command -> periodic spawning -> authoritative full-wipe detection -> stop
spawn + Restart popup -> clear enemies / reset players -> awaiting Start`.

GameCore owns reusable combat rules. Player/Enemy own their concrete actor
adapters, profiles and visuals. TechnicalPrototype owns prototype match flow
and scene composition. Template remains reusable. Shared stays .NET-only.
No changes to networking SDKs, packages, startup ownership or assembly
definitions are proposed.

## Approved Initial Plan — Revision 1

| ID | Verifiable outcome | Included systems / paths / objects | Reuse / expected change | Dependencies and reason | Validation |
| --- | --- | --- | --- | --- | --- |
| FEAT-01 | Player executes a test attack with fixed pose, phase-gated contact and latest-action buffering; Dodge has three phases and a recovery transition window. | GC Input/Combat/Movement/MovementSimulation integration points; TP Player input/root/states/prefab/input actions; hurtbox adapters on both Enemy prefabs; existing DebugVisualization. | Extend current one-slot buffer, ordered entity lifecycle and movement integration. Add data-driven phases and active zones with per-execution target deduplication. No HP, full reaction or generated art yet. | First: hit/phase identity and action locks are prerequisites for valid damage. | Editor behavior tests for phases/windows/latest-action replacement/shape contact and duplicate targets. Play Mode: locks + changed input direction, Dodge phases, active-only contact, multiple targets, cancellation/cleanup, Host/client phase agreement. |
| FEAT-02 | Confirmed hits change HP once; phased Dodge prevents damage only in active; hitstun cooldown starts at onset; dead actors cannot act. | GC Combat authoritative damage/health/reaction state; TP Player/Enemy roots and death/action/targeting adapters; existing registry/spawn contracts. | Preserve legacy abstract contract consumers, add active runtime implementations. Expose state to presentation without implementing final graphics. | FEAT-01: relies on action execution/contact and phase state. | Host-only damage, no friendly fire, death and duplicate-hit checks; repeated hitstun before/after cooldown expiry including cooldown shorter than hitstun; dead-target exclusion; Solo/Host/client cases. |
| FEAT-03 | All three player attack profiles and both enemy attack types are playable; dash attack contact follows the actor visual while action timing remains independent. | GC Combat profiles/zones; TP Player profiles; Enemy targeting/chase/ability/action integration; both Enemy prefabs and dash config. | Reuse chase/steering/dash motor; introduce attack intent without coupling attack completion to DashActionTimer. Use test profile assignment, not an automatic completion of the separate attack-switching feature. | FEAT-01 + FEAT-02: meaningful AI combat needs phases and damage/death rules. | Three player shapes against several targets; both enemies perform frontal attacks; moving dash contact and duplicate prevention; test movement/action duration mismatch; interruption and dead actors; Solo/2/4 progression as available. |
| FEAT-04 | Attacks, impact and death have integrated generated placeholder animations; HP UI follows characters and updates consistently. | TP Player/Enemy animation frames/clips/controllers/visual adapters; GC Combat public presentation state; scene-owned health UI resources/adapters and existing UI hosts. | Generate consistent temporary frames from inspected references; preserve four-direction player convention. Integrate sword/fire-spin/ice-thrust trails, animation-only hitstop, target flash/shake and death. Compact MVVM for stateful HP presentation; no global singleton. | FEAT-02 + FEAT-03: visual timing and health bind to real action/damage state. Art inventory and resource/prototype acceptance occur within this part before complete wiring. | Import/reference checks; visual prototype acceptance; phase/animation alignment and distinguishing all attacks; no duplicate feedback on correction; HP positioning/scaling, spawn/despawn and disposal; FHD/UHD and relevant aspect ratios; no SFX. |
| FEAT-05 | Host can Start periodic combat, observe a full wipe, Restart into a clean idle match and explicitly Start again. | Proposed TP Match feature; TechnicalPrototypeContextInstaller and gameplay scene; narrow EnemySpawnComponent/PlayerSpawnComponent operations; existing Popup definitions/catalog/host; health/action reset contracts. | Reuse spawn/despawn/registry and Popup lifetime with a per-view ViewModel. Add replicated match state and Host-validated commands. Preserve Preloader entry and network session. | FEAT-02 + FEAT-03 + FEAT-04: needs reliable life-state, enemy combat and accepted presentation. | Host-only controls, double-click/reopen safety, periodic spawn, wipe stop, remaining-enemy removal, full HP/start positions, action/buffer/target/feedback cleanup, no automatic restart, second Start; complete Solo/2/4 journey and scene re-entry. |

After FEAT-05, run the workflow's final verification and audit over every
RQ/AC; do not create a separate feature subtask solely to duplicate those gates.
Each deliverable still passes its own audit, developer diff review and required
Play Mode before an accepted checkpoint. FEAT-04 visual prototype acceptance
is distinct from complete-diff and Play Mode acceptance.

The initial plan does not implement a weapon/build system, full attack swapping,
final art, SFX, a production lobby, or a general presentation/action framework.
The source's three Dev tasks remain external board tasks; these five approved
implementation deliverables do not automatically create or close Notion tasks.

## Material Boundaries and Required Experiments

| Boundary / unknown | Evidence | Effect and next action |
| --- | --- | --- |
| Buffer lifetime vs pending action / new direction | Current Dodge expiry is 0.15 s and command descriptor carries only an ID. Accepted contract keeps collecting direction and latest action while locked. Side-conversation clarifications on 2026-10-15 confirm finite Attack lifetime (initial configurable 0.15 s) and reuse of the existing buffer; BufferCommand already supports replacement while locked. | FEAT-01 architecture revision 7 reuses InputBufferCommandDescriptor, the existing timer and unchanged IInputBufferController / InputBufferController. Executors query known intents with HasBufferedCommand and consume through BeginAction at the phase gate; no peek API or descriptor exposure is added. Demonstrate expiry through locked phases, no refresh from held input, and deterministic event ordering without priorities. Complete architecture approval remains pending in the main conversation. |
| Tick order and prediction | Player root has an explicit input -> mediator -> movement state -> body component sequence and replicated state. | Introduce the minimum phase/movement gating at an agreed point, verify correction/re-simulation, and validate no one-frame rotation/motion leak. |
| Legacy base contract compatibility | Prototype health and trigger derive from GC damage bases. | Enumerate callers before API changes; prefer additive contracts/adapters unless an exact compatible change is approved. Keep active runtime independent of legacy code. |
| Moving dash hitbox vs independent action | Existing dash intent/movement has its own duration and cooldown. | FEAT-03 tests must intentionally use different action/movement durations; follow actor pose, but never infer hit eligibility from a movement timer. |
| Generated frame consistency / gameplay timing | Player has four-direction clips; enemies have no Animator. Generated frames are not guaranteed temporally consistent. | FEAT-04 resource architecture names references, directions, frame sets, imports and phase timing. Preview generated results and repair consistency before integration acceptance. Do not treat image generation as a completed animation pipeline. |
| Presentation and authority | No existing current health UI; generic Popup/ScreenNavigation hosts are already installed. | Keep plain per-view presentation state, stable state projection, explicit subscriptions/cancellation and camera projection. Verify late/reopened UI and no feedback duplication. |
| Spawn/death/reset cleanup | Root Despawned unregisters entities only; current Player join spawn does not record a restart operation. | FEAT-02 and FEAT-05 must agree narrow cleanup/reset responsibilities, original spawn positions and local camera continuity; do not silently change identity/reconnect behavior. |
| Working values and load definition | Behavioral contract deliberately leaves numbers/resource details open; AC-10 has no FPS target. | Propose phase/window/zone/HP/reaction parameters at the owning architecture gate. Record actual multi-target test conditions and timing/allocation observations; do not claim a separate crowd-capacity target passed. |
| Existing imported asset names | Strict baseline reports four ASSET003 errors in unrelated untracked Enemy PNGs. | Preserve these files. This is a pre-existing blocker to a clean strict gate/accepted automatic commit, not permission to rename developer assets or change validation policy. |

## Baseline and Verification Limits

- Unity CLI `1.0.0-beta.13`; Editor `6000.3.14f1`, one ready instance for this project.
- Editor is not in Play Mode; active loaded scene is the clean `Scene_Preloader`.
  No scene was opened, modified or saved during this research.
- Read-only live eval inspected the three current network prefabs and the gameplay
  scene's asset dependencies. No GameObjects/assets/packages were mutated.
- `dotnet build .\ProjectCore.Runtime.csproj --no-restore -v:minimal`: passed,
  0 errors, 8 MSB3277 dependency-version warnings (Memory/Buffers and related
  Unity/package references). This is not a passed gameplay test.
- `Validate-Architecture.ps1 -Mode Strict`: failed, 533 scripts / 171 resources,
  4 ASSET003 naming errors, 0 warnings. All four errors concern the developer's
  untracked Enemy PNG names beginning `ChatGPT Image 30 июл. 2026 г.`.
- Live `console_status`: compilationFailed=false, compiling=false,
  consoleErrors=0, consoleWarnings=2 at `2026-10-15T10:11:02.4255916Z`.
- No new tests, Play Mode run, multiplayer session or performance benchmark
  was executed at this research stage. These remain required implementation evidence.

## Approval

Status: approved by the developer on 2026-10-15 through «Утверждаю план на задачу»,
APR-002. The five deliverables, scopes, order and dependencies are fixed. FEAT-01
architecture may now be prepared; runtime implementation still requires acceptance
of that architecture. No planning-only automatic commit is authorized.

## Parallel Implementation Requirement

The same developer message requires each architecture to identify at least two
usefully independent implementation packages and their correct integration
(DEC-009). This adds an execution constraint, not new FEATs or reordered feature
scope. FEAT-01 proposes A: action phases/input and B: geometry/query, with contracts
and all root/DI/Unity integration owned by main. Full allowlists and merge/check
rules are in `subtasks/FEAT-01.md`. Later architectures must perform the same
independence analysis before dispatch. Implementation workers may edit only their
approved source packages; the main agent keeps Git, Unity, state and gate ownership.

## Revision 8 Source Refresh — 2026-10-16

Isolated refresh combat-r8-source-refresh-20261016 completed, received-request
SHA-256 671c63b9e33f1aed676033624e5b04aed09362420fd67260240597243ee3964a.
No new material behavior/acceptance delta found. Spec version remains
2026-10-15T21:40:41.580Z, task version 2026-10-12T17:30:47.137Z (In progress),
live documentation standard v1.2. Spec normalized SHA-256
5a7334db1dd69b7a3ccd16d1490417554d23b0169e071451445293807f39987e.
Timestamp-only prior baselines cannot prove byte-identical content; historical
delivery notes are distinguished from approved current requirements.

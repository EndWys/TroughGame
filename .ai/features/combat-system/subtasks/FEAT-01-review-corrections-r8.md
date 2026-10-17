# FEAT-01 Architecture Revision 8

## Status and Origin

Approved by the developer on 2026-10-16 (APR-006): «Подтверждаю корректность изменений. Реализуй правки». Implementation in progress. Developer review feedback dated 2026-10-16,
items 1-10, reopens local architecture for the same FEAT-01. Architecture revision
7 and APR-003 remain historical accepted evidence. Review revision 1 is not accepted.
The five-subtask plan, requirements, timing values and world geometry remain unchanged.

Reconciliation: HEAD d9d3914e6b54c62659b0cdeacbcbf18d12dec90b, empty index;
112 of 113 review-manifest files match byte-for-byte. The remaining change is only
line 33 indentation in CombatHitProbeComponent.cs (8 spaces to 12), verified
against the saved review patch. Preserve this external formatting correction.
Existing unrelated assets and stashes remain untouched.

## Review Findings and Proposed Resolution

| Item | Verified Current Fact | Proposed Correction |
| --- | --- | --- |
| 1 | ResetLocalRuntime exists only on three concrete combat components; actor roots call them individually. | Add ResetLocalRuntime to INetworkEntityComponent. Root/composite own recursive cleanup through that interface. |
| 2 | Parent/Object/IsValid/Runner checks are repeated in combat components. | One lifecycle processor evaluates network-context validity; base wrappers use it. Authority and forward-only rules remain component-specific. |
| 3 | Readiness flags are local to individual combat components; public lifecycle methods are overridable. | Base lifecycle wrappers cannot be overridden; protected hooks own component work. Ready becomes true only after successful initialization. |
| 4 | CombatAttackConfig is an authored definition; player action and probe each serialize a reference. There is no runtime multi-attack selection yet. | Keep immutable authored SO definitions. Resolve the executed ActionId through one actor-owned resolver; remove probe's independent config reference. Three attack selections stay FEAT-03. |
| 5 | Discarded ToWorld result at probe line 33 validates by throwing. | Explicit CombatShapeValidation.ValidateSettings call from definition initialization; no discarded shape construction. |
| 6 | ContactDetected has one subscriber, in PlayerCombatPhasePlayModeTests; no production subscriber. | Remove event. Expose read-only per-forward-tick results with explicit tick identity for FEAT-02 consumption. Test uses Fusion AfterTick observation. |
| 7 | Probe casts parent to BaseNetworkEntityRoot to read EntityId. | Root implements INetworkEntityIdAccessor; base exposes injected EntityId without consumer casts. |
| 8 | Hurtbox reads root transform; MovementBody.Position already adds Offset=(0,0.2). Targetable and dash also currently use root transforms, so there is no pre-existing uniform position accessor. | Add movement-owned IMovementOriginAccessor. OriginPosition is the unoffset entity origin; Position remains collision center. Combat uses injected origin, preserving authored offsets and world shapes. |
| 9 | ToWorld creates CombatShapeData from local settings, world origin and forward. | Rename to CreateWorldShape and update every caller/test. |
| 10 | PlayerActionComponent handles combat attack and combat Dodge phases only. | Rename type/file to PlayerCombatActionComponent; preserve script GUID and migrate serialized type/field names. |

## Contract and Ownership Delta

All gameplay types retain the existing flat ProjectCore.GameCore or
ProjectCore.TechnicalPrototype namespaces and existing assemblies.

| Owner / Type | Proposed API | Callers / Compatibility |
| --- | --- | --- |
| NetworkEntities / INetworkEntityComponent | Existing Init(), NetworkTick(), ClientRender(); add void ResetLocalRuntime(). | All implementations and test doubles must implement reset. No default-interface no-op. |
| NetworkEntities / BaseNetworkEntityComponent | Nonvirtual public Init(), NetworkTick(), ClientRender(), ResetLocalRuntime(); public bool IsInitialized; protected bool HasValidNetworkContext; protected NetworkEntityIdData EntityId. | Existing MonoBehaviour subclasses migrate override bodies to hooks. Readiness stays local, not replicated. |
| NetworkEntities / BaseNetworkEntityBehaviourComponent (new) | Same lifecycle wrappers/properties, inheriting Fusion.NetworkBehaviour. | CameraTargetComponent, TransformMovementBodyComponent, EnemyMovementStateComponent migrate to this base. No shared-state singleton. |
| Both bases | protected virtual void OnInitialize(), OnNetworkTick(), OnClientRender(), OnResetLocalRuntime(), OnNetworkContextInvalidated(). | Default hooks do nothing. Invalid-context hook clears probe output/ledger; it does not clear on Fusion replay. |
| NetworkEntities / NetworkEntityComponentLifecycleProcessor (new) | bool IsInitialized; bool HasValidNetworkContext(NetworkBehaviour parent); bool CanExecute(NetworkBehaviour parent); void Initialize(Action initialize, Action reset); void Reset(Action reset). | One plain C# lifecycle owner per component, reused by both bases. Initialization-only delegates; no per-tick delegate creation. Failed initialization never marks ready and triggers cleanup. |
| NetworkEntities / INetworkEntityIdAccessor (new) | NetworkEntityIdData EntityId { get; } | BaseNetworkEntityRoot implements; existing BindInterfacesTo root registration supplies DI. Read only with valid network context, since ID is networked. |
| MovementSimulation / IMovementOriginAccessor (new) | Vector2 OriginPosition { get; } | TransformMovementBodyComponent implements from its simulation transform. Position remains OriginPosition + Offset. Combat consumers receive it through entity-local DI. |
| Combat / ICombatAttackResolver (new) | bool TryGetAttack(ushort actionId, out CombatAttackConfig attack). | Player root implements and delegates to PlayerCombatActionComponent. Probe resolves snapshot.ActionId; unknown active attack fails explicitly. Single authored attack remains in FEAT-01. |
| PlayerCombatActionComponent | bool TryGetAttack(ushort actionId, out CombatAttackConfig attack). | Resolves its one authored definition by profile ActionId, never by input CommandId. Init validates profile and shape. |
| Combat / ICombatContactAccessor (new) | int ContactTick { get; }; IReadOnlyList<CombatContactData> Contacts { get; }. | Probe implements. Exposes a cached read-only view, not a mutable List. Results belong only to ContactTick and are replaced on the next forward tick; invalid/reset clears results and invalidates tick. |
| Combat / CombatShapeValidation (new) | static void ValidateSettings(in CombatShapeSettings settings); static void ValidateWorldShape(in CombatShapeData shape). | Extract existing geometry validation, preserving accepted finite/dimension rules. Geometry utility and definition initialization call it explicitly. |
| CombatGeometryUtility | static CombatShapeData CreateWorldShape(in CombatShapeSettings settings, Vector2 origin, Vector2 forward). | Rename only; preserve transform and overlap mathematics. Existing Overlaps API remains. |

Contact processing stays server/state-authority/forward-only. Replay does not
clear emitted results or the duplicate ledger. A future FEAT-02 consumer runs
after probe, requires ContactTick == Runner.Tick.Raw and forward authority, and
processes only that tick's results. This proposal introduces no damage consumer
or test-only production subscription. Play Mode test observation uses the installed
Fusion IAfterTick callback on a nested test-only SimulationBehaviour, registered
with NetworkRunner.AddGlobal and removed with RemoveGlobal, and copies results
once per distinct tick. These APIs were verified in installed Fusion.Runtime.xml;
their revised test integration has not been executed.

## Lifecycle and Execution Chain

1. Existing application/context/feature initialization completes through the
   unified flow. Entity installer only registers dependencies and explicit instances.
2. Fusion Spawned invokes root BeforeComponentsInitialized, registration and
   component composition, then Init in the existing order. Root tick/render stay
   gated until all components and AfterComponentsInitialized succeed.
3. Init first clears old local runtime, invokes OnInitialize and marks ready
   only after successful return. Composite owns child initialization. Failure
   resets initialized/partially initialized children and unregisters the root;
   cleanup continues for remaining children even if one reset throws, reporting
   the original error and cleanup failures. No later component tick is dispatched.
4. NetworkTick/ClientRender wrappers require readiness and a valid injected
   parent, Object, Object.IsValid and Runner. Component-specific authority stays
   inside OnNetworkTick; proxy ClientRender remains allowed. No isActiveAndEnabled
   change is imposed on all components; hurtbox retains its own availability rule.
5. Player input -> PlayerCombatActionComponent -> network action snapshot ->
   movement -> movement body -> hit probe -> readonly tick contacts. Probe resolves
   the current attack definition by ActionId and shape origin via movement accessor.
6. Root Despawned disables dispatch, resets top-level components in reverse order,
   and unregisters in finally. Composite resets only its own children in reverse
   order, so root does not also traverse descendants. Reset is idempotent, works
   after Object becomes invalid, and never writes replicated fields. Camera's own
   Despawned/OnDestroy unregister remains idempotent. New spawn repeats Init.

The root remains the dispatch owner; both component bases share the lifecycle
processor, not each other's Unity inheritance. Explicit action cancellation
remains a network-authority operation and is distinct from local teardown.

## Exact Expected File and Unity Inventory

Paths below are relative to Assets/ProjectCore/Contexts unless prefixed .ai/.
Every new/moved Unity source retains or receives its matching .meta through
the Editor; no scene/ProjectContext/assembly/package changes are proposed.

| Area | New / Changed / Reused Files | Responsibility |
| --- | --- | --- |
| GameCore NetworkEntities, Scripts/Abstract | Change INetworkEntityComponent.cs, BaseNetworkEntityComponent.cs, BaseNetworkEntityCompositeComponent.cs, BaseNetworkEntityRoot.cs; new BaseNetworkEntityBehaviourComponent.cs, INetworkEntityIdAccessor.cs. | Lifecycle wrappers, root cleanup/readiness and ID DI. Existing INetworkBehaviourAccessor and BaseNetworkEntityInstaller reused. |
| GameCore NetworkEntities, Scripts/Other/Processors | New NetworkEntityComponentLifecycleProcessor.cs. | Shared initialization/reset/context-validity implementation. |
| GameCore Movement | Change Scripts/Abstract/BaseMovementStateMachine.cs. | Migrate lifecycle overrides, preserve state ordering/authority. |
| GameCore CameraTarget | Change Scripts/Components/CameraTargetComponent.cs. | Fusion-compatible lifecycle base and idempotent unregister/reset. |
| GameCore MovementSimulation | New Scripts/Abstract/IMovementOriginAccessor.cs; change Scripts/Components/TransformMovementBodyComponent.cs. | Explicit origin, existing collision center, Fusion-compatible lifecycle and local velocity reset. |
| GameCore Combat, Scripts/Abstract | New ICombatAttackResolver.cs, ICombatContactAccessor.cs. | ActionId definition lookup and tick-scoped contact reads. Existing action state/snapshot/mutator hierarchy unchanged. |
| GameCore Combat, Scripts | Change Components/CombatHitProbeComponent.cs, Components/CombatHurtboxComponent.cs, Static/Utilities/CombatGeometryUtility.cs; new Static/Validation/CombatShapeValidation.cs. | Remove duplicated lifecycle/config/casts/event; use origin and explicit validation. Existing CombatAttackConfig, action model, contact processor and query API reused. |
| TechnicalPrototype Player, Scripts/Components | Move PlayerActionComponent.cs + meta to PlayerCombatActionComponent.cs + meta; change PlayerNetworkEntityComponent.cs, PlayerInputSourceComponent.cs, PlayerMediatorComponent.cs. | Hook migration, resolver delegation, root reference/type migration and generic reset. Existing serialized field names retained where possible; renamed action field uses FormerlySerializedAs. Renamed type uses MovedFrom. |
| TechnicalPrototype Enemy, Scripts/Components | Change EnemyNetworkEntityComponent.cs, EnemyInputSourceComponent.cs, EnemyAbilityCollectionComponent.cs, EnemyMovementStateComponent.cs, StateMachines/EnemyBehaviourStateMachine.cs. | Lifecycle-hook compatibility and removal of bespoke root reset. Ordinary gameplay-state Init methods are not renamed. |
| Prototype Player, Scripts/Components | Change PlayerCameraComponent.cs, GroundDetectorComponent.cs, PlayerPoseComponent.cs, ClimbDetectorComponent.cs, PlayerMediatorComponent.cs. | Compile-compatible hooks and cleanup for shared-base consumers; no new legacy gameplay behavior. |
| TechnicalPrototype Player/Enemy, Scripts/Init | Change PlayerNetworkEntityInstaller.cs and EnemyNetworkEntityInstaller.cs. | Explicit serialized TransformMovementBodyComponent reference and entity-local IMovementOriginAccessor binding. Root BindInterfacesTo supplies ID/resolver; no scene search. |
| TechnicalPrototype actor assets | Resave Player/Assets/Prefabs/Prefab_Player_NetworkEntity.prefab and Enemy/Assets/Prefabs/Prefab_Enemy_NetworkEntity.prefab, Prefab_Enemy_Dash_NetworkEntity.prefab through CLI. | Wire installer body references, preserve renamed script GUID, remove probe config override. Preserve hierarchy and all phase/shape/buffer values. |
| Tests | Change GameCore/Tests/Editor/CombatGeometryUtilityTests.cs, CombatTargetQueryServiceTests.cs; TechnicalPrototype/Tests/Editor/PlayerPrefabTests.cs, EnemyPrefabTests.cs, MovementStateConfigurationTests.cs; TechnicalPrototype/Tests/Play/PlayerCombatPhasePlayModeTests.cs. New GameCore/Tests/Editor/NetworkEntityComponentLifecycleTests.cs. | New API/test-double/DI compatibility, explicit validation, initialization failure/reset and tick-result observation. Extend lifecycle Play scenarios in the existing fixture. |
| Workflow | This proposal, state.yaml, decisions.md; after approval update FEAT-01.md and generate review revision 2 manifest/patch and audit result. | Preserve revision 1 and accepted common-contract history. No checkpoint/acceptance yet. |

No broad AI position migration is included. The inspected existing targeting/dash
paths already use the same unoffset root-origin meaning; the new movement accessor
provides that exact meaning for combat without changing collision-center semantics.
Movement integration still precedes hit probing, preserving moving-hitbox support.

## Validation Delta and Approval

| Check | Required Result / Limit |
| --- | --- |
| Lifecycle unit tests | No tick/render before successful Init or after reset; failed Init cleanup; repeated reset safe; composite cleanup order and root failure gating. |
| Connected Editor compilation and full Edit Mode | Both Unity base hierarchies compile/weave; all old contexts, test doubles, prefab references and geometry tests pass. |
| Solo Play | Prior phase/buffer/contact scenarios pass using production read access, including one hit per target/execution. Verify explicit invalid-context cleanup and spawn/despawn/reinitialization; distinguish real object reuse from fresh spawn. |
| Host/client Play | State/input authority paths and proxy Render readiness survive common guards; contact processing stays server/forward-only. Replay check must demonstrate unchanged ledger and no repeated contact emission. Unexecuted cases remain gaps. |
| Geometry/DI | Nonzero body Offset changes collision center once; combat world shapes remain unchanged. Both enemy prefabs and player resolve origin, ID and attack contracts as applicable. |
| Project checks | Strict architecture, runtime dotnet build and git diff --check. Existing four unrelated ASSET003 image errors remain visible; no claim that strict passed while those remain. |
| Review gates | Audit and developer review of complete revision 2, then required Play Mode acceptance. No commit or FEAT-02 implementation before gates. |

Developer decision required: approve this local architecture delta (revision 8),
particularly shared lifecycle ownership, origin semantics, ActionId resolver and
tick-scoped contact output. The developer's exact requested renames remain accepted
review directions and are not separate approval questions.

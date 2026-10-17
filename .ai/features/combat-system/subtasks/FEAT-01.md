# Current Architecture: Movement-Owned Dodge / Passive Mediator

APR-020 adds [R16 hurtbox visualization](FEAT-01-hurtbox-debug-r16.md) to the
existing architecture: actual hurtbox shapes, local Host/Solo contact flash and
same-tick debug capture after the probe. Gameplay contracts remain unchanged.
Implementation/checks: [R16 result](FEAT-01-hurtbox-debug-r16-result.md).

APR-011 implements root position and movement/combat separation under requirements
revision6; [revision11](FEAT-01-implementation-r11.md) owns that mapping.
APR-012/013 normalize absent movement timer deadlines and accept the verified
getter/test correction. APR-014 accepts [revision13](FEAT-01-passive-mediator-r13.md)
TEMPORARILY: PlayerActionControllerComponent admits buffered commands; mediator
only dispatches Notify/handlers. Focused R13 diff accepted by APR-015. Full FEAT-01
review/current peer validation remain separate gates. Earlier architectures below
are historical where overridden; accepted requirements/five-subtask plan stand.

Current working-tree timer API refinement is captured in review5: EndTick belongs
to IMovementStateTimerAccessor; mutator starts by ticks and stops the timer. Audit
preserves these observed changes and checks all implementations/test doubles.

# Previously Accepted Architecture: Revision 9

Approved by APR-009 on 2026-10-16: the developer accepted the concrete universal
availability contract and required exactly `_origin != null` for hurtbox local
validity. [Revision 9](FEAT-01-availability-r9.md) overrides the availability API
and callback-entry policy of revision 8 below. Init/reset remain unconditional;
network invalidation is distinct from disable/local invalidity. Keep the current
FEAT-01 and five-subtask plan. Prior full review snapshots remain historical;
architecture acceptance does not accept an unseen implementation diff.

# Previously Accepted Architecture: Revision 8

Approved by APR-006 on 2026-10-16: «Подтверждаю корректность изменений. Реализуй правки».
The exact local delta in [revision 8](FEAT-01-review-corrections-r8.md) overrides
the revision 7 lifecycle/component/config/contact naming details below. Existing
requirements, five-subtask plan, phase/window/input/shape behavior and the common
network action/state interface hierarchy remain unchanged. Revision 7 is retained
as historical approval evidence. Review revision 1 remains unaccepted.

Implementation detail within the approved teardown ownership: root caches the
registered ID and uses an internal NetworkEntityRegistry.Unregister(root,id)
overload, so despawn cleanup never needs to read a potentially invalid networked
ID. Existing public registry behavior/API is retained. NetworkEntityRegistry.cs
is therefore one additional affected infrastructure file; there is no new service
owner or network state field.

# FEAT-01 — Action Phases and Active Contacts

Architecture revision: 7. Status: approved by developer, APR-003, 2026-10-15.
Approval source: the main-chat developer instructed continuing under this updated
contract and explicitly accepted the proposed test balance values and timings.
The side-conversation changes below are included in that accepted revision.
Side-conversation correction, 2026-10-15: the developer confirmed finite input
buffer lifetime, initially 0.15 s and configurable. The main conversation
subsequently accepted the complete updated architecture under APR-003.
Additional side-conversation correction: reuse InputBufferCommandDescriptor,
IInputBufferController and InputBufferController with their existing API unchanged.
Do not introduce a separate action-buffer interface or a descriptor-returning peek.
The developer additionally requested packed buffer state: one byte for command ID
and execution lock inside the existing network model; the expiry timer stays separate.
The developer confirmed the action interface hierarchy in the side conversation:
state accessor + snapshot accessor -> combined accessor -> mutator. This specific
contract and the complete architecture are now accepted under APR-003.
The developer also accepted independent input command IDs and gameplay action IDs:
Attack is one buffered intent regardless of the selected attack. The executor
resolves the concrete action when starting it; no profile ID is stored in input.
Approved host plan: revision 1, APR-002. Parallel-development requirement: DEC-009.
Implementation is authorized by APR-003. Combined diff review and required Play Mode
acceptance remain pending; approval is not evidence of implementation or checks.

## Outcome and Scope

The player can execute one test attack, retain the latest pending action in one
slot, chain only inside an authored recovery subwindow, and perform a three-phase
Dodge. Attack position/facing remain fixed. Active attack geometry detects each
target once per execution. Debug evidence makes phases, buffer and contacts visible.

Covered now: RQ/AC-01, RQ/AC-02 and the phase/state portion of RQ/AC-04;
authority/lifecycle checks support RQ-09. HP, actual invulnerability rejection,
hitstun/death, enemy attack AI, moving dash attack, generated art, HP UI and match
flow remain in their already approved subsequent FEATs.

`GC` = `Assets/ProjectCore/Contexts/GameCore`, `TP` =
`Assets/ProjectCore/Contexts/TechnicalPrototype`. New namespaces remain
`ProjectCore.GameCore` / `ProjectCore.TechnicalPrototype`.

## Before and After

| Boundary | Current implementation | Proposed behavior |
| --- | --- | --- |
| Player input | Move and Dodge, two local button/value snapshots | Add Attack; compare actual button callback sequences and send only the last eligible action from that sample |
| Pending command | Existing single replicated slot with 0.15 s Dodge expiry | Reuse the same slot and expiry timer; allow replacement while locked, but consume only an unexpired command at an allowed transition |
| Action execution | Dodge consumes input and owns one movement timer | One replicated action execution with pure tick-derived preparation/active/recovery and an explicit recovery subwindow |
| Movement/facing | Idle/locomotion process facing and Dodge transitions | Action start happens before movement; attack state writes zero velocity, and ordinary facing processors do not run during an action |
| Contact detection | Legacy damage contracts and query wrappers only | Add XY shape overlap and hurtbox query through the current entity component service; emit authoritative contact records without applying damage |
| Ownership | Root component array drives input, movement and body | Preserve explicit root initialization/tick order; add phase orchestration before movement and contact probing after body movement |

## Frozen Cross-Package Contracts

The main agent publishes these complete files first, after approval, and owns
their signatures during parallel development. Workers may request a contract
change but may not independently change the contract or another worker's files.

### Action State and Evaluation

```csharp
public enum CombatActionPhase : byte { None, Preparation, Active, Recovery }
public enum CombatActionKind : byte { Attack, Dodge }

public struct CombatActionStateModel : Fusion.INetworkStruct
{
    public ushort ActionId;       // 0 means idle
    public uint ExecutionId;      // nonzero, incremented on each accepted start
    public int StartedTick;
    public UnityEngine.Vector2 Direction; // normalized action-start direction
}

public interface ICombatActionStateAccessor
{
    CombatActionStateModel ActionState { get; }
    UnityEngine.Vector2 LatestInputDirection { get; }
}
public interface ICombatActionSnapshotAccessor
{
    CombatActionSnapshotData ActionSnapshot { get; }
}
public interface ICombatActionAccessor :
    ICombatActionStateAccessor, ICombatActionSnapshotAccessor
{
}
public interface ICombatActionMutator : ICombatActionAccessor
{
    void SetActionState(in CombatActionStateModel state);
    void SetLatestInputDirection(UnityEngine.Vector2 direction);
}

public sealed class CombatActionPhaseProcessor
{
    public void Start(ref CombatActionStateModel state, ushort actionId,
        UnityEngine.Vector2 direction, int currentTick);
    public CombatActionSnapshotData Evaluate(in CombatActionStateModel state,
        CombatActionKind kind, in CombatActionTimingData timing, int currentTick);
    public void Cancel(ref CombatActionStateModel state);
}
```

`CombatActionTimingData` is an immutable value with PreparationTicks,
ActiveTicks, RecoveryTicks, ComboWindowStartTicks and ComboWindowEndTicks.
Window coordinates are relative to the beginning of recovery. A window is
half-open `[start,end)`, not all of recovery. Equal start/end disables chaining.

`CombatActionSnapshotData` is immutable with ActionId, ExecutionId, Kind, Phase,
Direction, CanStartNextAction and IsComplete, and a constructor accepting those
values. Idle is Phase=None. IsComplete reports expiration of a non-idle execution;
the actor adapter then completes/cancels it before a new start.

Evaluate uses only the supplied replicated state, immutable timing and tick;
there is no persistent phase cache in the processor. Start increments the
execution counter, skipping zero. Cancel clears the current action while
preserving that counter. A fresh entity spawn clears the full model. Tick
subtraction is unchecked signed subtraction with validated total duration less
than half the tick-counter range.

`CombatActionProfileConfig` has serialized private fields with read-only access:
`ushort _actionId`, `CombatActionKind _kind`, `float _preparationSeconds`,
`_activeSeconds`, `_recoverySeconds`, `_comboWindowStartSeconds`,
`_comboWindowEndSeconds`. `CombatActionProfileValidation.CompileTiming(config,
float tickDeltaSeconds)` validates finite positive phases and legal window bounds,
then returns CombatActionTimingData. Each phase is rounded up to at least one
simulation tick. Window boundaries use ceiling conversion and are clamped to the
quantized recovery interval. A nonempty authored window that collapses after
quantization is rejected rather than silently disabled; equal authored bounds
explicitly disable the window.
Profiles are compiled at actor Init, not before injection or every tick. They
remain unchanged during the session and must match on communicating peers.
Init rejects zero/duplicate gameplay ActionIds among the actor's configured
profiles and kind mismatches: the attack config references an Attack profile,
the Dodge profile is Dodge. Input CommandIds are validated separately against
recognized input intents and their 1..127 storage range. Unknown network command
IDs are rejected at ingestion before replacing the pending slot.

#### Input Intent, Selected Action and Execution Identity

CommandId identifies an input intent such as Attack or Dodge. All player attack
variants use the same Attack CommandId; input providers, PlayerInputData,
InputBufferCommandDescriptor and InputBufferController do not know the chosen
weapon, combo step or concrete attack profile.

ActionId identifies the concrete gameplay action/profile selected by the
executor. It remains a ushort in CombatActionStateModel and is independent of
CommandId; zero is idle, valid profile IDs are 1..65535, not limited to 127.
No equality, cast-based conversion or one-to-one mapping between the two ID
spaces is required. Adding attack profiles does not allocate input command IDs.
ExecutionId identifies a particular accepted execution on one actor, not a
network entity. A contact's execution identity is (EntityId, ExecutionId).

PlayerActionComponent owns selection at the allowed start/transition tick,
using gameplay state at execution time. FEAT-01 maps Attack intent to the one
configured test attack and Dodge intent to its configured Dodge profile; later
FEAT-03 extends concrete attack selection inside the executor. Weapon/build
systems are not introduced here. Selection must remain deterministic on peers
that simulate the actor; ActionId in the network model makes the selected
execution explicit to phase evaluation and presentation.

At a transition, check the executor's known intent with HasBufferedCommand(CommandId),
resolve and validate a concrete profile without side effects, consume with
BeginAction(CommandId), then start
with the selected profile's ActionId only if consumption succeeded. Failed
selection never starts an execution or consumes the command. Chaining gameplay
state advances only after successful consumption/start. The buffer retains
only intent and its existing deadline; selection does not refresh that deadline.

### Existing Single-Slot Input

```csharp
public interface IInputBufferController
{
    bool IsLocked { get; }
    bool HasBufferedCommand(ushort commandId);
    void BufferCommand(InputBufferCommandDescriptor command, float lifetimeSeconds);
    bool BeginAction(ushort commandId);
    void EndAction();
}
```

Existing InputBufferController remains the single implementation of the existing
IInputBufferController. InputBufferCommandDescriptor is reused as an ordinary
value; InputBufferStateModel remains the existing INetworkStruct with the packed
layout below. No extra command type, queue, controller or interface is
introduced. BufferCommand already replaces the slot even while locked and uses
the existing expiry TickTimer; it
never stores a pending action with TickTimer.None. Each new accepted press
replaces the slot and starts a new finite positive lifetime. Initially both
Attack and Dodge use 0.15 s, configured through serialized
PlayerInputSourceComponent._actionBufferLifetimeSeconds rather than hardcoded.
The timer continues during preparation, active and recovery; execution locks,
phase changes, held buttons and missing input frames never refresh or pause it.
HasBufferedCommand(commandId) checks a known intent without consuming it or
changing the lock and clears a matching expired command via the existing mutator.
On simulated authoritative/predicted updates, check both recognized Attack and
Dodge command IDs even while locked or missing an input frame, so the matching
expired slot is cleared. These are independent queries of one slot, not input
priorities. Proxies do not call this potentially mutating validation path.
BeginAction rechecks expiry, succeeds only while unlocked, clears the slot and
locks execution. Cancellation uses existing IInputBufferStateMutator.
ClearBufferedCommand() plus EndAction(), rather than adding a duplicate clear API.
Existing EndAction unlocks without extending the pending command's deadline.
Existing methods/constructors remain compatible. Writes while locked already
work; no changes to IInputBufferController or InputBufferController are planned.
The executor knows the intent ID it handles and receives only the boolean result
of HasBufferedCommand / BeginAction, never an input container or descriptor.
Descriptors remain confined to input ingestion/storage. Action phase gating
belongs to PlayerActionComponent, not the generic input buffer.

#### Packed Network Buffer State

PlayerNetworkEntityComponent keeps its existing `[Networked]`
InputBufferStateModel property as the sole replicated buffer state. The descriptor
is reconstructed from that model when read; it does not need INetworkStruct and
does not own an additional replicated copy.

Replace the model's stored ushort BufferedCommandId and NetworkBool IsLocked
with one private byte `_packedCommandAndLock`; retain the TickTimer field:

```csharp
public struct InputBufferStateModel : Fusion.INetworkStruct
{
    private byte _packedCommandAndLock;
    public Fusion.TickTimer BufferedCommandTimer;

    public ushort BufferedCommandId
    {
        get => (ushort)(_packedCommandAndLock >> 1);
        set
        {
            if (value > 127)
                throw new System.ArgumentOutOfRangeException(nameof(value));
            _packedCommandAndLock = (byte)((value << 1) | (_packedCommandAndLock & 1));
        }
    }

    public bool IsLocked
    {
        get => (_packedCommandAndLock & 1) != 0;
        set => _packedCommandAndLock =
            (byte)((_packedCommandAndLock & 0xFE) | (value ? 1 : 0));
    }
}
```

These computed properties introduce no additional backing fields.
Bit 0 is the execution lock; bits 1..7 are the command ID. Decode ID as
`packed >> 1`, lock as `(packed & 1) != 0`. Command assignment uses
`(byte)((commandId << 1) | (packed & 1))`; lock assignment changes only bit 0.
ID 0 means empty; 1..127 are executable IDs. Reject values above 127 before
conversion/shift rather than truncating them. Existing ushort public ID signatures
and the descriptor stay compatible; their storage need not remain ushort.

Replacing a command preserves the lock and restarts its deadline. Expiry/clear
sets ID to zero and timer to None while preserving the lock. Lock/unlock preserves
the ID and timer. A fresh-spawn reset clears the entire model. Validate configured
bufferable Attack/Dodge IDs in 1..127 during Init; reject out-of-range incoming
IDs before replacing pending input. This limit applies only to input CommandIds;
concrete gameplay ActionIds remain independent ushort values and may exceed 127.

The root's existing copy-modify-assign setters continue writing the complete
Networked model, so packed state and timer remain part of Fusion snapshots and
prediction/reconciliation. No new NetworkBool field is retained. The computed
bool property is safe because it introduces no bool storage in INetworkStruct.
All peers must use the new schema; live compatibility with an older schema is
not claimed. During implementation inspect the woven layout/state word count
for the installed Fusion version and record the actual result. One logical byte
does not promise one byte of network traffic or a measured bandwidth reduction.

LocalButtonStateData adds `ulong PressSequence` through a new constructor while
retaining its four-argument constructor. LocalInputProvider records an increasing
sequence at button-start callbacks and captures it with WasPressed. Capture does
not invent event order from dictionary enumeration. PlayerNetworkInputComponent
selects the latest eligible Attack/Dodge press by sequence and sends one
`ushort LatestActionCommandId` in PlayerInputData beside the unchanged MoveDirection
and legacy DodgePressed member. A held button does not generate new actions.
There is no action-kind priority and no network command-history queue.

The existing directional eligibility for Dodge is preserved: a Dodge press without
movement direction does not become an executable Dodge. Attack may use the last
nonzero direction, falling back to current facing. Actual callback order resolves
multiple eligible presses in the same captured sample.
Directional eligibility is checked when the command is accepted, not again when
it is consumed. A valid retained Dodge remains pending if later raw direction is
zero; the existing live-direction active motion may then be zero. Consumption
validates the recognized command and profile, not a new directional condition.

PlayerInputSourceComponent retains raw direction for movement and replaces the
existing action slot. PlayerActionComponent then reads this valid input frame
and updates the root's replicated last-nonzero intent through
ICombatActionMutator.SetLatestInputDirection. Input is still collected while
locked. A missing input
frame does not erase unexpired pending input or the remembered direction and
does not suspend expiry processing. Attack snapshots
the normalized latest intent at its start; subsequent input affects the next
attack, not the current hitbox. PlayerFacingDirectionUtility continues to map
this direction to the existing four-direction visual facing.

### Geometry and Contact Boundary

```csharp
public enum CombatShapeType : byte { Circle, Box }
public interface ICombatHurtbox
{
    bool IsAvailable { get; }
    CombatShapeData WorldShape { get; }
}
public interface ICombatTargetQuery
{
    void FillTargetIds(NetworkEntityIdData sourceEntityId,
        in CombatShapeData attackShape,
        System.Collections.Generic.ICollection<NetworkEntityIdData> results);
}
public static class CombatGeometryUtility
{
    public static CombatShapeData ToWorld(in CombatShapeSettings settings,
        UnityEngine.Vector2 origin, UnityEngine.Vector2 forward);
    public static bool Overlaps(in CombatShapeData first, in CombatShapeData second);
}
```

CombatShapeSettings is a serializable passive value with private `_shapeType`,
`Vector2 _offset`, `float _radius`, `Vector2 _size`, `_rotationDegrees`, read-only
accessors and a constructor for these values. CombatShapeData is an immutable
world value with ShapeType, Center, HalfExtents, Radius, RotationDegrees and a
constructor for these fields. Geometry validates finite authored values and
positive used dimensions. Local +X is forward and +Y is left. Forward is normalized;
offset rotates with it, and box angle adds the authored rotation. World Z and
cosmetic sprite scale do not affect overlap. Circle-circle, circle-box and
box-box overlap include touching boundaries.

CombatAttackConfig contains references to one CombatActionProfileConfig
`_actionProfile` and one CombatShapeSettings `_hitbox`. It is passive data,
not a new execution owner. Player action and probe components reference the
same test-attack config asset.

CombatHurtboxComponent derives BaseNetworkEntityComponent, implements
ICombatHurtbox and serializes `_shape`. Its world origin is the entity root,
not the animated Visual transform. Availability requires completed Init,
enabled active object and a valid spawned parent NetworkObject. It does not
require target authority. `ResetLocalRuntime()` clears its initialized flag.

CombatTargetQueryService receives IGameEntityComponentService. It appends
overlapping available entity IDs, excludes the attacker and deduplicates at
entity level, including multiple future hurtboxes. Caller clears its reusable
result list. No second registry or Unity collider/Rigidbody is introduced.

CombatHitProbeComponent is the main-owned integration adapter. It serializes
`CombatAttackConfig _attackConfig`, receives ICombatActionSnapshotAccessor and
ICombatTargetQuery through DI, and exposes:

```csharp
public event System.Action<CombatContactData> ContactDetected;
public void ResetLocalRuntime();
```

CombatContactData is immutable SourceEntityId, TargetEntityId, ExecutionId,
Tick, with a constructor for those fields. These are geometric contacts, not
damage confirmations. A scene/per-actor global event bus is not introduced.

Probe runs only on State Authority in a running server/Solo forward simulation
(`Runner.IsServer && HasStateAuthority && Runner.IsForward`). IsForward is
available in the installed Fusion.Runtime.xml. Client prediction never emits
authoritative contacts. The per-execution managed HashSet is therefore confined
to forward authoritative contact processing, not predicted replay. It is cleared
on a new execution, completion/cancel, local reset and authority invalidation.
An `IsForward == false` replay tick only skips probing; it must not clear the
ledger. This distinction prevents replay from erasing prior contact history and
causing the next forward tick to emit a duplicate.
An authority/session transition cancels/discards contact work rather than migrating
the ledger. Shared-mode authority migration and historical lag compensation are
outside the current supported Host/Solo pipeline.

The source key is `(EntityId, ExecutionId)`; a newly added target emits once.
The probe delegates this per-actor ledger to a main-owned CombatContactProcessor:
`Process(NetworkEntityIdData source, uint executionId, int tick,
IReadOnlyList<NetworkEntityIdData> candidates, ICollection<CombatContactData> contacts)`
and `Reset()`. Process appends only newly accepted geometric contacts and resets
its set when the source/execution key changes. It receives no Unity callbacks,
network state or authority decision. The probe applies the authority guard before
calling it and emits ContactDetected only after processing results. This separates
testable deduplication from actual network-role checks without a test-only probe API.
Probing occurs only in the active phase. Quantizing each phase to at least one
tick prevents a whole active phase from being silently skipped. This first slice
uses stationary attack sampled overlap; exact swept detection for the later
moving dash is a FEAT-03 architecture decision, not a claimed capability here.

## Exact Production Inventory and Parallel Ownership

Each entry below means a `.cs` file with the listed top-level type; every new
asset/script receives its Unity `.meta` through the main agent's Editor workflow.
Existing metadata/GUIDs are preserved.

### Main-Owned Contract Files (Published Before Workers Start)

| Path prefix | New files |
| --- | --- |
| `GC/Features/Combat/Scripts/Enums/` | `CombatActionPhase.cs`, `CombatActionKind.cs`, `CombatShapeType.cs` |
| `GC/Features/Combat/Scripts/Abstract/` | `ICombatActionStateAccessor.cs`, `ICombatActionSnapshotAccessor.cs`, `ICombatActionAccessor.cs`, `ICombatActionMutator.cs`, `ICombatHurtbox.cs`, `ICombatTargetQuery.cs` |
| `GC/Features/Combat/Scripts/DataHolders/Models/` | `CombatActionStateModel.cs` |
| `GC/Features/Combat/Scripts/DataHolders/Data/` | `CombatActionTimingData.cs`, `CombatActionSnapshotData.cs`, `CombatShapeData.cs`, `CombatContactData.cs` |
| `GC/Features/Combat/Scripts/DataHolders/Settings/` | `CombatShapeSettings.cs` |
| `GC/Features/Combat/Scripts/DataHolders/Configs/` | `CombatActionProfileConfig.cs`, `CombatAttackConfig.cs` |

Reuse `GC/Features/Input/Scripts/Abstract/IInputBufferController.cs` and
`GC/Features/Input/Scripts/Managers/Controllers/InputBufferController.cs`
unchanged. No additional input-buffer interface or method is created.
Main also updates existing
`GC/Features/Input/Scripts/DataHolders/Models/InputBufferStateModel.cs` with the
packed byte, preserved TickTimer and validated computed properties before freezing
contracts. Worker A consumes this model and adds packing regression tests in
InputBufferControllerTests.cs; it does not change the frozen model independently.

### Package A — Phases and Retained Input (Independent Implementation Worker)

| Change | Exact files |
| --- | --- |
| Extend existing Input files | `GC/Features/Input/Scripts/DataHolders/Data/LocalButtonStateData.cs`; `Scripts/Managers/Providers/LocalInputProvider.cs` |
| New Combat mechanics | `GC/Features/Combat/Scripts/Other/Processors/CombatActionPhaseProcessor.cs`; `Scripts/Static/Validation/CombatActionProfileValidation.cs` |
| Tests | Extend `GC/Tests/Editor/LocalInputReaderTests.cs`, `InputBufferControllerTests.cs`; add `CombatActionPhaseProcessorTests.cs`, `CombatActionProfileValidationTests.cs` |

A implements no shape/query/probe, TP actor file or Unity asset. Phase tests use
copied model/timing values, not package B internals. Existing timed-input tests
remain regression coverage.

### Package B — Geometry and Target Query (Independent Implementation Worker)

| Change | Exact files |
| --- | --- |
| New geometry | `GC/Features/Combat/Scripts/Static/Utilities/CombatGeometryUtility.cs` |
| New hurtbox | `GC/Features/Combat/Scripts/Components/CombatHurtboxComponent.cs` |
| New query | `GC/Features/Combat/Scripts/Managers/Services/CombatTargetQueryService.cs` |
| Tests | `GC/Tests/Editor/CombatGeometryUtilityTests.cs`, `CombatTargetQueryServiceTests.cs` |

B uses frozen shape/query contracts and existing component services. It does not
reference phase processor, input implementation, actor controllers or Unity assets.
Test doubles make query testing independent of A. No existing damage base is changed.

### Main-Owned Integration Files

| Change | Exact files / responsibility |
| --- | --- |
| New GC probe | `GC/Features/Combat/Scripts/Components/CombatHitProbeComponent.cs`: phase snapshot -> geometry -> per-execution contact ledger/event |
| New GC contact ledger | `GC/Features/Combat/Scripts/Other/Processors/CombatContactProcessor.cs`: per-actor source/execution deduplication, reset and immutable contact output |
| Existing Combat composition | `GC/Features/Combat/Scripts/Init/CombatFeature.cs`: add ICombatTargetQuery -> CombatTargetQueryService, retain both existing damage-service bindings |
| New TP action adapter | `TP/Features/Player/Scripts/Components/PlayerActionComponent.cs`: config compilation, phase orchestration, input consumption, direction capture, cancel/reset; exposes computed ActionSnapshot to its root through the explicit serialized reference; no separate DI accessor implementation |
| New TP attack state | `TP/Features/Player/Scripts/Components/States/PlayerAttackState.cs`: zero-velocity attack movement state |
| New TP phase movement processor | `TP/Features/Player/Scripts/Other/Processors/PlayerActionMovementProcessor.cs`: action-phase velocity gates through existing movement body |
| Existing player input | `TP/Features/Player/Scripts/DataHolders/Data/PlayerInputData.cs`; `Components/PlayerNetworkInputComponent.cs`; `Components/PlayerInputSourceComponent.cs`; `Static/Constants/PlayerInputActionConstants.cs` |
| Existing root and DI | `TP/Features/Player/Scripts/Components/PlayerNetworkEntityComponent.cs`: action model/remembered direction/references/tick order/local despawn cleanup and complete ICombatActionMutator implementation; `Scripts/Init/PlayerNetworkEntityInstaller.cs`: preserve existing InputBufferController binding; all four action interfaces resolve to the same root |
| Existing player movement | `TP/Features/Player/Scripts/Enums/PlayerMovementState.cs`: append Attack=3, preserve existing codes; `Components/StateMachines/PlayerMovementStateMachine.cs`: select phase-owned state before processors run; `Components/States/PlayerIdleState.cs`, `PlayerLocomotionState.cs`: remove legacy DodgeTransitionProcessor from this actor pipeline; `PlayerDodgeState.cs`: relinquish input/timer ownership and use phased movement processor |
| Existing player presentation/debug | `TP/Features/Player/Scripts/Components/PlayerMediatorComponent.cs`: Attack uses current idle placeholder; `PlayerDebugVisualizationComponent.cs`: phase, execution, combo window, pending command and contact geometry/count |
| Existing enemy root | `TP/Features/Enemy/Scripts/Components/EnemyNetworkEntityComponent.cs`: hurtbox reference, Components inclusion and local reset on despawn |

PlayerActionMovementProcessor implements
`IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>`. It receives
the action snapshot accessor, movement-body velocity mutator, expected action
kind, active movement speed and completed movement state. Attack always sets
velocity to zero. Dodge moves only in active and preserves current live-direction
steering; preparation/recovery set zero velocity. This architecture does not
silently add an unrequested fixed-direction Dodge rule.

PlayerMovementStateMachine uses its existing BeforeMovementUpdate hook to select
Attack/Dodge while an action runs, and Idle/Locomotion on the same tick it ends.
Old Dodge Enter/Exit no longer consume/unlock input or start a separate timer;
the action adapter is the sole execution owner. Existing generic DodgeProcessor,
DodgeTransitionProcessor and movement timer contracts remain available to other
consumers, including existing tests. DodgeMovementConfig remains the speed source;
its legacy duration no longer drives this player's new action phases.

The root implements ICombatActionMutator with one `[Networked]`
CombatActionStateModel and a remembered nonzero intent Vector2. No action state
is owned by the processor or animation. PlayerActionComponent receives root
state through ICombatActionMutator, existing IInputBufferController, existing IInputBufferStateMutator for
explicit cancellation, and facing mutation through DI. Its public
`CancelAction()` clears pending input, releases the action lock, clears current
action while preserving execution counter, and zeros movement when the network
object is valid and simulated by this peer. `ResetLocalRuntime()` only clears
local caches/readiness; it never writes invalid network state during despawn.

New serialized references are explicit:

- PlayerActionComponent: `_inputSource` (PlayerInputSourceComponent),
  `_attackConfig` (CombatAttackConfig), `_dodgePhaseProfile`
  (CombatActionProfileConfig), `_movementBody` (TransformMovementBodyComponent).
  Root state/facing, IInputBufferController and IInputBufferStateMutator are
  injected. Only this adapter updates remembered input direction.
- PlayerNetworkEntityComponent: `_playerActionComponent`,
  `_combatHitProbeComponent`, `_combatHurtboxComponent`, plus the existing fields.
- EnemyNetworkEntityComponent: `_combatHurtboxComponent`, plus existing fields.
- PlayerAttackState: `_movementBody`; injected ICombatActionSnapshotAccessor.
- PlayerDodgeState preserves `_movementBody` and `_dodgeMovementConfig`; its new
  snapshot accessor is injected. Its Init validates dependencies/configuration
  without reading live input or starting an action.
- CombatHitProbeComponent and CombatHurtboxComponent fields are defined above;
  no scene lookup or component search is used for gameplay composition.

PlayerNetworkEntityInstaller uses the root's existing BindInterfacesTo registration
to supply ICombatActionStateAccessor, ICombatActionSnapshotAccessor,
ICombatActionAccessor and ICombatActionMutator from the same root instance.
No separate snapshot binding or additional installer action-component field is
needed; root already serializes `_playerActionComponent`. Root.ActionSnapshot
delegates to this component's computed ActionSnapshot. That computation reads
only root.ActionState / LatestInputDirection, compiled profiles and the current
simulation tick, never root.ActionSnapshot; it creates no recursive getter chain
or extra snapshot storage. Do not access the snapshot before actor Init/readiness;
initialization validates references/profiles without evaluating live state.

Movement/probe/phase presentation receive ICombatActionSnapshotAccessor;
state-only readers receive ICombatActionStateAccessor; combined debug readers
receive ICombatActionAccessor; the action execution adapter receives
ICombatActionMutator. No snapshot mutator exists: the snapshot is derived from
the model, profile and tick, not independently writable. Interface inheritance
requires a mutator implementation to satisfy the complete read contract; narrow
DI dependencies express intended access rather than preventing explicit casts.
The existing
InputBufferController registration remains unchanged: one instance through
IInputBufferController. Existing root bindings supply IInputBufferStateMutator.

When chaining: evaluate old execution -> HasBufferedCommand(known intent ID) ->
resolve/validate concrete gameplay profile ->
EndAction/unlock -> BeginAction(commandId) (recheck expiry, consume and lock) ->
start new execution with resolved profile.ActionId only on success -> apply
new facing and movement state in this tick. No extra command is dequeued and no
action-kind priority is introduced. Outside the window, the pending command
remains eligible only until its deadline. It may start after normal completion
only if still unexpired; otherwise the player must press again. The recovery
window controls when execution is allowed, independently of input lifetime.
Only the matching command can succeed because the buffer has one slot; querying
Attack and Dodge does not create a priority. Resolve before EndAction/BeginAction
so a rejected profile leaves the current execution and pending intent unchanged.
If BeginAction fails, do not start or advance gameplay selection; retain the
current execution unless it has already completed normally.

### Unity Assets and GameObjects (Main Agent Only)

| Asset | Exact change |
| --- | --- |
| `TP/Features/Player/Assets/Prefabs/Prefab_Player_NetworkEntity.prefab` | Add PlayerActionComponent, CombatHitProbeComponent and CombatHurtboxComponent on root; add PlayerAttackState on existing Movement child; wire explicit references and root Components array. Preserve Visual/Movement child names and existing network components. |
| `TP/Features/Enemy/Assets/Prefabs/Prefab_Enemy_NetworkEntity.prefab` | Add root CombatHurtboxComponent and serialized root reference. No AI, motor or art changes. |
| `TP/Features/Enemy/Assets/Prefabs/Prefab_Enemy_Dash_NetworkEntity.prefab` | Same hurtbox addition, preserving dash ability and motor config. |
| `TP/Assets/InputActions/InputActions_TechnicalPrototype.inputactions` | Add Gameplay/Attack, proposed Mouse left button and Gamepad buttonWest bindings; preserve Move/Dodge action IDs and existing bindings. |
| `TP/Features/Player/Assets/Configs/Config_TechnicalPrototype_Player_AttackPhases.asset` | New CombatActionProfileConfig for the first test attack. |
| `TP/Features/Player/Assets/Configs/Config_TechnicalPrototype_Player_DodgePhases.asset` | New CombatActionProfileConfig for phased Dodge. |
| `TP/Features/Player/Assets/Configs/Config_TechnicalPrototype_Player_TestAttack.asset` | New CombatAttackConfig referencing attack phases and the authored test box. |

No gameplay scene, ProjectContext, Build Settings, packages, existing animations,
Template UI assets or unrelated imported images are edited in FEAT-01.
Prefab/config/input asset creation and wiring use the connected Editor through
Unity CLI, never raw prefab/asset YAML editing.

### Main-Owned Integration Tests

Extend `TP/Tests/Editor/PlayerPrefabTests.cs`, `EnemyPrefabTests.cs`,
`MovementStateConfigurationTests.cs`; add
`GC/Tests/Editor/CombatContactProcessorTests.cs` for ledger/execution identity.
Actual probe authority/forward behavior is exercised by the Play fixture.
Add `TP/Tests/Play/PlayerCombatPhasePlayModeTests.cs` and
`TP/Tests/Play/ProjectCore.Tests.Play.asmref` referencing the existing Template
Play test assembly. No `.asmdef` is added. Existing input/movement/facing tests
remain relevant regression coverage.

## Proposed Working Values for This Architecture

These prototype values were accepted by the developer under APR-003; they are not
final balance requirements. Convert seconds to ticks at Init as described above.

| Parameter | Test attack | Dodge |
| --- | --- | --- |
| Input CommandId | 2 (Attack intent for every attack variant) | 1 (existing Dodge input ID) |
| Gameplay ActionId | 2 (test profile only; independent of input ID) | 1 (Dodge profile; independent of input ID) |
| Preparation | 0.20 s | 0.03 s |
| Active | 0.10 s | 0.20 s |
| Recovery | 0.30 s | 0.10 s |
| Recovery combo window | [0.15 s, 0.25 s) relative to recovery start | [0.05 s, 0.10 s) relative to recovery start |
| Pending input lifetime | 0.15 s from the latest accepted press; configurable on PlayerInputSourceComponent | Same 0.15 s setting; never paused by action phases |
| Movement | Zero throughout | Existing DodgeMovementConfig.Speed (8 by current default), live-direction movement only in active |
| Hit shape | Box 0.80 x 0.80 world units; local offset (0.65,0); rotation 45 degrees relative to action direction | None; only derived active-invulnerability eligibility, no damage filtering yet |

Initial player/enemy hurtbox: separate circle radius 0.25, root-local world-XY
offset (0,0.20). It does not change the existing motor collision radius 0.20.
Live inspection confirmed both actor roots have scale (1,1,1), Visual offset
(0,0.40) and Visual scale (0.15,0.15,0.15). Cosmetic scale is not used as a
gameplay shape multiplier.

Existing Dodge animations remain temporary phase-independent presentation in
this first deliverable. Correct attack/death/trail frames and animation-phase
alignment are mandatory FEAT-04 work, not claimed by this phase implementation.

## Initialization, Tick Order, Authority and Cleanup

| Trigger / owner | Guaranteed readiness | Safe action | Invalidation / cleanup |
| --- | --- | --- | --- |
| ApplicationEntryPoint -> ApplicationInitializationFlow -> gameplay IGameSceneLifecycle -> FeatureInitializationFlow | Scene bindings installed before awaited initialization completes; InputFeature initializes LocalInputProvider from the configured asset | Register query binding through CombatFeature and use existing player/enemy factories | Scene disposal releases injected scene services and local input subscriptions |
| Actor factory / GameObjectContext injection | Root, body, action adapter and serialized configs injected before entity component Init | Compile timing/validate profiles in PlayerActionComponent.Init; initialize local query/ledger and hurtbox readiness in their Init | Fresh spawn resets local caches; State Authority resets new replicated action state |
| Player root Spawned / Components array | Base root registers, builds explicit component list, then initializes each | New order: input -> hurtbox -> action adapter -> mediator -> movement state machine -> body -> contact probe -> camera -> targetable | Hurtbox is unavailable until its own Init; no target query during partial initialization |
| Root FixedUpdateNetwork on all peers | Base root ticks its components even on proxies; all components initialized | PlayerInputSourceComponent and PlayerActionComponent explicitly require HasStateAuthority or HasInputAuthority before replicated buffer/direction/action/lock mutations; proxy evaluation/render is read-only. On simulated peers: input -> phases/chain -> movement -> body -> probe | Do not mistake the base callback for an authority guarantee; no animation callback controls gameplay |
| Contact probe, authoritative forward only | Host/Solo server has active action snapshot and valid root/query service | Query current XY poses; emit a new contact only after successful ledger insertion | No contacts on clients, replay, idle/preparation/recovery; clear ledger on execution change/cancel |
| Render/debug on any peer | Replicated action state available; profiles already compiled | Derive phase snapshot for debug/presentation from current state; never advance authoritative gameplay from Render | No shared static phase/contact caches; presentation may skip intermediate states without changing simulation |
| Explicit action cancellation while spawned | Valid network object; State/Input Authority simulation | Cancel execution, unlock and clear retained command; zero velocity | Subsequent normal tick restores ordinary locomotion and facing |
| Player/Enemy root Despawned | Network state may already be invalid | Main-owned derived root calls local-only reset on action/probe/hurtbox and then base unregister | Never call network-state setters after despawn; root unregister removes query discoverability; no extra hurtbox registry |
| Re-spawn / pooled object reuse | New root entity identity and component Init | Clear transient contact buffers, readiness, action-derived caches and stale velocity; authority reinitializes action/input state | Old handlers/session references must not survive; preserve monotonic registry identity rules |

Authority contact events are a Host/Solo capability, consistent with existing
spawn factories. The geometry result is sampled current simulation state, not
lag compensation or swept high-speed detection. FEAT-03 must extend movement-hit
coverage rather than treating this stationary test as proof for dash attacks.

## Parallel Development and Safe Integration

Human authorization DEC-009 explicitly permits implementation workers for the
approved scoped packages. The read-only inspection reference continues to apply
to reviewers; it does not revoke this direct human instruction. Architecture
inspection agents were read-only and made no project mutations.

1. After approval, main creates frozen contracts/config types and verifies their
   import/compilation before dispatch. Record contract revision and source hashes.
2. Main prepares two isolated managed workspaces from the same verified source
   baseline and copies the exact frozen contracts/current relevant source snapshot.
   Worker A and B receive explicit allowlists, exclusions and acceptance checks.
   Workers do not perform Git operations, Unity mutations or development-state edits.
3. A develops its input/phase implementation and focused tests. B develops geometry,
   hurtbox/query implementation and focused tests independently. Neither may modify
   shared contracts, the other package, root/DI integration or prefab/scene assets.
4. Workers return complete diffs/files, changed-path manifests, self-review findings
   and verification limits. A contract change returns to main before either package
   silently adapts its assumptions. No worker can approve its own work or close a FEAT.
5. Main checks allowlists/base hashes, reviews both outputs, applies per-file patches
   into the primary checkout without including unrelated work, and preserves existing
   `.meta` files. New `.meta` files are created by main's Editor import workflow.
6. Main executes package tests, then implements the explicitly main-owned actor/DI/
   asset integration and runs combined regressions. Workers never race a live Editor
   or concurrently modify Unity serialized assets. Main owns integration order and
   actual validation; no independent worker commit substitutes for accepted checkpoint.
7. Final combined diff passes audit, developer review and required Play Mode before
   one FEAT-01 checkpoint. Retain worker workspaces through review/recovery and archive
   them only when their work has been safely integrated and is no longer needed.

These are internal implementation packages of FEAT-01, not new FEATs or a plan
reordering. The main actor adapter intentionally joins both packages; separating
it would create shared-root/DI writes rather than useful independence.

Apply the same analysis to later architectures: damage/health versus reactions
(FEAT-02), player profiles versus enemy intent (FEAT-03), generated resources versus
HP UI (FEAT-04), match-domain state versus popup presentation (FEAT-05), only after
each owning architecture freezes its exact seams and allowed paths.

## Validation and Review

- A: phase boundary/window inclusivity, rounded short phases, tick wrap, replay from
  copied state, execution IDs, slot expiry at the configured deadline even while
  locked or missing input, consumption just before versus at/after expiry,
  replacement restarting the deadline, held input not refreshing it,
  expired input never executing at a later window/completion, last-event replacement,
  callback order without priority, timed-buffer compatibility and cancellation.
  Include unknown input IDs, duplicate/mismatched profiles, retained directional
  Dodge after input becomes zero, and a rejected collapsed quantized window.
  Cover all IDs 0..127 with both lock values, rejection of 128 and ushort.MaxValue,
  command replacement/expiry preserving the lock, lock changes preserving ID/timer,
  fresh reset, and existing descriptor/ushort API compatibility. Invalid commands
  must not overwrite a valid pending command.
  Validate gameplay profile IDs separately: an ActionId above 127 is valid and
  must not be rejected by input packing constraints.
- B: circles, rotated boxes, edge/corner touching, diamond/narrow shapes, normalized
  diagonal direction and offsets, Z independence, unavailable/self targets, multiple
  hurtboxes yielding one entity, unregister exclusion.
- Main: changed config/prefab references and composition, no movement/facing leak,
  all four action DI contracts resolve to the same root, snapshot delegation is
  nonrecursive and consistent with state/tick, and the adapter has no separate
  snapshot binding or independently mutable phase cache;
  input CommandId.Attack resolves to a profile with a different ActionId (including
  one above 127), consumption still uses CommandId and replicated execution uses
  ActionId. Selection failure/expired input must not start an action or advance
  gameplay selection state. Later concrete attack variants require no new input IDs;
  newest direction on chained attack, no early transition outside authored window,
  active-only contacts, each target once per execution including leave/re-enter,
  same target on next execution, multiple targets, client non-authority behavior,
  explicit cancellation, despawn/reuse and stale velocity cleanup.
  A skipped replay tick preserves the contact ledger; a real authority/session
  invalidation clears it. Proxy component callbacks never mutate replicated state.
  Verify the packed model through actual Fusion state snapshots/reconciliation,
  not only descriptor reads; inspect the installed weaver's layout/word count.
- Run relevant existing input/movement/facing/prefab tests through
  `Tools/AI/validate-unity.ps1`; run new Play tests with the connected Editor.
  Developer Play Mode covers Solo and executing Host/client roles, transitions and
  spawn/despawn/reuse. Any unavailable peer scenario is recorded as untested.
- Run strict architecture validation, runtime dotnet build and git diff --check.
  Existing four ASSET003 errors in unrelated imported Enemy PNGs remain a baseline
  blocker to a clean strict gate; no permission to rename those assets is inferred.
- Architecture approval is recorded above; no test success is claimed. Exact APIs
  used here were checked against existing source; Runner.IsForward was confirmed in
  the installed Fusion XML. Future edits must preserve the agreed lifecycle map.

Read-only independent inspections verified the package boundary and identified
the explicit proxy guards, retained-Dodge eligibility timing, command/profile
validation and replay-ledger distinction above. Their fixes are included in this
proposal; no implementation or test success is implied by architecture inspection.

## Common Contracts Implementation Slice

The main-owned contracts and packed input model are implemented, compiled and
hashed in `FEAT-01-contracts.json` (contract revision 1, architecture revision 7).
See `FEAT-01-contracts-implementation.md` for the exact inventory, scoped audit,
source refresh and executed checks: Editor compilation passed; 7 input-buffer
tests passed; runtime build passed with 10 dependency warnings; strict architecture
retains only the 4 existing unrelated ASSET003 errors. Live Fusion layout is
8 bytes / 2 words for the buffer, 20 bytes / 5 words for the action model.
Gameplay/network reconciliation and combined FEAT-01 acceptance remain pending.
Package A must preserve the preliminary packing tests added by main.

## Combined Implementation and Review Handoff

A/B and main integration are implemented. BUG-001 exact correction was approved
by APR-005, applied and retested. Source review revision 1 is captured in
`FEAT-01-review-manifest.json` and `FEAT-01-review.patch`; see
`FEAT-01-integration-result.md` and `FEAT-01-audit.md` for actual inventory/evidence.
Original input fixture: 10/10; full Edit Mode: 653/653; automated Solo Play: 2/2.
Audit: no P0/P1, AUD-001 is P2 (per-query temporary allocations, unprofiled).
Final diff review, required manual/network Play Mode and clean strict gate remain
pending. The same host review covers corrected BUG-001; no separate checkpoint.

## Approval Record

APR-003 approves architecture revision 7, including independent CommandId/ActionId,
executor-owned action selection, the accepted action-interface
hierarchy and single-root DI ownership, existing-buffer reuse,
unchanged input-buffer API, finite input lifetime, packed network buffer state,
frozen contracts, exact A/B/main
ownership, actor/config/prefab changes, working values and validation. Approval permits
implementation of these packages; the developer still reviews the combined diff and
required Play Mode results before acceptance.

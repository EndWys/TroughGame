using System;
using System.Collections.Generic;
using Shared;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerNetworkEntityComponent :
        BaseNetworkEntityRoot,
        IColleague,
        IMovementStateDataMutator<PlayerMovementState>,
        IMovementStateTimerMutator,
        IInputBufferStateMutator,
        IPlayerFacingDirectionMutator,
        ICombatActionMutator,
        ICombatAttackResolver,
        ICombatInvulnerabilityMutator
    {
        #region Components

        [SerializeField] private PlayerInputSourceComponent _inputSourceComponent;
        [SerializeField] private PlayerCombatActionComponent _playerActionComponent;
        [SerializeField] private CombatHurtboxComponent _combatHurtboxComponent;
        [SerializeField] private CombatHitProbeComponent _combatHitProbeComponent;
        [SerializeField] private CombatDebugVisualizationComponent _combatDebugVisualizationComponent;
        [SerializeField] private PlayerMediatorComponent _playerMediatorComponent;
        [SerializeField] private PlayerActionControllerComponent _actionControllerComponent;
        [SerializeField] private PlayerMovementStateMachine _movementStateMachine;
        [SerializeField] private TransformMovementBodyComponent _movementBodyComponent;
        [SerializeField] private CameraTargetComponent _cameraTargetComponent;
        [SerializeField] private TargetableNetworkEntityComponent _targetableNetworkEntityComponent;

        private Transform _entitiesContainer;

        [Inject]
        private void Construct(
            [Inject(Id = PlayerNetworkEntityConstants.EntitiesContainer)] Transform entitiesContainer)
        {
            _entitiesContainer = entitiesContainer != null
                ? entitiesContainer
                : throw new ArgumentNullException(nameof(entitiesContainer));
        }

        #endregion

        #region Networked State

        [Networked, OnChangedRender(nameof(OnCurrentStateChanged))]
        public PlayerMovementState CurrentState { get; private set; }
        [Networked] public PlayerMovementState PreviousState { get; private set; }
        [Networked, OnChangedRender(nameof(OnFacingDirectionChanged))]
        public PlayerFacingDirection FacingDirection { get; private set; }
        [Networked] public TickTimer MovementStateTimer { get; private set; }
        [Networked] public TickTimer InvulnerabilityTimer { get; private set; }
        [Networked] private InputBufferStateModel InputBufferStateValue { get; set; }
        [Networked] private CombatActionStateModel ActionStateValue { get; set; }
        [Networked] private Vector2 LatestInputDirectionValue { get; set; }

        public CombatActionStateModel ActionState => ActionStateValue;
        public Vector2 LatestInputDirection => LatestInputDirectionValue;
        public CombatActionSnapshotData ActionSnapshot => _playerActionComponent.ActionSnapshot;

        public void SetActionState(in CombatActionStateModel state) => ActionStateValue = state;
        public void SetLatestInputDirection(Vector2 direction) => LatestInputDirectionValue = direction;

        #endregion

        #region Facing Direction

        public void SetFacingDirection(PlayerFacingDirection facingDirection)
        {
            if (FacingDirection == facingDirection)
            {
                return;
            }

            FacingDirection = facingDirection;
        }

        #endregion

        #region Movement State Timing

        public int? StateTimerEndTick => MovementStateTimer.IsRunning ? MovementStateTimer.TargetTick : null;

        public bool IsStateTimerFinished =>
            MovementStateTimer.IsRunning && MovementStateTimer.ExpiredOrNotRunning(Runner);

        public void StartStateTimerTicks(int durationTicks)
        {
            if (durationTicks <= 0) throw new ArgumentOutOfRangeException(nameof(durationTicks));
            MovementStateTimer = TickTimer.CreateFromTicks(Runner, durationTicks);
        }

        public bool IsInvulnerable => Object != null && Object.IsValid && Runner != null &&
            InvulnerabilityTimer.IsRunning &&
            !InvulnerabilityTimer.ExpiredOrNotRunning(Runner);

        public void SetInvulnerabilityUntilTick(int? endTick)
        {
            if (!HasStateAuthority && !HasInputAuthority) return;
            int remaining = endTick.HasValue ? unchecked(endTick.Value - Runner.Tick.Raw) : 0;
            InvulnerabilityTimer = remaining > 0
                ? TickTimer.CreateFromTicks(Runner, remaining) : TickTimer.None;
        }

        public void StopStateTimer()
        {
            MovementStateTimer = TickTimer.None;
        }

        #endregion

        #region Input Buffer State

        public InputBufferCommandDescriptor BufferedCommand => new(InputBufferStateValue.BufferedCommandId);

        public TickTimer BufferedCommandTimer => InputBufferStateValue.BufferedCommandTimer;

        public bool IsLocked => InputBufferStateValue.IsLocked;

        public void SetBufferedCommand(InputBufferCommandDescriptor command, TickTimer expirationTimer)
        {
            if (command.CommandId == 0)
            {
                throw new ArgumentException("Input command id must be non-zero.", nameof(command));
            }

            InputBufferStateModel state = InputBufferStateValue;
            state.BufferedCommandId = command.CommandId;
            state.BufferedCommandTimer = expirationTimer;
            InputBufferStateValue = state;
        }

        public void ClearBufferedCommand()
        {
            InputBufferStateModel state = InputBufferStateValue;
            state.BufferedCommandId = 0;
            state.BufferedCommandTimer = TickTimer.None;
            InputBufferStateValue = state;
        }

        public void LockInput()
        {
            InputBufferStateModel state = InputBufferStateValue;
            state.IsLocked = true;
            InputBufferStateValue = state;
        }

        public void UnlockInput()
        {
            InputBufferStateModel state = InputBufferStateValue;
            state.IsLocked = false;
            InputBufferStateValue = state;
        }

        #endregion

        #region Movement State Data

        public PlayerMovementState CurrentMovementStates => CurrentState;
        public PlayerMovementState PreviousMovementStates => PreviousState;

        #endregion

        #region Entity Lifecycle

        protected override void BeforeComponentsInitialized()
        {
            if (HasStateAuthority)
            {
                ActionStateValue = default;
                LatestInputDirectionValue = default;
                InputBufferStateValue = default;
                MovementStateTimer = TickTimer.None;
                InvulnerabilityTimer = TickTimer.None;
            }
            if (HasStateAuthority || HasInputAuthority)
            {
                Runner.SetIsSimulated(Object, true);
            }
        }

        protected override void AfterComponentsInitialized()
        {
            transform.SetParent(_entitiesContainer, true);
            NotifyMovementStateChanged();
            NotifyFacingDirectionChanged();
        }

        protected override IEnumerable<INetworkEntityComponent> CreateComponents()
        {
            return new INetworkEntityComponent[]
            {
                _inputSourceComponent,
                _combatHurtboxComponent,
                _playerActionComponent,
                _playerMediatorComponent,
                _actionControllerComponent,
                _movementStateMachine,
                _movementBodyComponent,
                _combatHitProbeComponent,
                _combatDebugVisualizationComponent,
                _cameraTargetComponent,
                _targetableNetworkEntityComponent,
            };
        }

        #endregion

        #region Movement State Mutation


        public bool TryGetAttack(ushort actionId, out CombatAttackConfig attack)
        {
            return _playerActionComponent.TryGetAttack(actionId, out attack);
        }

        public void ChangeMovementState(PlayerMovementState newState)
        {
            ChangeState(newState);
        }

        public void ChangeState(PlayerMovementState newState)
        {
            PreviousState = CurrentState;
            CurrentState = newState;
        }

        #endregion

        #region Movement State Presentation

        private void OnCurrentStateChanged()
        {
            NotifyMovementStateChanged();
        }

        private void OnFacingDirectionChanged()
        {
            NotifyFacingDirectionChanged();
        }

        private void NotifyMovementStateChanged()
        {
            _playerMediatorComponent.Notify(
                new PlayerMovementStateChangedPayload(this, CurrentState, PreviousState));
        }

        private void NotifyFacingDirectionChanged()
        {
            _playerMediatorComponent.Notify(new PlayerFacingDirectionChangedPayload(this, FacingDirection));
        }

        #endregion
    }
}

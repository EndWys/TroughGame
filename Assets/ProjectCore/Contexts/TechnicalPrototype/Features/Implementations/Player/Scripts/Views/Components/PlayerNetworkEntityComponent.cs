using System;
using System.Collections.Generic;
using Domain;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerNetworkEntityComponent :
        BaseNetworkEntityRoot,
        IColleague,
        IMovementStateDataMutator<PlayerMovementState>,
        IMovementStateTimerMutator,
        IInputBufferStateMutator
    {
        #region Components

        [SerializeField] private PlayerInputSourceComponent _inputSourceComponent;
        [SerializeField] private PlayerMediatorComponent _playerMediatorComponent;
        [SerializeField] private PlayerMovementStateMachine _movementStateMachine;
        [SerializeField] private TransformMovementBodyComponent _movementBodyComponent;
        [SerializeField] private CameraTargetComponent _cameraTargetComponent;

        #endregion

        #region Networked State

        [Networked, OnChangedRender(nameof(OnCurrentStateChanged))]
        public PlayerMovementState CurrentState { get; private set; }
        [Networked] public PlayerMovementState PreviousState { get; private set; }
        [Networked] public TickTimer MovementStateTimer { get; private set; }
        [Networked] private InputBufferStateModel InputBufferStateValue { get; set; }

        #endregion

        #region Movement State Timing

        public bool IsStateTimerFinished =>
            MovementStateTimer.IsRunning && MovementStateTimer.ExpiredOrNotRunning(Runner);

        public void StartStateTimer(float durationSeconds)
        {
            if (durationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(durationSeconds), "State timer duration must be positive.");
            }

            MovementStateTimer = TickTimer.CreateFromSeconds(Runner, durationSeconds);
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
            if (HasStateAuthority || HasInputAuthority)
            {
                Runner.SetIsSimulated(Object, true);
            }
        }

        protected override void AfterComponentsInitialized()
        {
            NotifyMovementStateChanged();
        }

        protected override IEnumerable<INetworkEntityComponent> CreateComponents()
        {
            return new INetworkEntityComponent[]
            {
                _inputSourceComponent,
                _playerMediatorComponent,
                _movementStateMachine,
                _movementBodyComponent,
                _cameraTargetComponent,
            };
        }

        #endregion

        #region Movement State Mutation

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

        private void NotifyMovementStateChanged()
        {
            _playerMediatorComponent.Notify(
                new PlayerMovementStateChangedPayload(this, CurrentState, PreviousState));
        }

        #endregion
    }
}

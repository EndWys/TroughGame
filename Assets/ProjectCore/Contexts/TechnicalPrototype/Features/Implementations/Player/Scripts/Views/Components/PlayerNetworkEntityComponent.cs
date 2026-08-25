using System;
using System.Collections.Generic;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerNetworkEntityComponent :
        BaseNetworkEntityRoot,
        IMovementStateDataMutator<PlayerMovementState>,
        IMovementStateTimerMutator,
        IInputBufferStateMutator
    {
        [SerializeField]
        private PlayerInputSourceComponent _inputSourceComponent;
        [SerializeField]
        private PlayerMovementStateMachine _movementStateMachine;
        [SerializeField]
        private TransformMovementBodyComponent _movementBodyComponent;
        [SerializeField]
        private CameraTargetComponent _cameraTargetComponent;

        [Networked] public PlayerMovementState CurrentState { get; private set; }
        [Networked] public PlayerMovementState PreviousState { get; private set; }
        [Networked] public TickTimer MovementStateTimer { get; private set; }
        [Networked] private InputBufferStateData InputBufferStateValue { get; set; }

        public bool IsStateTimerFinished => MovementStateTimer.IsRunning && MovementStateTimer.ExpiredOrNotRunning(Runner);

        public void StartStateTimer(float durationSeconds)
        {
            if (durationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(durationSeconds),
                    "State timer duration must be positive.");
            }

            MovementStateTimer = TickTimer.CreateFromSeconds(
                Runner,
                durationSeconds);
        }

        public void StopStateTimer()
        {
            MovementStateTimer = TickTimer.None;
        }

        InputBufferCommandData IInputBufferStateAccessor.BufferedCommand =>
            InputBufferStateValue.BufferedCommand;

        TickTimer IInputBufferStateAccessor.BufferedCommandTimer =>
            InputBufferStateValue.BufferedCommandTimer;

        bool IInputBufferStateAccessor.IsLocked =>
            InputBufferStateValue.IsLocked;

        void IInputBufferStateMutator.SetBufferedCommand(
            InputBufferCommandData command,
            TickTimer expirationTimer)
        {
            if (command.CommandId == 0)
            {
                throw new ArgumentException(
                    "Input command id must be non-zero.",
                    nameof(command));
            }

            InputBufferStateData state = InputBufferStateValue;
            state.BufferedCommand = command;
            state.BufferedCommandTimer = expirationTimer;
            InputBufferStateValue = state;
        }

        void IInputBufferStateMutator.ClearBufferedCommand()
        {
            InputBufferStateData state = InputBufferStateValue;
            state.BufferedCommand = default;
            state.BufferedCommandTimer = TickTimer.None;
            InputBufferStateValue = state;
        }

        void IInputBufferStateMutator.LockInput()
        {
            InputBufferStateData state = InputBufferStateValue;
            state.IsLocked = true;
            InputBufferStateValue = state;
        }

        void IInputBufferStateMutator.UnlockInput()
        {
            InputBufferStateData state = InputBufferStateValue;
            state.IsLocked = false;
            InputBufferStateValue = state;
        }

        public PlayerMovementState CurrentMovementStates => CurrentState;
        public PlayerMovementState PreviousMovementStates => PreviousState;

        protected override void BeforeComponentsInitialized()
        {
            if (HasStateAuthority || HasInputAuthority)
            {
                Runner.SetIsSimulated(Object, true);
            }
        }

        protected override IEnumerable<INetworkEntityComponent> CreateComponents()
        {
            return new INetworkEntityComponent[]
            {
                _inputSourceComponent,
                _movementStateMachine,
                _movementBodyComponent,
                _cameraTargetComponent,
            };
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
    }
}

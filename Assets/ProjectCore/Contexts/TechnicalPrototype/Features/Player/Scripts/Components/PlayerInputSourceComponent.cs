using System;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerInputSourceComponent : BaseEntityInputSourceComponent<PlayerInputSnapshotData>,
        IPlayerInputAccessor
    {
        [SerializeField, Min(0.001f)] private float _actionBufferLifetimeSeconds = 0.15f;

        private IInputBufferController _inputBufferController;
        private IPlayerInputDirectionMutator _direction;
        private IInputBufferStateAccessor _bufferState;
        private Vector2 _currentDirection;
        private bool _hasInput;

        [Inject]
        private void Construct(
            IInputBufferController inputBufferController,
            IPlayerInputDirectionMutator direction,
            IInputBufferStateAccessor bufferState)
        {
            _inputBufferController = inputBufferController ?? throw new ArgumentNullException(
                nameof(
                    inputBufferController));

            _direction = direction ?? throw new ArgumentNullException(nameof(direction));
            _bufferState = bufferState ?? throw new ArgumentNullException(nameof(bufferState));
        }

        public PlayerInputSnapshotData Input =>
            new(
                _hasInput ? _currentDirection : Vector2.zero,
                _direction.LatestInputDirection);

        public override bool TryGetInput(out PlayerInputSnapshotData input)
        {
            input = Input;
            return _hasInput;
        }

        protected override void OnInitialize()
        {
            if (float.IsNaN(_actionBufferLifetimeSeconds)
                || float.IsInfinity(_actionBufferLifetimeSeconds)
                || _actionBufferLifetimeSeconds <= 0f)
                throw new InvalidOperationException("Action buffer lifetime must be finite and positive.");

            _currentDirection = default;
            _hasInput = false;
        }

        protected override void OnResetLocalRuntime()
        {
            _currentDirection = default;
            _hasInput = false;
        }

        protected override void OnNetworkTick()
        {
            if (!HasSimulationAuthority)
                return;

            _inputBufferController.HasBufferedCommand(_bufferState.BufferedCommand.CommandId);

            if (!ParentNetworkBehaviour.GetInput(out PlayerInputData input))
            {
                _currentDirection = default;
                _hasInput = false;
                return;
            }

            if (input.MoveDirection.sqrMagnitude > 0f)
                _direction.SetLatestInputDirection(input.MoveDirection.normalized);

            ushort commandId = input.LatestActionCommandId;

            if (commandId == PlayerInputActionConstants.AttackCommandId
                || commandId == MovementInputCommandConstants.DodgeCommandId
                && input.MoveDirection.sqrMagnitude > 0f)
            {
                _inputBufferController.BufferCommand(
                    new(commandId),
                    _actionBufferLifetimeSeconds);
            }

            _currentDirection = Vector2.ClampMagnitude(input.MoveDirection, 1f);
            _hasInput = true;
        }
    }
}

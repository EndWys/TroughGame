using System;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerInputSourceComponent : BaseEntityInputSourceComponent<PlayerInputFrameData>
    {
        [SerializeField, Min(0.001f)] private float _actionBufferLifetimeSeconds = 0.15f;
        private IInputBufferController _inputBufferController;
        private PlayerInputFrameData _currentInput;
        private bool _hasInput;

        [Inject]
        private void Construct(IInputBufferController inputBufferController)
        {
            _inputBufferController = inputBufferController;
        }

        public override bool TryGetInput(out PlayerInputFrameData input)
        {
            input = _currentInput;
            return _hasInput;
        }

        protected override void OnInitialize()
        {
            if (float.IsNaN(_actionBufferLifetimeSeconds) || float.IsInfinity(_actionBufferLifetimeSeconds) ||
                _actionBufferLifetimeSeconds <= 0f)
                throw new InvalidOperationException("Action buffer lifetime must be finite and positive.");
            _currentInput = default;
            _hasInput = false;
        }

        protected override void OnResetLocalRuntime()
        {
            _currentInput = default;
            _hasInput = false;
        }

        protected override void OnNetworkTick()
        {
            if (!ParentNetworkBehaviour.HasStateAuthority && !ParentNetworkBehaviour.HasInputAuthority) return;
            if (!ParentNetworkBehaviour.GetInput(out PlayerInputData input))
            {
                _currentInput = default;
                _hasInput = false;
                return;
            }

            ushort commandId = input.LatestActionCommandId;
            if (commandId == PlayerInputActionConstants.AttackCommandId ||
                commandId == MovementInputCommandConstants.DodgeCommandId && input.MoveDirection.sqrMagnitude > 0f)
            {
                _inputBufferController.BufferCommand(
                    new(commandId), _actionBufferLifetimeSeconds);
            }

            _currentInput = new(Vector2.ClampMagnitude(input.MoveDirection, 1f));
            _hasInput = true;
        }
    }
}

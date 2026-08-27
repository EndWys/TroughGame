using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerInputSourceComponent : BaseEntityInputSourceComponent<PlayerInputFrameData>
    {
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

        public override void Init()
        {
            _currentInput = default;
            _hasInput = false;
        }

        public override void NetworkTick()
        {
            if (!ParentNetworkBehaviour.GetInput(out PlayerInputData input))
            {
                _currentInput = default;
                _hasInput = false;
                return;
            }

            if (input is { DodgePressed: true, MoveDirection: { sqrMagnitude: > 0f } })
            {
                _inputBufferController.BufferCommand(
                    new(MovementInputCommandConstants.DodgeCommandId),
                    MovementInputCommandConstants.DodgeInputBufferLifetimeSeconds);
            }

            _currentInput = new(Vector2.ClampMagnitude(input.MoveDirection, 1f));
            _hasInput = true;
        }
    }
}

using System;
using Domain;

namespace ProjectCore.GameCore
{
    public sealed class DodgeTransitionProcessor<TStateType, TStatePayload> :
        IMovementStateProcessor<TStateType, TStatePayload>
        where TStateType : struct, Enum
        where TStatePayload : struct
    {
        private readonly IInputBufferController _inputBufferController;
        private readonly ushort _dodgeCommandId;
        private readonly TStateType _dodgeMovementState;

        public DodgeTransitionProcessor(
            IInputBufferController inputBufferController,
            ushort dodgeCommandId,
            TStateType dodgeMovementState)
        {
            _inputBufferController = inputBufferController ??
                throw new ArgumentNullException(nameof(inputBufferController));
            _dodgeCommandId = dodgeCommandId;
            _dodgeMovementState = dodgeMovementState;
        }

        public bool Execute(
            TStatePayload payload,
            out TStateType resultState)
        {
            if (!_inputBufferController.HasBufferedCommand(_dodgeCommandId))
            {
                resultState = default;
                return false;
            }

            resultState = _dodgeMovementState;
            return true;
        }
    }
}

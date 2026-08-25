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
        private readonly ushort _commandId;
        private readonly TStateType _dodgeState;

        public DodgeTransitionProcessor(
            IInputBufferController inputBufferController,
            ushort commandId,
            TStateType dodgeState)
        {
            _inputBufferController = inputBufferController ??
                throw new ArgumentNullException(nameof(inputBufferController));
            _commandId = commandId;
            _dodgeState = dodgeState;
        }

        public bool Execute(
            TStatePayload payload,
            out TStateType resultState)
        {
            if (!_inputBufferController.HasBufferedCommand(_commandId))
            {
                resultState = default;
                return false;
            }

            resultState = _dodgeState;
            return true;
        }
    }
}

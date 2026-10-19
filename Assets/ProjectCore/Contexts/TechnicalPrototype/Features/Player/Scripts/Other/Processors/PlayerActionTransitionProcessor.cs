using System;
using ProjectCore.GameCore;
using Shared;
namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerActionTransitionProcessor : IStateProcessor<PlayerEntityState, PlayerEntityPayload>
    {
        private readonly IInputBufferController _buffer;
        private readonly IEntityStateRestart _restart;
        private readonly ushort _commandId;
        private readonly PlayerEntityState _target;
        private readonly bool _reenter;
        private readonly Func<bool> _canTransition;

        public PlayerActionTransitionProcessor(IInputBufferController buffer, IEntityStateRestart restart,
            ushort commandId, PlayerEntityState target, bool reenter, Func<bool> canTransition)
        {
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _restart = restart ?? throw new ArgumentNullException(nameof(restart));
            _commandId = commandId;
            _target = target;
            _reenter = reenter;
            _canTransition = canTransition ?? throw new ArgumentNullException(nameof(canTransition));
        }

        public bool Execute(PlayerEntityPayload payload, out PlayerEntityState resultState)
        {
            resultState = default;
            if (!_canTransition() || !_buffer.HasBufferedCommand(_commandId)) return false;
            _buffer.ReleaseCommandLock();
            if (!_buffer.TryConsumeBufferedCommandAndLock(_commandId)) return false;
            if (_reenter) _restart.RequestRestart();
            resultState = _target;
            return true;
        }
    }
}

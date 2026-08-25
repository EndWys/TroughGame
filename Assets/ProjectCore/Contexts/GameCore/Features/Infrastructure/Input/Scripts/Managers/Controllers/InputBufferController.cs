using System;
using Fusion;

namespace ProjectCore.GameCore
{
    public sealed class InputBufferController : IInputBufferController
    {
        private readonly IInputBufferStateAccessor _stateAccessor;
        private readonly IInputBufferStateMutator _stateMutator;
        private readonly INetworkBehaviourAccessor _networkBehaviourAccessor;

        public InputBufferController(
            IInputBufferStateAccessor stateAccessor,
            IInputBufferStateMutator stateMutator,
            INetworkBehaviourAccessor networkBehaviourAccessor)
        {
            _stateAccessor = stateAccessor ??
                throw new ArgumentNullException(nameof(stateAccessor));
            _stateMutator = stateMutator ??
                throw new ArgumentNullException(nameof(stateMutator));
            _networkBehaviourAccessor = networkBehaviourAccessor ??
                throw new ArgumentNullException(nameof(networkBehaviourAccessor));
        }

        private NetworkRunner Runner =>
            _networkBehaviourAccessor.ParentNetworkBehaviour?.Runner;

        public bool IsLocked =>
            _stateAccessor.IsLocked;

        public bool HasBufferedCommand(ushort commandId)
        {
            if (commandId == 0)
            {
                return false;
            }

            if (_stateAccessor.BufferedCommand.CommandId != commandId)
            {
                return false;
            }

            if (!_stateAccessor.BufferedCommandTimer.IsRunning)
            {
                return Runner == null;
            }

            if (Runner != null &&
                !_stateAccessor.BufferedCommandTimer.ExpiredOrNotRunning(Runner))
            {
                return true;
            }

            _stateMutator.ClearBufferedCommand();
            return false;
        }

        public void BufferCommand(
            InputBufferCommandData command,
            float lifetimeSeconds)
        {
            if (command.CommandId == 0 || lifetimeSeconds <= 0f || Runner == null)
            {
                return;
            }

            TickTimer expirationTimer = TickTimer.CreateFromSeconds(
                Runner,
                lifetimeSeconds);
            _stateMutator.SetBufferedCommand(command, expirationTimer);
        }

        public bool BeginAction(ushort commandId)
        {
            if (commandId == 0 ||
                IsLocked ||
                !HasBufferedCommand(commandId))
            {
                return false;
            }

            _stateMutator.ClearBufferedCommand();
            _stateMutator.LockInput();
            return true;
        }

        public void EndAction()
        {
            _stateMutator.UnlockInput();
        }

    }
}

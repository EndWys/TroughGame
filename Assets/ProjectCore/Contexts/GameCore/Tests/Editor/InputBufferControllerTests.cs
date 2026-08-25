using Fusion;
using NUnit.Framework;

namespace ProjectCore.GameCore
{
    public sealed class InputBufferControllerTests
    {
        [Test]
        public void BeginActionConsumesBufferedCommandAndLocksController()
        {
            var state = new TestInputBufferState
            {
                BufferedCommand = new InputBufferCommandData
                {
                    CommandId = 1,
                },
            };
            var controller = new InputBufferController(
                state,
                state,
                new TestNetworkBehaviourAccessor());

            Assert.That(controller.BeginAction(1), Is.True);
            Assert.That(
                state.BufferedCommand.CommandId,
                Is.EqualTo(0));
            Assert.That(state.IsLocked, Is.True);
            Assert.That(controller.IsLocked, Is.True);
        }

        [Test]
        public void EndActionUnlocksControllerWithoutTouchingBufferedCommand()
        {
            var state = new TestInputBufferState
            {
                IsLocked = true,
                BufferedCommand = new InputBufferCommandData
                {
                    CommandId = 2,
                },
            };
            var controller = new InputBufferController(
                state,
                state,
                new TestNetworkBehaviourAccessor());

            controller.EndAction();

            Assert.That(controller.IsLocked, Is.False);
            Assert.That(state.IsLocked, Is.False);
            Assert.That(
                state.BufferedCommand.CommandId,
                Is.EqualTo(2));
        }

        private sealed class TestInputBufferState :
            IInputBufferStateMutator
        {
            public InputBufferCommandData BufferedCommand { get; set; }
            public TickTimer BufferedCommandTimer { get; set; }
            public bool IsLocked { get; set; }

            public void SetBufferedCommand(
                InputBufferCommandData command,
                TickTimer expirationTimer)
            {
                BufferedCommand = command;
                BufferedCommandTimer = expirationTimer;
            }

            public void ClearBufferedCommand()
            {
                BufferedCommand = default;
                BufferedCommandTimer = TickTimer.None;
            }

            public void LockInput()
            {
                IsLocked = true;
            }

            public void UnlockInput()
            {
                IsLocked = false;
            }
        }

        private sealed class TestNetworkBehaviourAccessor :
            INetworkBehaviourAccessor
        {
            public NetworkBehaviour ParentNetworkBehaviour =>
                null;
        }
    }
}

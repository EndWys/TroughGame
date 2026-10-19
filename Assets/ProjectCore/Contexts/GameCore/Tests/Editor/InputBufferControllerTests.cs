using System;
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
                BufferedCommand = new InputBufferCommandDescriptor(1),
            };
            var controller = new InputBufferController(
                state,
                state,
                new TestNetworkBehaviourAccessor());

            Assert.That(controller.TryConsumeBufferedCommandAndLock(1), Is.True);
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
                BufferedCommand = new InputBufferCommandDescriptor(2),
            };
            var controller = new InputBufferController(
                state,
                state,
                new TestNetworkBehaviourAccessor());

            controller.ReleaseCommandLock();

            Assert.That(controller.IsLocked, Is.False);
            Assert.That(state.IsLocked, Is.False);
            Assert.That(
                state.BufferedCommand.CommandId,
                Is.EqualTo(2));
        }

        [Test]
        public void LockedControllerCanInspectPendingCommandButCannotConsumeIt()
        {
            var state = new TestInputBufferState
            {
                IsLocked = true,
                BufferedCommand = new InputBufferCommandDescriptor(2),
            };
            var controller = new InputBufferController(state, state, new TestNetworkBehaviourAccessor());

            Assert.That(controller.HasBufferedCommand(2), Is.True);
            Assert.That(controller.TryConsumeBufferedCommandAndLock(2), Is.False);
            Assert.That(state.BufferedCommand.CommandId, Is.EqualTo(2));
            Assert.That(state.IsLocked, Is.True);

            controller.ReleaseCommandLock();
            Assert.That(controller.TryConsumeBufferedCommandAndLock(2), Is.True);
            Assert.That(state.BufferedCommand.CommandId, Is.Zero);
            Assert.That(controller.IsLocked, Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(127)]
        [TestCase(65535)]
        public void NonmatchingCommandDoesNotConsumeOrClearPendingInput(int commandId)
        {
            var state = new TestInputBufferState
            {
                BufferedCommand = new InputBufferCommandDescriptor(2),
            };
            var controller = new InputBufferController(state, state, new TestNetworkBehaviourAccessor());

            Assert.That(controller.HasBufferedCommand((ushort)commandId), Is.False);
            Assert.That(controller.TryConsumeBufferedCommandAndLock((ushort)commandId), Is.False);
            Assert.That(state.BufferedCommand.CommandId, Is.EqualTo(2));
            Assert.That(controller.IsLocked, Is.False);
        }

        [Test]
        public void MissingRunnerDoesNotCreateUntimedPendingCommandOrReplaceExistingInput()
        {
            var state = new TestInputBufferState
            {
                IsLocked = true,
                BufferedCommand = new InputBufferCommandDescriptor(1),
            };
            var controller = new InputBufferController(state, state, new TestNetworkBehaviourAccessor());

            controller.BufferCommand(new InputBufferCommandDescriptor(2), 0.15f);

            Assert.That(state.BufferedCommand.CommandId, Is.EqualTo(1));
            Assert.That(state.BufferedCommandTimer, Is.EqualTo(TickTimer.None));
            Assert.That(controller.IsLocked, Is.True);
        }

        [Test]
        public void ExplicitCancellationClearsPendingCommandAndReleasesLockUsingExistingContracts()
        {
            var state = new TestInputBufferState
            {
                IsLocked = true,
                BufferedCommand = new InputBufferCommandDescriptor(2),
            };
            var controller = new InputBufferController(state, state, new TestNetworkBehaviourAccessor());

            state.ClearBufferedCommand();
            controller.ReleaseCommandLock();

            Assert.That(controller.IsLocked, Is.False);
            Assert.That(controller.HasBufferedCommand(2), Is.False);
            Assert.That(state.BufferedCommandTimer, Is.EqualTo(TickTimer.None));
        }

        [Test]
        public void ExistingDescriptorPreservesUshortIdIndependentlyOfPackedStorageValidation()
        {
            var command = new InputBufferCommandDescriptor(ushort.MaxValue);
            Assert.That(command.CommandId, Is.EqualTo(ushort.MaxValue));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PackedModelPreservesCommandAndLockForEverySupportedId(bool isLocked)
        {
            var model = new InputBufferStateModel { IsLocked = isLocked };

            for (ushort commandId = 0; commandId <= 127; commandId++)
            {
                model.BufferedCommandId = commandId;
                Assert.That(model.BufferedCommandId, Is.EqualTo(commandId));
                Assert.That(model.IsLocked, Is.EqualTo(isLocked));

                model.IsLocked = !isLocked;
                Assert.That(model.BufferedCommandId, Is.EqualTo(commandId));
                Assert.That(model.IsLocked, Is.EqualTo(!isLocked));
                model.IsLocked = isLocked;
            }
        }

        [TestCase(128)]
        [TestCase(65535)]
        public void PackedModelRejectsOutOfRangeIdWithoutReplacingPendingCommand(int commandId)
        {
            var model = new InputBufferStateModel
            {
                BufferedCommandId = 17,
                IsLocked = true,
            };

            Assert.Throws<ArgumentOutOfRangeException>(
                () => model.BufferedCommandId = (ushort)commandId);

            Assert.That(model.BufferedCommandId, Is.EqualTo(17));
            Assert.That(model.IsLocked, Is.True);
        }

        [Test]
        public void PackedModelCopyAndClearPreserveIndependentExecutionLock()
        {
            var original = new InputBufferStateModel
            {
                BufferedCommandId = 127,
                IsLocked = true,
            };
            InputBufferStateModel copy = original;
            copy.BufferedCommandId = 0;
            copy.BufferedCommandTimer = TickTimer.None;

            Assert.That(copy.BufferedCommandId, Is.Zero);
            Assert.That(copy.IsLocked, Is.True);
            Assert.That(original.BufferedCommandId, Is.EqualTo(127));
            Assert.That(original.IsLocked, Is.True);

            copy = default;
            Assert.That(copy.BufferedCommandId, Is.Zero);
            Assert.That(copy.IsLocked, Is.False);
            Assert.That(copy.BufferedCommandTimer.IsRunning, Is.False);
        }

        private sealed class TestInputBufferState :
            IInputBufferStateMutator
        {
            public InputBufferCommandDescriptor BufferedCommand { get; set; }
            public TickTimer BufferedCommandTimer { get; set; }
            public bool IsLocked { get; set; }

            public void SetBufferedCommand(
                InputBufferCommandDescriptor command,
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

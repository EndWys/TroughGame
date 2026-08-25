using NUnit.Framework;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class DodgeProcessorTests
    {
        [Test]
        public void TransitionProcessorDetectsMatchingBufferedCommandWithoutConsumingIt()
        {
            var inputBufferController = new TestInputBufferController();
            inputBufferController.BufferCommand(new InputBufferCommandData
            {
                CommandId = MovementInputCommandConstants.DodgeCommandId,
            }, 0.15f);

            var processor = new DodgeTransitionProcessor<
                TestState,
                TestPayload>(
                inputBufferController,
                MovementInputCommandConstants.DodgeCommandId,
                TestState.Dodge);

            bool transitioned = processor.Execute(
                default,
                out TestState resultState);

            Assert.That(transitioned, Is.True);
            Assert.That(resultState, Is.EqualTo(TestState.Dodge));
            Assert.That(inputBufferController.HasBufferedCommand(
                MovementInputCommandConstants.DodgeCommandId), Is.True);
            Assert.That(inputBufferController.IsLocked, Is.False);
        }

        [Test]
        public void DodgeProcessorWritesVelocityWhileCommandIsLocked()
        {
            var body = new TestMovementBody();
            var timerAccessor = new TestMovementStateTimerAccessor
            {
                IsStateTimerFinished = false,
            };
            DodgeMovementConfig config =
                ScriptableObject.CreateInstance<DodgeMovementConfig>();

            try
            {
                var processor = new DodgeProcessor<
                    TestState,
                    TestPayload>(
                    body,
                    timerAccessor,
                    config,
                    TestState.Locomotion);

                bool transitioned = processor.Execute(
                    new TestPayload(Vector2.right),
                    out TestState resultState);

                Assert.That(transitioned, Is.False);
                Assert.That(resultState, Is.EqualTo(default(TestState)));
                Assert.That(body.Velocity, Is.EqualTo(Vector2.right * config.Speed));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        [Test]
        public void DodgeProcessorTransitionsWhenActionHasFinished()
        {
            var body = new TestMovementBody
            {
                Velocity = Vector2.up,
            };
            var timerAccessor = new TestMovementStateTimerAccessor
            {
                IsStateTimerFinished = true,
            };
            DodgeMovementConfig config =
                ScriptableObject.CreateInstance<DodgeMovementConfig>();

            try
            {
                var processor = new DodgeProcessor<
                    TestState,
                    TestPayload>(
                    body,
                    timerAccessor,
                    config,
                    TestState.Locomotion);

                bool transitioned = processor.Execute(
                    default,
                    out TestState resultState);

                Assert.That(transitioned, Is.True);
                Assert.That(resultState, Is.EqualTo(TestState.Locomotion));
                Assert.That(body.Velocity, Is.EqualTo(Vector2.zero));
            }
            finally
            {
                Object.DestroyImmediate(config);
            }
        }

        private enum TestState
        {
            Locomotion,
            Dodge,
        }

        private readonly struct TestPayload : ILocomotionPayload
        {
            public TestPayload(Vector2 direction)
            {
                Direction = direction;
            }

            public Vector2 Direction { get; }
        }

        private sealed class TestInputBufferController :
            IInputBufferController
        {
            public bool IsLocked { get; set; }
            public InputBufferCommandData BufferedCommand { get; private set; }

            public bool HasBufferedCommand(ushort commandId)
            {
                return BufferedCommand.CommandId == commandId;
            }

            public void BufferCommand(
                InputBufferCommandData command,
                float lifetimeSeconds)
            {
                BufferedCommand = command;
            }

            public bool BeginAction(ushort commandId)
            {
                if (IsLocked ||
                    !HasBufferedCommand(commandId))
                {
                    return false;
                }

                BufferedCommand = default;
                IsLocked = true;
                return true;
            }

            public void EndAction()
            {
                IsLocked = false;
            }
        }

        private sealed class TestMovementStateTimerAccessor :
            IMovementStateTimerAccessor
        {
            public bool IsStateTimerFinished { get; set; }
        }

        private sealed class TestMovementBody : IMovementBodyVelocityMutator
        {
            public Vector2 Velocity { get; set; }
        }
    }
}

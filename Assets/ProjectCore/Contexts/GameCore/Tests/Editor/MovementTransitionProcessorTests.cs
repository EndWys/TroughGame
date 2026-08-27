using NUnit.Framework;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class MovementTransitionProcessorTests
    {
        [Test]
        public void DirectionalInputTransitionsToLocomotion()
        {
            var processor =
                new DirectionalLocomotionTransitionProcessor<
                    TestState,
                    TestPayload>(
                    locomotionMovementState: TestState.Locomotion);

            bool transitioned = processor.Execute(
                new TestPayload(Vector2.right),
                out TestState resultState);

            Assert.That(transitioned, Is.True);
            Assert.That(resultState, Is.EqualTo(TestState.Locomotion));
        }

        [Test]
        public void MissingDirectionTransitionsToIdle()
        {
            var processor =
                new NoDirectionTransitionProcessor<
                    TestState,
                    TestPayload>(
                    idleMovementState: TestState.Idle);

            bool transitioned = processor.Execute(
                new TestPayload(Vector2.zero),
                out TestState resultState);

            Assert.That(transitioned, Is.True);
            Assert.That(resultState, Is.EqualTo(TestState.Idle));
        }

        private enum TestState
        {
            Idle,
            Locomotion,
        }

        private readonly struct TestPayload : ILocomotionPayload
        {
            public TestPayload(Vector2 direction)
            {
                Direction = direction;
            }

            public Vector2 Direction { get; }
        }
    }
}

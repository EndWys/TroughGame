using System;
using Domain;
using NUnit.Framework;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class MovementSimulationServiceTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void SimulateMovesBodyUsingItsVelocity()
        {
            var body = new FixedMovementBody(
                new Vector2(10f, 20f),
                new Vector2(3f, 4f));
            var service = new MovementSimulationService(
                new FreeMovementCollisionService());

            service.Simulate(body, 0.2f);

            AssertVector(body.Position, new Vector2(10.6f, 20.8f));
            AssertVector(body.Velocity, new Vector2(3f, 4f));
            Assert.That(body.AppliedSimulationCount, Is.EqualTo(1));
        }

        [Test]
        public void SimulateAppliesCollisionResolvedVelocity()
        {
            var body = new FixedMovementBody(Vector2.one, Vector2.right * 4f);
            var service = new MovementSimulationService(
                new ScaledMovementCollisionStrategy(0.5f));

            service.Simulate(body, 0.25f);

            AssertVector(body.Position, new Vector2(1.5f, 1f));
            AssertVector(body.Velocity, Vector2.right * 2f);
            Assert.That(body.AppliedSimulationCount, Is.EqualTo(1));
        }

        [Test]
        public void SimulateIgnoresNonPositiveDeltaTime()
        {
            var body = new FixedMovementBody(Vector2.one, Vector2.right);
            var service = new MovementSimulationService(
                new FreeMovementCollisionService());

            service.Simulate(body, 0f);

            AssertVector(body.Position, Vector2.one);
            AssertVector(body.Velocity, Vector2.right);
            Assert.That(body.AppliedSimulationCount, Is.Zero);
        }

        [Test]
        public void SimulateRejectsMissingBody()
        {
            var service = new MovementSimulationService(
                new FreeMovementCollisionService());

            Assert.Throws<ArgumentNullException>(() => service.Simulate(null, 0.1f));
        }

        [Test]
        public void LocomotionProcessorOnlyAssignsBodyVelocity()
        {
            var body = new FixedMovementBody(Vector2.zero, Vector2.zero);
            var processor = new TestLocomotionProcessor(
                new TestMovementState(),
                body);

            bool isCompleted = processor.Execute(
                new TestMovementPayload(Vector2.up * 5f),
                out TestMovementStateType resultState);

            Assert.That(isCompleted, Is.False);
            Assert.That(resultState, Is.EqualTo(TestMovementStateType.Default));
            AssertVector(body.Velocity, Vector2.up * 5f);
            AssertVector(body.Position, Vector2.zero);
            Assert.That(body.AppliedSimulationCount, Is.Zero);
        }

        private static void AssertVector(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
        }

        private sealed class FixedMovementBody : IMovementBodyMutator
        {
            public FixedMovementBody(Vector2 position, Vector2 velocity)
            {
                Position = position;
                Velocity = velocity;
            }

            public Vector2 Position { get; private set; }
            public Vector2 Velocity { get; set; }
            public float CollisionRadius => 0.5f;
            public LayerMask CollisionMask => -1;
            public int AppliedSimulationCount { get; private set; }

            public void ApplySimulation(
                Vector2 resolvedVelocity,
                Vector2 resolvedDisplacement)
            {
                Velocity = resolvedVelocity;
                Position += resolvedDisplacement;
                AppliedSimulationCount++;
            }
        }

        private sealed class ScaledMovementCollisionStrategy : IMovementCollisionStrategy
        {
            private readonly float _scale;

            public ScaledMovementCollisionStrategy(float scale)
            {
                _scale = scale;
            }

            public Vector2 ResolveDisplacement(
                IMovementCollisionBodyAccessor body,
                Vector2 desiredDisplacement)
            {
                return desiredDisplacement * _scale;
            }
        }

        private sealed class TestLocomotionProcessor :
            BaseMovementStateProcessor<
                TestMovementStateType,
                TestMovementState,
                TestMovementPayload>
        {
            private readonly IMovementBodyVelocityMutator _movementBody;

            public TestLocomotionProcessor(
                TestMovementState state,
                IMovementBodyVelocityMutator movementBody) : base(state)
            {
                _movementBody = movementBody ??
                    throw new ArgumentNullException(nameof(movementBody));
            }

            public override bool Execute(
                TestMovementPayload payload,
                out TestMovementStateType resultState)
            {
                _movementBody.Velocity = payload.Velocity;
                return Continue(out resultState);
            }
        }

        private sealed class TestMovementState :
            IState<TestMovementStateType, TestMovementPayload>
        {
            public void Enter() { }

            public TestMovementStateType Tick(TestMovementPayload payload)
            {
                return TestMovementStateType.Default;
            }

            public void Exit() { }
        }

        private enum TestMovementStateType
        {
            Default = 0,
        }

        private readonly struct TestMovementPayload
        {
            public TestMovementPayload(Vector2 velocity)
            {
                Velocity = velocity;
            }

            public Vector2 Velocity { get; }
        }
    }
}

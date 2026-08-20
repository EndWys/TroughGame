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
                new PassThroughMovementCollisionStrategy());

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
                new PassThroughMovementCollisionStrategy());

            service.Simulate(body, 0f);

            AssertVector(body.Position, Vector2.one);
            AssertVector(body.Velocity, Vector2.right);
            Assert.That(body.AppliedSimulationCount, Is.Zero);
        }

        [Test]
        public void SimulateRejectsMissingBody()
        {
            var service = new MovementSimulationService(
                new PassThroughMovementCollisionStrategy());

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

        [Test]
        public void LevelCollisionStopsBodyBeforeWall()
        {
            GameObject wall = CreateWall();

            try
            {
                var body = new FixedMovementBody(
                    Vector2.zero,
                    Vector2.zero,
                    0.5f,
                    1 << wall.layer);
                var service = new LevelCollisionService();

                Vector2 displacement = service.ResolveDisplacement(
                    body,
                    Vector2.right * 3f);

                Assert.That(displacement.x, Is.InRange(0.98f, 1f));
                Assert.That(displacement.y, Is.Zero.Within(Tolerance));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(wall);
            }
        }

        [Test]
        public void LevelCollisionSlidesBodyAlongWall()
        {
            GameObject wall = CreateWall();

            try
            {
                var body = new FixedMovementBody(
                    Vector2.zero,
                    Vector2.zero,
                    0.5f,
                    1 << wall.layer);
                var service = new LevelCollisionService();

                Vector2 displacement = service.ResolveDisplacement(
                    body,
                    new Vector2(3f, 1f));

                Assert.That(displacement.x, Is.InRange(0.98f, 1f));
                Assert.That(displacement.y, Is.EqualTo(1f).Within(Tolerance));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(wall);
            }
        }

        [Test]
        public void LevelCollisionIgnoresLayersOutsideBodyMask()
        {
            GameObject wall = CreateWall();

            try
            {
                var body = new FixedMovementBody(
                    Vector2.zero,
                    Vector2.zero,
                    0.5f,
                    0);
                var service = new LevelCollisionService();

                Vector2 displacement = service.ResolveDisplacement(
                    body,
                    Vector2.right * 3f);

                AssertVector(displacement, Vector2.right * 3f);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(wall);
            }
        }

        private static void AssertVector(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(Tolerance));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(Tolerance));
        }

        private static GameObject CreateWall()
        {
            var wall = new GameObject("Wall");
            wall.transform.position = new Vector2(2f, 0f);
            var collider = wall.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 10f);
            Physics2D.SyncTransforms();
            return wall;
        }

        private sealed class FixedMovementBody : IMovementBodyMutator
        {
            public FixedMovementBody(
                Vector2 position,
                Vector2 velocity,
                float collisionRadius = 0.5f,
                int collisionMask = -1)
            {
                Position = position;
                Velocity = velocity;
                CollisionRadius = collisionRadius;
                CollisionMask = collisionMask;
            }

            public Vector2 Position { get; private set; }
            public Vector2 Velocity { get; set; }
            public float CollisionRadius { get; }
            public LayerMask CollisionMask { get; }
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

        private sealed class PassThroughMovementCollisionStrategy :
            IMovementCollisionStrategy
        {
            public Vector2 ResolveDisplacement(
                IMovementCollisionBodyAccessor body,
                Vector2 desiredDisplacement)
            {
                return desiredDisplacement;
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

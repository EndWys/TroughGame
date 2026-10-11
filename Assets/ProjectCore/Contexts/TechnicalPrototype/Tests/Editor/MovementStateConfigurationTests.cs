using System;
using System.Reflection;
using Fusion;
using NUnit.Framework;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class MovementStateConfigurationTests
    {
        private GameObject _player;
        private GameObject _enemy;
        private DodgeMovementConfig _dodgeConfig;
        private DashMovementConfig _dashConfig;
        private PlayerDodgeState _dodgeState;
        private EnemyDashState _dashState;

        [SetUp]
        public void SetUp()
        {
            _player = new GameObject("DodgeInitializationTest");
            _enemy = new GameObject("DashInitializationTest");
            _dodgeConfig = ScriptableObject.CreateInstance<DodgeMovementConfig>();
            _dodgeConfig.name = "TestDodge";
            _dashConfig = ScriptableObject.CreateInstance<DashMovementConfig>();
            _dashConfig.name = "TestDash";
            _dodgeState = _player.AddComponent<PlayerDodgeState>();
            _dashState = _enemy.AddComponent<EnemyDashState>();

            SetField(_dodgeState, "_movementBody", _player.AddComponent<TransformMovementBodyComponent>());
            SetField(_dodgeState, "_dodgeMovementConfig", _dodgeConfig);
            SetField(_dashState, "_movementBody", _enemy.AddComponent<TransformMovementBodyComponent>());
            SetField(_dashState, "_dashMovementConfig", _dashConfig);

            var container = new DiContainer();
            var movementState = new InitializationMovementState();
            container.Bind<IInputBufferController>().FromInstance(new InitializationInputController());
            container.Bind<IMovementStateTimerMutator>().FromInstance((IMovementStateTimerMutator)movementState);
            container.Bind<IEnemyDashStateMutator>().FromInstance((IEnemyDashStateMutator)movementState);
            container.Bind<IMovementObstacleProbe>().FromInstance(new InitializationObstacleProbe());
            container.Bind<INetworkBehaviourAccessor>().FromInstance(new InitializationNetworkAccessor());
            container.Inject(_dodgeState);
            container.Inject(_dashState);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_player);
            Object.DestroyImmediate(_enemy);
            Object.DestroyImmediate(_dodgeConfig);
            Object.DestroyImmediate(_dashConfig);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void DodgeRejectsInvalidDurationDuringInit(float duration)
        {
            SetField(_dodgeConfig, "_durationSeconds", duration);
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(_dodgeState.Init);
            Assert.That(exception.Message, Does.Contain("TestDodge").And.Contain("DurationSeconds"));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void DashRejectsInvalidDurationDuringInit(float duration)
        {
            SetField(_dashConfig, "_durationSeconds", duration);
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(_dashState.Init);
            Assert.That(exception.Message, Does.Contain("TestDash").And.Contain("DurationSeconds"));
        }

        [Test]
        public void ValidStatesInitializeBeforeInputOrSimulation()
        {
            Assert.DoesNotThrow(_dodgeState.Init);
            Assert.DoesNotThrow(_dashState.Init);
        }

        private static void SetField<TTarget, TValue>(TTarget target, string field, TValue value)
        {
            typeof(TTarget).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }

        private sealed class InitializationInputController : IInputBufferController
        {
            public bool IsLocked => throw new InvalidOperationException("Initialization must not read input.");

            public bool HasBufferedCommand(ushort commandId) =>
                throw new InvalidOperationException("Initialization must not read input.");

            public void BufferCommand(InputBufferCommandDescriptor command, float lifetimeSeconds) =>
                throw new InvalidOperationException("Initialization must not buffer input.");

            public bool BeginAction(ushort commandId) =>
                throw new InvalidOperationException("Initialization must not begin an action.");

            public void EndAction() =>
                throw new InvalidOperationException("Initialization must not end an action.");
        }

        private sealed class InitializationObstacleProbe : IMovementObstacleProbe
        {
            public bool TryProbe(
                IMovementCollisionBodyAccessor body, Vector2 direction, float distance, out Vector2 obstacleNormal) =>
                throw new InvalidOperationException("Initialization must not simulate movement.");
        }

        private sealed class InitializationMovementState : IMovementStateTimerMutator, IEnemyDashStateMutator
        {
            public bool IsStateTimerFinished => throw new InvalidOperationException("Unexpected timer read.");
            public Vector2 DashTargetPosition => throw new InvalidOperationException("Unexpected dash state read.");
            public Vector2 DashDirection => throw new InvalidOperationException("Unexpected dash state read.");
            public bool IsDashActionFinished => throw new InvalidOperationException("Unexpected dash state read.");
            public bool IsDashCooldownFinished => throw new InvalidOperationException("Unexpected dash state read.");
            public TickTimer DashActionTimer => throw new InvalidOperationException("Unexpected dash timer read.");
            public TickTimer DashCooldownTimer => throw new InvalidOperationException("Unexpected dash timer read.");

            public void StartStateTimer(float durationSeconds) =>
                throw new InvalidOperationException("Initialization must not start a timer.");

            public void StopStateTimer() =>
                throw new InvalidOperationException("Initialization must not stop a timer.");

            public void StartDash(Vector2 targetPosition, Vector2 direction, float durationSeconds, float cooldownSeconds) =>
                throw new InvalidOperationException("Initialization must not start a dash.");

            public void StopDash() =>
                throw new InvalidOperationException("Initialization must not stop a dash.");
        }

        private sealed class InitializationNetworkAccessor : INetworkBehaviourAccessor
        {
            public NetworkBehaviour ParentNetworkBehaviour =>
                throw new InvalidOperationException("Initialization must not access simulation.");
        }
    }
}

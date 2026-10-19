using System;
using System.Reflection;
using Fusion;
using NUnit.Framework;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;
using ProjectCore.Template;
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

        [TestCase(ActionPhase.Preparation)]
        [TestCase(ActionPhase.Active)]
        [TestCase(ActionPhase.Recovery)]
        public void DodgeMovementUsesOnlyItsMovementPhase(ActionPhase phase)
        {
            var body = new MovementVelocityMutator();
            var dodge = new PlayerDodgeMovementProcessor(body, _dodgeConfig, () => phase);
            Assert.That(dodge.Execute(new PlayerEntityPayload(new PlayerInputSnapshotData(Vector2.right, default)), out _), Is.False);
            Assert.That(body.Velocity, Is.EqualTo(phase == ActionPhase.Active ? Vector2.right * 8f : Vector2.zero));
        }

        [Test]
        public void EntityScopedFactoryKeepsMovementDependenciesIsolated()
        {
            var first = new MovementVelocityMutator();
            var second = new MovementVelocityMutator();
            var config = ScriptableObject.CreateInstance<LocomotionMovementConfig>();
            try
            {
                foreach (var body in new[] { first, second })
                {
                    var scope = new DiContainer();
                    scope.BindInterfacesAndSelfTo<ClassFactory>().AsSingle();
                    scope.Bind<IMovementBodyVelocityMutator>().FromInstance(body);
                    var processor = scope.Resolve<IClassFactory>().Create<LocomotionProcessor<PlayerEntityState, PlayerEntityPayload>>(new object[] { config });
                    processor.Execute(new PlayerEntityPayload(new PlayerInputSnapshotData(body == first ? Vector2.right : Vector2.up, default)), out _);
                }
                Assert.That(first.Velocity.x, Is.GreaterThan(0));
                Assert.That(first.Velocity.y, Is.Zero);
                Assert.That(second.Velocity.x, Is.Zero);
                Assert.That(second.Velocity.y, Is.GreaterThan(0));
            }
            finally { Object.DestroyImmediate(config); }
        }

        [TestCase(0, ActionPhase.Preparation)]
        [TestCase(1, ActionPhase.Preparation)]
        [TestCase(2, ActionPhase.Active)]
        [TestCase(13, ActionPhase.Active)]
        [TestCase(14, ActionPhase.Recovery)]
        [TestCase(19, ActionPhase.Recovery)]
        [TestCase(20, ActionPhase.None)]
        public void DodgeBoundariesUseTheSingleMovementEndTick(int elapsed, ActionPhase expected)
        {
            var timing = new ActionTimingData(2, 12, 6);
            Assert.That(ActionTimelineData.FromEndTick(timing, 120).Evaluate(100 + elapsed).Phase, Is.EqualTo(expected));
            int start = int.MaxValue - 4;
            Assert.That(ActionTimelineData.FromEndTick(timing, unchecked(start + 20))
                .Evaluate(unchecked(start + elapsed)).Phase, Is.EqualTo(expected));
        }

        [Test]
        public void DodgeQuantizesPhasesSeparatelyAndHasNoEarlyRecoveryWindow()
        {
            ActionTimingData timing = MovementConfigValidation.CompileDodgeTiming(_dodgeConfig, 1f / 72f);
            Assert.That(timing.PreparationTicks, Is.EqualTo(3));
            Assert.That(timing.ActiveTicks, Is.EqualTo(15));
            Assert.That(timing.RecoveryTicks, Is.EqualTo(8));
            Assert.That(timing.TotalTicks, Is.EqualTo(26));
            Assert.That(timing.HasRecoveryCancelWindow, Is.False);
            Assert.That(ActionTimelineData.FromEndTick(timing, null).Evaluate(0).Phase, Is.EqualTo(ActionPhase.None));
        }

        [Test]
        public void MovementGraphBuildsAndMovesWithoutCombatBindings()
        {
            var root = _player.AddComponent<MovementOnlyRoot>();
            var machine = _player.AddComponent<PlayerEntityStateMachine>();
            var idle = _player.AddComponent<PlayerIdleState>();
            var locomotion = _player.AddComponent<PlayerLocomotionState>();
            var config = ScriptableObject.CreateInstance<LocomotionMovementConfig>();
            try
            {
                var body = _player.GetComponent<TransformMovementBodyComponent>();
                SetField(machine, "_idleState", idle);
                SetField(machine, "_locomotionState", locomotion);
                SetField(locomotion, "_locomotionMovementConfig", config);
                var state = new MovementOnlyState();
                var container = new DiContainer();
                container.Bind<INetworkBehaviourAccessor>().FromInstance(_player.GetComponent<INetworkBehaviourAccessor>());
                container.Bind<INetworkEntityIdAccessor>().FromInstance(_player.GetComponent<INetworkEntityIdAccessor>());
                container.Bind<Shared.IStateDataMutator<PlayerEntityState>>().FromInstance(state);
                container.BindInterfacesAndSelfTo<ClassFactory>().AsSingle();
                container.Bind<IMovementBodyVelocityMutator>().FromInstance(_player.GetComponent<IMovementBodyVelocityMutator>());
                container.Bind<IEntityStateRestart>().FromInstance(machine);
                container.Bind<IInputBufferStateMutator>().FromInstance(new InitializationBufferState());
                container.Bind<IInputBufferController>().FromInstance(new EmptyInputController());
                container.Bind<IPlayerFacingDirectionMutator>().FromInstance(new MovementOnlyFacing());
                container.Bind<IPlayerInputAccessor>().FromInstance(new MovementOnlyInput());
                Assert.DoesNotThrow(() => { container.Inject(machine); container.Inject(idle); container.Inject(locomotion); });
                Assert.That(machine.CreateStatesDictionary().Keys, Is.EquivalentTo(new[] { PlayerEntityState.Idle, PlayerEntityState.Locomotion }));
                idle.Init();
                locomotion.Init();
                Assert.That(idle.Tick(new PlayerEntityPayload(new PlayerInputSnapshotData(Vector2.right, default))), Is.EqualTo(PlayerEntityState.Locomotion));
                locomotion.Tick(new PlayerEntityPayload(new PlayerInputSnapshotData(Vector2.right, default)));
                Assert.That(body.Velocity.x, Is.GreaterThan(0f));
            }
            finally { Object.DestroyImmediate(config); }
        }

        public sealed class MovementOnlyRoot : BaseNetworkEntityRoot
        {
            protected override System.Collections.Generic.IEnumerable<INetworkEntityComponent> CreateComponents() =>
                System.Array.Empty<INetworkEntityComponent>();
        }

        private sealed class MovementOnlyState : Shared.IStateDataMutator<PlayerEntityState>
        {
            public PlayerEntityState CurrentState { get; private set; }
            public PlayerEntityState PreviousState { get; private set; }
            public PlayerEntityState CurrentMovementStates => CurrentState;
            public PlayerEntityState PreviousMovementStates => PreviousState;
            public void ChangeMovementState(PlayerEntityState state) => ChangeState(state);
            public void ChangeState(PlayerEntityState state) { PreviousState = CurrentState; CurrentState = state; }
        }

        private sealed class MovementOnlyFacing : IPlayerFacingDirectionMutator
        {
            public PlayerFacingDirection FacingDirection { get; private set; }
            public void SetFacingDirection(PlayerFacingDirection facing) { FacingDirection = facing; }
        }

        private sealed class MovementOnlyInput : IPlayerInputAccessor
        {
            public PlayerInputSnapshotData Input => default;
        }

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
            _enemy.AddComponent<TransformMovementBodyComponent>();
            SetField(_dashState, "_dashMovementConfig", _dashConfig);

            var container = new DiContainer();
            var movementState = new InitializationMovementState();
            container.BindInterfacesAndSelfTo<ClassFactory>().AsSingle();
            container.Bind<IMovementBodyVelocityMutator>().FromInstance(_player.GetComponent<IMovementBodyVelocityMutator>());
            container.Bind<IEntityStateTimerAccessor>().FromInstance(movementState);
            container.Bind<IMovementCollisionBodyAccessor>().FromInstance(_enemy.GetComponent<IMovementCollisionBodyAccessor>());
            container.Bind<IInputBufferStateMutator>().FromInstance(new InitializationBufferState());
            container.Bind<ICombatInvulnerabilityMutator>().FromInstance(new InitializationInvulnerability());
            container.Bind<IInputBufferController>().FromInstance(new InitializationInputController());
            container.Bind<IPlayerMediator>().FromInstance(new InitializationMediator());
            container.Bind<IEntityStateTimerMutator>().FromInstance((IEntityStateTimerMutator)movementState);
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

        private sealed class MovementVelocityMutator : IMovementBodyVelocityMutator
        {
            public Vector2 Velocity { get; set; }
        }

        private sealed class EmptyInputController : IInputBufferController
        {
            public bool IsLocked => false;
            public bool HasBufferedCommand(ushort commandId) => false;
            public void BufferCommand(InputBufferCommandDescriptor command, float lifetimeSeconds) { }
            public bool TryConsumeBufferedCommandAndLock(ushort commandId) => false;
            public void ReleaseCommandLock() { }
        }
        private sealed class InitializationBufferState : IInputBufferStateMutator
        {
            public InputBufferCommandDescriptor BufferedCommand => default;
            public TickTimer BufferedCommandTimer => default;
            public bool IsLocked => false;
            public void SetBufferedCommand(InputBufferCommandDescriptor command, TickTimer timer) { throw new InvalidOperationException(); }
            public void ClearBufferedCommand() { throw new InvalidOperationException(); }
            public void LockInput() { throw new InvalidOperationException(); }
            public void UnlockInput() { throw new InvalidOperationException(); }
        }
        private sealed class InitializationInvulnerability : ICombatInvulnerabilityMutator
        {
            public bool IsInvulnerable => false;
            public void SetInvulnerabilityUntilTick(int? endTick) { throw new InvalidOperationException(); }
        }

        private sealed class InitializationInputController : IInputBufferController
        {
            public bool IsLocked => throw new InvalidOperationException("Initialization must not read input.");

            public bool HasBufferedCommand(ushort commandId) =>
                throw new InvalidOperationException("Initialization must not read input.");

            public void BufferCommand(InputBufferCommandDescriptor command, float lifetimeSeconds) =>
                throw new InvalidOperationException("Initialization must not buffer input.");

            public bool TryConsumeBufferedCommandAndLock(ushort commandId) =>
                throw new InvalidOperationException("Initialization must not begin an action.");

            public void ReleaseCommandLock() =>
                throw new InvalidOperationException("Initialization must not end an action.");
        }

        private sealed class InitializationMediator : IPlayerMediator
        {
            public void Notify(Shared.HandlerPayload payload) =>
                throw new InvalidOperationException("Initialization must not publish gameplay messages.");
        }

        private sealed class InitializationObstacleProbe : IMovementObstacleProbe
        {
            public bool TryProbe(
                IMovementCollisionBodyAccessor body, Vector2 direction, float distance, out Vector2 obstacleNormal) =>
                throw new InvalidOperationException("Initialization must not simulate movement.");
        }

        private sealed class InitializationMovementState : IEntityStateTimerMutator, IEnemyDashStateMutator
        {
            public int? StateTimerEndTick => throw new InvalidOperationException("Initialization must not read a timer.");
            public void StartStateTimerTicks(int ticks) => throw new InvalidOperationException("Initialization must not start a timer.");
            public bool IsStateTimerFinished => throw new InvalidOperationException("Unexpected timer read.");
            public Vector2 DashTargetPosition => throw new InvalidOperationException("Unexpected dash state read.");
            public Vector2 DashDirection => throw new InvalidOperationException("Unexpected dash state read.");
            public bool IsDashActionFinished => throw new InvalidOperationException("Unexpected dash state read.");
            public bool IsDashCooldownFinished => throw new InvalidOperationException("Unexpected dash state read.");
            public TickTimer DashActionTimer => throw new InvalidOperationException("Unexpected dash timer read.");
            public TickTimer DashCooldownTimer => throw new InvalidOperationException("Unexpected dash timer read.");

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

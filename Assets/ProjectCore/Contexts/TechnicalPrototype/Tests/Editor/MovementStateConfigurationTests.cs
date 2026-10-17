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

        [TestCase(DodgeMovementPhase.Preparation)]
        [TestCase(DodgeMovementPhase.Active)]
        [TestCase(DodgeMovementPhase.Recovery)]
        public void DodgeMovementUsesOnlyItsMovementPhase(DodgeMovementPhase phase)
        {
            var body = new MovementVelocityMutator();
            var dodge = new PlayerDodgeMovementProcessor(body, _dodgeConfig, () => phase);
            Assert.That(dodge.Execute(new PlayerMovementPayload(Vector2.right), out _), Is.False);
            Assert.That(body.Velocity, Is.EqualTo(phase == DodgeMovementPhase.Active ? Vector2.right * 8f : Vector2.zero));
        }

        [Test]
        public void AttackEnterStopsVelocityWithoutACombatDependency()
        {
            var attack = _player.AddComponent<PlayerAttackState>();
            var body = _player.GetComponent<TransformMovementBodyComponent>();
            SetField(attack, "_movementBody", body);
            attack.Init();
            _player.GetComponent<IMovementBodyVelocityMutator>().Velocity = Vector2.one;
            attack.Enter();
            Assert.That(body.Velocity, Is.EqualTo(Vector2.zero));
            Assert.That(attack.Tick(new PlayerMovementPayload(Vector2.right)), Is.EqualTo(PlayerMovementState.Attack));
        }

        [TestCase(0, DodgeMovementPhase.Preparation)]
        [TestCase(1, DodgeMovementPhase.Preparation)]
        [TestCase(2, DodgeMovementPhase.Active)]
        [TestCase(13, DodgeMovementPhase.Active)]
        [TestCase(14, DodgeMovementPhase.Recovery)]
        [TestCase(19, DodgeMovementPhase.Recovery)]
        [TestCase(20, DodgeMovementPhase.None)]
        public void DodgeBoundariesUseTheSingleMovementEndTick(int elapsed, DodgeMovementPhase expected)
        {
            var timing = new DodgeTimingData(2, 12, 6);
            Assert.That(DodgePhaseUtility.Evaluate(timing, 120, 100 + elapsed), Is.EqualTo(expected));
            int start = int.MaxValue - 4;
            Assert.That(DodgePhaseUtility.Evaluate(timing, unchecked(start + 20), unchecked(start + elapsed)), Is.EqualTo(expected));
        }

        [Test]
        public void DodgeQuantizesPhasesSeparatelyAndHasNoEarlyRecoveryWindow()
        {
            DodgeTimingData timing = MovementConfigValidation.CompileDodgeTiming(_dodgeConfig, 1f / 72f);
            Assert.That(timing.PreparationTicks, Is.EqualTo(3));
            Assert.That(timing.ActiveTicks, Is.EqualTo(15));
            Assert.That(timing.RecoveryTicks, Is.EqualTo(8));
            Assert.That(timing.TotalTicks, Is.EqualTo(26));
            Assert.That(DodgePhaseUtility.Evaluate(timing, null, 0), Is.EqualTo(DodgeMovementPhase.None));
        }

        [Test]
        public void MovementGraphBuildsAndMovesWithoutCombatBindings()
        {
            var root = _player.AddComponent<MovementOnlyRoot>();
            var machine = _player.AddComponent<PlayerMovementStateMachine>();
            var idle = _player.AddComponent<PlayerIdleState>();
            var locomotion = _player.AddComponent<PlayerLocomotionState>();
            var config = ScriptableObject.CreateInstance<LocomotionMovementConfig>();
            try
            {
                var body = _player.GetComponent<TransformMovementBodyComponent>();
                SetField(machine, "_idleState", idle);
                SetField(machine, "_locomotionState", locomotion);
                SetField(idle, "_movementBody", body);
                SetField(locomotion, "_movementBody", body);
                SetField(locomotion, "_locomotionMovementConfig", config);
                var state = new MovementOnlyState();
                var container = new DiContainer();
                container.Bind<INetworkBehaviourAccessor>().FromInstance(_player.GetComponent<INetworkBehaviourAccessor>());
                container.Bind<INetworkEntityIdAccessor>().FromInstance(_player.GetComponent<INetworkEntityIdAccessor>());
                container.Bind<Shared.IStateDataMutator<PlayerMovementState>>().FromInstance(state);
                container.Bind<IMovementStateDataMutator<PlayerMovementState>>().FromInstance(state);
                container.Bind<IInputBufferController>().FromInstance(new InitializationInputController());
                container.Bind<IPlayerFacingDirectionMutator>().FromInstance(new MovementOnlyFacing());
                Assert.DoesNotThrow(() => { container.Inject(machine); container.Inject(idle); container.Inject(locomotion); });
                Assert.That(machine.CreateStatesDictionary().Keys, Is.EquivalentTo(new[] { PlayerMovementState.Idle, PlayerMovementState.Locomotion }));
                idle.Init();
                locomotion.Init();
                Assert.That(idle.Tick(new PlayerMovementPayload(Vector2.right)), Is.EqualTo(PlayerMovementState.Locomotion));
                locomotion.Tick(new PlayerMovementPayload(Vector2.right));
                Assert.That(body.Velocity.x, Is.GreaterThan(0f));
            }
            finally { Object.DestroyImmediate(config); }
        }

        public sealed class MovementOnlyRoot : BaseNetworkEntityRoot
        {
            protected override System.Collections.Generic.IEnumerable<INetworkEntityComponent> CreateComponents() =>
                System.Array.Empty<INetworkEntityComponent>();
        }

        private sealed class MovementOnlyState : IMovementStateDataMutator<PlayerMovementState>
        {
            public PlayerMovementState CurrentState { get; private set; }
            public PlayerMovementState PreviousState { get; private set; }
            public PlayerMovementState CurrentMovementStates => CurrentState;
            public PlayerMovementState PreviousMovementStates => PreviousState;
            public void ChangeMovementState(PlayerMovementState state) => ChangeState(state);
            public void ChangeState(PlayerMovementState state) { PreviousState = CurrentState; CurrentState = state; }
        }

        private sealed class MovementOnlyFacing : IPlayerFacingDirectionMutator
        {
            public PlayerFacingDirection FacingDirection { get; private set; }
            public void SetFacingDirection(PlayerFacingDirection facing) { FacingDirection = facing; }
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
            SetField(_dashState, "_movementBody", _enemy.AddComponent<TransformMovementBodyComponent>());
            SetField(_dashState, "_dashMovementConfig", _dashConfig);

            var container = new DiContainer();
            var movementState = new InitializationMovementState();
            container.Bind<IInputBufferController>().FromInstance(new InitializationInputController());
            container.Bind<IPlayerMediator>().FromInstance(new InitializationMediator());
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

        private sealed class MovementVelocityMutator : IMovementBodyVelocityMutator
        {
            public Vector2 Velocity { get; set; }
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

        private sealed class InitializationMovementState : IMovementStateTimerMutator, IEnemyDashStateMutator
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

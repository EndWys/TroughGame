using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using Shared;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(
        true,
        sourceNamespace: "ProjectCore.TechnicalPrototype",
        sourceAssembly: "ProjectCore.Runtime",
        sourceClassName: "DodgeState")]
    public sealed class PlayerDodgeState : BaseEntityState<PlayerEntityState, PlayerEntityPayload>
    {
        [SerializeField] private DodgeMovementConfig _dodgeMovementConfig;

        private IMovementBodyVelocityMutator _movementBody;
        private IEntityStateTimerMutator _timer;
        private INetworkBehaviourAccessor _network;
        private ICombatInvulnerabilityMutator _invulnerability;
        private IInputBufferController _buffer;
        private IInputBufferStateMutator _bufferState;
        private ActionTimingData? _timing;

        [Inject]
        private void Construct(
            IEntityStateTimerMutator timer,
            INetworkBehaviourAccessor network,
            IMovementBodyVelocityMutator body,
            IInputBufferController buffer,
            IInputBufferStateMutator bufferState,
            ICombatInvulnerabilityMutator invulnerability)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _movementBody = body ?? throw new ArgumentNullException(nameof(body));
            _buffer = buffer ?? throw new ArgumentNullException(nameof(buffer));
            _bufferState = bufferState ?? throw new ArgumentNullException(nameof(bufferState));
            _invulnerability = invulnerability ?? throw new ArgumentNullException(nameof(invulnerability));
        }

        private ActionTimingData Timing =>
            _timing ??= MovementConfigValidation.CompileDodgeTiming(
                _dodgeMovementConfig,
                _network.ParentNetworkBehaviour.Runner.DeltaTime);

        public ActionPhase Phase => Timeline.Evaluate(_network.ParentNetworkBehaviour.Runner.Tick.Raw).Phase;

        private ActionTimelineData Timeline =>
            ActionTimelineData.FromEndTick(
                Timing,
                _timer.StateTimerEndTick);

        public override void Init()
        {
            MovementConfigValidation.ValidateDodge(_dodgeMovementConfig);
            _timing = null;
            base.Init();
        }

        public override void Enter()
        {
            _movementBody.Velocity = Vector2.zero;
            _bufferState.LockInput();

            // A restored network timer already identifies the execution; never restart it on correction.
            if (!_timer.StateTimerEndTick.HasValue)
                _timer.StartStateTimerTicks(Timing.TotalTicks);
        }

        public override PlayerEntityState Tick(PlayerEntityPayload payload)
        {
            PlayerEntityState next = base.Tick(payload);
            ActionTimelineData timeline = Timeline;

            ActionPhaseSnapshotData snapshot = timeline.Evaluate(
                _network.ParentNetworkBehaviour.Runner.Tick.Raw);

            _invulnerability.SetInvulnerabilityUntilTick(
                snapshot.Phase == ActionPhase.Active ? timeline.ActiveEndTick : null);

            return next;
        }

        public override void Exit()
        {
            _movementBody.Velocity = Vector2.zero;
            _timer.StopStateTimer();
            _invulnerability.SetInvulnerabilityUntilTick(null);
            _buffer.ReleaseCommandLock();
        }

        protected override IReadOnlyList<IStateProcessor<PlayerEntityState, PlayerEntityPayload>>
            CreateProcessors()
        {
            return new IStateProcessor<PlayerEntityState, PlayerEntityPayload>[]
            {
                CreateProcessor<PlayerDodgeMovementProcessor>(
                    _dodgeMovementConfig,
                    (Func<ActionPhase>)(() => Phase)),
            };
        }

        protected override PlayerEntityState FallbackState => PlayerEntityState.Dodge;
    }
}
